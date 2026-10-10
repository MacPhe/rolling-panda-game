using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    // Built once into the existing pool. Shared-material meshes reduce scenery draw calls.
    public sealed class ForestAtmosphere : MonoBehaviour
    {
        readonly List<Mesh> ownedMeshes = new List<Mesh>();
        public readonly List<Transform> LampSockets = new List<Transform>();
        Material glow;
        public void SetWarmth(float darkness) { if(glow!=null) glow.SetColor("_EmissionColor",new Color(1,.4f,.08f)*Mathf.Lerp(.15f,2.5f,darkness)); }
        public void Build(Transform section, Material grass, Material leaf, Material bark, Material stone, Material roof, Material red, Material trim)
        {
            if(glow==null)
            {
                glow=new Material(trim); glow.SetColor("_BaseColor",new Color(1,.65f,.25f));
                glow.EnableKeyword("_EMISSION"); glow.SetColor("_EmissionColor",new Color(.15f,.06f,.01f));
            }
            var root = new GameObject("Layered forest and shrines").transform;
            root.SetParent(section,false);
            for(int side=-1;side<=1;side+=2)
            {
                Shape(root,PrimitiveType.Cube,new Vector3(side*17,-.6f,48),new Vector3(24,1,96),grass);
                for(int i=0;i<6;i++)
                {
                    float z=8+i*16;
                    Shape(root,PrimitiveType.Sphere,new Vector3(side*(22+i%2*8),-.5f,z),new Vector3(22,9+i%3*4,28),grass);
                    Shape(root,PrimitiveType.Sphere,new Vector3(side*(43+i%2*7),4,z),new Vector3(29,26+i%3*9,34),leaf);
                    for(int k=0;k<3;k++)
                    {
                        float x=side*(8+i%3*1.3f+k*3.5f), bz=z+k*2.1f;
                        float h=6+(i+k)%4;
                        Shape(root,PrimitiveType.Cylinder,new Vector3(x,h/2,bz),new Vector3(.14f,h/2,.14f),bark);
                        for(int n=0;n<3;n++)
                        {
                            var frond=Shape(root,PrimitiveType.Sphere,new Vector3(x+side*n*.4f,h-.4f+n*.2f,bz),new Vector3(3.2f,.3f,1.1f),leaf);
                            frond.localRotation=Quaternion.Euler(0,n*58+i*13,side*(15+n*12));
                        }
                    }
                    for(int blade=0;blade<9;blade++)
                    {
                        float x=side*(5.9f+blade%3*.48f);
                        var tuft=Shape(root,PrimitiveType.Cube,new Vector3(x,.25f,z+blade*.6f),new Vector3(.055f,.5f+blade%3*.18f,.22f),grass);
                        tuft.localRotation=Quaternion.Euler(blade%2*18,blade*31,side*18);
                    }
                    Shape(root,PrimitiveType.Sphere,new Vector3(side*6.5f,.15f,z+4),new Vector3(1.2f,.7f,.9f),stone);
                }
                Pavilion(root,side*11,48,stone,roof,red,trim);
                Shrine(root,side*7,18,stone,roof,trim);
                Shrine(root,side*7,78,stone,roof,trim);
            }
            Combine(root);
        }
        void Pavilion(Transform root,float x,float z,Material stone,Material roof,Material red,Material trim)
        {
            for(int step=0;step<3;step++)
                Shape(root,PrimitiveType.Cube,new Vector3(x,.12f+step*.18f,z),new Vector3(5-step*.5f,.2f,6-step*.5f),stone);
            foreach(float dx in new[]{-1.6f,1.6f}) foreach(float dz in new[]{-2f,2f})
            {
                Shape(root,PrimitiveType.Cylinder,new Vector3(x+dx,2.3f,z+dz),new Vector3(.23f,1.8f,.23f),red);
                Shape(root,PrimitiveType.Cube,new Vector3(x+dx,.65f,z+dz),new Vector3(.6f,.3f,.6f),stone);
            }
            Shape(root,PrimitiveType.Cube,new Vector3(x,3.85f,z),new Vector3(3.8f,.22f,4.7f),trim);
            Roof(root,x,4,z,5.8f,6.6f,roof,trim);
            // Open sides and a small altar give the pavilion depth.
            Shape(root,PrimitiveType.Cube,new Vector3(x,1,z+.6f),new Vector3(1.5f,.8f,1),stone);
            Shape(root,PrimitiveType.Sphere,new Vector3(x,1.6f,z+.6f),new Vector3(.45f,.65f,.45f),trim);
            foreach(float dx in new[]{-1.4f,1.4f})
            {
                Shape(root,PrimitiveType.Cylinder,new Vector3(x+dx,3.1f,z-2),new Vector3(.35f,.38f,.35f),glow);
                Shape(root,PrimitiveType.Cube,new Vector3(x+dx,3.55f,z-2),new Vector3(.06f,.25f,.06f),red);
            }
        }
        void Shrine(Transform root,float x,float z,Material stone,Material roof,Material trim)
        {
            Shape(root,PrimitiveType.Cube,new Vector3(x,.2f,z),new Vector3(1.4f,.4f,1.4f),stone);
            Shape(root,PrimitiveType.Cube,new Vector3(x,1,z),new Vector3(.5f,1.3f,.5f),stone);
            Shape(root,PrimitiveType.Cube,new Vector3(x,1.85f,z),new Vector3(1,.6f,1),stone);
            Shape(root,PrimitiveType.Cube,new Vector3(x,1.85f,z-.51f),new Vector3(.45f,.35f,.02f),glow);
            var socket=new GameObject("Shrine light socket").transform; socket.SetParent(root,false);
            socket.localPosition=new Vector3(x,1.85f,z-.7f); LampSockets.Add(socket);
            Roof(root,x,2.2f,z,1.7f,1.7f,roof,trim);
        }
        void Roof(Transform root,float x,float y,float z,float width,float depth,Material roof,Material trim)
        {
            for(int side=-1;side<=1;side+=2)
            {
                var slope=Shape(root,PrimitiveType.Cube,new Vector3(x+side*width*.23f,y+.38f,z),new Vector3(width*.55f,.18f,depth),roof);
                slope.localRotation=Quaternion.Euler(0,0,-side*18);
                var eave=Shape(root,PrimitiveType.Cube,new Vector3(x+side*width*.48f,y+.12f,z),new Vector3(width*.12f,.13f,depth+.3f),trim);
                eave.localRotation=Quaternion.Euler(0,0,side*15);
            }
            Shape(root,PrimitiveType.Cylinder,new Vector3(x,y+.7f,z),new Vector3(.15f,depth*.52f,.15f),trim).localRotation=Quaternion.Euler(90,0,0);
        }
        static Transform Shape(Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type); go.transform.SetParent(parent,false);
            go.transform.localPosition=position; go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>(); collider.enabled=false; Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial=material;
            return go.transform;
        }
        void Combine(Transform root)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                Material material=filter.GetComponent<Renderer>().sharedMaterial;
                if(!groups.ContainsKey(material)) groups[material]=new List<CombineInstance>();
                groups[material].Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                filter.gameObject.SetActive(false); Destroy(filter.gameObject);
            }
            foreach(var group in groups)
            {
                var mesh=new Mesh(); mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.CombineMeshes(group.Value.ToArray(),true,true); ownedMeshes.Add(mesh);
                var go=new GameObject("Batched forest scenery"); go.transform.SetParent(root,false);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>(); renderer.sharedMaterial=group.Key;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        void OnDestroy(){ foreach(var mesh in ownedMeshes) if(mesh!=null) Destroy(mesh); if(glow!=null) Destroy(glow); }
    }
}

