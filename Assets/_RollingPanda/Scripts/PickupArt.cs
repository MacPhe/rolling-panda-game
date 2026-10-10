using UnityEngine;
namespace RollingPanda
{
    public sealed class PickupArt : MonoBehaviour
    {
        Renderer source, art;
        Transform visual;
        GameSession session;
        public void Initialize(Mesh mesh,GameSession game)
        {
            if(mesh==null) return;
            source=GetComponent<Renderer>();session=game;
            var go=new GameObject("Diamondo visual");visual=go.transform;visual.SetParent(transform,false);
            float size=Mathf.Max(mesh.bounds.size.x,Mathf.Max(mesh.bounds.size.y,mesh.bounds.size.z));
            visual.localScale=Vector3.one/Mathf.Max(.001f,size);
            var model=new GameObject("Gem mesh");model.transform.SetParent(visual,false);model.transform.localPosition=-mesh.bounds.center;
            model.AddComponent<MeshFilter>().sharedMesh=mesh;art=model.AddComponent<MeshRenderer>();
            source.enabled=false;Refresh();
        }
        void LateUpdate()
        {
            if(visual==null) return;
            Refresh();
            if(session!=null && session.IsRunning) visual.Rotate(0,60*Time.deltaTime,0,Space.Self);
        }
        void Refresh(){if(art.sharedMaterial!=source.sharedMaterial) art.sharedMaterial=source.sharedMaterial;}
    }
}


