using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RollingPanda
{
    // The menu clearing and held endless runner share one world; Play hands over control.
    public sealed class StartMenu : MonoBehaviour
    {
        Transform sleeper;
        Camera view;
        RunEntryTransition entrance;
        RunnerCamera chaseCamera;
        VisualPrototype worldArt;
        RunnerMotor gameRunner;
        bool ready;
        EnvironmentArt environment;
        readonly List<Mesh> meshes=new List<Mesh>();
        Mesh bambooBlade;
        Font font;
        GUIStyle title, text, button;
        readonly List<Material> materials = new List<Material>();
        string panel = "";
        bool loading;
#if !UNITY_EDITOR && !UNITY_STANDALONE
        int pressed = -1;
        Vector2 pressPosition;
#endif
        float volume;
        float Width, Height, Scale;
        Rect safe;
        bool Wide => Width > Height * 1.2f;
        float Left => Wide ? Width * .61f : 24;
        float PanelWidth => Wide ? Width * .35f : Width - 48;
        float Top => Wide ? Height * .24f : Height * .49f;

        void Awake()
        {
            font = Resources.Load<Font>("Presentation/alagard");
            volume = PlayerPrefs.GetFloat("RollingPanda.Volume", 1);
            AudioListener.volume = volume;
            RunnerInput.Reset();
            entrance=new GameObject("Forest entrance hold").AddComponent<RunEntryTransition>();
            environment=Resources.Load<EnvironmentArt>("Presentation/EnvironmentArt");
            BuildWorld();
        }
        IEnumerator Start()
        {
            // Configure while inactive so Awake builds the actual endless course once.
            var world=new GameObject("Menu and forest run");world.SetActive(false);
            world.AddComponent<MovementPrototype>().ConfigureEndless();
            worldArt=world.AddComponent<VisualPrototype>();
            world.SetActive(true);
            chaseCamera=view.GetComponent<RunnerCamera>();chaseCamera.enabled=false;
            gameRunner=world.GetComponentInChildren<RunnerMotor>();
            yield return null;
            yield return null;
            if(worldArt.Panda!=null) worldArt.Panda.gameObject.SetActive(false);
            ready=gameRunner!=null && worldArt.Panda!=null;
        }
        void Layout()
        {
            safe = Screen.safeArea;
            Scale = Mathf.Max(.01f, Mathf.Min(safe.width / 390f, safe.height / 720f));
            Width = safe.width / Scale; Height = safe.height / Scale;
        }
        Rect Row(int i) => new Rect(Left, Top + 80 + i * 58, PanelWidth, 50);
        void Update()
        {
            Layout();
            if (!loading) sleeper.localScale = new Vector3(1, 1 + Mathf.Sin(Time.unscaledTime * 1.5f) * .018f, 1);
            if (!loading)
            {
            view.orthographicSize = Screen.width > Screen.height ? 5.4f : 8.2f;
            view.transform.position = Screen.width > Screen.height ? new Vector3(2, 5.8f, -13) : new Vector3(0, 7.5f, -14);
            view.transform.LookAt(Screen.width > Screen.height ? new Vector3(-2.6f, 3, 0) : new Vector3(-4.5f, 1.5f, 0));
            }
#if !UNITY_EDITOR && !UNITY_STANDALONE
            var touch = Touchscreen.current;
            if (touch == null || loading) return;
            var t = touch.primaryTouch;
            Vector2 p = new Vector2((t.position.ReadValue().x-safe.x)/Scale, (safe.yMax-t.position.ReadValue().y)/Scale);
            if (t.press.wasPressedThisFrame) { pressed = Hit(p); pressPosition = p; }
            if (t.press.wasReleasedThisFrame)
            {
                int hit = Hit(p);
                if (hit >= 0 && hit == pressed && Vector2.Distance(p, pressPosition) < 24) Activate(hit);
                pressed = -1;
            }
#endif
        }
        int Hit(Vector2 p)
        {
            for (int i=0; i<(panel=="" ? 4 : panel=="Settings" ? 3 : 1); i++)
                if (Row(i).Contains(p)) return i;
            return -1;
        }
        void Activate(int i)
        {
            if (loading || !ready) return;
            if (panel=="")
            {
                if (i==0)
                {
                    loading=true; StartCoroutine(RollIntoRun());
                }
                else panel = i==1 ? "Settings" : i==2 ? "Leaderboards" : "Shop";
            }
            else if (panel=="Settings" && i<2)
            {
                volume=Mathf.Clamp01(volume+(i==0 ? -.1f : .1f));
                AudioListener.volume=volume;
                PlayerPrefs.SetFloat("RollingPanda.Volume",volume); PlayerPrefs.Save();
            }
            else panel="";
        }
        IEnumerator RollIntoRun()
        {
            // Preload during the roll and activate only once the short fade fully covers the view.
            var overlay=entrance;
            Vector3 start = sleeper.localPosition;
            Quaternion rotation = sleeper.localRotation;
            Quaternion upright = Quaternion.Euler(0,180,0)*Quaternion.Euler(0,0,-90); // Head (-X) becomes up; face remains toward -Z.
            Vector3 cameraStart = view.transform.position;
            Quaternion cameraRotation = view.transform.rotation;
            Matrix4x4 menuProjection = view.projectionMatrix;
            var parts = new List<Transform>();
            var poses = new List<Vector3>();
            foreach (Transform child in sleeper) { parts.Add(child); poses.Add(child.localPosition); }
            float elapsed = 0;
            while (elapsed < 2.7f)
            {
                elapsed += Time.unscaledDeltaTime;
                float wake = Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.45f));
                float fall = Mathf.Clamp01((elapsed-.45f)/.85f);
                float roll = Mathf.Clamp01((elapsed-1.3f)/1.4f);
                float tuck = Mathf.SmoothStep(0,1,fall);
                Vector3 landing = new Vector3(-3,.75f,-4.5f);
                if(elapsed < .45f)
                {
                    sleeper.localPosition=start+Vector3.up*(Mathf.Sin(wake*Mathf.PI)*.12f);
                    sleeper.localRotation=rotation*Quaternion.Euler(0,0,-18*wake);
                }
                else if(elapsed < 1.3f)
                {
                    sleeper.localPosition=new Vector3(Mathf.Lerp(start.x,landing.x,fall),Mathf.Lerp(start.y,landing.y,fall*fall),Mathf.Lerp(start.z,landing.z,fall));
                    sleeper.localRotation=Quaternion.Slerp(rotation,upright,Mathf.SmoothStep(0,1,fall));
                }
                else
                {
                    sleeper.localPosition=landing+new Vector3(3*roll,Mathf.Abs(Mathf.Sin(roll*Mathf.PI*3))*.12f,4.5f*roll);
                    sleeper.localRotation=Quaternion.AngleAxis(720*roll,Vector3.right)*upright;
                }
                sleeper.localScale=Vector3.Lerp(Vector3.one,new Vector3(.85f,1,.95f),tuck);
                for(int i=0;i<parts.Count;i++)
                {
                    Vector3 target=poses[i];
                    if(parts[i].name=="Relaxed paw") target=new Vector3(target.x*.55f,-.1f,-.45f);
                    if(parts[i].name=="Head" || parts[i].name=="Eye patch" || parts[i].name=="Closed eyelid" || parts[i].name=="Nose" || parts[i].name=="Ear" || parts[i].name.StartsWith("Red headband") || parts[i].name=="Headband ribbon")
                        target+=new Vector3(.18f,-.12f,0);
                    parts[i].localPosition=Vector3.Lerp(poses[i],target,tuck);
                }
                // Finish at the actual runner's chase view in this same world.
                float chase = Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.65f)/1.75f));
                Vector3 feet=sleeper.localPosition-Vector3.up*.7f;
                Vector3 cameraEnd=feet+new Vector3(0,5,-8);
                view.transform.position=Vector3.Lerp(cameraStart,cameraEnd,chase);
                Quaternion look=Quaternion.LookRotation(feet+new Vector3(0,1,6)-view.transform.position);
                view.transform.rotation=Quaternion.Slerp(cameraRotation,look,chase);
                if(elapsed>.65f)
                {
                    view.orthographic=false; view.fieldOfView=60;
                    Matrix4x4 perspective=Matrix4x4.Perspective(60,view.aspect,view.nearClipPlane,view.farClipPlane);
                    Matrix4x4 projection=menuProjection; for(int cell=0;cell<16;cell++) projection[cell]=Mathf.Lerp(menuProjection[cell],perspective[cell],chase); view.projectionMatrix=projection;
                }
                overlay.Opacity=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-2.45f)/.25f));
                yield return null;
            }
            // Keep the camera alignment, with scene activation hidden by the short fade.
            view.ResetProjectionMatrix();
            overlay.Opacity=1;
            yield return null;
            sleeper.gameObject.SetActive(false);
            if(worldArt.Panda!=null) worldArt.Panda.gameObject.SetActive(true);
            view.orthographic=false;view.fieldOfView=60;view.ResetProjectionMatrix();
            chaseCamera.enabled=true;
            overlay.Finish(null);
            enabled=false;
        }
        void OnGUI()
        {
            if (loading) return;
            Layout();
            if(title==null)
            {
                title=new GUIStyle(GUI.skin.label){font=font,fontSize=40,alignment=TextAnchor.MiddleCenter};
                text=new GUIStyle(title){fontSize=19,wordWrap=true};
                button=new GUIStyle(GUI.skin.button){font=font,fontSize=25};
            }
            var previous=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3(safe.x,Screen.height-safe.yMax,0),Quaternion.identity,new Vector3(Scale,Scale,1));
            GUI.Box(new Rect(Left-12,Top-8,PanelWidth+24,panel=="" ? 328 : 290),"");
            GUI.Label(new Rect(Left,Top,PanelWidth,52),panel=="" ? "Rolling Panda" : panel,title);
            if(panel=="")
            {
                GUI.Label(new Rect(Left,Top+50,PanelWidth,24),loading ? "Waking up the forest..." : "Rest. Roll. Repeat.",text);
                DrawButton(0,"Roll / Play"); DrawButton(1,"Settings"); DrawButton(2,"Leaderboards"); DrawButton(3,"Shop");
            }
            else if(panel=="Settings")
            {
                GUI.Label(new Rect(Left,Top+50,PanelWidth,26),"Master volume  "+Mathf.RoundToInt(volume*100)+"%",text);
                DrawButton(0,"Volume -");DrawButton(1,"Volume +");DrawButton(2,"Back");
            }
            else
            {
                string detail=panel=="Leaderboards" ? "Online leaderboards are coming later.\nPersonal best: "+PlayerPrefs.GetString("RollingPanda.BestScore","0") : "The shop is coming later.\nPouches will become currency as we develop the game.";
                GUI.Label(new Rect(Left,Top+144,PanelWidth,118),detail,text);
                DrawButton(0,"Back");
            }

            GUI.matrix=previous;
        }
        void DrawButton(int i,string label)
        {
            GUI.enabled=!loading && ready;
            bool clicked=GUI.Button(Row(i),label,button);
#if UNITY_EDITOR || UNITY_STANDALONE
            if(clicked) Activate(i);
#endif
            GUI.enabled=true;
        }
        Material Mat(string name,Color color)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name}; m.color=color; materials.Add(m); return m;
        }
        Transform Shape(PrimitiveType type,string name,Transform parent,Vector3 pos,Vector3 size,Material mat)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=mat;Destroy(go.GetComponent<Collider>());return go.transform;
        }
        void Branch(Transform parent,Vector3 a,Vector3 b,float radius,Material mat)
        {
            var t=Shape(PrimitiveType.Cylinder,"Branch",parent,(a+b)*.5f,new Vector3(radius,(b-a).magnitude*.5f,radius),mat);
            t.up=(b-a).normalized;
        }
        bool PlaceImported(GameObject prefab,Vector3 position,float height,float yaw)
        {
            if(prefab==null) return false;
            var go=Instantiate(prefab,transform,false);go.name="Menu "+prefab.name;
            go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
            foreach(var c in go.GetComponentsInChildren<Collider>(true)) c.enabled=false;
            foreach(var c in go.GetComponentsInChildren<MonoBehaviour>(true)) c.enabled=false;
            foreach(var body in go.GetComponentsInChildren<Rigidbody>(true)) {body.isKinematic=true;body.useGravity=false;}
            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0) {Destroy(go);return false;}
            Bounds bounds=renderers[0].bounds;foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            float scale=height/Mathf.Max(.01f,height>=6 ? bounds.size.y : Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));
            var wrapper=new GameObject("Grounded menu prop").transform;wrapper.SetParent(transform,false);wrapper.localPosition=position;wrapper.localRotation=Quaternion.Euler(0,yaw,0);
            go.transform.SetParent(wrapper,false);go.transform.localScale=Vector3.one*scale;
            go.transform.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale;
            var copies=new Dictionary<Material,Material>();
            foreach(var renderer in renderers)
            {
                var list=renderer.sharedMaterials;
                for(int i=0;i<list.Length;i++)
                {
                    Material original=list[i];if(original==null) continue;
                    if(!copies.TryGetValue(original,out var converted))
                    {
                        Color color=original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor") : original.HasProperty("_Color") ? original.GetColor("_Color") : Color.white;
                        converted=Mat("Menu URP "+original.name,color);
                        Texture map=original.HasProperty("_BaseMap") ? original.GetTexture("_BaseMap") : original.HasProperty("_MainTex") ? original.GetTexture("_MainTex") : null;
                        if(map!=null) converted.SetTexture("_BaseMap",map);
                        copies[original]=converted;
                    }
                    list[i]=converted;
                }
                renderer.sharedMaterials=list;
            }
            return true;
        }
        void BuildPouch(Vector3 position,Material cloth,Material trim)
        {
            // Open, pleated sack with a gathered neck; keeps room for actual Diamondo gems.
            float[] heights={0,.12f,.45f,.75f,.9f,1};
            float[] radii={.27f,.5f,.58f,.5f,.31f,.38f};
            const int sides=20;
            var vertices=new Vector3[heights.Length*sides];var uv=new Vector2[vertices.Length];
            var triangles=new int[(heights.Length-1)*sides*6];int index=0;
            for(int ring=0;ring<heights.Length;ring++) for(int i=0;i<sides;i++)
            {
                float angle=i*Mathf.PI*2/sides;
                float radius=radii[ring]*(1+(i%2==0 ? .06f : -.06f));
                vertices[ring*sides+i]=new Vector3(Mathf.Cos(angle)*radius,heights[ring],Mathf.Sin(angle)*radius*.8f);
                uv[ring*sides+i]=new Vector2((float)i/sides,(float)ring/(heights.Length-1));
                if(ring==heights.Length-1) continue;
                int a=ring*sides+i,b=ring*sides+(i+1)%sides,c=a+sides,d=b+sides;
                triangles[index++]=a;triangles[index++]=c;triangles[index++]=b;
                triangles[index++]=b;triangles[index++]=c;triangles[index++]=d;
            }
            var mesh=new Mesh{name="Pleated loot sack"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var sack=new GameObject("Open loot sack");sack.transform.SetParent(transform,false);sack.transform.localPosition=position;
            sack.AddComponent<MeshFilter>().sharedMesh=mesh;sack.AddComponent<MeshRenderer>().sharedMaterial=cloth;
            var rope=Mat("Pouch golden drawcord",new Color(.72f,.53f,.24f));
            for(int i=0;i<20;i++)
            {
                float a=i*Mathf.PI*2/20,b=(i+1)*Mathf.PI*2/20;
                Branch(sack.transform,new Vector3(Mathf.Cos(a)*.33f,.91f,Mathf.Sin(a)*.27f),new Vector3(Mathf.Cos(b)*.33f,.91f,Mathf.Sin(b)*.27f),.035f,rope);
            }
            Branch(sack.transform,new Vector3(.3f,.91f,-.1f),new Vector3(.48f,.56f,-.2f),.035f,rope);
            Shape(PrimitiveType.Sphere,"Drawcord knot",sack.transform,new Vector3(.32f,.88f,-.1f),Vector3.one*.11f,rope);
            Shape(PrimitiveType.Cube,"Sack seam",sack.transform,new Vector3(0,.48f,-.455f),new Vector3(.025f,.65f,.025f),trim);
        }
        Mesh BambooBlade()
        {
            if(bambooBlade!=null) return bambooBlade;
            bambooBlade=new Mesh{name="Tapered bamboo leaf"};
            bambooBlade.vertices=new[]{new Vector3(-.5f,0,0),new Vector3(.5f,0,0),new Vector3(0,.04f,-.12f),new Vector3(0,.04f,.12f)};
            bambooBlade.triangles=new[]{0,2,1,0,1,3};bambooBlade.RecalculateNormals();bambooBlade.RecalculateBounds();meshes.Add(bambooBlade);return bambooBlade;
        }
        void BuildWorld()
        {
            view=Camera.main;view.orthographic=true;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.13f,.23f,.22f);
            var bark=Mat("Cherry bark",new Color(.25f,.13f,.1f));var pink=Mat("Blossom",new Color(1,.54f,.67f));var pale=Mat("Petal tips",new Color(1,.8f,.83f));
            var green=Mat("Bamboo",new Color(.19f,.37f,.16f));var leaf=Mat("Leaves",new Color(.29f,.48f,.22f));var ivory=Mat("Panda ivory",new Color(.92f,.91f,.82f));var black=Mat("Panda ink",new Color(.055f,.06f,.065f));var red=Mat("Headband",new Color(.75f,.06f,.045f));
            leaf.SetFloat("_Cull",0);
            Shape(PrimitiveType.Sphere,"Mossy tree bank",transform,new Vector3(-7,-.45f,0),new Vector3(7,1,9),green);
            bool tree=environment!=null && PlaceImported(environment.MenuTree,new Vector3(-6,0,0),8,15);
            if(!tree) Branch(transform,new Vector3(-6,0,0),new Vector3(-6,6,0),.7f,bark);
            Branch(transform,new Vector3(-6,2.4f,0),new Vector3(-3.5f,3.1f,-.3f),.4f,bark);
            var rng=new System.Random(7301);
            if(environment!=null)
            {
                for(int i=0;i<4;i++)
                {
                    if(environment.AutumnBushes!=null && environment.AutumnBushes.Length>0) PlaceImported(environment.AutumnBushes[i%environment.AutumnBushes.Length],new Vector3(-8+i*.8f,0,-2.5f),1.4f,i*50);
                    if(i<2 && environment.Logs!=null && environment.Logs.Length>i) PlaceImported(environment.Logs[i],new Vector3(-7+i*2,0,2.5f),1.8f,i*45);
                }
            }
            for(int i=0;i<24;i++)
            {
                float x=i<12 ? -8.5f+(i%4)*.45f : -4.8f+(i%4)*.3f,z=2+i%6;
                float h=4+i%5*.6f;
                Branch(transform,new Vector3(x,0,z),new Vector3(x+.3f,h,z),.09f,green);
                for(int node=1;node<h;node++) Shape(PrimitiveType.Cylinder,"Bamboo node",transform,new Vector3(x+.3f*node/h,node,z),new Vector3(.115f,.025f,.115f),leaf);
                for(int j=0;j<6;j++)
                {
                    var l=Shape(PrimitiveType.Cube,"Pointed bamboo foliage",transform,new Vector3(x+(j%2==0 ? .4f : -.4f),h-j*.8f,z),new Vector3(.85f,.025f,.12f),leaf);l.GetComponent<MeshFilter>().sharedMesh=BambooBlade();l.localScale=Vector3.one*.85f;l.localRotation=Quaternion.Euler(0,j*55,j%2==0 ? 20 : -20);
                }
            }
            sleeper=new GameObject("Sleeping red-headband panda").transform;sleeper.SetParent(transform,false);sleeper.localPosition=new Vector3(-4.5f,3.3f,-.3f);sleeper.localRotation=Quaternion.Euler(0,-12,-8);
            Shape(PrimitiveType.Sphere,"Body",sleeper,new Vector3(.4f,0,0),new Vector3(1.7f,.95f,1),ivory);
            Shape(PrimitiveType.Sphere,"Shoulders",sleeper,new Vector3(-.1f,0,0),new Vector3(.65f,.9f,1.02f),black);
            Shape(PrimitiveType.Sphere,"Head",sleeper,new Vector3(-.7f,.12f,-.17f),new Vector3(1.05f,.95f,.95f),ivory);
            for(int i=0;i<2;i++)
            {
                float x=-1.04f+i*.64f;
                Shape(PrimitiveType.Sphere,"Ear",sleeper,new Vector3(x,.56f,-.1f),Vector3.one*.34f,black);
                Shape(PrimitiveType.Sphere,"Eye patch",sleeper,new Vector3(-.94f+i*.43f,.19f,-.58f),new Vector3(.29f,.32f,.09f),black);
                Shape(PrimitiveType.Sphere,"Closed eyelid",sleeper,new Vector3(-.94f+i*.43f,.19f,-.635f),new Vector3(.18f,.025f,.025f),ivory);
                Shape(PrimitiveType.Sphere,"Relaxed paw",sleeper,new Vector3(-.28f+i*1.25f,-.36f,-.42f),new Vector3(.48f,.65f,.5f),black);
            }
            Shape(PrimitiveType.Sphere,"Nose",sleeper,new Vector3(-.72f,-.05f,-.66f),new Vector3(.19f,.12f,.1f),black);
            Shape(PrimitiveType.Sphere,"Red headband",sleeper,new Vector3(-.7f,.38f,-.18f),new Vector3(1.07f,.1f,.94f),red);
            Shape(PrimitiveType.Cube,"Headband ribbon",sleeper,new Vector3(-1.35f,.33f,-.1f),new Vector3(.5f,.1f,.08f),red);
            var bag=Mat("Loot pouch",new Color(.43f,.24f,.1f));
            BuildPouch(new Vector3(-5,.05f,-2),bag,bark);

            var art=Resources.Load<GameArt>("Presentation/GameArt");
            Color[] colors={new Color(.04f,.65f,1),new Color(.95f,.06f,.12f),new Color(.15f,.9f,.32f)};
            for(int i=0;i<18;i++)
            {
                var gem=Shape(PrimitiveType.Cube,"Loot gem",transform,new Vector3(-5+(float)rng.NextDouble()*.8f-.4f,i<10 ? .95f+(float)rng.NextDouble()*.25f : .18f,-2+(float)rng.NextDouble()*1.5f-.75f),Vector3.one*.24f,Mat("Gem",colors[i%3]));
                if(art!=null&&art.Diamond!=null)
                {
                    gem.GetComponent<MeshFilter>().sharedMesh=art.Diamond;
                    float extent=Mathf.Max(art.Diamond.bounds.size.x,Mathf.Max(art.Diamond.bounds.size.y,art.Diamond.bounds.size.z));
                    gem.localScale=Vector3.one*(.28f/Mathf.Max(.001f,extent));
                }
                gem.localRotation=Quaternion.Euler(15,i*37,45);
            }
            var petals=new GameObject("Drifting blossom petals");petals.transform.SetParent(transform,false);petals.transform.localPosition=new Vector3(-6,7,0);
            var ps=petals.AddComponent<ParticleSystem>();var main=ps.main;main.maxParticles=48;main.startLifetime=7;main.startSpeed=.1f;main.startSize=.07f;main.startColor=new Color(1,.7f,.8f);main.gravityModifier=.025f;
            var emission=ps.emission;emission.rateOverTime=5;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(7,.2f,3);
            var pm=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));materials.Add(pm);pm.SetColor("_BaseColor",new Color(1,.7f,.8f));ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=pm;
        }
        void OnDestroy()
        {
            foreach(var m in materials) Destroy(m);
            foreach(var mesh in meshes) Destroy(mesh);
            if(entrance!=null) Destroy(entrance.gameObject);
        }
    }
}


