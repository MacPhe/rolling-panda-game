using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    // Presentation only: no colliders, input, spawning rules or scoring changes.
    public sealed class VisualPrototype : MonoBehaviour
    {
        readonly List<Material> materials = new List<Material>();
        RunnerMotor runner;
        Transform panda, leftArm, rightArm, leftFoot, rightFoot, tails;
        float phase;
        Material ivory, ink, red, bamboo, leaves, stone, roof, gold;

        void Start()
        {
            runner = GetComponentInChildren<RunnerMotor>();
            if (runner == null) { enabled = false; return; }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) { enabled = false; return; }
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
            var head = Part(panda,"Head",PrimitiveType.Sphere,new Vector3(0,1.6f,.03f),new Vector3(.94f,.81f,.76f),ivory);
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
            foreach (Transform child in transform)
                if (child.name.StartsWith("Track section ")) Decorate(child);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.43f,.59f,.57f);
            RenderSettings.fogStartDistance = 65;
            RenderSettings.fogEndDistance = 155;
            RenderSettings.ambientLight = new Color(.57f,.65f,.62f);
            Camera camera = Camera.main;
            if (camera != null) { camera.backgroundColor = RenderSettings.fogColor; camera.clearFlags = CameraClearFlags.SolidColor; }
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
            // Children travel with the existing pooled sections, including origin shifts.
            for (int side=-1;side<=1;side+=2)
            {
                Part(section,"Stone verge",PrimitiveType.Cube,new Vector3(side*5.3f,-.05f,48),new Vector3(.6f,.35f,96),stone);
                for (int z=6;z<96;z+=12)
                {
                    float x = side*(7.4f+(z%3)*.45f);
                    float height = 5+(z%4)*.6f;
                    Part(section,"Bamboo stalk",PrimitiveType.Cylinder,new Vector3(x,height/2,z),new Vector3(.18f,height/2,.18f),bamboo);
                    for (int node=1;node<4;node++)
                        Part(section,"Bamboo node",PrimitiveType.Cylinder,new Vector3(x,node*height/4,z),new Vector3(.22f,.035f,.22f),gold);
                    var canopy = Part(section,"Bamboo foliage",PrimitiveType.Sphere,new Vector3(x,height-.4f,z),new Vector3(2.5f,.6f,1.4f),leaves);
                    canopy.localRotation = Quaternion.Euler(0,z*7,side*18);
                }
                Part(section,"Temple plinth",PrimitiveType.Cube,new Vector3(side*10,0,48),new Vector3(4,.7f,6),stone);
                Part(section,"Temple pavilion",PrimitiveType.Cube,new Vector3(side*10,2,48),new Vector3(3,3.4f,4),red);
                Part(section,"Temple roof lower",PrimitiveType.Cube,new Vector3(side*10,3.9f,48),new Vector3(4.7f,.3f,5.6f),roof);
                Part(section,"Temple roof crown",PrimitiveType.Cube,new Vector3(side*10,4.25f,48),new Vector3(3.3f,.45f,4.2f),roof);
                Part(section,"Temple gold trim",PrimitiveType.Cube,new Vector3(side*10,3.65f,48),new Vector3(3.3f,.12f,4.3f),gold);
            }
        }

        void LateUpdate()
        {
            if (panda == null) return;
            bool stopped = runner.Paused || runner.Finished || (runner.Session != null && runner.Session.Defeated);
            if (stopped) return;
            phase += Time.deltaTime * runner.CurrentSpeed * .9f;
            bool airborne = !runner.GetComponent<CharacterController>().isGrounded;
            float swing = airborne ? 0 : Mathf.Sin(phase)*32;
            panda.localPosition = new Vector3(0,runner.Sliding ? .4f : airborne ? 0 : Mathf.Abs(Mathf.Sin(phase))*.045f,0);
            panda.localRotation = Quaternion.Euler(runner.Sliding ? 72 : airborne ? -8 : 5,0,0);
            leftArm.localRotation = Quaternion.Euler(airborne ? -65 : swing,0,-10);
            rightArm.localRotation = Quaternion.Euler(airborne ? -65 : -swing,0,10);
            leftFoot.localRotation = Quaternion.Euler(airborne ? -35 : -swing,0,0);
            rightFoot.localRotation = Quaternion.Euler(airborne ? 30 : swing,0,0);
            tails.localRotation = Quaternion.Euler(Mathf.Sin(phase)*12,0,Mathf.Sin(phase*.7f)*10);
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
