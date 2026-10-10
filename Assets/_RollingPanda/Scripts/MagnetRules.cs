namespace RollingPanda
{
    public static class MagnetRules
    {
        public const float Duration=15;
        public static double Next(double previous,int step,bool first) => (first ? 12 : previous)+24*(13+step%8);
        public static float Tick(float remaining,float delta,bool running) => !running ? remaining : System.Math.Max(0,remaining-System.Math.Max(0,delta));
        public static bool InRange(float x,float y,float z) => z>=-3 && z<=24 && System.Math.Abs(x)<=7 && System.Math.Abs(y)<=4.5f;
        public static bool CanCollect(float x,float delta,float previous) => System.Math.Abs(x)<1 && (System.Math.Abs(delta)<1.3f || previous>0 && delta<=0);
    }
}
