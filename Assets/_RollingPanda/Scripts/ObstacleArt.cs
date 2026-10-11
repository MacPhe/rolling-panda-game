using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    // All art remains within the original pooled obstacle envelope.
    public sealed class ObstacleArt : MonoBehaviour
    {
        readonly GameObject[] variants=new GameObject[3];
        readonly List<Mesh> meshes=new List<Mesh>();
        Material paper, rockLight, rockDark;
        public Transform LanternSocket => variants[2]==null ? null : variants[2].transform;
        int current=-1;
        GameSession session;
        float swayClock,swayPhase;
        public void Initialize(Material stone,Material wood,Material red,Material gold,Material purple,Material moss)
        {
            session=GetComponentInParent<GameSession>();
            swayPhase=transform.position.x*.71f+transform.position.z*.11f;
            GetComponent<Renderer>().enabled=false;
            rockLight=new Material(stone); rockLight.SetColor("_BaseColor",new Color(.86f,.89f,.94f));
            rockDark=new Material(stone); rockDark.SetColor("_BaseColor",new Color(.42f,.49f,.58f));
            paper=new Material(purple); paper.EnableKeyword("_EMISSION");
            paper.SetColor("_BaseColor",new Color(1f,.64f,.23f));
            paper.SetColor("_EmissionColor",new Color(4.5f,1.6f,.25f));
            for(int kind=0;kind<3;kind++)
            {
                var root=new GameObject(kind==0 ? "Large asymmetric rock" : kind==1 ? "Small rock formation" : "Floating lantern");
                root.transform.SetParent(transform,false); variants[kind]=root;
                if(kind<2)
                {
                    Rock(root.transform,stone,kind);
                    foreach(float x in new[]{-.32f,.12f})
                        Cube(root.transform,new Vector3(x,-.34f,-.49f),new Vector3(.22f,.09f,.014f),moss);
                    Cube(root.transform,new Vector3(-.22f,.03f,-.493f),new Vector3(.07f,.47f,.008f),kind==0 ? red : gold);
                    Cube(root.transform,new Vector3(.2f,-.22f,-.493f),new Vector3(.13f,.09f,.008f),kind==0 ? red : gold);
                }
                else
                {
                    Lantern(root.transform,wood,gold,red);
                }                root.SetActive(false);
            }
            Refresh();
        }
        void Lantern(Transform parent,Material wood,Material gold,Material red)
        {
            const int sides=20;
            float[] ys={-.46f,-.3f,0,.3f,.46f};
            float[] widths={.34f,.46f,.5f,.46f,.34f};
            var vertices=new Vector3[sides*5];var faces=new List<int>();
            for(int ring=0;ring<5;ring++) for(int edge=0;edge<sides;edge++)
            {
                float angle=edge*Mathf.PI*2/sides;
                vertices[ring*sides+edge]=new Vector3(Mathf.Cos(angle)*widths[ring],ys[ring],Mathf.Sin(angle)*widths[ring]);
                if(ring<4)
                {
                    int a=ring*sides+edge,b=ring*sides+(edge+1)%sides,c=a+sides,d=b+sides;
                    faces.AddRange(new[]{a,c,b,b,c,d});
                }
            }
            var mesh=new Mesh();mesh.vertices=vertices;mesh.SetTriangles(faces,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var body=new GameObject("Warm curved paper lantern");body.transform.SetParent(parent,false);
            body.AddComponent<MeshFilter>().sharedMesh=mesh;body.AddComponent<MeshRenderer>().sharedMaterial=paper;
            foreach(float y in new[]{-.475f,.475f})
            {
                var cap=GameObject.CreatePrimitive(PrimitiveType.Cylinder);cap.name="Dark wooden lantern cap";
                cap.transform.SetParent(parent,false);cap.transform.localPosition=new Vector3(0,y,0);cap.transform.localScale=new Vector3(.72f,.025f,.72f);
                var collider=cap.GetComponent<Collider>();collider.enabled=false;Destroy(collider);cap.GetComponent<Renderer>().sharedMaterial=wood;
            }
            // Short rib segments follow the curved paper, rather than a rectangular frame.
            for(int rib=0;rib<10;rib++) for(int ring=0;ring<4;ring++)
            {
                float angle=rib*Mathf.PI*2/10;
                Vector3 a=new Vector3(Mathf.Cos(angle)*(widths[ring]-.008f),ys[ring],Mathf.Sin(angle)*(widths[ring]-.008f));
                Vector3 b=new Vector3(Mathf.Cos(angle)*(widths[ring+1]-.008f),ys[ring+1],Mathf.Sin(angle)*(widths[ring+1]-.008f));
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Curved lantern rib";go.transform.SetParent(parent,false);
                go.transform.localPosition=(a+b)*.5f;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
                go.transform.localScale=new Vector3(.012f,(b-a).magnitude,.012f);
                var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);go.GetComponent<Renderer>().sharedMaterial=gold;
            }
            Cube(parent,new Vector3(0,0,-.499f),new Vector3(.1f,.16f,.002f),red);
            BatchLantern(parent);
        }
        void BatchLantern(Transform root)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                Material material=filter.GetComponent<Renderer>().sharedMaterial;
                if(!groups.ContainsKey(material)) groups[material]=new List<CombineInstance>();
                groups[material].Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});
                filter.gameObject.SetActive(false);Destroy(filter.gameObject);
            }
            foreach(var group in groups)
            {
                var mesh=new Mesh();mesh.CombineMeshes(group.Value.ToArray(),true,true);meshes.Add(mesh);
                var go=new GameObject("Batched lantern detail");go.transform.SetParent(root,false);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
            }
        }
        void Rock(Transform parent,Material material,int variant)
        {
            // Eight uneven edges per ring break the old rectangular slab silhouette.
            const int sides=8;
            var vertices=new Vector3[24];
            float[] radius={1,.86f,.52f};
            for(int ring=0;ring<3;ring++) for(int edge=0;edge<sides;edge++)
            {
                float angle=edge*Mathf.PI/4;
                float variation=1-((edge*3+ring*5+variant)%5)*.065f;
                float x=Mathf.Cos(angle)*.5f*radius[ring]*variation;
                float z=Mathf.Sin(angle)*.5f*radius[ring]*variation;
                float y=ring==0 ? -.5f : ring==1 ? -.05f+(edge%3)*.085f : .5f-(edge%4)*.09f;
                if(ring==2){x-=.13f;z+=.06f;}
                vertices[ring*sides+edge]=new Vector3(x,y,z);
            }
            var faces=new List<int>();
            for(int ring=0;ring<2;ring++) for(int edge=0;edge<sides;edge++)
            {
                int a=ring*sides+edge,b=ring*sides+(edge+1)%sides,c=a+sides,d=b+sides;
                faces.AddRange(new[]{a,c,b,b,c,d});
            }
            for(int edge=1;edge<7;edge++){faces.AddRange(new[]{0,edge,edge+1,16,16+edge+1,16+edge});}
            var flat=new Vector3[faces.Count];
            var triangles=new[]{new List<int>(),new List<int>(),new List<int>()};
            for(int i=0;i<faces.Count;i++) flat[i]=vertices[faces[i]];
            for(int i=0;i<faces.Count;i+=3)
            {
                int shade=(i/3+variant)%5;
                triangles[shade==0 ? 2 : shade<3 ? 0 : 1].AddRange(new[]{i,i+1,i+2});
            }
            var mesh=new Mesh();mesh.vertices=flat;mesh.subMeshCount=3;
            for(int i=0;i<3;i++) mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var go=new GameObject("Broken cool-grey rock facets");go.transform.SetParent(parent,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials=new[]{material,rockLight,rockDark};
        }
        void OnEnable()
        {
            swayClock=0;
            if(variants[2]!=null) {variants[2].transform.localPosition=Vector3.zero;variants[2].transform.localRotation=Quaternion.identity;}
            if(variants[0]!=null) Refresh();
        }
        void LateUpdate()
        {
            if(variants[0]==null) return;
            Refresh();
            if(current!=2 || session==null || !session.IsRunning) return;
            swayClock+=Time.deltaTime;
            // Move only the collider-free lantern art. Obstacle root and BoxCollider stay fixed.
            Vector3 scale=transform.lossyScale;
            variants[2].transform.localPosition=new Vector3(.04f/Mathf.Max(.01f,Mathf.Abs(scale.x))*Mathf.Sin(swayClock*.9f+swayPhase),.035f/Mathf.Max(.01f,Mathf.Abs(scale.y))*Mathf.Sin(swayClock*1.3f+swayPhase*.7f),0);
            variants[2].transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(swayClock*.8f+swayPhase)*1.2f);
        }
        void Refresh()
        {
            float height=transform.localScale.y;int kind=height>3 ? 0 : height<1 ? 1 : 2;
            if(kind==current) return;current=kind;
            for(int i=0;i<3;i++) variants[i].SetActive(i==kind);
        }
        static void Cube(Transform parent,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Obstacle detail";
            go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial=material;
        }
        void OnDestroy(){foreach(var mesh in meshes) if(mesh!=null) Destroy(mesh);if(paper!=null) Destroy(paper);if(rockLight!=null) Destroy(rockLight);if(rockDark!=null) Destroy(rockDark);}
    }
}




