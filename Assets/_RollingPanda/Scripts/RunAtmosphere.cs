using UnityEngine;
using UnityEngine.Rendering;

namespace RollingPanda
{
    // Distance-driven presentation only. Four nearby, shadowless shrine lights.
    public sealed class RunAtmosphere : MonoBehaviour
    {
        RunnerMotor runner;
        ForestAtmosphere forest;
        Light sun;
        Camera view;
        Material foliage, grass;
        readonly Light[] lamps = new Light[4];
        readonly Transform[] selected = new Transform[4];
        readonly float[] nearest = new float[4];
        public void Initialize(RunnerMotor player, ForestAtmosphere scenery, Material leaf, Material bank)
        {
            runner=player; forest=scenery; foliage=leaf; grass=bank; view=Camera.main;
            foreach(var light in FindObjectsByType<Light>())
                if(light.type==LightType.Directional) { sun=light; break; }
            for(int i=0;i<4;i++)
            {
                var go=new GameObject("Nearby warm shrine light"); go.transform.SetParent(transform,false);
                lamps[i]=go.AddComponent<Light>(); lamps[i].type=LightType.Point;
                lamps[i].color=new Color(1,.55f,.2f); lamps[i].range=8;
                lamps[i].shadows=LightShadows.None; lamps[i].enabled=false;
            }
            RenderSettings.ambientMode=AmbientMode.Flat;
            Apply();
        }
        void LateUpdate() { if(runner!=null) Apply(); }
        void Apply()
        {
            float distance=(float)(runner.DistanceTravelled%6000);
            float mist=Rise(distance,800,1500)*(1-Rise(distance,2300,3000));
            float dark=Rise(distance,1800,3400)*(1-Rise(distance,4800,6000));
            float dusk=Rise(distance,1600,2400)*(1-Rise(distance,2700,3400));
            Color fog=Color.Lerp(new Color(.54f,.7f,.66f),new Color(.1f,.16f,.24f),dark);
            fog=Color.Lerp(fog,new Color(.65f,.43f,.36f),dusk*.4f);
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Linear; RenderSettings.fogColor=fog;
            RenderSettings.fogStartDistance=Mathf.Lerp(65,40,mist);
            RenderSettings.fogEndDistance=Mathf.Lerp(155,120,mist);
            RenderSettings.ambientLight=Color.Lerp(new Color(.62f,.7f,.65f),new Color(.32f,.4f,.52f),dark);
            if(view!=null) view.backgroundColor=fog;
            if(sun!=null)
            {
                sun.intensity=Mathf.Lerp(1.15f,.42f,dark);
                sun.color=Color.Lerp(new Color(1,.88f,.68f),new Color(.59f,.72f,1),dark);
                sun.transform.rotation=Quaternion.Euler(Mathf.Lerp(48,23,dark),-35,0);
            }
            foliage.SetColor("_BaseColor",Color.Lerp(new Color(.12f,.29f,.16f),new Color(.11f,.21f,.23f),dark));
            grass.SetColor("_BaseColor",Color.Lerp(new Color(.31f,.43f,.18f),new Color(.19f,.3f,.26f),dark));
            forest.SetWarmth(dark);
            for(int i=0;i<4;i++){nearest[i]=float.MaxValue; selected[i]=null;}
            foreach(Transform socket in forest.LampSockets)
            {
                float gap=socket.position.z-runner.transform.position.z;
                if(gap < -12 || gap > 65) continue;
                float d=(socket.position-runner.transform.position).sqrMagnitude;
                for(int i=0;i<4;i++) if(d<nearest[i])
                {
                    for(int j=3;j>i;j--){nearest[j]=nearest[j-1];selected[j]=selected[j-1];}
                    nearest[i]=d;selected[i]=socket;break;
                }
            }
            for(int i=0;i<4;i++)
            {
                lamps[i].enabled=dark>.08f && selected[i]!=null;
                if(selected[i]!=null) lamps[i].transform.position=selected[i].position;
                lamps[i].intensity=selected[i]==null ? 0 : dark*2.3f*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(35,65,Mathf.Sqrt(nearest[i]))));
            }
        }
        static float Rise(float value,float start,float end) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(start,end,value));
    }
}

