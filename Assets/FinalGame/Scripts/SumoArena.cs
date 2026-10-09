using System.Collections.Generic;
using UnityEngine;

namespace FinalGame
{
    // Wave and power-up loop adapted from the player's Challenge 4 course work.
    public class SumoArena : MonoBehaviour
    {
        public GameObject ballTemplate;
        public Material playerMaterial;
        public Material enemyMaterial;
        public Transform Player { get; private set; }
        public int Wave { get; private set; }
        public bool GameOver { get; private set; }
        public bool Powered => powerTime > 0;
        public Vector3 Forward => Vector3.forward;
        readonly List<GameObject> enemies = new List<GameObject>();
        float powerTime, boostCooldown;
        GameObject powerup;
        Camera view;

        void Start()
        {
            view = Camera.main;
            Player = SpawnBall(new Vector3(0, 1, -4), true).transform;
            NextWave();
        }
        GameObject SpawnBall(Vector3 position, bool player)
        {
            GameObject ball = Instantiate(ballTemplate, position, Quaternion.identity);
            ball.name = player ? "Player" : "Sumo opponent";
            ball.SetActive(true);
            foreach (var renderer in ball.GetComponentsInChildren<Renderer>())
                if (renderer.name.Contains("Belt") || renderer.name.Contains("Mawashi"))
                    renderer.sharedMaterial = player ? playerMaterial : enemyMaterial;
            var controller = ball.GetComponent<SumoBall>();
            controller.isPlayer = player;
            controller.arena = this;
            return ball;
        }
        void NextWave()
        {
            Wave++;
            int count = Mathf.Min(Wave, 8);
            for (int i = 0; i < count; i++)
            {
                float angle = (i * Mathf.PI * 2 / count) + Random.Range(-.2f, .2f);
                enemies.Add(SpawnBall(new Vector3(Mathf.Sin(angle) * 8, 1, Mathf.Cos(angle) * 8), false));
            }
            if (powerup != null) Destroy(powerup);
            powerup = GameObject.CreatePrimitive(PrimitiveType.Cube);
            powerup.name = "Powerup - touch for seven seconds of strength";
            powerup.transform.position = new Vector3(3, 1, -2);
            powerup.transform.localScale = Vector3.one * .8f;
            Destroy(powerup.GetComponent<Collider>());
            powerup.GetComponent<Renderer>().sharedMaterial = playerMaterial;
        }
        void Update()
        {
            if (Time.timeScale == 0 || GameOver) return;
            powerTime = Mathf.Max(0, powerTime - Time.deltaTime);
            boostCooldown = Mathf.Max(0, boostCooldown - Time.deltaTime);
            if (GameInput.SpacePressed && boostCooldown <= 0)
            {
                Player.GetComponent<Rigidbody>().AddForce(Player.GetComponent<SumoBall>().Facing * 10, ForceMode.Impulse);
                boostCooldown = 2;
            }
            if (Player.position.y < -5) { GameOver = true; return; }
            if (powerup != null)
            {
                powerup.transform.Rotate(30 * Time.deltaTime, 70 * Time.deltaTime, 0);
                if (Vector3.Distance(Player.position, powerup.transform.position) < 1.7f)
                { powerTime = 7; Destroy(powerup); }
            }
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i] == null || enemies[i].transform.position.y < -5)
                { if (enemies[i] != null) Destroy(enemies[i]); enemies.RemoveAt(i); }
            }
            if (enemies.Count == 0) NextWave();
        }
        void LateUpdate()
        {
            if (view == null) return;
            view.transform.position = new Vector3(0, 18, -22);
            view.transform.LookAt(Vector3.zero);
        }
        void OnGUI()
        {
            var menu = FindFirstObjectByType<GameMenus>();
            if (menu != null && menu.IsPaused) return;
            var style = new GUIStyle(GUI.skin.box) { fontSize = Mathf.Max(14, Screen.height / 36), alignment = TextAnchor.MiddleCenter };
            string status = GameOver ? "You fell! Press ESC > Restart to try again." : "YOU: BLUE BELT  |  Wave " + Wave + "  |  Rivals: " + enemies.Count + (Powered ? "  |  POWER " + Mathf.CeilToInt(powerTime) + "s" : "  |  Push red wrestlers off!");
            GUI.Box(new Rect(Screen.width * .1f, Screen.height - 65, Screen.width * .8f, 50), status, style);
        }
    }
}
