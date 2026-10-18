using UnityEngine;
namespace RollingPanda
{
    // Two bounded voices, generated once; a seamless bed and sparse original bird calls.
    public sealed class ForestAmbience : MonoBehaviour
    {
        RunnerMotor runner;AudioSource wind,birds;AudioClip bed,call;
        float envelope,clock=7;bool paused;uint random=7193;
        public void Bind(RunnerMotor player){runner=player;}
        void Awake()
        {
            wind=gameObject.AddComponent<AudioSource>();birds=gameObject.AddComponent<AudioSource>();
            foreach(var source in new[]{wind,birds}){source.playOnAwake=false;source.spatialBlend=0;source.volume=0;source.priority=180;}
            const int rate=22050;var samples=new float[rate*6];double tau=System.Math.PI*2;
            float filtered=0;
            // Warm the filter with the same periodic noise before writing its steady-state cycle.
            for(int pass=0;pass<2;pass++)
            {
                var noise=new System.Random(815);
                for(int i=0;i<samples.Length;i++){filtered+=.065f*((float)noise.NextDouble()*2-1-filtered);if(pass==1)samples[i]=filtered*.65f;}
            }
            bed=AudioClip.Create("Original soft forest air",samples.Length,1,rate,false);bed.SetData(samples,0);wind.clip=bed;wind.loop=true;wind.Play();
            samples=new float[rate];double phase=0;
            for(int i=0;i<samples.Length;i++)
            {
                double t=i/(double)rate;phase+=tau*(1450+280*System.Math.Sin(t*27))/rate;
                double pulse=System.Math.Max(0,System.Math.Sin(t*System.Math.PI*5));
                samples[i]=(float)(.09*System.Math.Sin(phase)*pulse*pulse*System.Math.Sin(t*System.Math.PI));
            }
            call=AudioClip.Create("Original distant forest chirp",samples.Length,1,rate,false);call.SetData(samples,0);birds.clip=call;
        }
        void Update()
        {
            bool pause=runner!=null && runner.Paused;
            if(pause!=paused){paused=pause;if(paused){wind.Pause();birds.Pause();}else{wind.UnPause();birds.UnPause();}}
            if(paused)return;
            bool defeated=runner!=null && runner.Session!=null && runner.Session.Defeated;
            envelope=Mathf.MoveTowards(envelope,defeated ? .25f : 1,Time.unscaledDeltaTime*.5f);
            wind.volume=.14f*envelope*GameAudioSettings.Ambience;birds.volume=.2f*envelope*GameAudioSettings.Ambience;
            clock-=Time.unscaledDeltaTime;
            if(clock<=0){random=random*1664525+1013904223;clock=11+(random%1000)*.014f;if(!defeated && GameAudioSettings.Ambience>0){birds.pitch=.94f+(random%100)*.0012f;birds.Play();}}
        }
        void OnDisable(){if(wind!=null)wind.Stop();if(birds!=null)birds.Stop();}
        void OnDestroy(){if(bed!=null)Destroy(bed);if(call!=null)Destroy(call);}
    }
}
