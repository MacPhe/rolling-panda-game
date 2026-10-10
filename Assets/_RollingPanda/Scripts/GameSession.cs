using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    public sealed class GameSession : MonoBehaviour
    {
        readonly List<BlueGem> gems = new List<BlueGem>();
        RunnerMotor runner;
        float distance;
        public bool Defeated { get; private set; }
        public int GemCount { get; private set; }
        public int Score => Mathf.FloorToInt(distance) + GemCount * 10;
        public string DefeatReason { get; private set; }

        public void Initialize(RunnerMotor player) { runner = player; player.Session = this; }
        public void RegisterGem(BlueGem gem) { gems.Add(gem); gem.Initialize(this); }
        public void CollectGem() { if (!Defeated) GemCount++; }

        void LateUpdate()
        {
            if (runner != null && !Defeated && !runner.Paused)
                distance = Mathf.Max(distance, runner.transform.position.z);
        }

        public void Fail(string reason)
        {
            if (Defeated) return;
            distance = Mathf.Max(distance, runner.transform.position.z);
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
        }

        void OnGUI()
        {
            if (runner == null) return;
            var previous = GUI.matrix;
            float scale = Mathf.Clamp(Screen.width / 1000f, 0.65f, 1.5f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.Box(new Rect(12, 12, 470, 185), "Rolling Panda — first game loop");
            GUI.Label(new Rect(24, 40, 440, 25), $"Score: {Score}    Distance: {Mathf.FloorToInt(distance)} m    Blue gems: {GemCount}");
            GUI.Label(new Rect(24, 65, 440, 25), "A/D: lanes    Space: jump    S/Down: slide    P: pause");
            GUI.Label(new Rect(24, 90, 440, 25), "Red: dodge    Gold: jump    Purple: slide    Blue: collect");
            string state = Defeated ? "Run ended — " + DefeatReason : runner.Finished ? "Course complete!" : runner.Paused ? "Paused — P to resume" : "Running — each gem adds 10 points";
            GUI.Label(new Rect(24, 115, 450, 30), state);
            if (GUI.Button(new Rect(24, 152, 140, 30), "Restart (R)")) runner.Restart();
            GUI.matrix = previous;
        }
    }
}
