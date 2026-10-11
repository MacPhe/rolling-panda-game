namespace RollingPanda
{
    public enum SpecialKind { Speed, Time, Bomb, Gold, Battery }
    public static class SpecialRules
    {
        public const float SpeedDuration=7.5f, BatteryDuration=15, ChargedJumpHeight=6.2f;
        static double Snap(double minimum,double maximum,int random)
        {
            long low=(long)System.Math.Ceiling((minimum-12)/24),high=(long)System.Math.Floor((maximum-12)/24);
            return 12+24*(low+(long)((uint)random%(ulong)(high-low+1)));
        }
        public static double Next(SpecialKind kind,double previous,int occurrence,int random)
        {
            switch(kind)
            {
                case SpecialKind.Speed:return Snap(occurrence==0 ? 1000 : previous+2500,occurrence==0 ? 3000 : previous+3000,random);
                case SpecialKind.Time:return Snap(occurrence==0 ? 2024 : previous+4000,occurrence==0 ? 6000 : previous+8000,random);
                case SpecialKind.Bomb:return Snap(occurrence==0 ? 120 : previous+3000,occurrence==0 ? 500 : previous+10000,random);
                case SpecialKind.Gold:return Snap(occurrence==0 ? 15000 : previous+10000,occurrence==0 ? 16000 : previous+20000,random);
                default:
                    double block=(occurrence/2)*10000.0;
                    return occurrence%2==0 ? Snap(block+500,block+4500,random) : Snap(block+5500,block+9500,random);
            }
        }
        public static float Speed(float baseline,float remaining,SpecialKind kind) => remaining<=0 ? baseline : kind==SpecialKind.Speed ? 50 : 10;
        public static bool VerticalHit(float feet,float height,float itemY) => itemY>=feet-.4f && itemY<=feet+height+.4f;
    }
}

