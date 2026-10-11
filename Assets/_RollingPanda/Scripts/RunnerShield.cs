using System;
using System.Collections.Generic;
using UnityEngine;
namespace RollingPanda
{
    // One reusable shield pickup. Protection suppresses defeat, never physical collisions.
    [DefaultExecutionOrder(-50)]
    public sealed class RunnerShield : MonoBehaviour
    {
        RunnerMotor runner;
        GameSession session;
        GameObject bubble;
        Material bubbleMaterial;
        Transform pickup,visual;
        readonly System.Random random=new System.Random();
        readonly List<Material> materials=new List<Material>();
        double nextDistance;
        int lane;
        float previousDelta=float.PositiveInfinity;
        public float Remaining {get;private set;}
        public void Initialize(RunnerMotor player,GameSession game)
        {
            runner=player;session=game;
            pickup=new GameObject("Pooled shield pickup").transform;pickup.SetParent(runner.transform,false);
            visual=new GameObject("Shield art").transform;visual.SetParent(pickup,false);
            var art=Resources.Load<EnvironmentArt>("Presentation/EnvironmentArt");
            if(art!=null && art.Shield!=null) Imported(art.Shield);else Fallback();
            bubble=GameObject.CreatePrimitive(PrimitiveType.Sphere);bubble.name="Cosmetic shield bubble";
            bubble.transform.SetParent(runner.transform,false);Destroy(bubble.GetComponent<Collider>());
            bubbleMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));materials.Add(bubbleMaterial);
            bubbleMaterial.SetFloat("_Surface",1);bubbleMaterial.SetFloat("_ZWrite",0);
            bubbleMaterial.SetFloat("_SrcBlend",5);bubbleMaterial.SetFloat("_DstBlend",10);
            bubbleMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");bubbleMaterial.renderQueue=3000;
            bubbleMaterial.SetColor("_BaseColor",new Color(.15f,.7f,1,.13f));
            bubble.GetComponent<Renderer>().sharedMaterial=bubbleMaterial;
            bubble.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            bubble.GetComponent<Renderer>().receiveShadows=false;
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
            var blue=new Material(Shader.Find("Universal Render Pipeline/Lit"));blue.color=new Color(.1f,.65f,1);materials.Add(blue);
            var plate=GameObject.CreatePrimitive(PrimitiveType.Cube);plate.transform.SetParent(visual,false);Destroy(plate.GetComponent<Collider>());
            plate.transform.localScale=new Vector3(.7f,.9f,.15f);plate.GetComponent<Renderer>().sharedMaterial=blue;
            plate.transform.localRotation=Quaternion.Euler(0,0,45);
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
            if(bubble!=null) bubble.SetActive(false);
            Schedule(true);
        }
        void Update()
        {
            if(session!=null) Remaining=MagnetRules.Tick(Remaining,Time.deltaTime,session.IsRunning);
        }
        void LateUpdate()
        {
            if(runner==null) return;
            bubble.SetActive(Remaining>0);
            if(!session.IsRunning) return;
            float dt=Time.deltaTime;
            // The timer is updated before RunnerMotor, including when blocked in place.
            bubble.SetActive(Remaining>0);
            bubble.transform.localPosition=new Vector3(0,runner.Sliding ? .5f : 1,0);
            bubble.transform.localScale=runner.Sliding ? new Vector3(1.6f,1.2f,1.6f) : new Vector3(1.6f,2.3f,1.6f);
            float delta=(float)(nextDistance-runner.DistanceTravelled);
            if(delta<=70 && delta>=-6)
            {
                pickup.gameObject.SetActive(true);
                pickup.localPosition=new Vector3(lane*RunnerMotor.LaneSpacing-runner.transform.position.x,1.1f,delta);
                visual.Rotate(0,95*dt,0,Space.Self);
                if(MagnetRules.CanCollect(runner.transform.position.x-lane*RunnerMotor.LaneSpacing,delta,previousDelta))
                {Remaining=ShieldRules.Duration;Schedule(false);return;}
            }
            if(delta < -6) Schedule(false);
            else previousDelta=delta;
        }
        void OnDestroy(){foreach(var m in materials) Destroy(m);}
    }
}


