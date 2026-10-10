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
            double next=MagnetRules.Next(previous,random.Next(),i==0),gap=next-previous;
            Check(System.Math.Floor(next/1000)==i,"exactly one spawn per kilometre block");
            Check(next-i*1000>=250 && next-i*1000<=750,"random middle-kilometre position");
            if(i>0) Check(gap>=500,"no boundary clustering or continuous magnet chaining");
            Check(next%24==12,"magnet lies between all obstacle rows");previous=next;
        }
        float timer=MagnetRules.Duration;
        for(int i=0;i<1000;i++) timer=MagnetRules.Tick(timer,1,false);
        Check(timer==7.5f,"pause freezes timer");
        for(int i=0;i<449;i++) timer=MagnetRules.Tick(timer,1f/60,true);
        Check(timer>0 && timer<.04f,"duration before expiry");
        for(int i=0;i<5;i++) timer=MagnetRules.Tick(timer,1f/60,true);
        Check(timer==0,"duration expires without going negative");
        Check(MagnetRules.InRange(6,3.7f,24),"all lanes and high pouch in range");
        Check(!MagnetRules.InRange(0,0,24.01f) && !MagnetRules.InRange(0,0,-3.01f),"forward/back range limits");
        Check(MagnetRules.CanCollect(0,-2,2),"swept collection at low frame rate");
        Check(!MagnetRules.CanCollect(3,-2,2),"wrong lane cannot collect");
        Check(!MagnetRules.CanCollect(0,-2,-1),"passed pickup cannot collect again");
        Check(ShieldRules.Duration==10,"shield duration");
        Check(!ShieldRules.Protects(0) && !ShieldRules.Protects(-1) && ShieldRules.Protects(.001f),"shield protection ends at zero");
        float shield=ShieldRules.Duration;
        for(int i=0;i<1000;i++) shield=MagnetRules.Tick(shield,.1f,false);
        Check(shield==10,"paused shield retains time");
        for(int i=0;i<101;i++) shield=MagnetRules.Tick(shield,.1f,true);
        Check(!ShieldRules.Protects(shield),"shield expires while distance remains blocked at zero");
        Console.WriteLine("Shield rules passed: 10-second duration, pause and stationary expiry/protection boundary; spawn spacing shares the verified kilometre scheduler.");
        Console.WriteLine("Magnet rules passed: 100,000 kilometre blocks/row exclusions/minimum gaps, paused and expired timer, all-lane range and swept collection.");
    }
}
