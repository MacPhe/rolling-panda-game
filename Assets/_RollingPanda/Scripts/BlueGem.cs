using UnityEngine;

namespace RollingPanda
{
    public sealed class BlueGem : MonoBehaviour
    {
        GameSession session;
        bool collected;

        public void Initialize(GameSession game) => session = game;

        void OnTriggerEnter(Collider other)
        {
            if (collected || session == null || !other.TryGetComponent<RunnerMotor>(out var runner)
                || runner.Session != session || runner.Paused || runner.Finished || session.Defeated) return;
            collected = true;
            session.CollectGem();
            gameObject.SetActive(false);
        }

        public void ResetGem()
        {
            collected = false;
            gameObject.SetActive(true);
        }
    }
}
