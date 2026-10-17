using System;
using RollingPanda;
static class Program
{
    static void Check(bool pass,string text){if(!pass)throw new Exception(text);}
    static void Main()
    {
        Check(PoacherRules.Net(.64f)==0 && PoacherRules.Net(1.4f)==1,"Net timing");
        Check(PoacherRules.Dart(1.84f)==0 && PoacherRules.Dart(2.1f)==1,"Dart timing");
        Check(PoacherRules.Sleep(2.1f)==0 && PoacherRules.Sleep(2.8f)==1,"Sleep follows dart");
        Check(!PoacherRules.Complete(3.59f) && PoacherRules.Complete(3.6f),"Result gate");
        Check(PoacherRules.Advance(1,1,true)==1,"Pause freezes capture");
        Check(PoacherRules.Advance(1,-1,false)==1,"Negative delta guard");
        Check(PoacherRules.Advance(3,10,false)==PoacherRules.CaptureDuration,"Clamp after long frame");
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
        Console.WriteLine("Poacher timeline checks passed: cue ordering, results gate, pause and long-frame clamp, 10,000 variable-frame captures.");
    }
}
