using UnityEngine;
using UnityEngine.Rendering;
namespace RollingPanda
{
    public sealed class PickupArt : MonoBehaviour
    {
        Renderer source,art,glint;
        Transform visual,star;
        GameSession session;
        GemPresentation palette;
        Material previousMaterial;
        Camera view;
        float clock,seed;
        MaterialPropertyBlock properties;
        int BaseColor;
        void Awake(){properties=new MaterialPropertyBlock();BaseColor=Shader.PropertyToID("_BaseColor");}
        public void Initialize(Mesh mesh,GameSession game,GemPresentation presentation)
        {
            source=GetComponent<Renderer>();session=game;palette=presentation;view=Camera.main;
            if(mesh==null) mesh=GetComponent<MeshFilter>().sharedMesh;
            if(mesh==null || palette==null) return;
            seed=palette.NextPhase();
            var go=new GameObject("Spinning gem visual");visual=go.transform;visual.SetParent(transform,false);
            float size=Mathf.Max(mesh.bounds.size.x,Mathf.Max(mesh.bounds.size.y,mesh.bounds.size.z));
            var model=new GameObject("Gem mesh");model.transform.SetParent(visual,false);
            model.transform.localScale=Vector3.one/Mathf.Max(.001f,size);
            model.transform.localPosition=-mesh.bounds.center/Mathf.Max(.001f,size);
            model.AddComponent<MeshFilter>().sharedMesh=mesh;art=model.AddComponent<MeshRenderer>();
            source.enabled=false;
            var sparkle=new GameObject("Occasional gem glint");star=sparkle.transform;star.SetParent(visual,false);
            star.localPosition=new Vector3(.22f,.3f,-.15f);
            sparkle.AddComponent<MeshFilter>().sharedMesh=palette.Star;glint=sparkle.AddComponent<MeshRenderer>();
            glint.sharedMaterial=palette.Sparkle;glint.shadowCastingMode=ShadowCastingMode.Off;glint.receiveShadows=false;
            glint.enabled=false;Refresh();
        }
        void OnEnable(){clock=0;if(glint!=null) glint.enabled=false;if(visual!=null) visual.localPosition=Vector3.zero;}
        void LateUpdate()
        {
            if(visual==null) return;
            Refresh();
            if(session==null || !session.IsRunning) return;
            clock+=Time.deltaTime;
            if(view==null) view=Camera.main;
            float depth=view==null ? 0 : Vector3.Dot(transform.position-view.transform.position,view.transform.forward);
            bool nearby=depth>0 && depth<85;
            if(!nearby){glint.enabled=false;return;}
            visual.localRotation=Quaternion.Euler(12,seed*40+clock*110,0);
            visual.localPosition=new Vector3(0,Mathf.Sin(clock*2.1f+seed)*.075f,0);
            float pulse=Mathf.Pow(Mathf.Max(0,Mathf.Sin(clock*2.3f+seed)),18);
            glint.enabled=pulse>.025f;
            if(glint.enabled)
            {
                star.rotation=view==null ? Quaternion.identity : view.transform.rotation;
                star.localScale=Vector3.one*Mathf.Lerp(.18f,.7f,pulse);
                properties.SetColor(BaseColor,new Color(1,.98f,.86f,pulse*.85f));glint.SetPropertyBlock(properties);
            }
        }
        void Refresh()
        {
            if(previousMaterial==source.sharedMaterial) return;
            previousMaterial=source.sharedMaterial;art.sharedMaterial=palette.Polished(previousMaterial);
        }
    }
}
