#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace FinalGame
{
    // Runs only with the explicit command-line test flag, never during normal play.
    public class FinalGameSmokeTest : MonoBehaviour
    {
        Keyboard keyboard;
        bool failed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-finalGameSmoke") >= 0)
                new GameObject("FinalGame automated smoke test").AddComponent<FinalGameSmokeTest>();
        }
        void Awake() { DontDestroyOnLoad(gameObject); Application.logMessageReceived += Log; }
        void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            { failed = true; System.IO.File.AppendAllText("/tmp/finalgame-smoke-result.txt", "ERROR: " + message + "\n" + stack + "\n"); }
        }
        void Check(bool condition, string label)
        {
            if (!condition) failed = true;
            System.IO.File.AppendAllText("/tmp/finalgame-smoke-result.txt", (condition ? "PASS " : "FAIL ") + label + "\n");
        }
        void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        IEnumerator Start()
        {
            System.IO.File.WriteAllText("/tmp/finalgame-smoke-result.txt", "FinalGame play-mode smoke test\n");
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return null;
            var menu = FindFirstObjectByType<GameMenus>();
            Check(menu != null && menu.IsMainMenu, "Main menu bootstrap");
            foreach (string scene in new[] { GameMenus.DrivingScene, GameMenus.FlyingScene, GameMenus.SumoScene })
            {
                menu.LoadGame(scene);
                yield return null;
                yield return new WaitForSecondsRealtime(.2f);
                Check(SceneManager.GetActiveScene().name == scene, scene + " navigation");
                Transform player = null;
                if (scene == GameMenus.DrivingScene) player = FindFirstObjectByType<Imported.Driving.PlayerController>().transform;
                if (scene == GameMenus.FlyingScene) player = FindFirstObjectByType<Imported.Flying.PlayerControllerX>().transform;
                if (scene == GameMenus.SumoScene) player = FindFirstObjectByType<SumoArena>().Player;
                if (scene == GameMenus.SumoScene)
                {
                    bool flat = true;
                    foreach (var point in new[] { Vector3.zero, Vector3.right * 8, Vector3.forward * 8 })
                    {
                        // Probe just the platform collider, avoiding characters above it.
                        var platform = GameObject.Find("Sumo platform").GetComponent<Collider>();
                        flat &= platform.Raycast(new Ray(point + Vector3.up * 20, Vector3.down), out var hit, 30) && Mathf.Abs(hit.point.y) < .05f;
                    }
                    Check(flat, "Sumo platform is flat at center and both edges");
                    Check(player.Find("Wrestler/Head") != null && player.Find("Wrestler/Topknot") != null, "Sumo player has wrestler appearance");
                    yield return new WaitForSecondsRealtime(.7f);
                    Check(player.position.y > .8f && player.position.y < 1.1f, "Sumo player settles on arena instead of floating or falling");
                }
                var initial = player.position;
                Keys(Key.W);
                yield return new WaitForSecondsRealtime(.5f);
                Keys();
                Vector3 delta = player.position - initial;
                Check(Vector3.ProjectOnPlane(delta, Vector3.up).magnitude > .5f, scene + " input produces horizontal travel, not gravity-only movement");
                if (scene == GameMenus.DrivingScene)
                {
                    Check(delta.z > 2f && Mathf.Abs(delta.y) < 2f, "Driving W moves forward along road");
                    float startZ = player.position.z;
                    Keys(Key.UpArrow);
                    yield return new WaitForSecondsRealtime(.2f);
                    Keys();
                    Check(player.position.z > startZ + .5f, "Driving Up arrow moves forward");
                }
                if (scene == GameMenus.SumoScene)
                {
                    float startX = player.position.x;
                    Keys(Key.D);
                    yield return new WaitForSecondsRealtime(.4f);
                    Keys();
                    Check(player.position.x > startX + .5f, "Sumo D moves right");
                }
                Keys(Key.Escape);
                yield return null;
                yield return null;
                Keys();
                Check(menu.IsPaused && Time.timeScale == 0 && AudioListener.pause, scene + " Escape pauses time and audio");
                var pausedPosition = player.position;
                Keys(Key.W, Key.Space);
                yield return new WaitForSecondsRealtime(.3f);
                Keys();
                Check(Vector3.Distance(pausedPosition, player.position) < .001f, scene + " remains still while paused");
                menu.SetPaused(false);
                yield return null;
                Check(Time.timeScale == 1 && !AudioListener.pause, scene + " Resume");
                menu.SetPaused(true);
                int previousHandle = SceneManager.GetActiveScene().handle;
                menu.Restart();
                yield return null;
                yield return new WaitForSecondsRealtime(.1f);
                Check(SceneManager.GetActiveScene().handle != previousHandle && Time.timeScale == 1 && !menu.IsPaused, scene + " Restart reloads and unpauses");
                if (scene == GameMenus.SumoScene)
                {
                    var arena = FindFirstObjectByType<SumoArena>();
                    Check(arena.Wave == 1 && !arena.GameOver, "Sumo restart resets wave and game-over state");
                    foreach (var ball in FindObjectsByType<SumoBall>(FindObjectsSortMode.None))
                        if (!ball.isPlayer) ball.GetComponent<Rigidbody>().position = new Vector3(0, -10, 0);
                    yield return new WaitForSecondsRealtime(.4f);
                    Check(arena.Wave == 2, "Sumo advances after opponents fall");
                    arena.Player.GetComponent<Rigidbody>().position = new Vector3(0, -10, 0);
                    yield return new WaitForSecondsRealtime(.3f);
                    Check(arena.GameOver, "Sumo detects player falling");
                }
                menu.SetPaused(true);
                menu.LoadGame(GameMenus.MainScene);
                yield return null;
                Check(menu.IsMainMenu && Time.timeScale == 1 && !AudioListener.pause, scene + " Back to Main Menu");
                Check(FindObjectsByType<GameMenus>(FindObjectsSortMode.None).Length == 1, "No duplicate menu managers");
            }
            InputSystem.RemoveDevice(keyboard);
            System.IO.File.AppendAllText("/tmp/finalgame-smoke-result.txt", failed ? "FAILED\n" : "ALL CHECKS PASSED\n");
            UnityEditor.EditorApplication.Exit(failed ? 1 : 0);
        }
    }
}
#endif
