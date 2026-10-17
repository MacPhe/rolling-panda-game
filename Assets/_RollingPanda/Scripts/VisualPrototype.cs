using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    // Presentation only: no colliders, input, spawning rules or scoring changes.
    public sealed class VisualPrototype : MonoBehaviour
    {
        readonly List<Material> materials = new List<Material>();
        RunnerMotor runner;
        Transform panda, headRig, leftArm, rightArm, leftFoot, rightFoot, tails;
        [Tooltip("Turn off to compare the previous side-lunge animation. Presentation only.")]
        public bool RollingMotion=true;
        float rollTime,kickTime=1,kickDirection,laneSway,previousX;
        int previousLane;
        float phase, airTime;
        bool wasAirborne;
        double previousDistance;
        CharacterController controller;
        ForestAtmosphere forest;
        Material ivory, ink, red, bamboo, leaves, stone, roof, gold;

        public Transform Panda => panda;
        public float FootfallPhase => phase;

        void Start()
        {
            runner = GetComponentInChildren<RunnerMotor>();
            if (runner == null) { enabled = false; return; }
            controller = runner.GetComponent<CharacterController>();
            forest = gameObject.AddComponent<ForestAtmosphere>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) { enabled = false; return; }
            var environmentArt=Resources.Load<EnvironmentArt>("Presentation/EnvironmentArt");
            forest.UseImportedForest=gameObject.AddComponent<ImportedForest>().Initialize(runner,environmentArt);
            ivory = Mat(shader, new Color(.92f,.89f,.79f));
            ink = Mat(shader, new Color(.035f,.045f,.045f));
            red = Mat(shader, new Color(.72f,.055f,.045f));
            bamboo = Mat(shader, new Color(.26f,.39f,.16f));
            leaves = Mat(shader, new Color(.12f,.27f,.15f));
            stone = Mat(shader, new Color(.39f,.42f,.35f));
            roof = Mat(shader, new Color(.12f,.22f,.22f));
            gold = Mat(shader, new Color(.7f,.48f,.21f));
            foreach (Renderer renderer in runner.GetComponentsInChildren<Renderer>()) renderer.enabled = false;
            panda = new GameObject("Panda art study").transform;
            panda.SetParent(runner.transform, false);
            Part(panda,"Pear shaped body",PrimitiveType.Sphere,new Vector3(0,.85f,0),new Vector3(.83f,1.05f,.67f),ivory);
            Part(panda,"Shoulders",PrimitiveType.Sphere,new Vector3(0,1.14f,0),new Vector3(.9f,.42f,.63f),ink);
            Part(panda,"Head",PrimitiveType.Sphere,new Vector3(0,1.6f,.03f),new Vector3(.94f,.81f,.76f),ivory);
            foreach (float side in new[]{-1f,1f})
            {
                Part(panda,"Round ear",PrimitiveType.Sphere,new Vector3(side*.36f,1.94f,.025f),new Vector3(.29f,.3f,.2f),ink);
                var patch = Part(panda,"Eye patch",PrimitiveType.Sphere,new Vector3(side*.21f,1.65f,.362f),new Vector3(.26f,.3f,.065f),ink);
                patch.localRotation = Quaternion.Euler(0,0,side*-18);
                Part(panda,"Eye",PrimitiveType.Sphere,new Vector3(side*.2f,1.67f,.397f),new Vector3(.08f,.1f,.04f),ivory);
                Part(panda,"Pupil",PrimitiveType.Sphere,new Vector3(side*.2f,1.67f,.42f),new Vector3(.044f,.064f,.025f),ink);
            }
            Part(panda,"Muzzle",PrimitiveType.Sphere,new Vector3(0,1.43f,.37f),new Vector3(.43f,.26f,.22f),ivory);
            Part(panda,"Nose",PrimitiveType.Sphere,new Vector3(0,1.48f,.49f),new Vector3(.16f,.1f,.08f),ink);
            Part(panda,"Red headband",PrimitiveType.Cylinder,new Vector3(0,1.79f,.03f),new Vector3(.91f,.055f,.72f),red);
            tails = Part(panda,"Headband knot",PrimitiveType.Sphere,new Vector3(.25f,1.78f,-.34f),Vector3.one*.14f,red);
            Part(tails,"Trailing ribbon",PrimitiveType.Cube,new Vector3(.08f,-.17f,-.12f),new Vector3(.12f,.4f,.035f),red).localRotation = Quaternion.Euler(25,0,-20);
            Part(tails,"Trailing ribbon",PrimitiveType.Cube,new Vector3(-.08f,-.13f,-.16f),new Vector3(.12f,.35f,.035f),red).localRotation = Quaternion.Euler(35,0,20);
            leftArm = Limb("Left arm",new Vector3(-.48f,1.15f,0),new Vector3(.24f,.6f,.26f),ink);
            rightArm = Limb("Right arm",new Vector3(.48f,1.15f,0),new Vector3(.24f,.6f,.26f),ink);
            leftFoot = Limb("Left leg",new Vector3(-.23f,.49f,0),new Vector3(.3f,.48f,.35f),ink);
            rightFoot = Limb("Right leg",new Vector3(.23f,.49f,0),new Vector3(.3f,.48f,.35f),ink);
            Part(panda,"Tail",PrimitiveType.Sphere,new Vector3(0,.66f,-.34f),Vector3.one*.22f,ivory);
            // Group the face, ears and headband so the head can curl into the tuck.
            headRig = new GameObject("Head tuck pivot").transform;
            headRig.SetParent(panda,false);
            headRig.localPosition = new Vector3(0,1.6f,0);
            var headParts = new List<Transform>();
            foreach (Transform part in panda)
                if (part.name == "Head" || part.name == "Round ear" || part.name == "Eye patch" ||
                    part.name == "Eye" || part.name == "Pupil" || part.name == "Muzzle" ||
                    part.name == "Nose" || part.name == "Red headband" || part == tails) headParts.Add(part);
            foreach (Transform part in headParts)
            {
                Vector3 position = part.localPosition;
                part.SetParent(headRig,false);
                part.localPosition = position - new Vector3(0,1.6f,0);
            }
            var oldHead=new List<Transform>();foreach(Transform part in headRig)oldHead.Add(part);
            var character=panda.gameObject.AddComponent<PandaLookStudy>();
            if(character.BuildGameplay(headRig,panda))
            {
                tails=null;
                foreach(var part in oldHead){part.gameObject.SetActive(false);Destroy(part.gameObject);}
                var cosmetics=new GameObject("Cosmetic socket - head").transform;cosmetics.SetParent(headRig,false);cosmetics.localPosition=new Vector3(0,.32f,0);
                var back=new GameObject("Cosmetic socket - back").transform;back.SetParent(panda,false);back.localPosition=new Vector3(0,1.02f,-.36f);
                var waist=new GameObject("Cosmetic socket - waist").transform;waist.SetParent(panda,false);waist.localPosition=new Vector3(0,.76f,0);
                foreach(var wrist in new[]{leftArm,rightArm}){var socket=new GameObject("Cosmetic socket - wrist").transform;socket.SetParent(wrist,false);socket.localPosition=new Vector3(0,-.36f,.02f);}
            }
            else {character.enabled=false;panda.gameObject.AddComponent<PandaAppearance>().Initialize(headRig,leftArm,rightArm);}
            foreach (Transform child in transform)
                if (child.name.StartsWith("Track section ")) Decorate(child);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.43f,.59f,.57f);
            RenderSettings.fogStartDistance = 55;
            RenderSettings.fogEndDistance = 155;
            RenderSettings.ambientLight = new Color(.57f,.65f,.62f);
            Camera camera = Camera.main;
            if (camera != null) { camera.backgroundColor = RenderSettings.fogColor; camera.clearFlags = CameraClearFlags.SolidColor; }
            gameObject.AddComponent<RunAtmosphere>().Initialize(runner,forest,leaves,bamboo);
            var gemArt=Resources.Load<GameArt>("Presentation/GameArt");
            var gemPresentation=gameObject.AddComponent<GemPresentation>();
            foreach(var gem in GetComponentsInChildren<BlueGem>(true))
                gem.gameObject.AddComponent<PickupArt>().Initialize(gemArt==null ? null : gemArt.Diamond,runner.Session,gemPresentation);
            if(RunEntryTransition.Active && !RunEntryTransition.Revealing && Object.FindAnyObjectByType<StartMenu>()!=null) panda.gameObject.SetActive(false);
            Material rock=Mat(shader,new Color(.7f,.76f,.84f));
            Material wood=Mat(shader,new Color(.28f,.16f,.085f));
            Material jumpAccent=Mat(shader,new Color(1,.65f,.12f));
            Material slideAccent=Mat(shader,new Color(.6f,.22f,.85f));
            var lanternGlow=gameObject.AddComponent<LanternGlow>();lanternGlow.Initialize(runner);
            foreach(var obstacle in GetComponentsInChildren<RunnerObstacle>(true))
            {
                var obstacleArt=obstacle.gameObject.AddComponent<ObstacleArt>();
                obstacleArt.Initialize(rock,wood,red,jumpAccent,slideAccent,leaves);
                lanternGlow.Add(obstacleArt.LanternSocket);
            }
            gameObject.AddComponent<PoacherPursuit>().Initialize(runner,panda);
        }

        Transform Limb(string name, Vector3 pivot, Vector3 scale, Material material)
        {
            var joint = new GameObject(name + " pivot").transform;
            joint.SetParent(panda,false); joint.localPosition = pivot;
            Part(joint,name,PrimitiveType.Sphere,new Vector3(0,-scale.y*.35f,.02f),scale,material);
            return joint;
        }

        void Decorate(Transform section)
        {
            forest.Build(section, bamboo, leaves, bamboo, stone, roof, red, gold);
        }

        void LateUpdate()
        {
            if(!RollingMotion){LateUpdateLegacy();return;}
            if(panda==null)return;
            bool reset=runner.DistanceTravelled<previousDistance;
            if(reset)
            {
                airTime=rollTime=phase=laneSway=0;kickTime=1;wasAirborne=false;
                previousLane=runner.TargetLane;previousX=runner.transform.position.x;
                panda.localPosition=Vector3.zero;panda.localRotation=Quaternion.identity;
            }
            previousDistance=runner.DistanceTravelled;
            if(runner.Paused || runner.Finished || (runner.Session!=null && runner.Session.Defeated))return;
            float dt=Time.deltaTime;if(dt<=0)return;
            float blend=1-Mathf.Exp(-18*dt);
            float lateral=Mathf.Clamp((runner.transform.position.x-previousX)/(12*dt),-1,1);
            previousX=runner.transform.position.x;
            laneSway=Mathf.Lerp(laneSway,lateral,1-Mathf.Exp(-12*dt));
            if(runner.TargetLane!=previousLane)
            {
                kickDirection=Mathf.Sign(runner.TargetLane-previousLane);kickTime=0;previousLane=runner.TargetLane;
            }
            kickTime+=dt;
            phase+=dt*Mathf.Lerp(12,18,Mathf.InverseLerp(17.5f,40,runner.CurrentSpeed));
            bool airborne=!controller.isGrounded && !runner.Sliding;
            if(airborne && !wasAirborne)airTime=0;
            if(airborne)airTime+=dt;else airTime=0;
            wasAirborne=airborne;
            bool entry=RunEntryTransition.Revealing;
            bool rolling=runner.Sliding && !entry;
            rollTime=rolling ? rollTime+dt : 0;
            float kick=!rolling && !airborne && !entry ? Mathf.Sin(Mathf.Clamp01(kickTime/.36f)*Mathf.PI) : 0;
            float tuck=airborne ? Mathf.SmoothStep(0,1,Mathf.Clamp01(airTime/.12f))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((airTime-.55f)/.17f))) : 0;
            if(entry)tuck=Mathf.Sin(RunEntryTransition.RevealProgress*Mathf.PI);
            if(rolling)tuck=1;
            float gait=Mathf.Sin(phase),counter=Mathf.Sin(phase+Mathf.PI);
            panda.localScale=rolling ? new Vector3(1,.95f,1) : new Vector3(1,1-.22f*tuck,1);
            Quaternion rotation;Vector3 position;
            if(rolling)
            {
                // Keep near-normal mass: the tuck, rather than global shrinking, creates the low silhouette.
                rotation=Quaternion.Euler(rollTime*720,0,-laneSway*5);
                var pivot=new Vector3(0,.9f,0);
                position=new Vector3(0,.65f,0)-rotation*Vector3.Scale(pivot,panda.localScale);
            }
            else if(entry || airborne)
            {
                float t=Mathf.Clamp01(airTime/.72f);
                float turn=entry ? 360*RunEntryTransition.RevealProgress : 360*t*t*(3-2*t);
                rotation=Quaternion.Euler(turn,0,0);
                var pivot=new Vector3(0,entry ? .9f : 1,0);
                position=pivot-rotation*Vector3.Scale(pivot,panda.localScale);
            }
            else
            {
                rotation=Quaternion.Euler(7+Mathf.Sin(phase*2)*2,laneSway*6+kickDirection*kick*12,gait*3-laneSway*9-kickDirection*kick*6);
                position=new Vector3(laneSway*.025f,.025f+Mathf.Cos(phase*2)*.025f+kick*.025f,0);
            }
            panda.localRotation=rolling || airborne || entry ? rotation : Quaternion.Slerp(panda.localRotation,rotation,blend);
            panda.localPosition=rolling || airborne || entry ? position : Vector3.Lerp(panda.localPosition,position,blend);
            // Collapse the roll immediately, so the first rotation does not swing an upright head below ground.
            float poseBlend=rolling ? 1 : blend;
            headRig.localPosition=Vector3.Lerp(headRig.localPosition,new Vector3(0,1.6f-(rolling ? .55f : .28f)*tuck,.12f*tuck),poseBlend);
            headRig.localRotation=Quaternion.Slerp(headRig.localRotation,Quaternion.Euler((rolling ? 45 : 32)*tuck,-laneSway*3,0),poseBlend);
            leftArm.localPosition=Vector3.Lerp(leftArm.localPosition,new Vector3(-.48f+.18f*tuck,1.15f-.12f*tuck,.15f*tuck),poseBlend);
            rightArm.localPosition=Vector3.Lerp(rightArm.localPosition,new Vector3(.48f-.18f*tuck,1.15f-.12f*tuck,.15f*tuck),poseBlend);
            leftFoot.localPosition=Vector3.Lerp(leftFoot.localPosition,new Vector3(-.23f,.49f+.25f*tuck,.18f*tuck),poseBlend);
            rightFoot.localPosition=Vector3.Lerp(rightFoot.localPosition,new Vector3(.23f,.49f+.25f*tuck,.18f*tuck),poseBlend);
            float leftKick=kickDirection<0 ? kick : 0,rightKick=kickDirection>0 ? kick : 0;
            Pose(leftArm,Mathf.Lerp(Mathf.Lerp(gait*38,-145,tuck),-65,kick),-kickDirection*kick*12,-12-kick*12,poseBlend);
            Pose(rightArm,Mathf.Lerp(Mathf.Lerp(counter*38,-145,tuck),-65,kick),-kickDirection*kick*12,12+kick*12,poseBlend);
            Pose(leftFoot,Mathf.Lerp(Mathf.Lerp(counter*36,-150,tuck),-25,leftKick),0,Mathf.Lerp(-3,-85,leftKick),poseBlend);
            Pose(rightFoot,Mathf.Lerp(Mathf.Lerp(gait*36,-150,tuck),-25,rightKick),0,Mathf.Lerp(3,85,rightKick),poseBlend);
            if(tails!=null)tails.localRotation=Quaternion.Euler(Mathf.Sin(phase-.5f)*16,Mathf.Sin(phase*.5f)*8,Mathf.Sin(phase*.7f)*12);
        }

        void LateUpdateLegacy()
        {
            if (panda == null) return;
            // A restart can happen while airborne or in the low kick pose.
            if (runner.DistanceTravelled < previousDistance)
            {
                airTime=0; wasAirborne=false; phase=0;
                panda.localPosition=Vector3.zero; panda.localRotation=Quaternion.identity;
            }
            previousDistance=runner.DistanceTravelled;
            bool stopped=runner.Paused || runner.Finished || (runner.Session!=null && runner.Session.Defeated);
            if(stopped) return;
            float dt=Time.deltaTime;
            phase+=dt*Mathf.Lerp(12,18,Mathf.InverseLerp(15,32,runner.CurrentSpeed));
            bool airborne=!controller.isGrounded && !runner.Sliding;
            if(airborne && !wasAirborne) airTime=0;
            if(airborne) airTime+=dt; else airTime=0;
            wasAirborne=airborne;
            float gait=Mathf.Sin(phase), counter=Mathf.Sin(phase+Mathf.PI);
            float blend=1-Mathf.Exp(-18*dt);
            bool entry=RunEntryTransition.Revealing;
            float tuck=airborne ? Mathf.SmoothStep(0,1,Mathf.Clamp01(airTime/.12f))*
                (1-Mathf.SmoothStep(0,1,Mathf.Clamp01((airTime-.55f)/.17f))) : 0;
            if(entry) tuck=Mathf.Sin(RunEntryTransition.RevealProgress*Mathf.PI);
            panda.localScale=new Vector3(1,1-.22f*tuck,1);
            Quaternion bodyRotation;
            Vector3 bodyPosition;
            if(entry)
            {
                bodyRotation=Quaternion.Euler(360*RunEntryTransition.RevealProgress,0,0);
                Vector3 pivot=new Vector3(0,.9f,0);
                bodyPosition=pivot-bodyRotation*Vector3.Scale(pivot,panda.localScale);
            }
            else if(runner.Sliding)
            {
                // Low, feet-first martial-arts lunge; the tested sliding capsule is unchanged.
                bodyRotation=Quaternion.Euler(-82,0,0)*Quaternion.Euler(0,75,0);
                bodyPosition=new Vector3(0,.55f,.6f);
            }
            else if(airborne)
            {
                float t=Mathf.Clamp01(airTime/.72f);
                float turn=360*t*t*(3-2*t);
                bodyRotation=Quaternion.Euler(turn,0,0);
                Vector3 pivot=new Vector3(0,1,0);
                bodyPosition=pivot-bodyRotation*Vector3.Scale(pivot,panda.localScale);
            }
            else
            {
                bodyRotation=Quaternion.Euler(7+Mathf.Sin(phase*2)*2,gait*3,gait*3);
                bodyPosition=new Vector3(0,.025f+Mathf.Cos(phase*2)*.025f,0);
            }
            panda.localRotation=airborne || entry ? bodyRotation : Quaternion.Slerp(panda.localRotation,bodyRotation,blend);
            panda.localPosition=airborne || entry ? bodyPosition : Vector3.Lerp(panda.localPosition,bodyPosition,blend);
            headRig.localPosition=Vector3.Lerp(headRig.localPosition,new Vector3(0,1.6f-.28f*tuck,.12f*tuck),blend);
            headRig.localRotation=Quaternion.Slerp(headRig.localRotation,Quaternion.Euler(32*tuck,0,0),blend);
            leftArm.localPosition=Vector3.Lerp(leftArm.localPosition,new Vector3(-.48f+.18f*tuck,1.15f-.12f*tuck,.15f*tuck),blend);
            rightArm.localPosition=Vector3.Lerp(rightArm.localPosition,new Vector3(.48f-.18f*tuck,1.15f-.12f*tuck,.15f*tuck),blend);
            leftFoot.localPosition=Vector3.Lerp(leftFoot.localPosition,new Vector3(-.23f,.49f+.25f*tuck,.18f*tuck),blend);
            rightFoot.localPosition=Vector3.Lerp(rightFoot.localPosition,new Vector3(.23f,.49f+.25f*tuck,.18f*tuck),blend);
            Pose(leftArm,runner.Sliding ? -65 : Mathf.Lerp(gait*38,-145,tuck),0,runner.Sliding ? -20 : -12,blend);
            Pose(rightArm,runner.Sliding ? -105 : Mathf.Lerp(counter*38,-145,tuck),0,runner.Sliding ? 30 : 12,blend);
            // One leg extends along travel; the other folds back for a side-kick silhouette.
            Pose(leftFoot,runner.Sliding ? 0 : Mathf.Lerp(counter*36,-150,tuck),0,-3,blend);
            Pose(rightFoot,runner.Sliding ? -105 : Mathf.Lerp(gait*36,-150,tuck),0,3,blend);
            if(tails!=null) tails.localRotation=Quaternion.Euler(Mathf.Sin(phase-.5f)*16,Mathf.Sin(phase*.5f)*8,Mathf.Sin(phase*.7f)*12);
        }

        static void Pose(Transform joint,float x,float y,float z,float blend)
        {
            joint.localRotation=Quaternion.Slerp(joint.localRotation,Quaternion.Euler(x,y,z),blend);
        }
        Material Mat(Shader shader, Color color)
        {
            var material = new Material(shader); material.SetColor("_BaseColor",color); material.SetFloat("_Smoothness",.18f);
            materials.Add(material); return material;
        }
        static Transform Part(Transform parent,string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {
            var part = GameObject.CreatePrimitive(type); part.name = name;
            part.transform.SetParent(parent,false); part.transform.localPosition=position; part.transform.localScale=scale;
            var collider=part.GetComponent<Collider>(); collider.enabled=false; Destroy(collider);
            part.GetComponent<Renderer>().sharedMaterial=material;
            return part.transform;
        }
        void OnDestroy() { foreach(var material in materials) if(material != null) Destroy(material); }
    }
}









