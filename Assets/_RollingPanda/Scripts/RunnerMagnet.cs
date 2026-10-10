using System;
using System.Collections.Generic;
using UnityEngine;
namespace RollingPanda
{
    // One reusable powerup, placed between obstacle rows. Attraction affects pickups only.
    public sealed class RunnerMagnet : MonoBehaviour
    {
        RunnerMotor runner;
        GameSession session;
        IReadOnlyList<BlueGem> gems;
        Transform pickup,visual;
        readonly System.Random random=new System.Random();
        readonly List<Material> materials=new List<Material>();
        double nextDistance;
        int lane;
        float previousDelta=float.PositiveInfinity;
        public float Remaining {get;private set;}
        public void Initialize(RunnerMotor player,GameSession game,IReadOnlyList<BlueGem> pickups)
        {
            runner=player;session=game;gems=pickups;
            pickup=new GameObject("Pooled magnet pickup").transform;pickup.SetParent(runner.transform,false);
            visual=new GameObject("Magnet art").transform;visual.SetParent(pickup,false);
            var art=Resources.Load<EnvironmentArt>("Presentation/EnvironmentArt");
            if(art!=null && art.Magnet!=null) Imported(art.Magnet);else Fallback();
            ResetPowerup();
        }
        void Imported(GameObject prefab)
        {
            var go=Instantiate(prefab,visual,false);go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
            foreach(var c in go.GetComponentsInChildren<Collider>(true)) c.enabled=false;
            foreach(var c in go.GetComponentsInChildren<MonoBehaviour>(true)) c.enabled=false;
            foreach(var b in go.GetComponentsInChildren<Rigidbody>(true)){b.isKinematic=true;b.useGravity=false;}
            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0){Destroy(go);Fallback();return;}
            Bounds bounds=renderers[0].bounds;foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            float size=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
            float scale=.95f/Mathf.Max(.01f,size);Vector3 centre=visual.InverseTransformPoint(bounds.center);
            go.transform.localScale=Vector3.one*scale;go.transform.localPosition=-centre*scale;
            var copies=new Dictionary<Material,Material>();
            foreach(var r in renderers)
            {
                var list=r.sharedMaterials;
                for(int i=0;i<list.Length;i++)
                {
                    var source=list[i];if(source==null) continue;
                    if(!copies.TryGetValue(source,out var copy))
                    {
                        copy=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        copy.SetColor("_BaseColor",source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : source.HasProperty("_Color") ? source.GetColor("_Color") : Color.red);
                        Texture texture=source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
                        if(texture!=null) copy.SetTexture("_BaseMap",texture);
                        copy.SetFloat("_Smoothness",.6f);materials.Add(copy);copies[source]=copy;
                    }
                    list[i]=copy;
                }
                r.sharedMaterials=list;
            }
        }
        void Fallback()
        {
            var red=new Material(Shader.Find("Universal Render Pipeline/Lit"));red.color=new Color(.9f,.07f,.09f);materials.Add(red);
            var white=new Material(red);white.color=Color.white;materials.Add(white);
            for(int i=0;i<5;i++)
            {
                var part=GameObject.CreatePrimitive(PrimitiveType.Cube);part.transform.SetParent(visual,false);Destroy(part.GetComponent<Collider>());
                part.transform.localPosition=i==0 ? new Vector3(0,-.3f,0) : new Vector3(i%2==1 ? -.3f : .3f,i<3 ? 0 : .3f,0);
                part.transform.localScale=i==0 ? new Vector3(.8f,.22f,.22f) : new Vector3(.22f,i<3 ? .6f : .2f,.22f);
                part.GetComponent<Renderer>().sharedMaterial=i<3 ? red : white;
            }
        }
        void Schedule(bool first)
        {
            // Obstacles sit on multiples of 24 m; half-row positions avoid their hitboxes.
            nextDistance=MagnetRules.Next(nextDistance,random.Next(),first);
            lane=random.Next(-1,2);previousDelta=float.PositiveInfinity;pickup.gameObject.SetActive(false);
        }
        public void ResetPowerup()
        {
            Remaining=0;
            if(gems!=null) foreach(var gem in gems) gem.ReleaseMagnet();
            Schedule(true);
        }
        void LateUpdate()
        {
            if(runner==null || !session.IsRunning) return;
            float dt=Time.deltaTime;
            if(Remaining>0)
            {
                Remaining=MagnetRules.Tick(Remaining,dt,true);
                Vector3 target=runner.transform.position+Vector3.up;
                if(Remaining>0) for(int i=0;i<gems.Count;i++)
                {
                    var gem=gems[i];
                    if(!gem.gameObject.activeInHierarchy) continue;
                    Vector3 offset=gem.transform.position-target;
                    if(MagnetRules.InRange(offset.x,offset.y,offset.z)) gem.PullTowards(target,dt);
                }
                else foreach(var gem in gems) gem.ReleaseMagnet();
            }
            float delta=(float)(nextDistance-runner.DistanceTravelled);
            if(delta<=70 && delta>=-6)
            {
                pickup.gameObject.SetActive(true);
                pickup.localPosition=new Vector3(lane*RunnerMotor.LaneSpacing-runner.transform.position.x,1.1f,delta);
                visual.Rotate(0,95*dt,0,Space.Self);
                if(MagnetRules.CanCollect(runner.transform.position.x-lane*RunnerMotor.LaneSpacing,delta,previousDelta))
                {Remaining=MagnetRules.Duration;Schedule(false);return;}
            }
            if(delta < -6) Schedule(false);
            else previousDelta=delta;
        }
        void OnDestroy(){foreach(var m in materials) Destroy(m);}
    }
}


