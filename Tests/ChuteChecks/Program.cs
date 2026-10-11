using System;
using RollingPanda;
static class Program
{
    static void Check(bool pass,string message){if(!pass)throw new Exception(message);}
    static void Main()
    {
        bool front;
        Check(ChuteRules.Sweep(0,0,0,200,0,100,out front) && front,"Fast step must catch front entrance");
        Check(!ChuteRules.Sweep(3,0,3,200,0,100,out front),"Adjacent lane must remain open");
        Check(ChuteRules.Sweep(3,110,0,111,0,100,out front) && !front,"Side entry must not gain protection");
        Check(ChuteRules.Sweep(1,90,1,110,0,100,out front) && !front,"Off-centre entry hits frame");
        Check(!ChuteRules.Sweep(0,129,0,150,0,100,out front),"Past exit must not re-enter");
        Check(!ChuteRules.Sweep(0,90,0,95,0,100,out front),"Before entrance is not protected");
        Check(!ChuteRules.HasExited(128,128),"Protection lasts beyond centre crossing");
        Check(!ChuteRules.HasExited(128.39,128),"Trailing capsule still protected");
        Check(ChuteRules.HasExited(128.41,128),"Protection ends once capsule clears");
        int scheduled=0;long prior=-1;
        for(long index=0;index<100000;index++)if(ChuteRules.Spawn(index))
        {Check(index>=10 && (prior<0 || index-prior==12),"Intro/spacing");prior=index;scheduled++;}
        var random=new Random(82);
        for(int i=0;i<100000;i++)
        {
            double step=(17.5+random.NextDouble()*32.5)*(.008+random.NextDouble()*.19);
            double start=100-.4-random.NextDouble()*step;
            Check(ChuteRules.Sweep(0,start,0,start+step,0,100,out front) && front,"Crossing entrance at any supported speed");
            Check(!ChuteRules.Sweep(-3,start,-3,start+step,0,100,out front),"Adjacent lane sweep");
            Check(ChuteRules.Sweep(0,start+960,0,start+step+960,0,1060,out front) && front,"Rebase translation preserves entry");
        }
        Console.WriteLine("Chute checks passed: front/side/frame entry, high-speed sweeps, exit protection, rebase invariance, 100,000 schedules and 300,000 moving sweeps.");
    }
}
