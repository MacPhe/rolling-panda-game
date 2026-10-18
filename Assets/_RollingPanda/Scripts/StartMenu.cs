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
        AudioSource menuMusic;
        ShopPandaPreview shopPreview;
        Camera view;
        RunEntryTransition entrance;
        RunnerCamera chaseCamera;
        VisualPrototype worldArt;
        RunnerMotor gameRunner;
        bool ready;int selectedItem;string shopMessage="";
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
#if !UNITY_EDITOR && !UNITY_STANDALONE
        int draggingSlider=-1;
#endif
        static readonly string[] AudioLabels={"Overall","Music","Sound effects","Ambient sounds"};
        float menuEnvelope;
        float Width, Height, Scale;
        Rect safe;
        bool Wide => Width > Height * 1.2f;
        float Left => Wide ? Width * .61f : 24;
        float PanelWidth => Wide ? Width * .35f : Width - 48;
        float Top => panel=="Shop" ? Mathf.Max(18,Mathf.Min(Wide ? Height*.12f : Height*.36f,Height-452)) : panel=="Settings" ? Mathf.Max(18,Mathf.Min(Wide ? Height*.12f : Height*.38f,Height-400)) : Wide ? Height * .24f : Height * .49f;

        void Awake()
        {
            font = Resources.Load<Font>("Presentation/alagard");
            GameAudioSettings.Get(0);
            gameObject.AddComponent<ForestAmbience>();
            RunnerInput.Reset();
            entrance=new GameObject("Forest entrance hold").AddComponent<RunEntryTransition>();
            environment=Resources.Load<EnvironmentArt>("Presentation/EnvironmentArt");
            BuildWorld();
            var audio=Resources.Load<MovementAudioArt>("Presentation/MovementAudioArt");
            if(audio!=null && audio.MenuShrine!=null)
            {
                var musicObject=new GameObject("Soft looping shrine music");musicObject.transform.SetParent(transform,false);
                menuMusic=musicObject.AddComponent<AudioSource>();menuMusic.playOnAwake=false;menuMusic.spatialBlend=0;menuMusic.loop=true;menuMusic.volume=0;menuMusic.clip=audio.MenuShrine;
                musicObject.AddComponent<AudioLowPassFilter>().cutoffFrequency=1800;
                menuMusic.Play();
            }
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
            GetComponent<ForestAmbience>().Bind(gameRunner);
            foreach(var wind in GetComponentsInChildren<ForestWind>()) wind.Bind(gameRunner);
            yield return null;
            yield return null;
            ready=gameRunner!=null && worldArt.Panda!=null;
            if(ready)
            {
                worldArt.enabled=false;
                Destroy(sleeper.gameObject);sleeper=worldArt.Panda;sleeper.gameObject.SetActive(true);
                sleeper.localPosition=new Vector3(-3.6f,3.15f,-.3f);sleeper.localRotation=Quaternion.Euler(0,180,90);
                sleeper.GetComponent<PandaLookStudy>().SetSleeping(true);
            }
            if(ready)
            {
                var display=new GameObject("Shop character preview");display.transform.SetParent(transform,false);
                display.transform.localPosition=new Vector3(-4.3f,.1f,-4);
                shopPreview=display.AddComponent<ShopPandaPreview>();shopPreview.Initialize(worldArt.Panda);
                display.SetActive(false);
            }
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
            menuEnvelope=Mathf.MoveTowards(menuEnvelope,loading ? 0 : .28f,Time.unscaledDeltaTime*.4f);
            if(menuMusic!=null)menuMusic.volume=menuEnvelope*GameAudioSettings.Music;
            Layout();
            if(shopPreview!=null)
            {
                bool shopping=panel=="Shop" && !loading;
                shopPreview.gameObject.SetActive(shopping);sleeper.gameObject.SetActive(!shopping);
            }
            if (!loading) sleeper.localScale = new Vector3(1, 1 + Mathf.Sin(Time.unscaledTime * 1.5f) * .018f, 1);
            if (!loading)
            {
            view.orthographicSize = Screen.width > Screen.height ? 5.4f : 8.2f;
            view.transform.position = Screen.width > Screen.height ? new Vector3(2, 5.8f, -13) : new Vector3(0, 7.5f, -14);
            view.transform.LookAt(Screen.width > Screen.height ? new Vector3(-2.6f, 3, 0) : new Vector3(-4.5f, 1.5f, 0));
                if(panel=="Shop")
                {
                    view.orthographicSize=Wide ? 2.6f : 3.8f;
                    view.transform.position=new Vector3(-3.2f,2.5f,-12);
                    view.transform.LookAt(Wide ? new Vector3(-2.8f,1.7f,-4) : new Vector3(-4.3f,-1.35f,-4));
                }
            }
#if !UNITY_EDITOR && !UNITY_STANDALONE
            var touch = Touchscreen.current;
            if(loading)return;
            if(touch==null){pressed=-1;draggingSlider=-1;return;}
            var t = touch.primaryTouch;
            if(t.phase.ReadValue()==UnityEngine.InputSystem.TouchPhase.Canceled){pressed=-1;draggingSlider=-1;GameAudioSettings.Save();return;}
            Vector2 p = new Vector2((t.position.ReadValue().x-safe.x)/Scale, (safe.yMax-t.position.ReadValue().y)/Scale);
            if(panel=="Settings")
            {
                if(t.press.wasPressedThisFrame)for(int i=0;i<4;i++)if(SliderHit(i).Contains(p)){draggingSlider=i;pressed=-1;break;}
                if(draggingSlider>=0)
                {
                    Rect rail=SliderRail(draggingSlider);GameAudioSettings.Set(draggingSlider,(p.x-rail.x)/rail.width);
                    if(t.press.wasReleasedThisFrame){draggingSlider=-1;GameAudioSettings.Save();}
                    return;
                }
            }
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
            if(panel=="Settings")return Row(4).Contains(p) ? 4 : -1;
            if(panel=="Shop"){for(int i=0;i<7;i++)if(ShopRect(i).Contains(p))return i;return -1;}
            for (int i=0; i<(panel=="" ? 4 : panel=="Shop" ? 4 : 1); i++)
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
                else
                {
                    panel = i==1 ? "Settings" : i==2 ? "Leaderboards" : "Shop";
                    if(panel=="Shop"){shopMessage="";shopPreview.Preview(ShopCatalog.Items[selectedItem].Id);}
                }
            }
            else if (panel=="Settings")
            {GameAudioSettings.Save();panel="";}
            else if(panel=="Shop")
            {
                if(i<2)
                {
                    selectedItem=(selectedItem+(i==0 ? -1 : 1)+ShopCatalog.Items.Length)%ShopCatalog.Items.Length;
                    shopMessage="";shopPreview.Preview(ShopCatalog.Items[selectedItem].Id);
                }
                else if(i==2)
                {
                    var result=PlayerProfile.BuyOrEquip(ShopCatalog.Items[selectedItem].Id);
                    shopMessage=result==ShopResult.NotEnough ? "Not enough green gems" : result==ShopResult.Purchased ? "Unlocked and equipped!" : "Equipped";
                    shopPreview.Preview(ShopCatalog.Items[selectedItem].Id);
                }
                else if(i==3)shopPreview.Turn(-45);
                else if(i==4)shopPreview.Turn(45);
                else if(i==5)shopPreview.TogglePose();
                else {shopPreview.Preview(null);panel="";}

            }
            else panel="";
        }
        IEnumerator RollIntoRun()
        {
            var pursuit=worldArt.GetComponent<PoacherPursuit>();
            if(pursuit!=null)yield return pursuit.Opening(sleeper);
            var look=sleeper.GetComponent<PandaLookStudy>();look.SetSleeping(false);
            Transform head=sleeper.Find("Head tuck pivot"),left=sleeper.Find("Left arm pivot"),right=sleeper.Find("Right arm pivot"),lf=sleeper.Find("Left leg pivot"),rf=sleeper.Find("Right leg pivot");
            Vector3 start=sleeper.localPosition,landing=new Vector3(-3,0,-4.5f),cameraStart=view.transform.position;
            Quaternion rest=sleeper.localRotation,cameraRotation=view.transform.rotation;
            Matrix4x4 menuProjection=view.projectionMatrix;
            float elapsed=0;
            while(elapsed<3.1f)
            {
                elapsed=Mathf.Min(3.1f,elapsed+Mathf.Min(Time.unscaledDeltaTime,.05f));
                float fall=PoacherRules.Progress(elapsed,.3f,1.2f),roll=Mathf.Clamp01((elapsed-1.2f)/1.9f);
                float travel=roll*roll*(2-roll); // Accelerate out of the landing rather than stop at the handoff.
                float tuck=PoacherRules.Progress(elapsed,.4f,1.05f)*(1-PoacherRules.Progress(elapsed,2.75f,3.1f));
                Vector3 feet=elapsed<1.2f ? Vector3.Lerp(start,landing,fall)+Vector3.up*Mathf.Sin(fall*Mathf.PI)*.18f : Vector3.Lerp(landing,Vector3.zero,travel);
                Quaternion rotation=elapsed<1.2f ? Quaternion.Slerp(rest,Quaternion.identity,fall) : Quaternion.Euler(720*travel,0,0);
                Vector3 pivot=new Vector3(0,.9f,0);
                sleeper.localScale=Vector3.one;sleeper.localRotation=rotation;
                sleeper.localPosition=elapsed<1.2f ? feet : feet+new Vector3(0,.9f-.22f*tuck,0)-rotation*pivot;
                head.localPosition=new Vector3(0,1.6f-.55f*tuck,.12f*tuck);head.localRotation=Quaternion.Euler(45*tuck,0,0);
                left.localPosition=new Vector3(-.48f+.18f*tuck,1.15f-.12f*tuck,.15f*tuck);right.localPosition=new Vector3(.48f-.18f*tuck,1.15f-.12f*tuck,.15f*tuck);
                left.localRotation=Quaternion.Euler(-145*tuck,0,-12);right.localRotation=Quaternion.Euler(-145*tuck,0,12);
                lf.localPosition=new Vector3(-.23f,.49f+.25f*tuck,.18f*tuck);rf.localPosition=new Vector3(.23f,.49f+.25f*tuck,.18f*tuck);
                lf.localRotation=Quaternion.Euler(-150*tuck,0,-3);rf.localRotation=Quaternion.Euler(-150*tuck,0,3);
                if(pursuit!=null)pursuit.EscapeFrame(elapsed);
                float chase=PoacherRules.Progress(elapsed,.2f,2.9f);
                Vector3 centre=sleeper.TransformPoint(new Vector3(0,.9f,0));
                Vector3 cameraEnd=new Vector3(centre.x*.3f,5,centre.z-8);
                view.transform.position=Vector3.Lerp(cameraStart,cameraEnd,chase);
                Quaternion aim=Quaternion.LookRotation(Vector3.Lerp(centre,gameRunner.transform.position+new Vector3(0,1,6),PoacherRules.Progress(elapsed,2.45f,3.1f))-view.transform.position);
                view.transform.rotation=Quaternion.Slerp(cameraRotation,aim,chase);
                view.orthographic=false;view.fieldOfView=60;
                Matrix4x4 perspective=Matrix4x4.Perspective(60,view.aspect,view.nearClipPlane,view.farClipPlane),projection=menuProjection;
                for(int cell=0;cell<16;cell++)projection[cell]=Mathf.Lerp(menuProjection[cell],perspective[cell],chase);view.projectionMatrix=projection;
                yield return null;
            }
            sleeper.localPosition=Vector3.zero;sleeper.localRotation=Quaternion.identity;sleeper.localScale=Vector3.one;
            view.ResetProjectionMatrix();chaseCamera.enabled=true;worldArt.enabled=true;
            if(pursuit!=null)pursuit.FinishEscape();
            entrance.FinishSeamless();enabled=false;
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
            GUI.Box(new Rect(Left-12,Top-8,PanelWidth+24,panel=="Shop" ? 440 : panel=="" || panel=="Settings" ? 390 : 290),"");
            GUI.Label(new Rect(Left,Top,PanelWidth,52),panel=="" ? "Rolling Panda" : panel,title);
            if(panel=="")
            {
                GUI.Label(new Rect(Left,Top+50,PanelWidth,24),loading ? "Waking up the forest..." : "Rest. Roll. Repeat.",text);
                DrawButton(0,"Roll / Play"); DrawButton(1,"Settings"); DrawButton(2,"Leaderboards"); DrawButton(3,"Shop");
            }
            else if(panel=="Settings")
            {
                for(int i=0;i<4;i++)
                {
                    float level=GameAudioSettings.Get(i);
                    GUI.Label(new Rect(Left,Top+53+i*58,PanelWidth,26),AudioLabels[i]+"  "+Mathf.RoundToInt(level*100)+"%",text);
#if UNITY_EDITOR || UNITY_STANDALONE
                    float next=GUI.HorizontalSlider(SliderRail(i),level,0,1);
                    if(next!=level)GameAudioSettings.Set(i,next);
#else
                    GUI.HorizontalSlider(SliderRail(i),level,0,1);
#endif
                }
                if(Event.current.type==EventType.MouseUp)GameAudioSettings.Save();
                DrawButton(4,"Back");
            }
            else if(panel=="Shop")
            {
                var item=ShopCatalog.Items[selectedItem];var inventory=PlayerProfile.Inventory;
                GUI.Label(new Rect(Left,Top+48,PanelWidth,27),"Green gems: "+inventory.Green,text);
                GUI.Label(new Rect(Left,Top+78,PanelWidth,30),item.Name,text);
                GUI.Label(new Rect(Left,Top+108,PanelWidth,27),shopMessage!="" ? shopMessage : item.Rarity+" | "+item.Slot+" | "+(inventory.Owns(item.Id) ? "Owned" : item.Price+" green"),text);
                ShopButton(0,"Previous");ShopButton(1,"Next");
                bool equipped=inventory.Equipped(item.Slot)==item.Id;
                ShopButton(2,equipped ? "Equipped" : inventory.Owns(item.Id) ? "Equip" : "Buy - "+item.Price+" green");
                ShopButton(3,"Turn left");ShopButton(4,"Turn right");ShopButton(5,shopPreview.PoseLabel);ShopButton(6,"Back");

            }
            else
            {
                string detail=panel=="Leaderboards" ? "Online leaderboards are coming later.\nPersonal best: "+PlayerPrefs.GetString("RollingPanda.BestScore","0") : "The shop is coming later.\nPouches will become currency as we develop the game.";
                GUI.Label(new Rect(Left,Top+144,PanelWidth,118),detail,text);
                DrawButton(0,"Back");
            }

            GUI.matrix=previous;
        }
        Rect ShopRect(int i)
        {
            if(i==0 || i==1 || i==3 || i==4){bool right=i==1 || i==4;return new Rect(Left+(right ? (PanelWidth+8)/2 : 0),Top+(i<2 ? 144 : 256),(PanelWidth-8)/2,46);}
            return new Rect(Left,Top+(i==2 ? 200 : i==5 ? 312 : 368),PanelWidth,46);
        }
        void ShopButton(int i,string label)
        {
            bool clicked=GUI.Button(ShopRect(i),label,button);
#if UNITY_EDITOR || UNITY_STANDALONE
            if(clicked)Activate(i);
#endif
        }
        Rect SliderRail(int i)=>new Rect(Left+12,Top+84+i*58,PanelWidth-24,20);
        Rect SliderHit(int i)=>new Rect(Left,Top+78+i*58,PanelWidth,40);
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
            if(height>=6 || prefab.name.StartsWith("Bush")) wrapper.gameObject.AddComponent<ForestWind>().Initialize(gameRunner,height>=6 ? .65f : 1.2f,yaw*.03f);
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
                var clump=new GameObject("Swaying bamboo").transform;clump.SetParent(transform,false);clump.localPosition=new Vector3(x,0,z);
                clump.gameObject.AddComponent<ForestWind>().Initialize(gameRunner,1.2f,i*.7f);
                Branch(clump,Vector3.zero,new Vector3(.3f,h,0),.09f,green);
                for(int node=1;node<h;node++) Shape(PrimitiveType.Cylinder,"Bamboo node",clump,new Vector3(.3f*node/h,node,0),new Vector3(.115f,.025f,.115f),leaf);
                for(int j=0;j<6;j++)
                {
                    var l=Shape(PrimitiveType.Cube,"Pointed bamboo foliage",clump,new Vector3(j%2==0 ? .4f : -.4f,h-j*.8f,0),new Vector3(.85f,.025f,.12f),leaf);l.GetComponent<MeshFilter>().sharedMesh=BambooBlade();l.localScale=Vector3.one*.85f;l.localRotation=Quaternion.Euler(0,j*55,j%2==0 ? 20 : -20);
                }
            }
            sleeper=new GameObject("Sleeping red-headband panda").transform;sleeper.SetParent(transform,false);sleeper.localPosition=new Vector3(-4.5f,3.3f,-.3f);sleeper.localRotation=Quaternion.Euler(0,-12,-8);
            Shape(PrimitiveType.Sphere,"Body",sleeper,new Vector3(.4f,0,0),new Vector3(1.7f,.95f,1),ivory);
            Shape(PrimitiveType.Sphere,"Shoulders",sleeper,new Vector3(-.1f,0,0),new Vector3(.65f,.9f,1.02f),black);
            for(int i=0;i<2;i++)
                Shape(PrimitiveType.Sphere,"Relaxed paw",sleeper,new Vector3(-.28f+i*1.25f,-.36f,-.42f),new Vector3(.48f,.65f,.5f),black);
            var sleepingHead=new GameObject("Head").transform;sleepingHead.SetParent(sleeper,false);
            sleepingHead.localPosition=new Vector3(-.7f,.12f,-.17f);
            // Match the horizontal resting body; the landing rotation cancels this tilt.
            sleepingHead.localRotation=Quaternion.Euler(0,0,90)*Quaternion.Euler(0,180,0);
            var sleepingLook=sleeper.gameObject.AddComponent<PandaLookStudy>();
            if(sleepingLook.BuildGameplay(sleepingHead,sleeper))sleepingLook.SetSleeping(true);
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
        void OnDisable(){PlayerProfile.Save();GameAudioSettings.Save();if(menuMusic!=null)menuMusic.Stop();}
        void OnDestroy()
        {
            foreach(var m in materials) Destroy(m);
            foreach(var mesh in meshes) Destroy(mesh);
            if(entrance!=null) Destroy(entrance.gameObject);
        }
    }
}






