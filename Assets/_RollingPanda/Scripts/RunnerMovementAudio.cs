using UnityEngine;
namespace RollingPanda
{
    // Uses the visual gait for footsteps, actual motor events for jump/slide.
    [DefaultExecutionOrder(100)]
    public sealed class RunnerMovementAudio : MonoBehaviour
    {
        RunnerMotor runner;CharacterController capsule;VisualPrototype art;
        MovementAudioArt clips;AudioSource steps,action;
        int previousBeat,index;double previousDistance;bool running;
        void Awake()
        {
            runner=GetComponent<RunnerMotor>();capsule=GetComponent<CharacterController>();
            art=GetComponentInParent<VisualPrototype>();clips=Resources.Load<MovementAudioArt>("Presentation/MovementAudioArt");
            steps=gameObject.AddComponent<AudioSource>();action=gameObject.AddComponent<AudioSource>();
            foreach(var source in new[]{steps,action}) {source.playOnAwake=false;source.spatialBlend=0;source.loop=false;}
            steps.volume=.28f;action.volume=.32f;
        }
        bool CanPlay => runner!=null && !RunEntryTransition.Active && !runner.Paused && !runner.Finished && (runner.Session==null || !runner.Session.Defeated);
        public void Jumped(){if(CanPlay && clips!=null && clips.Jump!=null){steps.Stop();action.pitch=1;action.clip=clips.Jump;action.Play();}}
        public void Slid(){if(CanPlay && clips!=null && clips.Slide!=null){steps.Stop();action.pitch=1;action.clip=clips.Slide;action.Play();}}
        void LateUpdate()
        {
            if(runner==null) return;
            int beat=art==null ? 0 : Mathf.FloorToInt((art.FootfallPhase+Mathf.PI*.5f)/Mathf.PI);
            double distance=runner.DistanceTravelled;bool active=CanPlay;
            if(!active || distance<previousDistance)
            {
                if(running){steps.Stop();action.Stop();}running=false;previousBeat=beat;previousDistance=distance;return;
            }
            bool moved=distance-previousDistance>.001;
            if(running && moved && !runner.Sliding && capsule.isGrounded && beat!=previousBeat && clips!=null && clips.Steps!=null && clips.Steps.Length>0)
            {
                var clip=clips.Steps[index%clips.Steps.Length];steps.pitch=.97f+(index%5)*.015f;index++;
                if(clip!=null) steps.PlayOneShot(clip);
            }
            if(!runner.Sliding && action.clip!=null && clips!=null && action.clip==clips.Slide) action.Stop();
            running=true;previousBeat=beat;previousDistance=distance;
        }
        void OnDisable(){if(steps!=null)steps.Stop();if(action!=null)action.Stop();running=false;}
    }
}

