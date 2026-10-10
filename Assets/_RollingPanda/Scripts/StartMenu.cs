using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace RollingPanda
{
    // A separate, bounded presentation scene. No runner or obstacle pool runs behind the menu.
    public sealed class StartMenu : MonoBehaviour
    {
        Transform sleeper;
        Camera view;
        Font font;
        GUIStyle title, text, button;
        readonly List<Material> materials = new List<Material>();
        string panel = "", error = "";
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
            BuildWorld();
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
            view.transform.position = Screen.width > Screen.height ? new Vector3(7, 5.8f, -13) : new Vector3(5, 7.5f, -14);
            view.transform.LookAt(Screen.width > Screen.height ? new Vector3(2.4f, 3, 0) : new Vector3(0, 1.5f, 0));
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
            if (loading) return;
            if (panel=="")
            {
                if (i==0)
                {
                    if (!Application.CanStreamedLevelBeLoaded("VisualPrototype")) { error="Add VisualPrototype to the Build Profile scene list."; return; }
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
            var overlay = new GameObject("Roll into run transition").AddComponent<RunEntryTransition>();
            var operation = SceneManager.LoadSceneAsync("VisualPrototype");
            operation.allowSceneActivation = false;
            Vector3 start = sleeper.localPosition;
            Quaternion rotation = sleeper.localRotation;
            Quaternion upright = Quaternion.Euler(0,0,-90); // Head (-X) becomes up; face remains toward -Z.
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
                Vector3 landing = new Vector3(1.5f,.7f,-1.5f);
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
                    sleeper.localPosition=landing+new Vector3(0,Mathf.Abs(Mathf.Sin(roll*Mathf.PI*3))*.12f,-4.5f*roll);
                    sleeper.localRotation=Quaternion.AngleAxis(-720*roll,Vector3.right)*upright;
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
                // Orbit from the front to the chase side while travel stays toward -Z.
                // The run uses +Z, but the same camera-relative forward direction.
                float chase = Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.65f)/1.75f));
                Vector3 orbit = Quaternion.Euler(0,180*chase,0)*new Vector3(0,5,-8);
                Vector3 feet = sleeper.localPosition-Vector3.up*.7f;
                view.transform.position=Vector3.Lerp(cameraStart,feet+orbit,chase);
                Vector3 aim=feet+new Vector3(0,1,Mathf.Lerp(0,-6,chase));
                Quaternion look=Quaternion.LookRotation(aim-view.transform.position);
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
            overlay.Finish(operation);
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
            if(error!="") GUI.Label(new Rect(24,Height-70,Width-48,64),error,text);
            GUI.matrix=previous;
        }
        void DrawButton(int i,string label)
        {
            GUI.enabled=!loading;
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
        void BuildWorld()
        {
            view=Camera.main;view.orthographic=true;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.13f,.23f,.22f);
            var bark=Mat("Cherry bark",new Color(.25f,.13f,.1f));var pink=Mat("Blossom",new Color(1,.54f,.67f));var pale=Mat("Petal tips",new Color(1,.8f,.83f));
            var green=Mat("Bamboo",new Color(.19f,.37f,.16f));var leaf=Mat("Leaves",new Color(.29f,.48f,.22f));var ivory=Mat("Panda ivory",new Color(.92f,.91f,.82f));var black=Mat("Panda ink",new Color(.055f,.06f,.065f));var red=Mat("Headband",new Color(.75f,.06f,.045f));
            Shape(PrimitiveType.Cylinder,"Mossy clearing",transform,new Vector3(0,-.4f,0),new Vector3(18,.4f,14),green);
            Branch(transform,new Vector3(-2,0,0),new Vector3(-1.7f,5.2f,.4f),.75f,bark);
            Branch(transform,new Vector3(-1.8f,2.4f,0),new Vector3(1.5f,3.1f,0),.48f,bark);
            Branch(transform,new Vector3(-1.7f,3.4f,0),new Vector3(-4,5.5f,1),.35f,bark);
            Branch(transform,new Vector3(-1.7f,4.5f,.3f),new Vector3(1.1f,6.2f,1),.3f,bark);
            var rng=new System.Random(7301);
            for(int i=0;i<95;i++)
            {
                float x=(float)rng.NextDouble()*7-4.5f, y=5+(float)rng.NextDouble()*2, z=(float)rng.NextDouble()*3;
                Shape(PrimitiveType.Sphere,"Cherry blossom cluster",transform,new Vector3(x,y,z),new Vector3(.65f,.48f,.6f),i%3==0 ? pale : pink);
            }
            for(int i=0;i<24;i++)
            {
                float x=i<12 ? -5.5f+(i%4)*.45f : 3.5f+(i%4)*.45f,z=2+i%6;
                float h=4+i%5*.6f;
                Branch(transform,new Vector3(x,0,z),new Vector3(x+.3f,h,z),.09f,green);
                for(int j=0;j<3;j++)
                {
                    var l=Shape(PrimitiveType.Sphere,"Bamboo leaf spray",transform,new Vector3(x+(j%2==0 ? .4f : -.4f),h-j*.8f,z),new Vector3(1.4f,.15f,.5f),leaf);l.localRotation=Quaternion.Euler(0,j*55,j%2==0 ? 20 : -20);
                }
            }
            sleeper=new GameObject("Sleeping red-headband panda").transform;sleeper.SetParent(transform,false);sleeper.localPosition=new Vector3(-.5f,3.3f,-.3f);sleeper.localRotation=Quaternion.Euler(0,-12,-8);
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
            Shape(PrimitiveType.Sphere,"Overflowing loot pouch",transform,new Vector3(.4f,.5f,-1.5f),new Vector3(1.1f,1.1f,.9f),bag);
            Shape(PrimitiveType.Cylinder,"Pouch mouth",transform,new Vector3(.4f,1,-1.5f),new Vector3(.72f,.1f,.66f),bark);
            var art=Resources.Load<GameArt>("Presentation/GameArt");
            Color[] colors={new Color(.04f,.65f,1),new Color(.95f,.06f,.12f),new Color(.15f,.9f,.32f)};
            for(int i=0;i<18;i++)
            {
                var gem=Shape(PrimitiveType.Cube,"Loot gem",transform,new Vector3(.4f+(float)rng.NextDouble()*.8f-.4f,i<10 ? 1.05f+(float)rng.NextDouble()*.3f : .18f,-1.5f+(float)rng.NextDouble()*1.5f-.75f),Vector3.one*.24f,Mat("Gem",colors[i%3]));
                if(art!=null&&art.Diamond!=null)
                {
                    gem.GetComponent<MeshFilter>().sharedMesh=art.Diamond;
                    float extent=Mathf.Max(art.Diamond.bounds.size.x,Mathf.Max(art.Diamond.bounds.size.y,art.Diamond.bounds.size.z));
                    gem.localScale=Vector3.one*(.28f/Mathf.Max(.001f,extent));
                }
                gem.localRotation=Quaternion.Euler(15,i*37,45);
            }
            var petals=new GameObject("Drifting blossom petals");petals.transform.SetParent(transform,false);petals.transform.localPosition=new Vector3(-1,7,0);
            var ps=petals.AddComponent<ParticleSystem>();var main=ps.main;main.maxParticles=48;main.startLifetime=7;main.startSpeed=.1f;main.startSize=.07f;main.startColor=new Color(1,.7f,.8f);main.gravityModifier=.025f;
            var emission=ps.emission;emission.rateOverTime=5;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(7,.2f,3);
            var pm=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));materials.Add(pm);pm.SetColor("_BaseColor",new Color(1,.7f,.8f));ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=pm;
        }
        void OnDestroy(){foreach(var m in materials) Destroy(m);}
    }
}


