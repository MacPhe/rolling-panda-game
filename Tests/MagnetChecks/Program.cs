using System;
using RollingPanda;
static class Program
{
    static void Check(bool ok,string name){if(!ok) throw new Exception(name);}
    static void Main()
    {
        var random=new Random(932);double previous=0;
        for(int i=0;i<100000;i++)
        {
            double next=MagnetRules.Next(previous,random.Next(8),i==0),gap=next-previous;
            Check(gap>=300 && gap<=500,"spawn interval");
            Check(next%24==12,"magnet lies between all obstacle rows");previous=next;
        }
        float timer=15;
        for(int i=0;i<1000;i++) timer=MagnetRules.Tick(timer,1,false);
        Check(timer==15,"pause freezes timer");
        for(int i=0;i<899;i++) timer=MagnetRules.Tick(timer,1f/60,true);
        Check(timer>0 && timer<.04f,"duration before expiry");
        for(int i=0;i<5;i++) timer=MagnetRules.Tick(timer,1f/60,true);
        Check(timer==0,"duration expires without going negative");
        Check(MagnetRules.InRange(6,3.7f,24),"all lanes and high pouch in range");
        Check(!MagnetRules.InRange(0,0,24.01f) && !MagnetRules.InRange(0,0,-3.01f),"forward/back range limits");
        Check(MagnetRules.CanCollect(0,-2,2),"swept collection at low frame rate");
        Check(!MagnetRules.CanCollect(3,-2,2),"wrong lane cannot collect");
        Check(!MagnetRules.CanCollect(0,-2,-1),"passed pickup cannot collect again");
        Console.WriteLine("Magnet rules passed: 100,000 spawn gaps/row exclusions, paused and expired timer, all-lane range and swept collection.");
    }
}
