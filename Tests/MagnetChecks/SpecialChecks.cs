using System;
using RollingPanda;
static class SpecialChecks
{
    static void Check(bool ok,string name){if(!ok) throw new Exception(name);}
    public static void Run()
    {
        var random=new Random(68431);
        foreach(SpecialKind kind in Enum.GetValues(typeof(SpecialKind)))
        {
            double previous=0;
            for(int i=0;i<100000;i++)
            {
                double next=SpecialRules.Next(kind,previous,i,random.Next()),gap=next-previous;
                Check(next%24==12,"special avoids obstacle rows");
                if(kind==SpecialKind.Speed) Check(i==0 ? next>=1000 && next<=3000 : gap>=2500 && gap<=3000,"speed spawn/cooldown");
                if(kind==SpecialKind.Time) Check(i==0 ? next>2000 : gap>=4000 && gap<=8000,"time threshold/cooldown");
                if(kind==SpecialKind.Bomb) Check(i==0 ? next>0 && next<=500 : gap>=3000 && gap<=10000,"bomb first spawn/spacing");
                if(kind==SpecialKind.Gold) Check(i==0 ? next>=15000 : gap>=10000 && gap<=20000,"gold threshold/spacing");
                if(kind==SpecialKind.Battery) Check(Math.Floor(next/10000)==i/2 && (i%2==0 ? next%10000<5000 : next%10000>5000),"exactly two batteries per 10km");
                previous=next;
            }
        }
        Check(SpecialRules.Speed(35,7.5f,SpecialKind.Speed)==50,"boost speed");
        Check(SpecialRules.Speed(35,7.5f,SpecialKind.Time)==10,"time overrides boost");
        Check(SpecialRules.Speed(35,0,SpecialKind.Time)==35,"baseline restored on expiry");
        Check(SpecialRules.BatteryDuration==15 && SpecialRules.ChargedJumpHeight>4.5f,"battery duration/tall obstacle clearance target");
        Check(!SpecialRules.VerticalHit(2,2,.55f),"jump clears low bomb");
        Check(SpecialRules.VerticalHit(0,2,.55f) && SpecialRules.VerticalHit(0,.9f,.55f),"grounded/sliding bomb hit");
        var currency=new RunPickups();currency.Collect(PickupKind.Blue);currency.Collect(PickupKind.Red);currency.Collect(PickupKind.Pouch);currency.AddPouches(50);
        Check(currency.Pouches==51 && currency.Points==35,"gold adds 50 green without score");currency.Reset();Check(currency.Pouches==0,"gold currency resets with run");
        Console.WriteLine("Special pickup rules passed: 500,000 schedules, cooldowns/thresholds, two batteries per 10km, speed replacement/expiry, vertical bomb dodge and gold currency.");
    }
}
