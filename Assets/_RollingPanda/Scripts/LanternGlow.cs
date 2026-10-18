using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace RollingPanda
{
    // Shared halo art and only two nearby shadowless lights, regardless of obstacle pool size.
    public sealed class LanternGlow : MonoBehaviour
    {
        sealed class Halo {public Transform Socket,Quad;}
        readonly List<Halo> halos=new List<Halo>();
        readonly Light[] lights=new Light[2];
        readonly Transform[] nearest=new Transform[2];
        readonly float[] gaps=new float[2];
        Material material;
        Texture2D texture;
        RunnerMotor runner;
        Camera view;
        float clock;
        public void Initialize(RunnerMotor player)
        {
            runner=player;view=Camera.main;
            texture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Soft amber lantern halo",wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[32*32];
            for(int y=0;y<32;y++) for(int x=0;x<32;x++)
            {
                float radius=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;
                pixels[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Max(0,1-radius),2)*.3f);
            }
            texture.SetPixels(pixels);texture.Apply(false,true);
            material=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Shared warm lantern halo"};
            material.SetTexture("_BaseMap",texture);material.SetColor("_BaseColor",new Color(1,.5f,.12f,1));
            material.SetFloat("_Surface",1);material.SetFloat("_ZWrite",0);material.SetFloat("_Cull",0);
            material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.One);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;
            for(int i=0;i<2;i++)
            {
                var go=new GameObject("Nearby lantern amber light");go.transform.SetParent(transform,false);
                lights[i]=go.AddComponent<Light>();lights[i].color=new Color(1,.47f,.12f);lights[i].range=5;lights[i].shadows=LightShadows.None;lights[i].enabled=false;
            }
        }
        public void Add(Transform socket)
        {
            if(socket==null) return;
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="Soft lantern glow";quad.transform.SetParent(socket,false);
            var collider=quad.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            var renderer=quad.GetComponent<Renderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            halos.Add(new Halo{Socket=socket,Quad=quad.transform});
        }
        void LateUpdate()
        {
            if(runner==null || view==null) return;
            if(runner.Session.IsRunning) clock+=Time.deltaTime;
            for(int i=0;i<2;i++){gaps[i]=float.MaxValue;nearest[i]=null;}
            foreach(var halo in halos)
            {
                if(!halo.Socket.gameObject.activeInHierarchy) continue;
                float depth=Vector3.Dot(halo.Socket.position-view.transform.position,view.transform.forward);
                bool visible=depth>0 && depth<90;
                halo.Quad.gameObject.SetActive(visible);if(!visible) continue;
                halo.Quad.rotation=view.transform.rotation;
                Vector3 scale=halo.Socket.lossyScale;
                halo.Quad.localScale=new Vector3(3.2f/Mathf.Max(.01f,Mathf.Abs(scale.x)),3.2f/Mathf.Max(.01f,Mathf.Abs(scale.y)),1);
                float distance=(halo.Socket.position-runner.transform.position).sqrMagnitude;
                if(distance>35*35) continue;
                for(int i=0;i<2;i++) if(distance<gaps[i])
                {
                    for(int j=1;j>i;j--){gaps[j]=gaps[j-1];nearest[j]=nearest[j-1];}
                    gaps[i]=distance;nearest[i]=halo.Socket;break;
                }
            }
            for(int i=0;i<2;i++)
            {
                lights[i].enabled=nearest[i]!=null;
                if(nearest[i]!=null) {lights[i].transform.position=nearest[i].position;lights[i].intensity=3.5f*(.95f+.05f*Mathf.Sin(clock*1.7f));}
            }
        }
        void OnDestroy(){if(material!=null) Destroy(material);if(texture!=null) Destroy(texture);}
    }
}
