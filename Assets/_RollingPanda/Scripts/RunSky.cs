using UnityEngine;
namespace RollingPanda
{
    // One fog-free skybox draw; no star/cloud objects or extra lights.
    public sealed class RunSky : MonoBehaviour
    {
        RunnerMotor runner;
        Camera view;
        Material sky,previous;
        CameraClearFlags oldFlags;
        float clock;
        public void Initialize(RunnerMotor player)
        {
            runner=player;view=Camera.main;
            var shader=Resources.Load<Shader>("Presentation/ForestSky");
            if(shader==null) {Debug.LogWarning("Rolling Panda: ForestSky shader resource is missing.",this);return;}
            previous=RenderSettings.skybox;sky=new Material(shader){name="Rolling Panda evolving sky"};
            RenderSettings.skybox=sky;
            if(view!=null) {oldFlags=view.clearFlags;view.clearFlags=CameraClearFlags.Skybox;}
        }
        void Update()
        {
            if(sky==null) return;
            if(runner.Session.IsRunning || RunEntryTransition.Active && !RunEntryTransition.Revealing) clock+=Time.deltaTime;
            sky.SetFloat("_Clock",clock);
        }
        public void Apply(float distance,float dark,float dusk,float mist)
        {
            if(sky==null) return;
            sky.SetFloat("_Night",dark);sky.SetFloat("_Dusk",dusk);sky.SetFloat("_Mist",mist);
            sky.SetVector("_SunDirection",new Vector4(distance>=4800 ? -.32f : .32f,Mathf.Lerp(.24f,-.1f,dark),1,0));
            RenderSettings.skybox=sky;
            if(view!=null) view.clearFlags=CameraClearFlags.Skybox;
        }
        void OnDestroy()
        {
            if(sky==null) return;
            if(RenderSettings.skybox==sky) RenderSettings.skybox=previous;
            if(view!=null && view.clearFlags==CameraClearFlags.Skybox) view.clearFlags=oldFlags;
            Destroy(sky);
        }
    }
}
