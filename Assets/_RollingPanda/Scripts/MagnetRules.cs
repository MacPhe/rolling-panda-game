namespace RollingPanda
{
    public static class MagnetRules
    {
        public const float Duration=7.5f;
        public static double Next(double previous,int step,bool first)
        {
            double block=first ? 0 : System.Math.Floor(previous/1000)+1;
            // Random within the middle of each kilometre avoids back-to-back boundary spawns.
            long low=(long)System.Math.Ceiling((block*1000+250-12)/24);
            long high=(long)System.Math.Floor((block*1000+750-12)/24);
            return 12+24*(low+step%(high-low+1));
        }
        public static float Tick(float remaining,float delta,bool running) => !running ? remaining : System.Math.Max(0,remaining-System.Math.Max(0,delta));
        public static bool InRange(float x,float y,float z) => z>=-3 && z<=24 && System.Math.Abs(x)<=7 && System.Math.Abs(y)<=4.5f;
        public static bool CanCollect(float x,float delta,float previous) => System.Math.Abs(x)<1 && (System.Math.Abs(delta)<1.3f || previous>0 && delta<=0);
    }
}
