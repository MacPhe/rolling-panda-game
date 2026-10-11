using UnityEngine;
namespace RollingPanda
{
    // Soft, bounded footstep cadence; jump/slide sounds follow accepted motor actions.
    [DefaultExecutionOrder(100)]
    public sealed class RunnerMovementAudio : MonoBehaviour
    {
        RunnerMotor runner;CharacterController capsule;
        MovementAudioArt clips;AudioSource steps,action,music;
        bool musicStarted,musicPaused;
        int index;double previousDistance;bool running;float stepClock;AudioClip jumpSwoosh;
        void Awake()
        {
            runner=GetComponent<RunnerMotor>();capsule=GetComponent<CharacterController>();
            clips=Resources.Load<MovementAudioArt>("Presentation/MovementAudioArt");
            var footObject=new GameObject("Soft footsteps");footObject.transform.SetParent(transform,false);
            steps=footObject.AddComponent<AudioSource>();action=gameObject.AddComponent<AudioSource>();
            footObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=2400;
            jumpSwoosh=BuildSwoosh();
            foreach(var source in new[]{steps,action}) {source.playOnAwake=false;source.spatialBlend=0;source.loop=false;}
            steps.volume=.09f;action.volume=.25f;
            var musicObject=new GameObject("Quiet gameplay percussion");musicObject.transform.SetParent(transform,false);
            music=musicObject.AddComponent<AudioSource>();music.playOnAwake=false;music.spatialBlend=0;music.loop=true;music.volume=0;
            if(clips!=null)music.clip=clips.Percussion;
        }
        bool CanPlay => runner!=null && !RunEntryTransition.Active && !runner.Paused && !runner.Finished && (runner.Session==null || !runner.Session.Defeated);
        public void Jumped(){if(CanPlay){steps.Stop();stepClock=0;action.pitch=1;action.clip=jumpSwoosh;action.Play();}}
        public void Slid(){if(CanPlay && clips!=null && clips.Slide!=null){steps.Stop();stepClock=0;action.pitch=1;action.clip=clips.Slide;action.Play();}}
        void LateUpdate()
        {
            if(runner==null) return;
            double distance=runner.DistanceTravelled;bool active=CanPlay;
            UpdateMusic(distance<previousDistance);
            if(!active || distance<previousDistance)
            {
                if(running){steps.Stop();action.Stop();}running=false;stepClock=0;previousDistance=distance;return;
            }
            bool moved=distance-previousDistance>.001;
            bool stepping=moved && !runner.Sliding && capsule.isGrounded;
            float interval=Mathf.Lerp(.32f,.26f,Mathf.InverseLerp(17.5f,40,runner.CurrentSpeed));
            if(stepping && running) stepClock+=Time.deltaTime; else stepClock=0;
            if(stepClock>=interval && clips!=null && clips.Steps!=null && clips.Steps.Length>0)
            {
                stepClock%=interval;
                var clip=clips.Steps[index%Mathf.Min(2,clips.Steps.Length)];steps.pitch=1;index++;
                if(clip!=null) steps.PlayOneShot(clip);
            }
            if(!runner.Sliding && action.clip!=null && clips!=null && action.clip==clips.Slide) action.Stop();
            running=true;previousDistance=distance;
        }
        void UpdateMusic(bool restarted)
        {
            if(music==null || music.clip==null)return;
            if(restarted || RunEntryTransition.Active || runner.Finished || (runner.Session!=null && runner.Session.Defeated))
            {
                music.Stop();music.volume=0;musicStarted=false;musicPaused=false;return;
            }
            if(runner.Paused)
            {
                if(musicStarted && !musicPaused){music.Pause();musicPaused=true;}
                return;
            }
            if(!musicStarted){music.Play();musicStarted=true;}
            else if(musicPaused){music.UnPause();musicPaused=false;}
            music.volume=Mathf.MoveTowards(music.volume,Mathf.Clamp01(clips.PercussionVolume),Time.unscaledDeltaTime*.25f);
        }
        static AudioClip BuildSwoosh()
        {
            // A short filtered-air sweep, generated once; no new sound-pack dependency.
            const int rate=22050;var samples=new float[(int)(rate*.55f)];
            var random=new System.Random(811);float smooth=0;
            for(int i=0;i<samples.Length;i++)
            {
                float t=i/(float)(samples.Length-1);
                float noise=(float)random.NextDouble()*2-1;
                smooth=Mathf.Lerp(smooth,noise,Mathf.Lerp(.04f,.16f,Mathf.Sin(t*Mathf.PI)));
                float envelope=Mathf.Sin(Mathf.PI*t);envelope*=envelope;
                samples[i]=smooth*envelope*.30f;
            }
            var clip=AudioClip.Create("Panda flip air swoosh",samples.Length,1,rate,false);
            clip.SetData(samples,0);return clip;
        }
        void OnDestroy(){if(jumpSwoosh!=null)Destroy(jumpSwoosh);}
        void OnDisable(){if(steps!=null)steps.Stop();if(action!=null)action.Stop();if(music!=null){music.Stop();music.volume=0;}musicStarted=false;musicPaused=false;running=false;}
    }
}


