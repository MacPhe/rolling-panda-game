using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    // All art remains within the original pooled obstacle envelope.
    public sealed class ObstacleArt : MonoBehaviour
    {
        readonly GameObject[] variants=new GameObject[3];
        readonly List<Mesh> meshes=new List<Mesh>();
        Material paper;
        int current=-1;
        public void Initialize(Material stone,Material wood,Material red,Material gold,Material purple,Material moss)
        {
            GetComponent<Renderer>().enabled=false;
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
            // Uneven crown and offset shoulders produce a faceted, asymmetric silhouette.
            var vertices=new Vector3[12];
            float[] heights={-.5f,.1f,.5f};
            for(int ring=0;ring<3;ring++) for(int corner=0;corner<4;corner++)
            {
                float x=corner==0 || corner==3 ? -.5f : .5f;
                float z=corner<2 ? -.5f : .5f;
                if(ring==2){x=x*(variant==0 ? .75f : .85f)-.05f;z*=.8f;}
                vertices[ring*4+corner]=new Vector3(x,heights[ring]-(ring==2 && corner%2==1 ? .15f : 0),z);
            }
            var faces=new List<int>{0,2,1,0,3,2,8,9,10,8,10,11};
            for(int ring=0;ring<2;ring++) for(int edge=0;edge<4;edge++)
            {
                int a=ring*4+edge,b=ring*4+(edge+1)%4,c=a+4,d=b+4;
                faces.AddRange(new[]{a,b,c,b,d,c});
            }
            // Duplicate triangle vertices for flat rock shading.
            var flat=new Vector3[faces.Count];var indices=new int[faces.Count];
            for(int i=0;i<faces.Count;i++){flat[i]=vertices[faces[i]];indices[i]=i;}
            for(int i=0;i<indices.Length;i+=3){int swap=indices[i+1];indices[i+1]=indices[i+2];indices[i+2]=swap;}
            var mesh=new Mesh();mesh.vertices=flat;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var go=new GameObject("Faceted stone");go.transform.SetParent(parent,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
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
        void OnDestroy(){foreach(var mesh in meshes) if(mesh!=null) Destroy(mesh);if(paper!=null) Destroy(paper);}
    }
}

