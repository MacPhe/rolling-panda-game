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
        int current=-1;
        public void Initialize(Material stone,Material wood,Material red,Material gold,Material purple,Material moss)
        {
            GetComponent<Renderer>().enabled=false;
            rockLight=new Material(stone); rockLight.SetColor("_BaseColor",new Color(.86f,.89f,.94f));
            rockDark=new Material(stone); rockDark.SetColor("_BaseColor",new Color(.42f,.49f,.58f));
            paper=new Material(purple); paper.EnableKeyword("_EMISSION");
            paper.SetColor("_BaseColor",new Color(.62f,.27f,.72f));
            paper.SetColor("_EmissionColor",new Color(.38f,.12f,.5f));
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
                    Cube(root.transform,Vector3.zero,new Vector3(.98f,.98f,.98f),paper);
                    foreach(float y in new[]{-.46f,.46f})
                        Cube(root.transform,new Vector3(0,y,0),new Vector3(1,.08f,1),wood);
                    for(int rib=0;rib<5;rib++)
                    {
                        float x=-.46f+rib*.23f;
                        Cube(root.transform,new Vector3(x,0,-.495f),new Vector3(.018f,.9f,.009f),gold);
                        Cube(root.transform,new Vector3(x,0,.495f),new Vector3(.018f,.9f,.009f),gold);
                    }
                    Cube(root.transform,new Vector3(0,0,-.499f),new Vector3(.16f,.23f,.002f),purple);
                }
                root.SetActive(false);
            }
            Refresh();
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
        void OnEnable(){if(variants[0]!=null) Refresh();}
        void LateUpdate(){if(variants[0]!=null) Refresh();}
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


