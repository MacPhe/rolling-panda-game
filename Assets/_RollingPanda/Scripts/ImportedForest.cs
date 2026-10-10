using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace RollingPanda
{
    // Both seasons are prepared once; existing section recycling carries the scenery.
    public sealed class ImportedForest : MonoBehaviour
    {
        sealed class Section { public Transform Track;public GameObject Summer,Autumn;public bool IsAutumn; }
        readonly List<Section> sections=new List<Section>();
        readonly Dictionary<Material,Material> materials=new Dictionary<Material,Material>();
        readonly List<ParticleSystem> petals=new List<ParticleSystem>();
        RunnerMotor runner;Shader lit,particleShader;

        public bool Initialize(RunnerMotor player,EnvironmentArt art)
        {
            if(art==null || art.SummerTrees==null || art.SummerTrees.Length==0 || art.SummerTrees[0]==null) return false;
            runner=player;lit=Shader.Find("Universal Render Pipeline/Lit");
            particleShader=Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if(lit==null) return false;
            foreach(Transform track in transform)
            {
                if(!track.name.StartsWith("Track section ")) continue;
                var section=new Section{Track=track};
                section.Summer=Season(track,art.SummerTrees,art.SummerBushes,"Summer forest");
                section.Autumn=Season(track,art.AutumnTrees,art.AutumnBushes,"Autumn forest");
                for(int side=-1;side<=1;side+=2)
                {
                    for(int i=0;i<3;i++)
                    {
                        Place(art.Logs,i,track,new Vector3(side*(8+i),0,12+i*30),2.2f,side*(35+i*22));
                        Place(art.Stumps,i,track,new Vector3(side*(6.8f+i),0,23+i*28),1.1f,i*67);
                    }
                    if(art.Petals!=null && particleShader!=null) AddPetals(art.Petals,track,new Vector3(side*7,4,48));
                }
                section.Autumn.SetActive(false);sections.Add(section);
            }
            UpdateSeasons();return sections.Count>0;
        }
        GameObject Season(Transform track,GameObject[] trees,GameObject[] bushes,string name)
        {
            var root=new GameObject(name);root.transform.SetParent(track,false);
            for(int side=-1;side<=1;side+=2) for(int i=0;i<6;i++)
            {
                Place(trees,i+ (side==1 ? 3 : 0),root.transform,new Vector3(side*(9+i%2*2),0,8+i*16),10+i%3,i*57+side*20);
                Place(bushes,i,root.transform,new Vector3(side*(6.5f+i%2*.6f),0,6+i*16),1.7f+i%3*.2f,i*73);
                Place(bushes,i+1,root.transform,new Vector3(side*8.5f,0,13+i*16),2.2f,i*41);
            }
            return root;
        }
        void Place(GameObject[] prefabs,int index,Transform parent,Vector3 position,float size,float yaw)
        {
            if(prefabs==null || prefabs.Length==0 || prefabs[index%prefabs.Length]==null) return;
            var go=Instantiate(prefabs[index%prefabs.Length],parent,false);go.name="Imported "+go.name;
            go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
            foreach(var collider in go.GetComponentsInChildren<Collider>(true)) collider.enabled=false;
            foreach(var script in go.GetComponentsInChildren<MonoBehaviour>(true)) script.enabled=false;
            foreach(var body in go.GetComponentsInChildren<Rigidbody>(true)){body.isKinematic=true;body.useGravity=false;}
            var renderers=go.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0){Destroy(go);return;}
            Bounds bounds=renderers[0].bounds;foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            bool tree=size>=8;
            float extent=tree ? bounds.size.y : Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
            float scale=size/Mathf.Max(.01f,extent);
            if(tree) scale=Mathf.Min(scale,6.8f/Mathf.Max(.01f,new Vector2(bounds.size.x,bounds.size.z).magnitude));
            // A wrapper normalizes arbitrary vendor pivots while keeping the base grounded.
            var wrapper=new GameObject("Grounded forest prop").transform;wrapper.SetParent(parent,false);
            wrapper.localPosition=position;wrapper.localRotation=Quaternion.Euler(0,yaw,0);
            go.transform.SetParent(wrapper,false);go.transform.localScale=Vector3.one*scale;
            Vector3 localCentre=parent.InverseTransformPoint(bounds.center);
            float bottom=parent.InverseTransformPoint(new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)).y;
            go.transform.localPosition=new Vector3(-localCentre.x,-bottom,-localCentre.z)*scale;
            foreach(var renderer in renderers)
            {
                var converted=renderer.sharedMaterials;for(int i=0;i<converted.Length;i++) converted[i]=Convert(converted[i],false);
                renderer.sharedMaterials=converted;renderer.shadowCastingMode=ShadowCastingMode.Off;
            }
        }
        Material Convert(Material source,bool particle)
        {
            if(source==null) return null;
            if(materials.TryGetValue(source,out var result)) return result;
            result=new Material(particle ? particleShader : lit);result.name="Runner URP "+source.name;
            Texture map=source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
            result.SetTexture("_BaseMap",map);
            result.SetColor("_BaseColor",source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white);
            if(particle)
            {
                result.SetFloat("_Surface",1);result.SetFloat("_Blend",0);result.SetFloat("_ZWrite",0);result.SetFloat("_Cull",0);
                result.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);result.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
                result.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");result.renderQueue=3000;
            }
            else result.SetFloat("_Smoothness",.12f);
            materials[source]=result;return result;
        }
        void AddPetals(GameObject prefab,Transform track,Vector3 position)
        {
            var go=Instantiate(prefab,track,false);go.name="Shrine cherry petals";go.transform.localPosition=position;
            foreach(var collider in go.GetComponentsInChildren<Collider>(true)) collider.enabled=false;
            foreach(var script in go.GetComponentsInChildren<MonoBehaviour>(true)) script.enabled=false;
            foreach(var body in go.GetComponentsInChildren<Rigidbody>(true)){body.isKinematic=true;body.useGravity=false;}
            foreach(var system in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=system.main;main.maxParticles=48;main.simulationSpace=ParticleSystemSimulationSpace.Local;
                main.simulationSpeed=1;main.loop=true;main.playOnAwake=false;main.startLifetime=4;main.startSpeed=.25f;main.startSize=.12f;
                var emission=system.emission;emission.rateOverTime=6;emission.rateOverDistance=0;emission.SetBursts(new ParticleSystem.Burst[0]);
                var shape=system.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(3,1,8);
                var velocity=system.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;
                velocity.x=0;velocity.y=-.35f;velocity.z=.15f;
                var trails=system.trails;trails.enabled=false;
                var sub=system.subEmitters;sub.enabled=false;
                var collision=system.collision;collision.enabled=false;
                var renderer=system.GetComponent<ParticleSystemRenderer>();
                if(renderer!=null) renderer.sharedMaterial=Convert(renderer.sharedMaterial,true);
                petals.Add(system);
            }
        }
        void LateUpdate()
        {
            if(runner==null) return;UpdateSeasons();
            bool running=runner.Session!=null && runner.Session.IsRunning;
            foreach(var system in petals)
            {
                float delta=system.transform.position.z-runner.transform.position.z;
                bool nearby=delta>-20 && delta<90;
                if(!nearby){if(system.isPlaying || system.particleCount>0) system.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);}
                else if(running && !system.isPlaying) system.Play(false);
                else if(!running && system.isPlaying) system.Pause(false);
            }

        }
        void UpdateSeasons()
        {
            foreach(var section in sections)
            {
                double centre=runner.DistanceTravelled+section.Track.position.z-runner.transform.position.z+48;
                double phase=((centre%6000)+6000)%6000;bool autumn=phase>=2400 && phase<4800;
                if(autumn==section.IsAutumn) continue;
                section.Summer.SetActive(!autumn);section.Autumn.SetActive(autumn);section.IsAutumn=autumn;
            }
        }
        void OnDestroy(){foreach(var material in materials.Values) if(material!=null) Destroy(material);}
    }
}


