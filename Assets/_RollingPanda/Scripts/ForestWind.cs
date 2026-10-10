using UnityEngine;
namespace RollingPanda
{
    // Grounded presentation pivot only: never moves a track, pickup or collision object.
    public sealed class ForestWind : MonoBehaviour
    {
        Quaternion rest;
        RunnerMotor runner;
        float strength, phase, clock;
        public void Initialize(RunnerMotor player,float degrees,float seed)
        { runner=player;strength=degrees;phase=seed;rest=transform.localRotation; }
        public void Bind(RunnerMotor player) { runner=player; }
        void LateUpdate()
        {
            bool menu=RunEntryTransition.Active && !RunEntryTransition.Revealing;
            if(runner!=null && runner.Session!=null && !runner.Session.IsRunning && !menu) return;
            clock+=Time.deltaTime;
            if(runner!=null)
            {
                float dz=transform.position.z-runner.transform.position.z;
                if(dz < -25 || dz > 125) return;
            }
            float breeze=Mathf.Sin(clock*.85f+phase)*.65f+Mathf.Sin(clock*1.63f+phase*.7f)*.2f;
            float gust=.7f+.3f*Mathf.Sin(clock*.23f);
            transform.localRotation=rest*Quaternion.Euler(breeze*strength*.35f,0,breeze*strength*gust);
        }
    }
}
