using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RollingPanda
{
    // Cosmetic pursuit and a replay-safe capture vignette; it does not change failure rules.
    [DefaultExecutionOrder(200)]
    public sealed class PoacherPursuit : MonoBehaviour
    {
        sealed class Actor
        {
            public Transform Root,Torso,Head,LeftArm,RightArm,LeftElbow,RightElbow,LeftHand,RightHand,LeftLeg,RightLeg,LeftKnee,RightKnee,LeftFoot,RightFoot,CarryNet,Tool;
        }
        struct BonePose {public Transform Bone;public Vector3 Position;public Quaternion Rotation;}
        readonly List<Mesh> detailMeshes=new List<Mesh>();
        readonly List<Texture2D> textures=new List<Texture2D>();
        readonly List<Transform> openEyes=new List<Transform>(),closedEyes=new List<Transform>();
        readonly BonePose[] capturedBones=new BonePose[5];
        readonly Vector3[] captureStarts=new Vector3[2];
        readonly Quaternion[] captureRotations=new Quaternion[2];
        Vector3 pandaStart,pandaScale,netRelease,dartRelease;Quaternion pandaRotation;Quaternion netRotation=Quaternion.identity;
        float previousRunnerX;bool chaseStarted,eyesSleeping;
        Texture2D weave,leather;Material secondCoat,seams;
        readonly Actor[] actors=new Actor[2];
        readonly List<Material> materials=new List<Material>();
        RunnerMotor runner;Transform panda,stage,captive,net,dart;
        Camera view;RunnerCamera chase;
        Transform captiveHead,leftArm,rightArm,leftLeg,rightLeg;
        AudioSource effects,voices;AudioClip[] sounds;Mesh ropeMesh;
        float age,gait,lag,cueClock=9;bool caught,opening,netPlayed,dartPlayed,oldChaseEnabled,pausedSounds;
        Vector3 cameraStart,captureCameraOffset;Quaternion cameraRotation;
        readonly RaycastHit[] cameraHits=new RaycastHit[32];
        public bool CaptureComplete=>caught && PoacherRules.Complete(age);
        Material cloth,skin,boots,rope,metal;
        public void Initialize(RunnerMotor player,Transform character)
        {
            runner=player;panda=character;view=Camera.main;chase=view==null ? null : view.GetComponent<RunnerCamera>();
            BuildTextures();
            cloth=Mat(new Color(.3f,.36f,.21f));skin=Mat(new Color(.67f,.46f,.3f));boots=Mat(new Color(.12f,.1f,.08f));rope=Mat(new Color(.8f,.63f,.32f));metal=Mat(new Color(.22f,.55f,.56f));
            cloth.name="Woven olive canvas";cloth.SetTexture("_BaseMap",weave);cloth.SetTextureScale("_BaseMap",new Vector2(3,3));
            secondCoat=new Material(cloth);secondCoat.name="Woven ochre canvas";secondCoat.color=new Color(.45f,.32f,.2f);materials.Add(secondCoat);
            boots.SetTexture("_BaseMap",leather);rope.SetTexture("_BaseMap",weave);rope.SetTextureScale("_BaseMap",new Vector2(4,4));
            seams=Mat(new Color(.58f,.47f,.29f));previousRunnerX=runner.transform.position.x;
            stage=new GameObject("Poacher chase and capture").transform;stage.SetParent(runner.transform,false);
            ropeMesh=BuildNet();
            for(int i=0;i<2;i++)actors[i]=BuildActor(i);
            var look=panda.GetComponent<PandaLookStudy>();if(look!=null)look.SetSleeping(true);
            captive=Instantiate(panda,stage,false);captive.name="Captured panda visual";
            foreach(var study in captive.GetComponentsInChildren<PandaLookStudy>(true)){study.enabled=false;Destroy(study);}
            if(look!=null)look.SetSleeping(false);
            captiveHead=captive.Find("Head tuck pivot");leftArm=captive.Find("Left arm pivot");rightArm=captive.Find("Right arm pivot");leftLeg=captive.Find("Left leg pivot");rightLeg=captive.Find("Right leg pivot");
            foreach(var part in captive.GetComponentsInChildren<Transform>(true))
            {
                string name=part.name;if(name=="Eye white" || name=="Amber iris" || name=="Pupil" || name=="Eye catchlight" || name=="Soft upper eyelid")openEyes.Add(part);
                if(name=="Sleeping eyelid")closedEyes.Add(part);
            }
            var bones=new[]{captiveHead,leftArm,rightArm,leftLeg,rightLeg};
            for(int i=0;i<capturedBones.Length;i++)capturedBones[i].Bone=bones[i];
            ResetCaptive();SetEyes(false);captive.gameObject.SetActive(false);
            net=new GameObject("Thrown capture net").transform;net.SetParent(stage,false);net.gameObject.AddComponent<MeshFilter>().sharedMesh=BuildNet(true);net.gameObject.AddComponent<MeshRenderer>().sharedMaterial=rope;net.gameObject.SetActive(false);
            dart=new GameObject("Small sleep dart cue").transform;dart.SetParent(stage,false);
            Part(dart,"Dart shaft",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.018f,.13f,.018f),boots).localRotation=Quaternion.Euler(90,0,0);
            Part(dart,"Turquoise dart feather",PrimitiveType.Cube,new Vector3(0,0,-.12f),new Vector3(.11f,.035f,.08f),metal);dart.gameObject.SetActive(false);
            effects=SoundSource("Twig and capture effects",.16f,4500);voices=SoundSource("Muffled poacher murmurs",.12f,1300);
            sounds=new[]{BuildSound(0),BuildSound(1),BuildSound(2),BuildSound(3)};
            runner.Session.Pursuit=this;stage.gameObject.SetActive(false);
        }
        Material Mat(Color colour){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=colour;m.SetFloat("_Smoothness",.16f);m.enableInstancing=true;materials.Add(m);return m;}
        Transform Part(Transform parent,string name,PrimitiveType kind,Vector3 pos,Vector3 size,Material material)
        {
            var go=GameObject.CreatePrimitive(kind);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=size;
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;
        }
        Transform Pivot(Transform parent,string name,Vector3 position)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;return go.transform;}
        Actor BuildActor(int index)
        {
            var actor=new Actor{Root=Pivot(stage,index==0 ? "Net poacher" : "Dart poacher",Vector3.zero)};var coat=index==0 ? cloth : secondCoat;
            actor.Torso=Pivot(actor.Root,"Breathing torso",new Vector3(0,1.1f,0));
            Part(actor.Torso,"Tailored coat",PrimitiveType.Capsule,new Vector3(0,-.08f,0),new Vector3(.57f,.4f,.38f),coat);
            Part(actor.Torso,"Hem",PrimitiveType.Cylinder,new Vector3(0,-.43f,0),new Vector3(.5f,.025f,.34f),seams);
            Part(actor.Torso,"Belt",PrimitiveType.Cube,new Vector3(0,-.27f,.17f),new Vector3(.51f,.07f,.07f),boots);
            Part(actor.Torso,"Buckle",PrimitiveType.Cube,new Vector3(0,-.27f,.214f),new Vector3(.09f,.07f,.02f),seams);
            foreach(float side in new[]{-1f,1f})
            {
                Part(actor.Torso,"Chest pocket",PrimitiveType.Cube,new Vector3(side*.15f,.04f,.182f),new Vector3(.17f,.2f,.035f),coat);
                Part(actor.Torso,"Pocket flap seam",PrimitiveType.Cube,new Vector3(side*.15f,.12f,.204f),new Vector3(.18f,.025f,.02f),seams);
                Part(actor.Torso,"Pack strap",PrimitiveType.Cube,new Vector3(side*.22f,.03f,.16f),new Vector3(.045f,.55f,.06f),boots).localRotation=Quaternion.Euler(0,0,side*8);
                Part(actor.Torso,"Collar",PrimitiveType.Cube,new Vector3(side*.12f,.28f,.15f),new Vector3(.14f,.13f,.05f),seams).localRotation=Quaternion.Euler(0,0,side*25);
            }
            Part(actor.Torso,"Canvas pack",PrimitiveType.Capsule,new Vector3(0,-.02f,-.29f),new Vector3(.47f,.3f,.28f),rope);
            Part(actor.Torso,"Pack flap",PrimitiveType.Sphere,new Vector3(0,.14f,-.39f),new Vector3(.46f,.17f,.12f),boots);
            actor.Head=Pivot(actor.Torso,"Head look",new Vector3(0,.53f,0));
            Part(actor.Head,"Face",PrimitiveType.Sphere,Vector3.zero,new Vector3(.36f,.41f,.34f),skin);
            Part(actor.Head,"Nose",PrimitiveType.Sphere,new Vector3(0,-.015f,.17f),new Vector3(.075f,.1f,.08f),skin);
            foreach(float side in new[]{-1f,1f})
            {
                Part(actor.Head,"Ear",PrimitiveType.Sphere,new Vector3(side*.175f,-.015f,0),new Vector3(.075f,.12f,.075f),skin);
                Part(actor.Head,"Eye",PrimitiveType.Sphere,new Vector3(side*.075f,.055f,.155f),new Vector3(.034f,.025f,.02f),boots);
                Part(actor.Head,"Brow",PrimitiveType.Cube,new Vector3(side*.075f,.09f,.154f),new Vector3(.09f,.023f,.025f),boots).localRotation=Quaternion.Euler(0,0,side*12);
            }
            Part(actor.Head,"Cloth scarf",PrimitiveType.Sphere,new Vector3(0,-.1f,.1f),new Vector3(.34f,.16f,.18f),boots);
            Part(actor.Head,"Brimmed hat",PrimitiveType.Cylinder,new Vector3(0,.18f,0),new Vector3(.57f,.018f,.52f),coat);
            Part(actor.Head,"Hat crown",PrimitiveType.Sphere,new Vector3(0,.23f,0),new Vector3(.38f,.19f,.35f),coat);
            Part(actor.Head,"Hat band",PrimitiveType.Cylinder,new Vector3(0,.2f,0),new Vector3(.38f,.023f,.35f),boots);
            actor.LeftArm=Pivot(actor.Torso,"Left shoulder",new Vector3(-.34f,.17f,0));actor.RightArm=Pivot(actor.Torso,"Right shoulder",new Vector3(.34f,.17f,0));
            BuildArm(actor.LeftArm,coat,out actor.LeftElbow,out actor.LeftHand);BuildArm(actor.RightArm,coat,out actor.RightElbow,out actor.RightHand);
            actor.LeftLeg=Pivot(actor.Root,"Left hip",new Vector3(-.16f,.75f,0));actor.RightLeg=Pivot(actor.Root,"Right hip",new Vector3(.16f,.75f,0));
            BuildLeg(actor.LeftLeg,coat,out actor.LeftKnee,out actor.LeftFoot);BuildLeg(actor.RightLeg,coat,out actor.RightKnee,out actor.RightFoot);
            if(index==0)
            {
                Part(actor.RightHand,"Net handle",PrimitiveType.Cylinder,new Vector3(0,.24f,.06f),new Vector3(.032f,.48f,.032f),rope);
                actor.CarryNet=Pivot(actor.RightHand,"Carried net",new Vector3(0,.78f,.06f));actor.CarryNet.localScale=new Vector3(.33f,.33f,.04f);
                var mesh=Pivot(actor.CarryNet,"Hoop mesh",Vector3.zero);mesh.gameObject.AddComponent<MeshFilter>().sharedMesh=ropeMesh;mesh.gameObject.AddComponent<MeshRenderer>().sharedMaterial=rope;
            }
            else
            {
                actor.Tool=Pivot(actor.RightHand,"Aimed dart tube",Vector3.zero);
                Part(actor.Tool,"Dart tube",PrimitiveType.Cylinder,new Vector3(0,0,.25f),new Vector3(.035f,.32f,.035f),boots).localRotation=Quaternion.Euler(90,0,0);
            }
            foreach(var bone in new[]{actor.Torso,actor.Head,actor.LeftArm,actor.RightArm,actor.LeftElbow,actor.RightElbow,actor.LeftHand,actor.RightHand,actor.LeftLeg,actor.RightLeg,actor.LeftKnee,actor.RightKnee,actor.LeftFoot,actor.RightFoot})BatchBone(bone);
            return actor;
        }
        void BuildArm(Transform shoulder,Material coat,out Transform elbow,out Transform hand)
        {
            Part(shoulder,"Upper sleeve",PrimitiveType.Capsule,new Vector3(0,-.15f,0),new Vector3(.18f,.16f,.19f),coat);
            elbow=Pivot(shoulder,"Elbow",new Vector3(0,-.3f,0));Part(elbow,"Lower sleeve",PrimitiveType.Capsule,new Vector3(0,-.12f,0),new Vector3(.16f,.13f,.17f),coat);
            Part(elbow,"Cuff",PrimitiveType.Cylinder,new Vector3(0,-.23f,0),new Vector3(.17f,.025f,.17f),seams);
            hand=Pivot(elbow,"Hand grip",new Vector3(0,-.28f,0));Part(hand,"Hand",PrimitiveType.Sphere,Vector3.zero,new Vector3(.16f,.18f,.12f),skin);
        }
        void BuildLeg(Transform hip,Material coat,out Transform knee,out Transform foot)
        {
            Part(hip,"Trouser thigh",PrimitiveType.Capsule,new Vector3(0,-.19f,0),new Vector3(.22f,.195f,.24f),coat);
            knee=Pivot(hip,"Knee",new Vector3(0,-.38f,0));Part(knee,"Trouser shin",PrimitiveType.Capsule,new Vector3(0,-.19f,0),new Vector3(.18f,.19f,.19f),coat);
            foot=Pivot(knee,"Boot",new Vector3(0,-.38f,0));Part(foot,"Worn boot",PrimitiveType.Cube,new Vector3(0,0,.065f),new Vector3(.22f,.12f,.32f),boots);
            Part(foot,"Sole",PrimitiveType.Cube,new Vector3(0,-.048f,.065f),new Vector3(.23f,.024f,.33f),seams);
        }
        void BatchBone(Transform bone)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();var old=new List<GameObject>();
            foreach(Transform child in bone)
            {
                var filter=child.GetComponent<MeshFilter>();var renderer=child.GetComponent<MeshRenderer>();if(filter==null || renderer==null)continue;
                if(!groups.TryGetValue(renderer.sharedMaterial,out var list)){list=new List<CombineInstance>();groups.Add(renderer.sharedMaterial,list);}
                list.Add(new CombineInstance{mesh=filter.sharedMesh,transform=bone.worldToLocalMatrix*child.localToWorldMatrix});old.Add(child.gameObject);
            }
            foreach(var pair in groups)
            {
                var mesh=new Mesh{name="Merged poacher "+bone.name};mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.UploadMeshData(true);detailMeshes.Add(mesh);
                var surface=Pivot(bone,"Batched "+pair.Key.name,Vector3.zero);surface.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=surface.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=pair.Key;renderer.shadowCastingMode=bone.name=="Breathing torso" ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            foreach(var obj in old){obj.SetActive(false);Destroy(obj);}
        }
        void BuildTextures()
        {
            const int size=256;var fabric=new Color[size*size];var hide=new Color[size*size];var random=new System.Random(1710);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                int i=y*size+x;float grain=(float)random.NextDouble()-.5f;
                float thread=Mathf.Sin(x*Mathf.PI*.5f)*Mathf.Sin(y*Mathf.PI*.5f);
                float broad=Mathf.Sin(x*.075f+y*.04f)*Mathf.Sin(y*.09f)*.035f;
                float value=.9f+grain*.055f+thread*.035f+broad;fabric[i]=new Color(value,value*.99f,value*.96f,1);
                float leatherValue=.78f+grain*.14f+Mathf.Sin(x*.12f)*Mathf.Sin(y*.14f)*.06f;hide[i]=new Color(leatherValue,leatherValue*.96f,leatherValue*.88f,1);
            }
            weave=Texture("Original woven canvas",fabric,false);leather=Texture("Original worn leather",hide,false);
        }
        Texture2D Texture(string name,Color[] pixels,bool linear)
        {
            var texture=new Texture2D(256,256,TextureFormat.RGBA32,true,linear){name=name,wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear};texture.SetPixels(pixels);texture.Apply(true,true);textures.Add(texture);return texture;
        }
        AudioSource SoundSource(string name,float volume,float cutoff)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=false;source.spatialBlend=0;source.volume=volume;go.AddComponent<AudioLowPassFilter>().cutoffFrequency=cutoff;return source;
        }
        public IEnumerator Opening(Transform sleeper)
        {
            opening=true;stage.gameObject.SetActive(true);stage.position=Vector3.zero;effects.PlayOneShot(sounds[0]);
            float t=0;bool spoke=false;Quaternion rest=sleeper.localRotation;
            while(t<1.45f)
            {
                t+=Time.unscaledDeltaTime;float p=PoacherRules.Progress(t,0,1.45f);
                if(!spoke && t>.3f){voices.PlayOneShot(sounds[1]);spoke=true;}
                for(int i=0;i<2;i++)
                {
                    actors[i].Root.localPosition=Vector3.Lerp(new Vector3(-1.6f+i*1.3f,0,3+i),new Vector3(-3.2f+i*1.3f,0,1.6f+i),p);
                    actors[i].Root.localRotation=Quaternion.Euler(0,-35,0);Walk(actors[i],t*9+i,Mathf.Sin(p*Mathf.PI)*.7f+.3f);
                }
                var look=sleeper.GetComponent<PandaLookStudy>();if(look!=null && t>.2f)look.SetSleeping(false);
                sleeper.localRotation=rest*Quaternion.Euler(0,0,Mathf.Sin(p*Mathf.PI)*-5);
                yield return null;
            }
            sleeper.localRotation=rest;opening=false;stage.gameObject.SetActive(false);
        }
        public void Caught()
        {
            if(caught)return;caught=true;age=0;netPlayed=dartPlayed=false;netRotation=Quaternion.identity;netRelease=dartRelease=Vector3.zero;opening=false;effects.Stop();voices.Stop();pausedSounds=false;
            stage.gameObject.SetActive(true);stage.localPosition=new Vector3(0,-runner.transform.position.y,0);stage.localRotation=Quaternion.identity;
            ResetCaptive();
            pandaStart=stage.InverseTransformPoint(panda.position);pandaRotation=Quaternion.Inverse(stage.rotation)*panda.rotation;pandaScale=panda.localScale;
            for(int i=0;i<capturedBones.Length;i++)
            {
                var source=panda.Find(capturedBones[i].Bone.name);capturedBones[i].Position=source.localPosition;capturedBones[i].Rotation=source.localRotation;
            }
            captive.localPosition=pandaStart;captive.localRotation=pandaRotation;captive.localScale=pandaScale;
            captive.gameObject.SetActive(true);panda.gameObject.SetActive(false);SetEyes(false);
            for(int i=0;i<2;i++){captureStarts[i]=actors[i].Root.localPosition;captureRotations[i]=actors[i].Root.localRotation;}
            net.gameObject.SetActive(false);dart.gameObject.SetActive(false);
            if(view!=null){cameraStart=view.transform.position;cameraRotation=view.transform.rotation;captureCameraOffset=CaptureView();}
            if(chase!=null){oldChaseEnabled=chase.enabled;chase.enabled=false;}
        }
        public void ResetPursuit()
        {
            bool wasCaught=caught;caught=false;age=gait=lag=0;cueClock=9;opening=false;chaseStarted=false;previousRunnerX=runner.transform.position.x;
            if(actors[0].CarryNet!=null)actors[0].CarryNet.gameObject.SetActive(true);
            if(actors[1].Tool!=null)actors[1].Tool.localRotation=Quaternion.identity;
            if(stage==null)return;effects.Stop();voices.Stop();pausedSounds=false;net.gameObject.SetActive(false);dart.gameObject.SetActive(false);captive.gameObject.SetActive(false);
            panda.gameObject.SetActive(!RunEntryTransition.Active);
            if(wasCaught && chase!=null){chase.enabled=oldChaseEnabled;if(chase.enabled)chase.RestoreChase();}
        }
        void ResetCaptive()
        {
            captive.localPosition=Vector3.zero;captive.localRotation=Quaternion.identity;captive.localScale=Vector3.one;
            captiveHead.localPosition=new Vector3(0,1.6f,0);captiveHead.localRotation=Quaternion.identity;
            leftArm.localPosition=new Vector3(-.48f,1.15f,0);rightArm.localPosition=new Vector3(.48f,1.15f,0);leftLeg.localPosition=new Vector3(-.23f,.49f,0);rightLeg.localPosition=new Vector3(.23f,.49f,0);
            leftArm.localRotation=Quaternion.Euler(0,0,-12);rightArm.localRotation=Quaternion.Euler(0,0,12);leftLeg.localRotation=rightLeg.localRotation=Quaternion.identity;
        }
        void SetEyes(bool sleepy)
        {
            if(eyesSleeping==sleepy && openEyes.Count>0 && openEyes[0].gameObject.activeSelf==!sleepy)return;
            eyesSleeping=sleepy;foreach(var part in openEyes)part.gameObject.SetActive(!sleepy);foreach(var part in closedEyes)part.gameObject.SetActive(sleepy);
        }
        void LateUpdate()
        {
            if(runner==null)return;
            effects.volume=.16f*GameAudioSettings.Effects;voices.volume=.12f*GameAudioSettings.Effects;
            if(opening)return;
            if(RunEntryTransition.Active && !RunEntryTransition.Revealing){stage.gameObject.SetActive(false);return;}
            stage.gameObject.SetActive(true);stage.localPosition=new Vector3(0,-runner.transform.position.y,0);
            if(runner.Paused && !pausedSounds){effects.Pause();voices.Pause();pausedSounds=true;}
            else if(!runner.Paused && pausedSounds){effects.UnPause();voices.UnPause();pausedSounds=false;}
            if(caught)
            {
                age=PoacherRules.Advance(age,Time.deltaTime,runner.Paused);
                CapturePose();return;
            }
            if(runner.Paused)return;
            if(!chaseStarted)lag=PoacherRules.Gap(runner.DistanceTravelled,runner.InChute);
            gait=Mathf.Repeat(gait+Time.deltaTime*Mathf.Lerp(10,16,Mathf.InverseLerp(17.5f,40,runner.CurrentSpeed)),Mathf.PI*2);lag=Mathf.MoveTowards(lag,PoacherRules.Gap(runner.DistanceTravelled,runner.InChute),Time.deltaTime*2.5f);
            cueClock-=Time.deltaTime;
            if(cueClock<=0 && lag<5 && !runner.InChute){voices.PlayOneShot(sounds[1],.42f);cueClock=12;}

            float dx=runner.transform.position.x-previousRunnerX;previousRunnerX=runner.transform.position.x;
            float follow=PoacherRules.Follow(6,Time.deltaTime),spacing=view!=null && view.aspect<1 ? .95f : 1.65f;
            for(int i=0;i<2;i++)
            {
                Vector3 target=new Vector3(i==0 ? -spacing : spacing,0,-lag-i*.65f);
                Vector3 prior=actors[i].Root.localPosition;prior.x-=dx;
                actors[i].Root.localPosition=chaseStarted ? Vector3.Lerp(prior,target,follow) : target;
                float lean=Mathf.Clamp(dx/Mathf.Max(.001f,Time.deltaTime),-12,12);
                actors[i].Root.localRotation=Quaternion.Slerp(actors[i].Root.localRotation,Quaternion.Euler(0,lean*.8f,0),follow);
                Walk(actors[i],gait+i*1.4f,1);actors[i].Torso.localRotation*=Quaternion.Euler(0,0,-lean*.45f);
            }
            chaseStarted=true;
        }
        void CapturePose()
        {
            float approach=PoacherRules.Progress(age,0,1.25f),throwNet=PoacherRules.Net(age),shot=PoacherRules.Dart(age),sleep=PoacherRules.Sleep(age),settle=PoacherRules.Progress(age,0,.5f);
            for(int i=0;i<2;i++)
            {
                actors[i].Root.localPosition=Vector3.Lerp(captureStarts[i],new Vector3(i==0 ? -1.25f : 1.25f,0,-.65f-i*.3f),approach);
                actors[i].Root.localRotation=Quaternion.Slerp(captureRotations[i],Quaternion.Euler(0,i==0 ? 55 : -50,0),approach);
                Walk(actors[i],gait+age*10+i,1-approach);
            }
            float wind=PoacherRules.Progress(age,.15f,.65f),release=PoacherRules.Progress(age,.65f,1.05f),recover=PoacherRules.Progress(age,1.05f,1.8f);
            actors[0].RightArm.localRotation=Quaternion.Euler(Mathf.Lerp(Mathf.Lerp(-15,-145,wind),-50,release)*(1-recover),0,12);
            actors[0].RightElbow.localRotation=Quaternion.Euler(Mathf.Lerp(-75,-12,release)*(1-recover),0,0);
            actors[0].LeftArm.localRotation=Quaternion.Euler(-35*wind*(1-recover),0,-18);
            actors[0].Torso.localRotation*=Quaternion.Euler(0,-16*wind*(1-release),0);
            float aim=PoacherRules.Progress(age,1.45f,1.85f);
            actors[1].RightArm.localRotation=Quaternion.Euler(-75*aim,0,8);actors[1].RightElbow.localRotation=Quaternion.Euler(-45*aim,0,0);
            actors[1].Head.localRotation=Quaternion.Euler(3,-8*aim,0);
            if(aim>0)actors[1].Tool.rotation=Quaternion.LookRotation(stage.TransformPoint(new Vector3(0,1.15f,0))-actors[1].Tool.position);
            else actors[1].Tool.localRotation=Quaternion.identity;
            // Preserve the defeated pose, then settle it into the net instead of snapping upright.
            captive.localPosition=Vector3.Lerp(pandaStart,Vector3.zero,settle);captive.localRotation=Quaternion.Slerp(pandaRotation,Quaternion.identity,settle);
            captive.localScale=Vector3.Scale(Vector3.Lerp(pandaScale,Vector3.one,settle),new Vector3(1,1-sleep*.08f,1));
            for(int i=0;i<capturedBones.Length;i++)
            {
                var bone=capturedBones[i].Bone;Vector3 target=i==0 ? new Vector3(0,1.6f,0) : i==1 ? new Vector3(-.48f,1.15f,0) : i==2 ? new Vector3(.48f,1.15f,0) : new Vector3(i==3 ? -.23f : .23f,.49f,0);
                Quaternion turn=i==0 ? Quaternion.Euler(sleep*22,-throwNet*8*(1-sleep),sleep*10) : i<=2 ? Quaternion.Euler(-25*(1-sleep),0,i==1 ? -12 : 12) : Quaternion.identity;
                bone.localPosition=Vector3.Lerp(capturedBones[i].Position,target,settle);bone.localRotation=Quaternion.Slerp(capturedBones[i].Rotation,turn,settle);
            }
            if(!runner.Paused && age>=.65f && !netPlayed)
            {effects.PlayOneShot(sounds[2]);netPlayed=true;netRelease=stage.InverseTransformPoint(actors[0].CarryNet.position);netRotation=Quaternion.Inverse(stage.rotation)*actors[0].CarryNet.rotation;actors[0].CarryNet.gameObject.SetActive(false);}
            net.gameObject.SetActive(age>=.65f);
            net.localPosition=Vector3.Lerp(netRelease,new Vector3(0,1.08f,0),throwNet)+Vector3.up*Mathf.Sin(throwNet*Mathf.PI)*.45f;
            float netSettle=PoacherRules.Progress(age,1.4f,2.2f);
            net.localScale=Vector3.Lerp(new Vector3(.35f,.35f,.05f),new Vector3(.86f,1.13f-netSettle*.07f,.78f),throwNet);net.localRotation=Quaternion.Slerp(netRotation,Quaternion.identity,throwNet);
            if(!runner.Paused && age>=1.85f && !dartPlayed)
            {effects.PlayOneShot(sounds[3]);dartPlayed=true;dartRelease=stage.InverseTransformPoint(actors[1].Tool.TransformPoint(new Vector3(0,0,.57f)));}
            dart.gameObject.SetActive(age>=1.85f);
            Vector3 to=stage.InverseTransformPoint(captive.TransformPoint(new Vector3(.38f,1.05f,.08f)));
            dart.localPosition=Vector3.Lerp(dartRelease,to,shot);if((to-dartRelease).sqrMagnitude>.0001f)dart.localRotation=Quaternion.LookRotation(to-dartRelease);
            SetEyes(sleep>.3f);
            if(view!=null)
            {
                Vector3 target=stage.position+new Vector3(0,1,0),desired=stage.position+captureCameraOffset;
                float cameraBlend=PoacherRules.Progress(age,0,.85f);view.transform.position=Vector3.Lerp(cameraStart,desired,cameraBlend);
                view.transform.rotation=Quaternion.Slerp(cameraRotation,Quaternion.LookRotation(target-view.transform.position),cameraBlend);
            }
        }
        Vector3 CaptureView()
        {
            // Prefer the face; try wider front angles if the fatal obstacle blocks the shot.
            Vector3 target=stage.position+Vector3.up;
            for(int candidate=0;candidate<6;candidate++)
            {
                float side=candidate%2==0 ? 1 : -1;
                Vector3 offset=new Vector3(side*(candidate<2 ? 3.8f : candidate<4 ? 5.8f : 6.5f),view.aspect<1 ? 3.2f : 2.65f,candidate<2 ? 5.2f : candidate<4 ? 2.5f : .8f);
                if(view.aspect<1)offset*=1.25f;
                Vector3 direction=stage.position+offset-target;bool blocked=false;
                int count=Physics.SphereCastNonAlloc(target,.18f,direction.normalized,cameraHits,direction.magnitude,~0,QueryTriggerInteraction.Ignore);
                for(int i=0;i<count;i++)if(!cameraHits[i].transform.IsChildOf(runner.transform)){blocked=true;break;}
                if(!blocked)return offset;
            }
            return new Vector3(6.5f,3.2f,.8f);
        }
        static void Walk(Actor actor,float phase,float strength)
        {
            float swing=Mathf.Sin(phase),bob=Mathf.Cos(phase*2)*.018f*strength;
            actor.Torso.localPosition=new Vector3(0,1.1f+bob,0);actor.Torso.localRotation=Quaternion.Euler(strength*7,-swing*4*strength,swing*2*strength);
            actor.Head.localRotation=Quaternion.Euler(-strength*4,swing*2*strength,0);
            actor.LeftArm.localRotation=Quaternion.Euler(-swing*28*strength-10,0,-8);actor.RightArm.localRotation=Quaternion.Euler(swing*28*strength-10,0,8);
            actor.LeftElbow.localRotation=Quaternion.Euler(-35-Mathf.Max(0,swing)*20*strength,0,0);actor.RightElbow.localRotation=Quaternion.Euler(-35-Mathf.Max(0,-swing)*20*strength,0,0);
            Leg(actor,actor.LeftLeg,actor.LeftKnee,actor.LeftFoot,-.16f,phase,strength);
            Leg(actor,actor.RightLeg,actor.RightKnee,actor.RightFoot,.16f,phase+Mathf.PI,strength);
        }
        static void Leg(Actor actor,Transform hip,Transform knee,Transform foot,float side,float phase,float strength)
        {
            float z,lift,pitch;PoacherRules.Step(phase,strength,out z,out lift,out pitch);
            Vector3 target=actor.Root.TransformPoint(new Vector3(side,.065f+lift,z)),delta=target-hip.position;
            float along,height;float distance=PoacherRules.Knee(delta.magnitude,out along,out height);Vector3 direction=delta.normalized;
            target=hip.position+direction*distance;
            Vector3 bend=Vector3.ProjectOnPlane(actor.Root.forward,direction).normalized;
            Vector3 joint=hip.position+direction*along+bend*height;
            hip.rotation=Quaternion.FromToRotation(Vector3.down,joint-hip.position);knee.rotation=Quaternion.FromToRotation(Vector3.down,target-joint);
            foot.rotation=actor.Root.rotation*Quaternion.Euler(pitch,0,0);
        }
        Mesh BuildNet(bool draped=false)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int ring=1;ring<10;ring++)for(int i=0;i<20;i++)
            {float y=ring*Mathf.PI/10,a=i*Mathf.PI*2/20,b=(i+1)*Mathf.PI*2/20;Rope(vertices,triangles,new Vector3(Mathf.Sin(y)*Mathf.Cos(a),Mathf.Cos(y),Mathf.Sin(y)*Mathf.Sin(a)),new Vector3(Mathf.Sin(y)*Mathf.Cos(b),Mathf.Cos(y),Mathf.Sin(y)*Mathf.Sin(b)));}
            for(int line=0;line<12;line++)for(int i=0;i<12;i++)
            {float a=line*Mathf.PI*2/12,y=i*Mathf.PI/12,z=(i+1)*Mathf.PI/12;Rope(vertices,triangles,new Vector3(Mathf.Sin(y)*Mathf.Cos(a),Mathf.Cos(y),Mathf.Sin(y)*Mathf.Sin(a)),new Vector3(Mathf.Sin(z)*Mathf.Cos(a),Mathf.Cos(z),Mathf.Sin(z)*Mathf.Sin(a)));}
            if(draped)for(int i=0;i<vertices.Count;i++)
            {
                Vector3 v=vertices[i];float angle=Mathf.Atan2(v.z,v.x);
                float fold=1+.1f*Mathf.Sin(angle*5+v.y*2);
                v.x*=fold;v.z*=fold;v.y-=.14f*(1-v.y*v.y)*(1+.5f*Mathf.Sin(angle*3));vertices[i]=v;
            }
            var mesh=new Mesh{name=draped ? "Loose folded capture net" : "Pooled capture rope net"};if(draped)detailMeshes.Add(mesh);mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.UploadMeshData(true);return mesh;
        }
        static void Rope(List<Vector3> vertices,List<int> indices,Vector3 a,Vector3 b)
        {
            int first=vertices.Count;var rotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
            for(int end=0;end<2;end++)for(int i=0;i<6;i++){float t=i*Mathf.PI*2/6;vertices.Add((end==0 ? a : b)+rotation*new Vector3(Mathf.Cos(t)*.012f,0,Mathf.Sin(t)*.012f));}
            for(int i=0;i<6;i++){int p=first+i,n=first+(i+1)%6;indices.AddRange(new[]{p,p+6,n,n,p+6,n+6});}
        }
        static AudioClip BuildSound(int kind)
        {
            const int rate=22050;float duration=kind==0 ? .24f : kind==1 ? 1.15f : kind==2 ? .32f : .14f;
            var data=new float[Mathf.CeilToInt(rate*duration)];var random=new System.Random(201+kind);double smooth=0,phase=0;
            for(int i=0;i<data.Length;i++)
            {
                double t=i/(double)rate,u=t/duration,noise=random.NextDouble()*2-1;smooth+=.12*(noise-smooth);
                if(kind==1)
                {
                    double syllable=System.Math.Pow(System.Math.Max(0,System.Math.Sin(u*System.Math.PI*5)),2);phase+=2*System.Math.PI*(105+18*System.Math.Sin(u*13))/rate;
                    data[i]=(float)(.35*syllable*System.Math.Sin(System.Math.PI*u)*(.65*System.Math.Sin(phase)+.23*System.Math.Sin(phase*2)+.1*smooth));
                }
                else if(kind==0){double burst=System.Math.Exp(-t*70)+.6*System.Math.Exp(-System.Math.Abs(t-.055)*120)+.3*System.Math.Exp(-System.Math.Abs(t-.11)*100);data[i]=(float)(.5*noise*burst);}
                else data[i]=(float)(smooth*System.Math.Sin(System.Math.PI*u)*(kind==2 ? .7 : 1.2));
            }
            var clip=AudioClip.Create(kind==0 ? "Twig snap placeholder" : kind==1 ? "Poacher nonverbal murmur placeholder" : kind==2 ? "Net throw swish" : "Soft dart puff",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void OnDisable(){if(caught && runner!=null)ResetPursuit();if(stage!=null)stage.gameObject.SetActive(false);if(effects!=null)effects.Stop();if(voices!=null)voices.Stop();}
        void OnDestroy()
        {
            if(stage!=null)Destroy(stage.gameObject);if(ropeMesh!=null)Destroy(ropeMesh);foreach(var mesh in detailMeshes)Destroy(mesh);foreach(var texture in textures)Destroy(texture);foreach(var material in materials)Destroy(material);if(sounds!=null)foreach(var sound in sounds)Destroy(sound);
        }
    }
}



