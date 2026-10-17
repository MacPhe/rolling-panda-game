using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RollingPanda
{
    // Cosmetic pursuit and a replay-safe capture vignette; it does not change failure rules.
    [DefaultExecutionOrder(200)]
    public sealed class PoacherPursuit : MonoBehaviour
    {
        sealed class Actor {public Transform Root,LeftArm,RightArm,LeftLeg,RightLeg;}
        readonly Actor[] actors=new Actor[2];
        readonly List<Material> materials=new List<Material>();
        RunnerMotor runner;Transform panda,stage,captive,net,dart;
        Camera view;RunnerCamera chase;
        Transform captiveHead,leftArm,rightArm,leftLeg,rightLeg;
        AudioSource effects,voices;AudioClip[] sounds;Mesh ropeMesh;
        float age,gait,lag;bool caught,opening,netPlayed,dartPlayed,oldChaseEnabled,pausedSounds;
        Vector3 cameraStart;Quaternion cameraRotation;
        public bool CaptureComplete=>caught && PoacherRules.Complete(age);
        Material cloth,skin,boots,rope,metal;
        public void Initialize(RunnerMotor player,Transform character)
        {
            runner=player;panda=character;view=Camera.main;chase=view==null ? null : view.GetComponent<RunnerCamera>();
            cloth=Mat(new Color(.22f,.28f,.16f));skin=Mat(new Color(.67f,.46f,.3f));boots=Mat(new Color(.12f,.1f,.08f));rope=Mat(new Color(.8f,.63f,.32f));metal=Mat(new Color(.22f,.55f,.56f));
            stage=new GameObject("Poacher chase and capture").transform;stage.SetParent(runner.transform,false);
            ropeMesh=BuildNet();
            for(int i=0;i<2;i++)actors[i]=BuildActor(i);
            var look=panda.GetComponent<PandaLookStudy>();if(look!=null)look.SetSleeping(true);
            captive=Instantiate(panda,stage,false);captive.name="Captured panda visual";
            foreach(var study in captive.GetComponentsInChildren<PandaLookStudy>(true)){study.enabled=false;Destroy(study);}
            if(look!=null)look.SetSleeping(false);
            captiveHead=captive.Find("Head tuck pivot");leftArm=captive.Find("Left arm pivot");rightArm=captive.Find("Right arm pivot");leftLeg=captive.Find("Left leg pivot");rightLeg=captive.Find("Right leg pivot");
            ResetCaptive();captive.gameObject.SetActive(false);
            net=new GameObject("Thrown capture net").transform;net.SetParent(stage,false);net.gameObject.AddComponent<MeshFilter>().sharedMesh=ropeMesh;net.gameObject.AddComponent<MeshRenderer>().sharedMaterial=rope;net.gameObject.SetActive(false);
            dart=new GameObject("Small sleep dart cue").transform;dart.SetParent(stage,false);
            Part(dart,"Dart shaft",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.018f,.13f,.018f),boots).localRotation=Quaternion.Euler(90,0,0);
            Part(dart,"Turquoise dart feather",PrimitiveType.Cube,new Vector3(0,0,-.12f),new Vector3(.11f,.035f,.08f),metal);dart.gameObject.SetActive(false);
            effects=SoundSource("Twig and capture effects",.16f,4500);voices=SoundSource("Muffled poacher murmurs",.12f,1300);
            sounds=new[]{BuildSound(0),BuildSound(1),BuildSound(2),BuildSound(3)};
            runner.Session.Pursuit=this;stage.gameObject.SetActive(false);
        }
        Material Mat(Color colour){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=colour;materials.Add(m);return m;}
        Transform Part(Transform parent,string name,PrimitiveType kind,Vector3 pos,Vector3 size,Material material)
        {
            var go=GameObject.CreatePrimitive(kind);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=size;
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;
        }
        Actor BuildActor(int index)
        {
            var actor=new Actor{Root=new GameObject(index==0 ? "Net poacher placeholder" : "Dart poacher placeholder").transform};actor.Root.SetParent(stage,false);
            Part(actor.Root,"Field coat",PrimitiveType.Capsule,new Vector3(0,1.02f,0),new Vector3(.58f,.44f,.38f),cloth);
            Part(actor.Root,"Head",PrimitiveType.Sphere,new Vector3(0,1.65f,0),new Vector3(.35f,.4f,.33f),skin);
            Part(actor.Root,"Dark scarf",PrimitiveType.Cube,new Vector3(0,1.55f,.15f),new Vector3(.34f,.14f,.06f),boots);
            Part(actor.Root,"Brimmed cap",PrimitiveType.Cylinder,new Vector3(0,1.84f,0),new Vector3(.56f,.025f,.52f),cloth);
            Part(actor.Root,"Cap crown",PrimitiveType.Sphere,new Vector3(0,1.86f,0),new Vector3(.37f,.17f,.35f),cloth);
            Part(actor.Root,"Pack",PrimitiveType.Cube,new Vector3(0,1.08f,-.26f),new Vector3(.46f,.58f,.26f),rope);
            actor.LeftArm=Limb(actor.Root,"Left arm",new Vector3(-.34f,1.27f,0),new Vector3(.16f,.52f,.17f),cloth);
            actor.RightArm=Limb(actor.Root,"Right arm",new Vector3(.34f,1.27f,0),new Vector3(.16f,.52f,.17f),cloth);
            actor.LeftLeg=Limb(actor.Root,"Left leg",new Vector3(-.16f,.7f,0),new Vector3(.2f,.66f,.24f),boots);
            actor.RightLeg=Limb(actor.Root,"Right leg",new Vector3(.16f,.7f,0),new Vector3(.2f,.66f,.24f),boots);
            if(index==0)
            {
                Part(actor.RightArm,"Net handle",PrimitiveType.Cylinder,new Vector3(.02f,.2f,.12f),new Vector3(.04f,.68f,.04f),rope);
                var hoop=new GameObject("Carried net hoop").transform;hoop.SetParent(actor.RightArm,false);hoop.localPosition=new Vector3(.02f,.98f,.12f);hoop.localScale=new Vector3(.33f,.33f,.04f);
                hoop.gameObject.AddComponent<MeshFilter>().sharedMesh=ropeMesh;hoop.gameObject.AddComponent<MeshRenderer>().sharedMaterial=rope;
            }
            else Part(actor.RightArm,"Dart tube",PrimitiveType.Cylinder,new Vector3(0,-.2f,.18f),new Vector3(.04f,.38f,.04f),boots).localRotation=Quaternion.Euler(90,0,0);
            return actor;
        }
        Transform Limb(Transform parent,string name,Vector3 pos,Vector3 size,Material material)
        {
            var pivot=new GameObject(name).transform;pivot.SetParent(parent,false);pivot.localPosition=pos;Part(pivot,"Clothed limb",PrimitiveType.Capsule,new Vector3(0,-size.y*.5f,0),new Vector3(size.x,size.y*.5f,size.z),material);return pivot;
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
                t+=Time.unscaledDeltaTime;float p=Mathf.Clamp01(t/1.45f);
                if(!spoke && t>.3f){voices.PlayOneShot(sounds[1]);spoke=true;}
                for(int i=0;i<2;i++)
                {
                    actors[i].Root.localPosition=Vector3.Lerp(new Vector3(-.8f+i*1.2f,0,4+i*1.4f),new Vector3(-3.2f+i*1.3f,0,.7f+i),p);
                    actors[i].Root.localRotation=Quaternion.Euler(0,-35,0);Walk(actors[i],t*9+i,1);
                }
                var look=sleeper.GetComponent<PandaLookStudy>();if(look!=null && t>.2f)look.SetSleeping(false);
                sleeper.localRotation=rest*Quaternion.Euler(0,0,Mathf.Sin(p*Mathf.PI)*-5);
                yield return null;
            }
            sleeper.localRotation=rest;opening=false;stage.gameObject.SetActive(false);
        }
        public void Caught()
        {
            if(caught)return;caught=true;age=0;netPlayed=dartPlayed=false;opening=false;effects.Stop();voices.Stop();pausedSounds=false;
            stage.gameObject.SetActive(true);stage.localPosition=new Vector3(0,-runner.transform.position.y,0);stage.localRotation=Quaternion.identity;
            ResetCaptive();captive.gameObject.SetActive(true);panda.gameObject.SetActive(false);SetEyes(false);
            for(int i=0;i<2;i++){actors[i].Root.localPosition=new Vector3(i==0 ? -1.6f : 1.6f,0,-5-i);actors[i].Root.localRotation=Quaternion.identity;}
            net.gameObject.SetActive(false);dart.gameObject.SetActive(false);
            if(view!=null){cameraStart=view.transform.position;cameraRotation=view.transform.rotation;}
            if(chase!=null){oldChaseEnabled=chase.enabled;chase.enabled=false;}
        }
        public void ResetPursuit()
        {
            bool wasCaught=caught;caught=false;age=gait=lag=0;opening=false;
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
            foreach(var part in captive.GetComponentsInChildren<Transform>(true))
            {
                string n=part.name;if(n=="Eye white" || n=="Amber iris" || n=="Pupil" || n=="Eye catchlight" || n=="Soft upper eyelid")part.gameObject.SetActive(!sleepy);
                if(n=="Sleeping eyelid")part.gameObject.SetActive(sleepy);
            }
        }
        void LateUpdate()
        {
            if(runner==null || opening)return;
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
            gait+=Time.deltaTime*11;lag=Mathf.MoveTowards(lag,runner.InChute ? 6 : 0,Time.deltaTime*3);
            for(int i=0;i<2;i++)
            {
                actors[i].Root.localPosition=new Vector3(i==0 ? -1.65f : 1.65f,0,-2.7f-i*.65f-lag);
                actors[i].Root.localRotation=Quaternion.Euler(0,Mathf.Sin(gait*.2f+i)*4,0);Walk(actors[i],gait+i*1.4f,1);
            }
        }
        void CapturePose()
        {
            float approach=PoacherRules.Progress(age,0,1.25f),throwNet=PoacherRules.Net(age),shot=PoacherRules.Dart(age),sleep=PoacherRules.Sleep(age);
            for(int i=0;i<2;i++)
            {
                actors[i].Root.localPosition=Vector3.Lerp(new Vector3(i==0 ? -1.6f : 1.6f,0,-5-i),new Vector3(i==0 ? -1.25f : 1.25f,0,-.65f-i*.3f),approach);
                actors[i].Root.localRotation=Quaternion.Euler(0,i==0 ? 55 : -50,0);Walk(actors[i],age*11+i,1-approach);
            }
            actors[0].RightArm.localRotation=Quaternion.Euler(-120*PoacherRules.Progress(age,.4f,.85f),0,0);
            actors[1].RightArm.localRotation=Quaternion.Euler(-80*PoacherRules.Progress(age,1.5f,1.85f),0,0);
            if(!runner.Paused && age>=.65f && !netPlayed){effects.PlayOneShot(sounds[2]);netPlayed=true;}
            net.gameObject.SetActive(age>=.65f);
            net.localPosition=Vector3.Lerp(new Vector3(-1.2f,1.8f,-1.8f),new Vector3(0,.92f,0),throwNet)+Vector3.up*Mathf.Sin(throwNet*Mathf.PI)*.7f;
            net.localScale=Vector3.Lerp(Vector3.one*.18f,new Vector3(.72f,.96f,.65f),throwNet);net.localRotation=Quaternion.Euler(throwNet*180,0,0);
            if(!runner.Paused && age>=1.85f && !dartPlayed){effects.PlayOneShot(sounds[3]);dartPlayed=true;}
            dart.gameObject.SetActive(age>=1.85f);
            Vector3 from=new Vector3(1.05f,1.35f,-.6f),to=new Vector3(.38f,1.05f,.08f);
            dart.localPosition=Vector3.Lerp(from,to,shot);dart.localRotation=Quaternion.LookRotation(to-from);
            SetEyes(sleep>.3f);captiveHead.localRotation=Quaternion.Euler(sleep*22,0,sleep*10);captive.localScale=new Vector3(1,1-sleep*.08f,1);
            if(view!=null)
            {
                Vector3 target=stage.position+new Vector3(0,1,0),desired=stage.position+new Vector3(3.8f,view.aspect<1 ? 3.2f : 2.65f,view.aspect<1 ? -8.5f : -5.2f);
                float cameraBlend=PoacherRules.Progress(age,0,.65f);view.transform.position=Vector3.Lerp(cameraStart,desired,cameraBlend);view.transform.rotation=Quaternion.Slerp(cameraRotation,Quaternion.LookRotation(target-desired),cameraBlend);
            }
        }
        static void Walk(Actor actor,float phase,float strength)
        {
            float a=Mathf.Sin(phase)*32*strength;actor.LeftLeg.localRotation=Quaternion.Euler(a,0,0);actor.RightLeg.localRotation=Quaternion.Euler(-a,0,0);actor.LeftArm.localRotation=Quaternion.Euler(-a,0,-8);actor.RightArm.localRotation=Quaternion.Euler(a,0,8);
        }
        Mesh BuildNet()
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int ring=1;ring<10;ring++)for(int i=0;i<20;i++)
            {float y=ring*Mathf.PI/10,a=i*Mathf.PI*2/20,b=(i+1)*Mathf.PI*2/20;Rope(vertices,triangles,new Vector3(Mathf.Sin(y)*Mathf.Cos(a),Mathf.Cos(y),Mathf.Sin(y)*Mathf.Sin(a)),new Vector3(Mathf.Sin(y)*Mathf.Cos(b),Mathf.Cos(y),Mathf.Sin(y)*Mathf.Sin(b)));}
            for(int line=0;line<12;line++)for(int i=0;i<12;i++)
            {float a=line*Mathf.PI*2/12,y=i*Mathf.PI/12,z=(i+1)*Mathf.PI/12;Rope(vertices,triangles,new Vector3(Mathf.Sin(y)*Mathf.Cos(a),Mathf.Cos(y),Mathf.Sin(y)*Mathf.Sin(a)),new Vector3(Mathf.Sin(z)*Mathf.Cos(a),Mathf.Cos(z),Mathf.Sin(z)*Mathf.Sin(a)));}
            var mesh=new Mesh{name="Pooled capture rope net"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
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
            if(stage!=null)Destroy(stage.gameObject);if(ropeMesh!=null)Destroy(ropeMesh);foreach(var material in materials)Destroy(material);if(sounds!=null)foreach(var sound in sounds)Destroy(sound);
        }
    }
}

