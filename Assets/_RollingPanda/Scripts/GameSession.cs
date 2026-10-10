using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    public sealed class GameSession : MonoBehaviour
    {
        readonly List<BlueGem> gems = new List<BlueGem>();
        RunnerMotor runner;
        double distance;
        public EndlessTrack Track { get; set; }
#if UNITY_EDITOR || UNITY_STANDALONE
        bool previewSafeArea;
#endif
        public bool Defeated { get; private set; }
        public int GemCount { get; private set; }
        public long Score => (long)distance + (long)GemCount * 10;
        public string DefeatReason { get; private set; }

        public void Initialize(RunnerMotor player)
        {
            runner = player; player.Session = this;
            RunnerInput.BlocksPointer = BlocksPointer;
            RunnerInput.TouchButtonAt = TouchButtonAt;
        }
        void OnDestroy()
        {
            if (RunnerInput.BlocksPointer == BlocksPointer) RunnerInput.BlocksPointer = null;
            if (RunnerInput.TouchButtonAt == TouchButtonAt) RunnerInput.TouchButtonAt = null;
        }
        int TouchButtonAt(Vector2 screenPosition)
        {
            Rect safe = SafeArea();
            float scale = UiScale(safe);
            if (scale <= 0) return 0;
            float width = safe.width / scale, height = safe.height / scale;
            Vector2 point = new Vector2((screenPosition.x - safe.x) / scale, (safe.yMax - screenPosition.y) / scale);
            if (new Rect(16, height - 68, (width - 40) / 2, 52).Contains(point)) return 1;
            if (new Rect(24 + (width - 40) / 2, height - 68, (width - 40) / 2, 52).Contains(point)) return 2;
            return 0;
        }

        void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.f8Key.wasPressedThisFrame)
            { previewSafeArea = !previewSafeArea; RunnerInput.Reset(); }
#endif
        }
        Rect SafeArea()
        {
            Rect safe = Screen.safeArea;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (previewSafeArea)
            {
                float unit = Mathf.Min(Screen.width, Screen.height);
                safe = new Rect(0, unit * 0.04f, Screen.width, Screen.height - unit * 0.12f);
            }
#endif
            return safe;
        }
        float UiScale(Rect safe) => Mathf.Min(safe.width / 390f, safe.height / 500f);
        bool BlocksPointer(Vector2 screenPosition)
        {
            Rect safe = SafeArea();
            if (!safe.Contains(screenPosition)) return true;
            float scale = UiScale(safe);
            float yFromTop = safe.yMax - screenPosition.y;
            return yFromTop < 178f * scale || yFromTop > safe.height - 80f * scale;
        }
        public void RegisterGem(BlueGem gem) { gems.Add(gem); gem.Initialize(this); }
        public void CollectGem() { if (!Defeated) GemCount++; }

        void LateUpdate()
        {
            if (runner != null && !runner.Paused)
                distance = System.Math.Max(distance, runner.DistanceTravelled);
        }

        public void Fail(string reason)
        {
            if (Defeated) return;
            distance = System.Math.Max(distance, runner.DistanceTravelled);
            Defeated = true;
            DefeatReason = reason;
        }

        public void ResetRun()
        {
            Defeated = false;
            DefeatReason = null;
            GemCount = 0;
            distance = 0f;
            foreach (var gem in gems) gem.ResetGem();
            if (Track != null) Track.ResetTrack();
        }

        void OnGUI()
        {
            if (runner == null) return;
            var previous = GUI.matrix;
            Rect safe = SafeArea();
            float scale = UiScale(safe);
            if (scale <= 0f) return;
            float width = safe.width / scale, height = safe.height / scale;
            GUI.matrix = Matrix4x4.TRS(new Vector3(safe.x, Screen.height - safe.yMax, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            var label = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            var button = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            GUI.Box(new Rect(8, 8, width - 16, 162), "Rolling Panda");
            GUI.Label(new Rect(18, 34, width - 36, 26), $"Score {Score}   Gems {GemCount}   {(long)distance} m", label);
            GUI.Label(new Rect(18, 62, width - 36, 43), "Swipe or mouse-drag: left/right = lanes, up = jump, down = slide", label);
            GUI.Label(new Rect(18, 105, width - 36, 22), "Red: dodge  •  Gold: jump  •  Purple: slide", label);
            string state = Defeated ? "Run ended — " + DefeatReason : runner.Finished ? "Course complete!" : runner.Paused ? "Paused — P to resume" : runner.Endless ? $"Endless run — speed {runner.CurrentSpeed:0.0} m/s" : "Running — each gem adds 10 points";
            GUI.Label(new Rect(18, 130, width - 36, 38), state, label);
            GUI.Box(new Rect(8, height - 76, width - 16, 68), "");
            bool restartClicked = GUI.Button(new Rect(16, height - 68, (width - 40) / 2, 52), "Restart", button);
            bool pauseClicked = GUI.Button(new Rect(24 + (width - 40) / 2, height - 68, (width - 40) / 2, 52), runner.Paused ? "Resume" : "Pause", button);
#if UNITY_EDITOR || UNITY_STANDALONE
            if (restartClicked) runner.Restart();
            if (pauseClicked) runner.TogglePause();
#endif
            GUI.matrix = previous;
        }
    }
}
