using UnityEngine;

namespace RollingPanda
{
    public sealed class BlueGem : MonoBehaviour
    {
        GameSession session;
        bool collected;
        public PickupKind Kind { get; set; } = PickupKind.Blue;

        public void Initialize(GameSession game) => session = game;

        bool attracted;
        Vector3 home;
        void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent<RunnerMotor>(out var runner) && runner.Session==session) TryCollect();
        }
        public bool TryCollect()
        {
            if(collected || session==null || !session.IsRunning || !gameObject.activeInHierarchy) return false;
            collected=true;session.CollectGem(Kind,transform.position);gameObject.SetActive(false);return true;
        }
        public void PullTowards(Vector3 target,float dt)
        {
            if(collected || !gameObject.activeInHierarchy) return;
            if(!attracted){home=transform.localPosition;attracted=true;}
            transform.position=Vector3.MoveTowards(transform.position,target,Mathf.Max(65,Vector3.Distance(transform.position,target)*8)*dt);
            if(Vector3.Distance(transform.position,target)<1.1f) TryCollect();
        }
        public void ReleaseMagnet()
        {
            if(attracted && !collected) transform.localPosition=home;
            attracted=false;
        }

        public void ResetGem()
        {
            collected = false;attracted=false;
            gameObject.SetActive(true);
        }
    }
}
