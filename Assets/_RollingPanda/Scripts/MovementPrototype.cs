using UnityEngine;

namespace RollingPanda
{
    // Only attached to MovementPrototype.unity. Creates disposable geometry on Play;
    // stopping Play leaves the saved scene and Phase 1 Main scene untouched.
    public sealed class MovementPrototype : MonoBehaviour
    {
        RunnerMotor runner;
        Material groundMaterial, lineMaterial, playerMaterial, bandMaterial;

        void Awake()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("Movement prototype needs the Universal Render Pipeline/Lit shader.", this);
                enabled = false;
                return;
            }
            groundMaterial = MakeMaterial(shader, new Color(0.25f, 0.36f, 0.28f));
            lineMaterial = MakeMaterial(shader, new Color(0.8f, 0.72f, 0.46f));
            playerMaterial = MakeMaterial(shader, new Color(0.92f, 0.92f, 0.88f));
            bandMaterial = MakeMaterial(shader, new Color(0.8f, 0.08f, 0.06f));

            MakeCube("Ground — 1 km test track", new Vector3(0f, -0.15f, 495f), new Vector3(10f, 0.3f, 1030f), groundMaterial, true);
            foreach (float x in new[] { -4.8f, -1.5f, 1.5f, 4.8f })
                MakeCube("Lane marking", new Vector3(x, 0.01f, 495f), new Vector3(0.08f, 0.02f, 1030f), lineMaterial, false);
            for (int z = 10; z < 990; z += 20)
                MakeCube("Distance marker", new Vector3(0f, 0.015f, z), new Vector3(9.5f, 0.02f, 0.12f), lineMaterial, false);

            var player = new GameObject("Panda placeholder");
            player.transform.SetParent(transform);
            player.transform.position = new Vector3(0f, 0.05f, 0f);
            var controller = player.AddComponent<CharacterController>();
            controller.radius = 0.4f;
            controller.skinWidth = 0.03f;
            controller.stepOffset = 0.2f;
            controller.minMoveDistance = 0f;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body visual";
            body.transform.SetParent(player.transform, false);
            RemoveCollider(body);
            body.GetComponent<Renderer>().sharedMaterial = playerMaterial;
            var band = MakeCube("Red headband cue", Vector3.zero, Vector3.one, bandMaterial, false);
            band.transform.SetParent(body.transform, false);
            band.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            band.transform.localScale = new Vector3(1.03f, 0.13f, 1.03f);
            runner = player.AddComponent<RunnerMotor>();
            runner.Initialize(body.transform);

            Camera camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }
            camera.fieldOfView = 60f;
            camera.farClipPlane = 160f;
            camera.gameObject.AddComponent<RunnerCamera>().Initialize(player.transform);
        }

        Material MakeMaterial(Shader shader, Color color)
        {
            var material = new Material(shader);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.15f);
            return material;
        }

        GameObject MakeCube(string objectName, Vector3 position, Vector3 scale, Material material, bool collidable)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(transform);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            if (!collidable) RemoveCollider(cube);
            return cube;
        }

        static void RemoveCollider(GameObject obj)
        {
            var collider = obj.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
        }

        void OnGUI()
        {
            if (runner == null) return;
            // Scale the keyboard test overlay to remain readable in small Game views.
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = Mathf.Clamp(Screen.width / 1000f, 0.65f, 1.5f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.Box(new Rect(12f, 12f, 430f, 122f), "Rolling Panda — movement checkpoint");
            GUI.Label(new Rect(24f, 40f, 410f, 25f), "A / D or arrows: lanes    Space / W / Up: jump");
            GUI.Label(new Rect(24f, 65f, 410f, 25f), "S / Down: slide    P / Esc: pause    R: restart");
            string state = runner.Finished ? "Track complete — press R to restart" : runner.Paused ? "Paused — press P to continue" : runner.Sliding ? "Sliding" : "Running — click Game view for keyboard input";
            GUI.Label(new Rect(24f, 94f, 410f, 25f), state);
            GUI.matrix = previousMatrix;
        }

        void OnDestroy()
        {
            if (groundMaterial != null) Destroy(groundMaterial);
            if (lineMaterial != null) Destroy(lineMaterial);
            if (playerMaterial != null) Destroy(playerMaterial);
            if (bandMaterial != null) Destroy(bandMaterial);
        }
    }
}
