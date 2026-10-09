using UnityEngine;
using UnityEngine.SceneManagement;

namespace FinalGame
{
    public class GameMenus : MonoBehaviour
    {
        public const string MainScene = "MainMenu";
        public const string DrivingScene = "Prototype 1";
        public const string FlyingScene = "Challenge 1";
        public const string SumoScene = "Sumo";
        public bool IsPaused { get; private set; }
        public bool IsMainMenu => SceneManager.GetActiveScene().name == MainScene;
        GUIStyle title, subtitle, button, small;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindFirstObjectByType<GameMenus>() == null)
                new GameObject("FinalGame Menus").AddComponent<GameMenus>();
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode) { SetPaused(false); }
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
        void Update()
        {
            if (!IsMainMenu && GameInput.EscapePressed) SetPaused(!IsPaused);
        }
        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        public void LoadGame(string scene)
        {
            SetPaused(false);
            SceneManager.LoadScene(scene);
        }
        public void Restart() => LoadGame(SceneManager.GetActiveScene().name);
        public void ExitGame()
        {
            SetPaused(false);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void Styles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 52, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            subtitle = new GUIStyle(GUI.skin.label) { fontSize = 19, alignment = TextAnchor.MiddleCenter };
            button = new GUIStyle(GUI.skin.button) { fontSize = 25, alignment = TextAnchor.MiddleCenter };
            small = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleCenter };
            title.normal.textColor = Color.white;
            subtitle.normal.textColor = new Color(.58f, .79f, .85f);
            small.normal.textColor = Color.white;
        }
        void OnGUI()
        {
            Styles();
            var oldMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1130f, Screen.height / 768f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1130*scale)/2, (Screen.height-768*scale)/2), Quaternion.identity, Vector3.one * scale);
            if (IsMainMenu || IsPaused)
            {
                var oldColor = GUI.color;
                GUI.color = new Color(.025f, .055f, .09f, IsMainMenu ? 1f : .95f);
                GUI.DrawTexture(new Rect(-Screen.width/scale, -Screen.height/scale, Screen.width/scale*3, Screen.height/scale*3), Texture2D.whiteTexture);
                GUI.color = oldColor;
                GUI.Label(new Rect(100, 95, 930, 90), IsMainMenu ? "FINALGAME" : "PAUSED", title);
                GUI.Label(new Rect(100, 187, 930, 40), IsMainMenu ? "THREE GAMES. ONE ARCADE." : "Take a breath. Jump back in.", subtitle);
                if (IsMainMenu)
                {
                    if (GUI.Button(new Rect(335, 285, 460, 62), "Mad Driver", button)) LoadGame(DrivingScene);
                    if (GUI.Button(new Rect(335, 365, 460, 62), "flyingplane", button)) LoadGame(FlyingScene);
                    if (GUI.Button(new Rect(335, 445, 460, 62), "I'm a Sumo and a Ball", button)) LoadGame(SumoScene);
                    if (GUI.Button(new Rect(335, 525, 460, 62), "Exit", button)) ExitGame();
                    GUI.Label(new Rect(100, 665, 930, 40), "Choose a game to begin", small);
                    GUI.Label(new Rect(700, 720, 400, 32), "By Chaw Yadanar Oo", small);
                }
                else
                {
                    if (GUI.Button(new Rect(335, 305, 460, 62), "Resume", button)) SetPaused(false);
                    if (GUI.Button(new Rect(335, 385, 460, 62), "Restart", button)) Restart();
                    if (GUI.Button(new Rect(335, 465, 460, 62), "Back to Main Menu", button)) LoadGame(MainScene);
                    GUI.Label(new Rect(100, 650, 930, 40), "ESC  /  Resume", small);
                }
            }
            else
            {
                string scene = SceneManager.GetActiveScene().name;
                string hint = scene == DrivingScene ? "MAD DRIVER  |  Hold W / Up to drive  •  S reverse  •  A/D steer" : scene == FlyingScene ? "flyingplane  |  W/S pitch" : "SUMO  |  WASD / arrows move  •  Space charge";
                GUI.Box(new Rect(15, 15, 1100, 42), GUIContent.none);
                GUI.Label(new Rect(20, 18, 1090, 36), hint + "  |  ESC menu", small);
            }
            GUI.matrix = oldMatrix;
        }
    }
}
