using System;
namespace RollingPanda
{
    public static class ChuteRules
    {
        public const float Length=28,StartOffset=40,HalfWidth=1.35f,ExitMargin=.4f;
        public static bool HasExited(double z,double exit)=>z>exit+ExitMargin;
        public static bool Spawn(long section)=>section>=10 && section%12==10;
        public static bool Sweep(double x0,double z0,double x1,double z1,double centre,double start,out bool front)
        {
            front=false;double lo=0,hi=1;
            if(!Slab(x0-centre,x1-x0,-HalfWidth,HalfWidth,ref lo,ref hi) || !Slab(z0,z1-z0,start-.4,start+Length+.4,ref lo,ref hi))return false;
            double x=x0+(x1-x0)*lo;
            front=z0<=start-.4 && z1>z0 && Math.Abs(x-centre)<=.75;
            return true;
        }
        static bool Slab(double p,double d,double min,double max,ref double lo,ref double hi)
        {
            if(Math.Abs(d)<1e-9)return p>=min && p<=max;
            double a=(min-p)/d,b=(max-p)/d;if(a>b){double t=a;a=b;b=t;}
            lo=Math.Max(lo,a);hi=Math.Min(hi,b);return lo<=hi;
        }
    }
}
