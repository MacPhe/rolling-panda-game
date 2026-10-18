using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace RollingPanda
{
    [DefaultExecutionOrder(-40)]
    public sealed class RunnerSpecials : MonoBehaviour
    {
        sealed class Slot {public SpecialKind Kind;public Transform Root,Art,Star;public Renderer Shine;public double Distance;public int Occurrence,Lane;public float PreviousDelta=float.PositiveInfinity;}
        readonly Slot[] slots=new Slot[5];
        readonly System.Random random=new System.Random();
        readonly List<Material> materials=new List<Material>();
        RunnerMotor runner;GameSession session;GemPresentation palette;
        MaterialPropertyBlock block;
        Camera view;
        ParticleSystem explosion;
        Vector3 previousPlayer;
        float clock;
        public float SpeedRemaining {get;private set;}
        public float BatteryRemaining {get;private set;}
        public SpecialKind SpeedKind {get;private set;}
        public float AdjustSpeed(float baseline) => SpecialRules.Speed(baseline,SpeedRemaining,SpeedKind);
        public void Initialize(RunnerMotor player,GameSession game)
        {
            runner=player;session=game;view=Camera.main;block=new MaterialPropertyBlock();
            palette=GetComponent<GemPresentation>();
            var art=Resources.Load<EnvironmentArt>("Presentation/EnvironmentArt");
            for(int i=0;i<5;i++)
            {
                var slot=new Slot{Kind=(SpecialKind)i};slots[i]=slot;
                slot.Root=new GameObject("Pooled "+slot.Kind).transform;slot.Root.SetParent(runner.transform,false);
                slot.Art=new GameObject(slot.Kind+" art").transform;slot.Art.SetParent(slot.Root,false);
                GameObject prefab=art!=null && art.Specials!=null && art.Specials.Length>i ? art.Specials[i] : null;
                Model(slot,prefab);
                if(palette!=null && i!=2)
                {
                    slot.Star=new GameObject("Pickup glisten").transform;slot.Star.SetParent(slot.Art,false);slot.Star.localPosition=new Vector3(.35f,.35f,-.15f);
                    slot.Star.gameObject.AddComponent<MeshFilter>().sharedMesh=palette.Star;
                    slot.Shine=slot.Star.gameObject.AddComponent<MeshRenderer>();slot.Shine.sharedMaterial=palette.Sparkle;slot.Shine.shadowCastingMode=ShadowCastingMode.Off;
                }
            }
            BuildExplosion();ResetPowerups();
        }
        Material Lit(Color color,float metallic,float smooth)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);materials.Add(m);return m;
        }
        void Model(Slot slot,GameObject prefab)
        {
            var go=prefab!=null ? Instantiate(prefab,slot.Art,false) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(slot.Art,false);go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
            foreach(var c in go.GetComponentsInChildren<Collider>(true)) c.enabled=false;
            foreach(var c in go.GetComponentsInChildren<MonoBehaviour>(true)) c.enabled=false;
            foreach(var b in go.GetComponentsInChildren<Rigidbody>(true)) {b.isKinematic=true;b.useGravity=false;}
            var renderers=go.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0) return;
            Bounds bounds=renderers[0].bounds;foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            float scale=.95f/Mathf.Max(.01f,Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));
            Vector3 centre=slot.Art.InverseTransformPoint(bounds.center);go.transform.localScale=Vector3.one*scale;go.transform.localPosition=-centre*scale;
            var copies=new Dictionary<Material,Material>();
            foreach(var r in renderers)
            {
                var list=r.sharedMaterials;
                for(int i=0;i<list.Length;i++)
                {
                    var source=list[i];if(source==null) continue;
                    if(!copies.TryGetValue(source,out var copy))
                    {
                        Color color=source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white;
                        bool blue=slot.Kind==SpecialKind.Speed,gold=slot.Kind==SpecialKind.Gold;
                        copy=Lit(blue ? new Color(.02f,.55f,1) : gold ? new Color(1,.78f,.24f) : color,gold ? .85f : .3f,.94f);
                        Texture texture=source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
                        if(texture!=null && !blue) copy.SetTexture("_BaseMap",texture);
                        if(blue || gold) {copy.EnableKeyword("_EMISSION");copy.SetColor("_EmissionColor",blue ? new Color(.02f,.6f,1)*1.7f : new Color(.55f,.28f,.02f));}
                        copies[source]=copy;
                    }
                    list[i]=copy;
                }
                r.sharedMaterials=list;
            }
            if(slot.Kind==SpecialKind.Time)
            {
                var glow=Lit(new Color(.1f,.8f,1,.16f),0,.9f);
                glow.SetFloat("_Surface",1);glow.SetFloat("_ZWrite",0);glow.SetFloat("_SrcBlend",5);glow.SetFloat("_DstBlend",1);glow.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glow.renderQueue=3000;
                Decorative(slot.Art,PrimitiveType.Sphere,Vector3.zero,new Vector3(.45f,.65f,.45f),glow);
                var wood=Lit(new Color(.24f,.095f,.025f),0,.3f);
                Decorative(slot.Art,PrimitiveType.Cylinder,new Vector3(0,.43f,0),new Vector3(.62f,.04f,.62f),wood);
                Decorative(slot.Art,PrimitiveType.Cylinder,new Vector3(0,-.43f,0),new Vector3(.62f,.04f,.62f),wood);
            }
            if(slot.Kind==SpecialKind.Speed)
            {
                var aura=new Material(Shader.Find("Universal Render Pipeline/Unlit"));materials.Add(aura);aura.SetColor("_BaseColor",new Color(.05f,.55f,1,.12f));
                aura.SetFloat("_Surface",1);aura.SetFloat("_ZWrite",0);aura.SetFloat("_SrcBlend",5);aura.SetFloat("_DstBlend",1);aura.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");aura.renderQueue=3000;
                Decorative(slot.Art,PrimitiveType.Sphere,Vector3.zero,Vector3.one*1.25f,aura);
            }
        }
        void Decorative(Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
        }
        void BuildExplosion()
        {
            var go=new GameObject("Reusable bomb explosion");go.transform.SetParent(runner.transform,false);explosion=go.AddComponent<ParticleSystem>();explosion.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=explosion.main;main.loop=false;main.playOnAwake=false;main.maxParticles=80;main.duration=1;main.startLifetime=new ParticleSystem.MinMaxCurve(.35f,.8f);main.startSpeed=new ParticleSystem.MinMaxCurve(3,6);main.startSize=new ParticleSystem.MinMaxCurve(.08f,.3f);main.gravityModifier=.3f;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=explosion.emission;emission.rateOverTime=0;
            var shape=explosion.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.35f;
            var color=explosion.colorOverLifetime;color.enabled=true;
            var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(1,.8f,.2f),0),new GradientColorKey(new Color(1,.15f,.02f),.4f),new GradientColorKey(new Color(.15f,.13f,.12f),1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});color.color=gradient;
            var renderer=explosion.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Mesh;
            var temp=GameObject.CreatePrimitive(PrimitiveType.Sphere);temp.SetActive(false);renderer.mesh=temp.GetComponent<MeshFilter>().sharedMesh;Destroy(temp);
            var material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));materials.Add(material);material.SetFloat("_Surface",1);material.SetFloat("_ZWrite",0);material.SetFloat("_SrcBlend",5);material.SetFloat("_DstBlend",10);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
        void Schedule(Slot slot,bool first)
        {
            if(first) {slot.Occurrence=0;slot.Distance=0;} else slot.Occurrence++;
            slot.Distance=SpecialRules.Next(slot.Kind,slot.Distance,slot.Occurrence,random.Next());
            slot.Lane=random.Next(-1,2);
            // Prefer a separate lane if another pickup has the same row.
            for(int attempt=0;attempt<3;attempt++)
            {
                bool conflict=false;foreach(var other in slots) if(other!=null && other!=slot && System.Math.Abs(other.Distance-slot.Distance)<3 && other.Lane==slot.Lane) conflict=true;
                if(!conflict) break;slot.Lane=(slot.Lane+2)%3-1;
            }
            slot.PreviousDelta=float.PositiveInfinity;slot.Root.gameObject.SetActive(false);
        }
        public void ResetPowerups()
        {
            SpeedRemaining=BatteryRemaining=clock=0;previousPlayer=runner.transform.position;
            foreach(var slot in slots) Schedule(slot,true);
            explosion.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        void Update()
        {
            if(session==null) return;
            SpeedRemaining=MagnetRules.Tick(SpeedRemaining,Time.deltaTime,session.IsRunning);
            BatteryRemaining=MagnetRules.Tick(BatteryRemaining,Time.deltaTime,session.IsRunning);
        }
        void LateUpdate()
        {
            if(runner==null || !session.IsRunning) return;
            clock+=Time.deltaTime;
            foreach(var slot in slots)
            {
                float delta=(float)(slot.Distance-runner.DistanceTravelled);
                if(delta<=90 && delta>=-6)
                {
                    float y=slot.Kind==SpecialKind.Bomb || slot.Kind==SpecialKind.Gold ? .55f : 1.1f;
                    slot.Root.gameObject.SetActive(true);slot.Root.localPosition=new Vector3(slot.Lane*3-runner.transform.position.x,y-runner.transform.position.y,delta);
                    if(slot.Kind!=SpecialKind.Bomb) slot.Art.localRotation=Quaternion.Euler(0,clock*75,0);
                    bool crossed=slot.PreviousDelta>0 && slot.PreviousDelta<float.PositiveInfinity && delta<=0;
                    float fraction=crossed ? Mathf.Clamp01(slot.PreviousDelta/(slot.PreviousDelta-delta)) : 1;
                    float x=Mathf.Lerp(previousPlayer.x,runner.transform.position.x,fraction),feet=Mathf.Lerp(previousPlayer.y,runner.transform.position.y,fraction);
                    if((Mathf.Abs(delta)<1 || crossed) && Mathf.Abs(x-slot.Lane*3)<.85f && SpecialRules.VerticalHit(feet,runner.Sliding ? .9f : 2,y))
                    {
                        Collect(slot);Schedule(slot,false);if(session.Defeated) break;
                    }
                    if(slot.Shine!=null)
                    {
                        float pulse=Mathf.Pow(Mathf.Max(0,Mathf.Sin(clock*2+((int)slot.Kind)*1.3f)),16);
                        slot.Shine.enabled=pulse>.03f;
                        if(view!=null) slot.Star.rotation=view.transform.rotation;
                        slot.Star.localScale=Vector3.one*Mathf.Lerp(.2f,.9f,pulse);
                        block.SetColor("_BaseColor",new Color(1,1,.9f,pulse));slot.Shine.SetPropertyBlock(block);
                    }
                }
                if(delta < -6) Schedule(slot,false);else slot.PreviousDelta=delta;
            }
            previousPlayer=runner.transform.position;
        }
        void Collect(Slot slot)
        {
            switch(slot.Kind)
            {
                case SpecialKind.Speed:case SpecialKind.Time:SpeedKind=slot.Kind;SpeedRemaining=SpecialRules.SpeedDuration;break;
                case SpecialKind.Battery:BatteryRemaining=SpecialRules.BatteryDuration;break;
                case SpecialKind.Gold:session.CollectGold(slot.Root.position);break;
                case SpecialKind.Bomb:
                    if(runner.InChute)break;
                    explosion.transform.position=slot.Root.position;explosion.Play(false);explosion.Emit(64);session.Fail("Bomb hit!");break;
            }
        }
        void OnDestroy(){foreach(var material in materials) Destroy(material);}
    }
}

