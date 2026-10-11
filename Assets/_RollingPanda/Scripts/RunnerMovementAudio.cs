using UnityEngine;
namespace RollingPanda
{
    // Soft, bounded footstep cadence; jump/slide sounds follow accepted motor actions.
    [DefaultExecutionOrder(100)]
    public sealed class RunnerMovementAudio : MonoBehaviour
    {
        RunnerMotor runner;CharacterController capsule;
        MovementAudioArt clips;AudioSource steps,action,music,jungle;
        bool musicStarted,musicPaused;
        AudioSource voice;AudioClip[] voiceClips;float voiceCooldown;bool defeatVoice;
        readonly AudioSource[] pickupSources=new AudioSource[3];
        readonly AudioClip[] pickupClips=new AudioClip[3];
        int index;double previousDistance;bool running;float stepClock;AudioClip jumpSwoosh;
        void Awake()
        {
            runner=GetComponent<RunnerMotor>();capsule=GetComponent<CharacterController>();
            clips=Resources.Load<MovementAudioArt>("Presentation/MovementAudioArt");
            var footObject=new GameObject("Soft footsteps");footObject.transform.SetParent(transform,false);
            steps=footObject.AddComponent<AudioSource>();action=gameObject.AddComponent<AudioSource>();
            footObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=2400;
            jumpSwoosh=BuildSwoosh();
            var voiceObject=new GameObject("Quiet panda placeholder voice");voiceObject.transform.SetParent(transform,false);
            voice=voiceObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.loop=false;voice.spatialBlend=0;voice.volume=.22f;
            voiceObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=2000;
            voiceClips=new[]{BuildPandaVoice(0),BuildPandaVoice(1),BuildPandaVoice(2)};
            for(int i=0;i<3;i++)
            {
                var pickupObject=new GameObject("Soft "+((PickupKind)i)+" pickup sound");pickupObject.transform.SetParent(transform,false);
                var source=pickupObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.loop=false;source.volume=.65f;
                pickupClips[i]=BuildPickupSound(i);source.clip=pickupClips[i];pickupSources[i]=source;
            }
            foreach(var source in new[]{steps,action}) {source.playOnAwake=false;source.spatialBlend=0;source.loop=false;}
            steps.volume=.063f;action.volume=.25f;
            var musicObject=new GameObject("Quiet gameplay percussion");musicObject.transform.SetParent(transform,false);
            music=musicObject.AddComponent<AudioSource>();music.playOnAwake=false;music.spatialBlend=0;music.loop=true;music.volume=0;
            if(clips!=null)music.clip=clips.Percussion;
            var jungleObject=new GameObject("Muted mist forest music");jungleObject.transform.SetParent(transform,false);
            jungle=jungleObject.AddComponent<AudioSource>();jungle.playOnAwake=false;jungle.spatialBlend=0;jungle.loop=true;jungle.volume=0;
            jungleObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=1600;
            if(clips!=null)jungle.clip=clips.Jungle;
        }
        bool CanPlay => runner!=null && !RunEntryTransition.Active && !runner.Paused && !runner.Finished && (runner.Session==null || !runner.Session.Defeated);
        public void Collected(PickupKind kind)
        {
            int index=(int)kind;if(!CanPlay || index<0 || index>=pickupSources.Length)return;
            var source=pickupSources[index];
            // One voice per colour: dense magnet streams cannot stack unbounded audio.
            // Red and pouch sounds have independent voices, so blue never masks them.
            if(source!=null && !source.isPlaying)source.Play();
        }
        void StopPickups(){foreach(var source in pickupSources)if(source!=null)source.Stop();}
        static AudioClip BuildPickupSound(int kind)
        {
            const int rate=44100;float duration=kind==0 ? .26f : kind==1 ? .31f : .47f;
            var samples=new float[Mathf.CeilToInt(duration*rate)];
            for(int i=0;i<samples.Length;i++)
            {
                double t=i/(double)rate;
                double value=kind==0 ? PickupNote(t,880,false) : kind==1 ? PickupNote(t,1108.73,false)+PickupNote(t-.05,1318.51,false) : PickupNote(t,523.25,true)+PickupNote(t-.06,659.25,true)+PickupNote(t-.12,783.99,true);
                samples[i]=(float)value;
            }
            var clip=AudioClip.Create("Original "+((PickupKind)kind)+" crystal pickup",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        static double PickupNote(double t,double frequency,bool rounded)
        {
            if(t<0 || t>(rounded ? .35 : .26))return 0;
            double attack=1-System.Math.Exp(-t/(rounded ? .009 : .006)),phase=2*System.Math.PI*frequency*t;
            if(rounded)return .14*attack*(System.Math.Sin(phase)*System.Math.Exp(-t/.09)+.12*System.Math.Sin(phase*2)*System.Math.Exp(-t/.045));
            return .17*attack*(System.Math.Sin(phase)*System.Math.Exp(-t/.065)+.16*System.Math.Sin(phase*2.03)*System.Math.Exp(-t/.03)+.035*System.Math.Sin(phase*3.1)*System.Math.Exp(-t/.018));
        }
        public void Jumped(){if(CanPlay){steps.Stop();stepClock=0;action.pitch=1;action.clip=jumpSwoosh;action.Play();ActionVoice(0);}}
        public void Slid()
        {
            if(!CanPlay)return;ActionVoice(1);
            if(clips!=null && clips.Slide!=null){steps.Stop();stepClock=0;action.pitch=1;action.clip=clips.Slide;action.Play();}
        }
        void ActionVoice(int kind)
        {
            if(voice==null || voiceCooldown>0 || voice.isPlaying)return;
            defeatVoice=false;voice.clip=voiceClips[kind];voice.pitch=1;voice.Play();voiceCooldown=1.2f;
        }
        public void ReactToDefeat()
        {
            if(voice==null)return;
            voice.Stop();voice.clip=voiceClips[2];voice.pitch=1;voice.Play();defeatVoice=true;
        }
        public void ResetVocals(){if(voice!=null)voice.Stop();voiceCooldown=0;defeatVoice=false;}
        static AudioClip BuildPandaVoice(int kind)
        {
            // Original synthetic cartoon grunts; deliberately temporary, not recorded panda calls.
            const int rate=22050;float duration=kind==2 ? .38f : .24f;var samples=new float[Mathf.CeilToInt(rate*duration)];
            var random=new System.Random(910+kind);double phase=0,breath=0;
            for(int i=0;i<samples.Length;i++)
            {
                double t=i/(double)rate,u=t/duration;
                double frequency=kind==0 ? 230+45*System.Math.Sin(u*System.Math.PI) : kind==1 ? 180+30*System.Math.Sin(u*System.Math.PI) : 250-115*u;
                phase+=2*System.Math.PI*frequency/rate;
                breath+=.12*((random.NextDouble()*2-1)-breath);
                double envelope=(1-System.Math.Exp(-t/.014))*System.Math.Pow(System.Math.Max(0,1-u),1.6);
                samples[i]=(float)(envelope*(.35*(.65*System.Math.Sin(phase)+.22*System.Math.Sin(2*phase)+.1*System.Math.Sin(3*phase))+.035*breath));
            }
            var clip=AudioClip.Create(kind==0 ? "Panda hop effort placeholder" : kind==1 ? "Panda roll effort placeholder" : "Panda surprised oof placeholder",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        void LateUpdate()
        {
            if(runner==null) return;
            double distance=runner.DistanceTravelled;bool active=CanPlay;
            UpdateMusic(distance<previousDistance);
            if(active)voiceCooldown=Mathf.Max(0,voiceCooldown-Time.deltaTime);
            if(distance<previousDistance || runner.Paused || RunEntryTransition.Active)ResetVocals();
            else if(!active && !defeatVoice && voice!=null)voice.Stop();
            if(!active || distance<previousDistance)
            {
                StopPickups();if(running){steps.Stop();action.Stop();}running=false;stepClock=0;previousDistance=distance;return;
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
        void StopMusic()
        {
            if(music!=null){music.Stop();music.volume=0;}
            if(jungle!=null){jungle.Stop();jungle.volume=0;}
            musicStarted=false;musicPaused=false;
        }
        void UpdateMusic(bool restarted)
        {
            if(restarted || RunEntryTransition.Active || runner.Finished || (runner.Session!=null && runner.Session.Defeated))
            {StopMusic();return;}
            if(runner.Paused)
            {
                if(!musicPaused){if(music!=null)music.Pause();if(jungle!=null)jungle.Pause();musicPaused=true;}
                return;
            }
            if(musicPaused){if(music!=null)music.UnPause();if(jungle!=null)jungle.UnPause();musicPaused=false;}
            if(music!=null && music.clip!=null)
            {
                if(!musicStarted){music.Play();musicStarted=true;}
                music.volume=Mathf.MoveTowards(music.volume,Mathf.Clamp01(clips.PercussionVolume),Time.unscaledDeltaTime*.25f);
            }
            if(jungle!=null && jungle.clip!=null)
            {
                float target=.12f*RunAtmosphere.MistAt(runner.DistanceTravelled);
                if(target>0 && !jungle.isPlaying)jungle.Play();
                jungle.volume=Mathf.MoveTowards(jungle.volume,target,Time.unscaledDeltaTime*.04f);
                if(target<=0 && jungle.volume<=0)jungle.Stop();
            }
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
        void OnDestroy(){if(jumpSwoosh!=null)Destroy(jumpSwoosh);foreach(var clip in pickupClips)if(clip!=null)Destroy(clip);if(voiceClips!=null)foreach(var clip in voiceClips)if(clip!=null)Destroy(clip);}
        void OnDisable(){if(steps!=null)steps.Stop();if(action!=null)action.Stop();StopMusic();StopPickups();ResetVocals();running=false;}
    }
}


