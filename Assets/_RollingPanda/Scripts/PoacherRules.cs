using System;
namespace RollingPanda
{
    public static class PoacherRules
    {
        public const float CaptureDuration=3.6f;
        public static float Progress(float age,float start,float end)
        {
            float t=Math.Max(0,Math.Min(1,(age-start)/(end-start)));return t*t*(3-2*t);
        }
        public static float Advance(float age,float dt,bool paused)=>paused ? age : Math.Min(CaptureDuration,age+Math.Max(0,dt));
        public static float Net(float age)=>Progress(age,.65f,1.4f);
        public static float Dart(float age)=>Progress(age,1.85f,2.1f);
        public static float Sleep(float age)=>Progress(age,2.1f,2.8f);
        public static bool Complete(float age)=>age>=CaptureDuration;
    }
}
