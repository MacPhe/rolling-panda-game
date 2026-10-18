using System;
using RollingPanda;
static class Program
{
    static void Check(bool pass,string text){if(!pass)throw new Exception(text);}
    static void Main()
    {
        double min=1,max=0;float prior=PoacherRules.Pressure(0);
        for(int metre=0;metre<100000;metre++)
        {
            float pressure=PoacherRules.Pressure(metre);min=Math.Min(min,pressure);max=Math.Max(max,pressure);
            Check(pressure>=0 && pressure<=1,"Bounded pursuit pressure");
            Check(Math.Abs(pressure-prior)<.005,"No chase discontinuity at rebase or cycle boundary");prior=pressure;
            Check(PoacherRules.Gap(metre,false)>=2 && PoacherRules.Gap(metre,false)<=14,"Chase stays behind panda");
            Check(PoacherRules.Gap(metre,true)==15,"Chute creates breathing room");
        }
        Check(min<.02 && max>.98,"Pursuit includes hot and cold periods");
        Check(PoacherRules.Net(.64f)==0 && PoacherRules.Net(1.4f)==1,"Net timing");
        Check(PoacherRules.Dart(1.84f)==0 && PoacherRules.Dart(2.1f)==1,"Dart timing");
        Check(PoacherRules.Sleep(2.1f)==0 && PoacherRules.Sleep(2.8f)==1,"Sleep follows dart");
        Check(!PoacherRules.Complete(3.59f) && PoacherRules.Complete(3.6f),"Result gate");
        Check(PoacherRules.Advance(1,1,true)==1,"Pause freezes capture");
        Check(PoacherRules.Advance(1,-1,false)==1,"Negative delta guard");
        Check(PoacherRules.Advance(3,10,false)==PoacherRules.CaptureDuration,"Clamp after long frame");
        for(int sample=0;sample<100000;sample++)
        {
            float phase=(sample-50000)*.0004f,z,lift,pitch;PoacherRules.Step(phase,1,out z,out lift,out pitch);
            Check(z>=-.25001f && z<=.25001f && lift>=-.00001f && lift<=.16001f,"Foot path stays in bounds");
            float distance=(float)Math.Sqrt((.685-lift)*(.685-lift)+z*z),along,height;
            float reach=PoacherRules.Knee(distance,out along,out height);
            Check(Math.Abs(reach-distance)<.00001,"Foot path remains reachable");
            Check(Math.Abs(along*along+height*height-.38*.38)<.000001,"Upper limb length");
            Check(Math.Abs((reach-along)*(reach-along)+height*height-.38*.38)<.000001,"Lower limb length");
        }
        foreach(int fps in new[]{30,60,120})
        {
            float x=0;for(int frame=0;frame<fps;frame++)x+=(3-x)*PoacherRules.Follow(6,1f/fps);
            Check(Math.Abs(x-3*(1-Math.Exp(-6)))<.00001,"Follow smoothing independent of frame rate");
        }
        float before,at,liftBefore,liftAt,pitchBefore,pitchAt;
        PoacherRules.Step((float)(Math.PI*2*.55)-.00001f,1,out before,out liftBefore,out pitchBefore);
        PoacherRules.Step((float)(Math.PI*2*.55)+.00001f,1,out at,out liftAt,out pitchAt);
        Check(Math.Abs(before-at)<.0001 && Math.Abs(liftBefore-liftAt)<.0001,"No toe-off position jump");
        PoacherRules.Step(0,1,out before,out liftBefore,out pitchBefore);PoacherRules.Step((float)(Math.PI*2),1,out at,out liftAt,out pitchAt);
        Check(Math.Abs(before-at)<.0001 && Math.Abs(liftBefore-liftAt)<.0001,"Loop continuity");
        var random=new Random(1017);
        for(int run=0;run<10000;run++)
        {
            float age=0,lastNet=0,lastDart=0,lastSleep=0;
            while(!PoacherRules.Complete(age))
            {
                bool paused=random.Next(4)==0;float previous=age;
                age=PoacherRules.Advance(age,.005f+(float)random.NextDouble()*.12f,paused);
                Check(!paused || previous==age,"Paused clock");
                float net=PoacherRules.Net(age),dart=PoacherRules.Dart(age),sleep=PoacherRules.Sleep(age);
                Check(net>=lastNet && dart>=lastDart && sleep>=lastSleep,"Monotonic capture");
                Check(dart==0 || net==1,"Net lands before dart");Check(sleep==0 || dart==1,"Sleep after dart");
                lastNet=net;lastDart=dart;lastSleep=sleep;
            }
        }
        Console.WriteLine("Poacher timeline checks passed: cue ordering, results gate, pause and long-frame clamp, 10,000 variable-frame captures; 100,000 reachable two-bone foot samples, step continuity and 30/60/120fps smoothing.");
    }
}
