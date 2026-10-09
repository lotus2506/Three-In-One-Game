using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FinalGame.Editor
{
    public static class FinalGameSetup
    {
        const string Root = "Assets/FinalGame/";
        const string Driving = "Assets/Imported/Driving/Scenes/Prototype 1.unity";
        const string Flying = "Assets/Imported/Flying/Challenge 1/Challenge 1.unity";
        public static void SmokeTest()
        {
            EditorSceneManager.OpenScene(Root + "Scenes/MainMenu.unity");
            EditorApplication.isPlaying = true;
        }
        public static void RepairFlyingLighting()
        {
            var scene = EditorSceneManager.OpenScene(Flying);
            foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                light.lightmapBakeType = LightmapBakeType.Realtime;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .6f, .65f);
            Lightmapping.lightingDataAsset = null;
            if (Lightmapping.lightingSettings != null)
            {
                Lightmapping.lightingSettings.bakedGI = false;
                Lightmapping.lightingSettings.realtimeGI = false;
                Lightmapping.lightingSettings.autoGenerate = false;
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
        [MenuItem("FinalGame/Build macOS App")]
        public static void BuildMac()
        {
            RepairGames();
            RepairFlyingLighting();
            var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                "Builds/FinalGame.app", BuildTarget.StandaloneOSX, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("FinalGame build failed: " + report.summary.result);
            Debug.Log("FINALGAME_BUILD_OK");
        }
        [MenuItem("FinalGame/Set Up Game Scenes")]
        public static void Setup()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            System.IO.Directory.CreateDirectory(Root + "Scenes");
            System.IO.Directory.CreateDirectory(Root + "Materials");
            System.IO.Directory.CreateDirectory(Root + "Prefabs");
            AssetDatabase.Refresh();
            var main = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Camera.main.backgroundColor = new Color(.025f, .055f, .09f);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            EditorSceneManager.SaveScene(main, Root + "Scenes/MainMenu.unity");

            var sumo = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(.035f, .065f, .12f);
            RenderSettings.ambientLight = new Color(.6f, .65f, .75f);
            var blue = Material("Player", new Color(.1f, .85f, 1));
            var red = Material("Opponent", new Color(1, .28f, .22f));
            var floorMat = Material("Arena", new Color(.22f, .29f, .37f));
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Sumo platform";
            ground.transform.localScale = new Vector3(24, .5f, 24);
            ground.transform.position = new Vector3(0, -.5f, 0);
            ground.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Reuse the course ball's mesh, collider and physics material.
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imported/SumoAssets/Challenge 4/Prefabs/Enemy.prefab");
            var ball = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(ball, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            foreach (var behaviour in ball.GetComponents<MonoBehaviour>()) UnityEngine.Object.DestroyImmediate(behaviour);
            ball.name = "SumoBall";
            ball.tag = "Untagged";
            var rb = ball.GetComponent<Rigidbody>();
            rb.linearDamping = .5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            ball.AddComponent<SumoBall>();
            var template = PrefabUtility.SaveAsPrefabAsset(ball, Root + "Prefabs/SumoBall.prefab");
            UnityEngine.Object.DestroyImmediate(ball);
            var arena = new GameObject("Sumo Arena").AddComponent<SumoArena>();
            arena.ballTemplate = template;
            arena.playerMaterial = blue;
            arena.enemyMaterial = red;
            Camera.main.transform.position = new Vector3(0, 19, -23);
            Camera.main.transform.LookAt(Vector3.zero);
            EditorSceneManager.SaveScene(sumo, Root + "Scenes/Sumo.unity");
            EditorBuildSettings.scenes = new[] { Root + "Scenes/MainMenu.unity", Driving, Flying, Root + "Scenes/Sumo.unity" }
                .Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            PlayerSettings.productName = "FinalGame";
            PlayerSettings.defaultScreenWidth = 1130;
            PlayerSettings.defaultScreenHeight = 768;
            AssetDatabase.SaveAssets();
            RepairGames();
            RepairFlyingLighting();
            Validate();
            EditorSceneManager.OpenScene(Root + "Scenes/MainMenu.unity");
            Debug.Log("FINALGAME_SETUP_OK: four scenes ready. Open MainMenu and press Play.");
        }
        static Material Material(string name, Color color)
        {
            string path = Root + "Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(mat, path); }
            mat.color = color;
            return mat;
        }
        [MenuItem("FinalGame/Repair Driving and Sumo")]
        public static void RepairGames()
        {
            var driving = EditorSceneManager.OpenScene(Driving);
            var driver = UnityEngine.Object.FindFirstObjectByType<Imported.Driving.PlayerController>();
            var bounds = new Bounds(driver.transform.position, Vector3.zero);
            foreach (var r in driver.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            foreach (var c in driver.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(c);
            var carCollider = driver.gameObject.AddComponent<BoxCollider>();
            carCollider.center = driver.transform.InverseTransformPoint(bounds.center);
            carCollider.size = new Vector3(bounds.size.x * .9f, bounds.size.y * .9f, bounds.size.z * .9f);
            foreach (var c in UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
            {
                if (c.size.y < .01f)
                {
                    c.size = new Vector3(c.size.x, .2f, c.size.z);
                    c.center += Vector3.down * .1f;
                }
            }
            EditorSceneManager.SaveScene(driving);

            var sumo = EditorSceneManager.OpenScene(Root + "Scenes/Sumo.unity");
            var ground = GameObject.Find("Sumo platform");
            foreach (var c in ground.GetComponents<Collider>()) UnityEngine.Object.DestroyImmediate(c);
            // Unity's primitive cylinder comes with a capsule collider. Use its flat mesh instead.
            var platformCollider = ground.AddComponent<MeshCollider>();
            platformCollider.sharedMesh = ground.GetComponent<MeshFilter>().sharedMesh;
            ground.GetComponent<Renderer>().sharedMaterial = Material("Arena", new Color(.67f, .49f, .29f));
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .6f, .67f);
            var arena = UnityEngine.Object.FindFirstObjectByType<SumoArena>();
            var skin = Material("SumoSkin", new Color(1f, .69f, .43f));
            var hair = Material("SumoHair", new Color(.035f, .028f, .027f));
            var white = Material("SumoEyes", Color.white);
            var belt = arena.playerMaterial;
            var wrestler = new GameObject("SumoBall");
            var body = wrestler.AddComponent<Rigidbody>();
            body.mass = 1;
            body.linearDamping = 1.8f;
            body.angularDamping = 4;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var collider = wrestler.AddComponent<SphereCollider>();
            collider.radius = .93f;
            var physicsPath = Root + "Materials/SumoGrip.physicMaterial";
            var physicsMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(physicsPath);
            if (physicsMaterial == null)
            {
                physicsMaterial = new PhysicsMaterial("SumoGrip") { dynamicFriction = .15f, staticFriction = .15f, bounciness = 0 };
                AssetDatabase.CreateAsset(physicsMaterial, physicsPath);
            }
            collider.sharedMaterial = physicsMaterial;
            wrestler.AddComponent<SumoBall>();
            var visual = new GameObject("Wrestler").transform;
            visual.SetParent(wrestler.transform, false);
            Shape(visual, "Round belly", PrimitiveType.Sphere, Vector3.zero, new Vector3(1.9f, 1.8f, 1.65f), skin);
            Shape(visual, "Head", PrimitiveType.Sphere, new Vector3(0, 1.03f, .08f), new Vector3(.85f, .8f, .78f), skin);
            Shape(visual, "Hair cap", PrimitiveType.Sphere, new Vector3(0, 1.32f, .02f), new Vector3(.83f, .29f, .72f), hair);
            Shape(visual, "Topknot", PrimitiveType.Sphere, new Vector3(0, 1.59f, -.03f), new Vector3(.29f, .36f, .29f), hair);
            Shape(visual, "Mawashi Belt", PrimitiveType.Cylinder, new Vector3(0, -.4f, 0), new Vector3(1.84f, .15f, 1.61f), belt);
            Shape(visual, "Mawashi front", PrimitiveType.Cube, new Vector3(0, -.62f, .69f), new Vector3(.45f, .55f, .18f), belt);
            for (int side = -1; side <= 1; side += 2)
            {
                Shape(visual, "Arm", PrimitiveType.Sphere, new Vector3(side * .98f, .14f, .17f), new Vector3(.5f, .65f, .5f), skin);
                Shape(visual, "Hand", PrimitiveType.Sphere, new Vector3(side * 1.08f, -.03f, .38f), Vector3.one * .38f, skin);
                Shape(visual, "Foot", PrimitiveType.Sphere, new Vector3(side * .48f, -.84f, .28f), new Vector3(.5f, .28f, .7f), skin);
                Shape(visual, "Eye white", PrimitiveType.Sphere, new Vector3(side * .17f, 1.12f, .421f), new Vector3(.16f, .13f, .07f), white);
                Shape(visual, "Pupil", PrimitiveType.Sphere, new Vector3(side * .17f, 1.12f, .463f), new Vector3(.07f, .09f, .04f), hair);
                Shape(visual, "Eyebrow", PrimitiveType.Cube, new Vector3(side * .17f, 1.24f, .43f), new Vector3(.19f, .045f, .055f), hair);
            }
            Shape(visual, "Nose", PrimitiveType.Sphere, new Vector3(0, 1.02f, .48f), new Vector3(.13f, .15f, .16f), skin);
            visual.localRotation = Quaternion.Euler(0, 180, 0);
            arena.ballTemplate = PrefabUtility.SaveAsPrefabAsset(wrestler, Root + "Prefabs/SumoBall.prefab");
            UnityEngine.Object.DestroyImmediate(wrestler);
            var oldRing = GameObject.Find("Dohyo boundary");
            if (oldRing != null) UnityEngine.Object.DestroyImmediate(oldRing);
            var ring = new GameObject("Dohyo boundary").AddComponent<LineRenderer>();
            ring.sharedMaterial = Material("DohyoRope", new Color(.96f, .83f, .52f));
            ring.positionCount = 96; ring.loop = true; ring.widthMultiplier = .16f;
            for (int i = 0; i < 96; i++)
            {
                float a = i * Mathf.PI * 2 / 96;
                ring.SetPosition(i, new Vector3(Mathf.Cos(a) * 11.5f, .03f, Mathf.Sin(a) * 11.5f));
            }
            EditorSceneManager.SaveScene(sumo);
            AssetDatabase.SaveAssets();
            Debug.Log("FINALGAME_REPAIRS_OK: physics car, flat sumo arena, round wrestlers.");
        }
        static void Shape(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = position;
            shape.transform.localScale = scale;
            shape.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(shape.GetComponent<Collider>());
        }
        [MenuItem("FinalGame/Validate Scenes")]
        public static void Validate()
        {
            foreach (var entry in EditorBuildSettings.scenes)
            {
                var scene = EditorSceneManager.OpenScene(entry.path);
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                            throw new Exception("Missing script: " + entry.path + " / " + t.name);
                if (Camera.main == null) throw new Exception("No main camera: " + entry.path);
            }
            Debug.Log("FINALGAME_VALIDATION_OK: all four scenes have cameras and no missing scripts.");
        }
    }
}
