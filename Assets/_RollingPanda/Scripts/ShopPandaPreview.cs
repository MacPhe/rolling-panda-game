using UnityEngine;
namespace RollingPanda
{
    // A render-only copy of the live panda: shared meshes/materials, independent pose.
    public sealed class ShopPandaPreview : MonoBehaviour
    {
        Transform model,head,leftArm,rightArm,leftFoot,rightFoot,band;Material bamboo;
        float age,yaw=180;bool eating=true;
        public void Initialize(Transform template)
        {
            model=Instantiate(template,transform,false);model.name="Shop panda display";model.gameObject.SetActive(true);
            var appearance=model.GetComponent<PandaAppearance>();if(appearance!=null){appearance.enabled=false;Destroy(appearance);}
            head=model.Find("Head tuck pivot");leftArm=model.Find("Left arm pivot");rightArm=model.Find("Right arm pivot");
            leftFoot=model.Find("Left leg pivot");rightFoot=model.Find("Right leg pivot");
            band=new GameObject("Preview bamboo").transform;band.SetParent(rightArm,false);band.localPosition=new Vector3(-.1f,-.35f,.05f);band.localRotation=Quaternion.Euler(0,0,35);
            bamboo=new Material(Shader.Find("Universal Render Pipeline/Lit"));bamboo.color=new Color(.22f,.43f,.12f);
            Stem("Bamboo stalk",Vector3.zero,new Vector3(.055f,.32f,.055f),PrimitiveType.Cylinder);
            for(int i=0;i<3;i++) {Stem("Bamboo joint",new Vector3(0,-.23f+i*.23f,0),new Vector3(.066f,.015f,.066f),PrimitiveType.Cylinder);var leaf=Stem("Bamboo leaf",new Vector3(.1f,.21f+i*.03f,0),new Vector3(.22f,.055f,.025f),PrimitiveType.Sphere);leaf.localRotation=Quaternion.Euler(0,0,25+i*20);}
        }
        Transform Stem(string name,Vector3 position,Vector3 scale,PrimitiveType type)
        {
            var obj=GameObject.CreatePrimitive(type);obj.name=name;obj.transform.SetParent(band,false);obj.transform.localPosition=position;obj.transform.localScale=scale;
            var collider=obj.GetComponent<Collider>();collider.enabled=false;Destroy(collider);obj.GetComponent<Renderer>().sharedMaterial=bamboo;return obj.transform;
        }
        public void Turn(float degrees){yaw+=degrees;}
        public void TogglePose(){eating=!eating;}
        public string PoseLabel => eating ? "Pose: bamboo" : "Pose: idle";
        void Update()
        {
            if(model==null) return;age+=Time.unscaledDeltaTime;float sway=Mathf.Sin(age*1.4f),breath=Mathf.Sin(age*1.9f);
            float chew=eating ? Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Sin(age*.8f)+.2f)*2)) : 0;
            model.localPosition=new Vector3(0,breath*.012f,0);model.localScale=new Vector3(1,1+breath*.008f,1);model.localRotation=Quaternion.Euler(0,yaw,sway*1.5f);
            head.localPosition=new Vector3(0,1.6f,0);head.localRotation=Quaternion.Euler(chew*(5+Mathf.Sin(age*7)*2),Mathf.Sin(age*.6f)*5,0);
            leftArm.localPosition=new Vector3(-.48f,1.15f,0);leftArm.localRotation=Quaternion.Euler(sway*3,0,-8);
            rightArm.localPosition=new Vector3(.48f-.22f*chew,1.15f,0);rightArm.localRotation=Quaternion.Euler(-105*chew,0,8+chew*12);
            leftFoot.localPosition=new Vector3(-.23f,.49f,0);rightFoot.localPosition=new Vector3(.23f,.49f,0);leftFoot.localRotation=Quaternion.Euler(0,0,-3);rightFoot.localRotation=Quaternion.Euler(0,0,3);
            band.gameObject.SetActive(eating);
        }
        void OnDestroy(){if(bamboo!=null)Destroy(bamboo);}
    }
}
