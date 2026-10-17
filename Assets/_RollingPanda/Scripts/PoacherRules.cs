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
        public static float Follow(float rate,float dt)=>(float)(1-Math.Exp(-Math.Max(0,rate)*Math.Max(0,dt)));
        public static void Step(float phase,float strength,out float z,out float lift,out float pitch)
        {
            float u=(float)(phase/(Math.PI*2)%1);if(u<0)u+=1;
            strength=Math.Max(0,Math.Min(1,strength));lift=pitch=0;
            if(u<.55f)z=(.25f-.5f*u/.55f)*strength;
            else
            {
                float swing=(u-.55f)/.45f,t=swing*swing*(3-2*swing);
                z=(-.25f+.5f*t)*strength;float arc=(float)Math.Sin(swing*Math.PI);lift=arc*.16f*strength;pitch=-arc*15*strength;
            }
        }
        public static float Knee(float distance,out float along,out float height)
        {
            float reach=Math.Max(.02f,Math.Min(.759f,distance));along=reach*.5f;
            height=(float)Math.Sqrt(Math.Max(0,.38*.38-along*along));return reach;
        }
        public static bool Complete(float age)=>age>=CaptureDuration;
    }
}
