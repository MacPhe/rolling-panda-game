using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    // Built once into the existing pool. Shared-material meshes reduce scenery draw calls.
    public sealed class ForestAtmosphere : MonoBehaviour
    {
        readonly List<Mesh> ownedMeshes = new List<Mesh>();
        public readonly List<Transform> LampSockets = new List<Transform>();
        public bool UseImportedForest { get; set; }
        Material glow;
        Mesh plantMesh;
        public void SetWarmth(float darkness) { if(glow!=null) glow.SetColor("_EmissionColor",new Color(1,.4f,.08f)*Mathf.Lerp(.15f,2.5f,darkness)); }
        public void Build(Transform section, Material grass, Material leaf, Material bark, Material stone, Material roof, Material red, Material trim)
        {
            if(glow==null)
            {
                glow=new Material(trim); glow.SetColor("_BaseColor",new Color(1,.65f,.25f));
                glow.EnableKeyword("_EMISSION"); glow.SetColor("_EmissionColor",new Color(.15f,.06f,.01f));
            }
            if(plantMesh==null) plantMesh=CreatePlantMesh();
            var root = new GameObject("Layered forest and shrines").transform;
            root.SetParent(section,false);
            Cobbles(root,stone);
            foreach(Transform child in section)
                if(child.name=="Lane line" || child.name=="Distance stripe") child.GetComponent<Renderer>().enabled=false;
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
                        float h=9+(i+k)%5;
                        Shape(root,PrimitiveType.Cylinder,new Vector3(x,h/2,bz),new Vector3(.24f,h/2,.24f),bark);
                        LeafyCrown(root,new Vector3(x,h-.6f,bz),1.05f,leaf,grass);
                    }
                    for(int blade=0;blade<9;blade++)
                    {
                        float x=side*(5.9f+blade%3*.48f);
                        var tuft=Shape(root,PrimitiveType.Cube,new Vector3(x,.25f,z+blade*.6f),new Vector3(.055f,.5f+blade%3*.18f,.22f),grass);
                        tuft.localRotation=Quaternion.Euler(blade%2*18,blade*31,side*18);
                    }
                    Shape(root,PrimitiveType.Sphere,new Vector3(side*6.5f,.15f,z+4),new Vector3(1.2f,.7f,.9f),stone);
                }
                for(int cluster=0;cluster<12;cluster++)
                {
                    float z=4+cluster*8;
                    Shape(root,PrimitiveType.Sphere,new Vector3(side*6.5f,-.1f,z),new Vector3(2.4f,1.2f,4),grass);
                    for(int bush=0;bush<(UseImportedForest ? 0 : 4);bush++)
                    {
                        float x=side*(7.2f+bush*2.3f);
                        Plant(root,new Vector3(x,.12f,z+bush%2*2),1+bush*.15f,cluster*29,bush%2==0 ? leaf : grass);
                        for(int blade=0;blade<4;blade++)
                        {
                            var spray=Shape(root,PrimitiveType.Cube,new Vector3(x,.65f,z+blade*.35f),new Vector3(.07f,1.2f,.28f),grass);
                            spray.localRotation=Quaternion.Euler(blade*9,blade*41,side*24);
                        }
                    }
                    if(cluster%2==0 && !UseImportedForest)
                    {
                        float height=10+cluster%3;
                        Shape(root,PrimitiveType.Cylinder,new Vector3(side*8,height/2,z),new Vector3(.6f,height/2,.6f),bark);
                        for(int branch=0;branch<3;branch++)
                        {
                            var limb=Shape(root,PrimitiveType.Cylinder,new Vector3(side*(8+branch*.5f),height-2+branch*.5f,z+.4f),new Vector3(.22f,1.6f,.22f),bark);
                            limb.localRotation=Quaternion.Euler(25,branch*80,side*35);
                        }
                        LeafyCrown(root,new Vector3(side*8,height,z),1.65f,leaf,grass);
                    }
                }
                // Rock faces, moss shelves and forest-floor detail form distinct depth layers.
                for(int ridge=0;ridge<4;ridge++)
                {
                    float rz=12+ridge*24;
                    var face=Shape(root,PrimitiveType.Cube,new Vector3(side*(26+ridge%2*5),5,rz),new Vector3(8,13+ridge%3*4,13),stone);
                    face.localRotation=Quaternion.Euler(8+ridge*3,side*18,side*12);
                    Shape(root,PrimitiveType.Sphere,new Vector3(side*(24+ridge%2*5),10,rz),new Vector3(9,2,15),grass);
                    for(int layer=0;layer<3;layer++)
                        Shape(root,PrimitiveType.Cube,new Vector3(side*(22+ridge%2*5),3+layer*2,rz),new Vector3(3,.22f,10),roof);
                    for(int fern=0;fern<4;fern++)
                    {
                        float fx=side*(6.4f+fern*.6f), fz=rz+fern*2;
                        for(int frond=0;frond<3;frond++)
                        {
                            Plant(root,new Vector3(fx,.05f,fz),.6f,frond*60+ridge*17,leaf);
                        }
                        var rootShape=Shape(root,PrimitiveType.Cylinder,new Vector3(side*7,.15f,fz+1),new Vector3(.16f,.9f,.16f),bark);
                        rootShape.localRotation=Quaternion.Euler(90,side*35,0);
                    }
                    // Weathered stepping stones lead toward the roadside shrines.
                    for(int step=0;step<4;step++)
                        Shape(root,PrimitiveType.Cube,new Vector3(side*(5.8f+step*.48f),.04f,rz+5),new Vector3(.38f,.08f,.7f),stone);
                }
                Pavilion(root,side*11,48,stone,roof,red,trim);
                Shrine(root,side*7,18,stone,roof,trim);
                Shrine(root,side*7,78,stone,roof,trim);
            }
            Combine(root);
        }
        void LeafyCrown(Transform root,Vector3 centre,float size,Material leaf,Material grass)
        {
            if(UseImportedForest) return;
            // Overlapping three-dimensional masses with visible leaf sprays, not flat discs.
            for(int clump=0;clump<5;clump++)
            {
                float angle=clump*Mathf.PI*2/5;
                Vector3 offset=new Vector3(Mathf.Cos(angle)*1.25f,(clump%3)*.75f,Mathf.Sin(angle)*1.25f)*size;
                var mass=Shape(root,PrimitiveType.Sphere,centre+offset,new Vector3(2.7f,2.5f,2.8f)*size,clump%3==0 ? grass : leaf);
                mass.localRotation=Quaternion.Euler(clump*11,clump*71,clump*9);
                Plant(root,centre+offset+Vector3.up*.9f*size,1.65f*size,clump*71,leaf);
            }
        }
        Mesh CreatePlantMesh()
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int frond=0;frond<7;frond++) for(int pair=0;pair<4;pair++)
            {
                float angle=frond*Mathf.PI*2/7;
                Vector3 direction=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                Vector3 across=new Vector3(-direction.z,0,direction.x);
                Vector3 centre=direction*(.15f+pair*.22f)+Vector3.up*(.25f+pair*.17f);
                float side=pair%2==0 ? 1 : -1;
                Vector3 tip=centre+direction*.35f+across*(side*.42f)+Vector3.up*.18f;
                Vector3 mid=(centre+tip)*.5f;
                int n=vertices.Count;
                vertices.AddRange(new[]{centre,mid+direction*.11f,mid+Vector3.up*.065f,mid-direction*.11f,tip});
                int[] face={0,1,2,0,2,3,1,4,2,2,4,3};
                for(int i=0;i<face.Length;i+=3)
                {
                    triangles.AddRange(new[]{n+face[i],n+face[i+1],n+face[i+2],n+face[i+2],n+face[i+1],n+face[i]});
                }
            }
            var flat=new Vector3[triangles.Count];var indices=new int[triangles.Count];
            for(int i=0;i<triangles.Count;i++){flat[i]=vertices[triangles[i]];indices[i]=i;}
            var mesh=new Mesh();mesh.vertices=flat;mesh.triangles=indices;
            mesh.RecalculateNormals();mesh.RecalculateBounds();ownedMeshes.Add(mesh);return mesh;
        }
        void Plant(Transform parent,Vector3 position,float size,float angle,Material material)
        {
            var go=new GameObject("Pointed leaf vegetation");go.transform.SetParent(parent,false);
            go.transform.localPosition=position;go.transform.localScale=Vector3.one*size;
            go.transform.localRotation=Quaternion.Euler(0,angle,0);
            go.AddComponent<MeshFilter>().sharedMesh=plantMesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
            var stem=Shape(parent,PrimitiveType.Cube,position+Vector3.up*size*.5f,new Vector3(.045f,size,.045f),material);
            stem.localRotation=Quaternion.Euler(8,angle,12);
        }
        void Cobbles(Transform root,Material stone)
        {
            // One surface mesh, without hundreds of individual cobble objects.
            var sample=GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh cube=sample.GetComponent<MeshFilter>().sharedMesh;sample.SetActive(false);Destroy(sample);
            var instances=new CombineInstance[9*64];int count=0;
            for(int row=0;row<64;row++) for(int col=0;col<9;col++)
            {
                float x=-4.2f+col*1.05f+(row%2==0 ? -.08f : .08f);
                float z=.75f+row*1.5f;
                float skew=((row*13+col*7)%5-2)*1.2f;
                instances[count++]=new CombineInstance{mesh=cube,transform=Matrix4x4.TRS(new Vector3(x,.008f,z),Quaternion.Euler(0,skew,0),new Vector3(.98f,.016f,1.41f))};
            }
            var mesh=new Mesh();mesh.CombineMeshes(instances,true,true);ownedMeshes.Add(mesh);
            var go=new GameObject("Cobbled forest road");go.transform.SetParent(root,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=stone;
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
                mesh.CombineMeshes(group.Value.ToArray(),true,true);mesh.UploadMeshData(true); ownedMeshes.Add(mesh);
                var go=new GameObject("Batched forest scenery"); go.transform.SetParent(root,false);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>(); renderer.sharedMaterial=group.Key;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        void OnDestroy(){ foreach(var mesh in ownedMeshes) if(mesh!=null) Destroy(mesh); if(glow!=null) Destroy(glow); }
    }
}







