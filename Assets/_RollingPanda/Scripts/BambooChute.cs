using System.Collections.Generic;
using UnityEngine;
namespace RollingPanda
{
    // Pooled visual cage. Entry and protection use swept rules, not frame-dependent triggers.
    public sealed class BambooChute : MonoBehaviour
    {
        readonly List<Vector3> vertices=new List<Vector3>();readonly List<int> indices=new List<int>();
        Mesh mesh;Material material;
        public float Centre=>transform.position.x;
        public float Entrance=>transform.position.z;
        public float Exit=>Entrance+ChuteRules.Length;
        public int Lane {get;set;}
        public void Build()
        {
            material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.38f,.52f,.13f);material.SetFloat("_Smoothness",.22f);
            for(int i=0;i<=10;i++)
            {
                float a=i*Mathf.PI/10;var p=new Vector3(Mathf.Cos(a)*1.16f,.12f+Mathf.Sin(a)*1.48f,0);
                Pole(p,p+Vector3.forward*ChuteRules.Length,.075f);
            }
            for(int z=0;z<=ChuteRules.Length;z+=4)for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;
                Pole(new Vector3(Mathf.Cos(a)*1.19f,.12f+Mathf.Sin(a)*1.51f,z),new Vector3(Mathf.Cos(b)*1.19f,.12f+Mathf.Sin(b)*1.51f,z),.055f);
            }
            // A tall entry frame makes the tunnel visible well before its low opening.
            Pole(new Vector3(-1.25f,0,0),new Vector3(-1.25f,2.6f,0),.11f);
            Pole(new Vector3(1.25f,0,0),new Vector3(1.25f,2.6f,0),.11f);
            Pole(new Vector3(-1.4f,2.55f,0),new Vector3(1.4f,2.55f,0),.12f);
            mesh=new Mesh{name="Bamboo rolling chute cage"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
            vertices.Clear();indices.Clear();
            var label=new GameObject("Roll entrance sign");label.transform.SetParent(transform,false);label.transform.localPosition=new Vector3(0,2.18f,-.15f);
            var text=label.AddComponent<TextMesh>();text.text="ROLL";text.anchor=TextAnchor.MiddleCenter;text.characterSize=.18f;text.fontSize=48;text.color=new Color(1,.95f,.75f);
            var font=Resources.Load<Font>("Presentation/alagard");if(font!=null){text.font=font;label.GetComponent<MeshRenderer>().sharedMaterial=font.material;}

        }
        void Pole(Vector3 a,Vector3 b,float radius)
        {
            const int sides=8;int first=vertices.Count;var rotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
            for(int end=0;end<2;end++)for(int i=0;i<sides;i++)
            {
                float angle=i*Mathf.PI*2/sides;vertices.Add((end==0 ? a : b)+rotation*new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius));
            }
            for(int i=0;i<sides;i++){int p=first+i,n=first+(i+1)%sides;indices.AddRange(new[]{p,p+sides,n,n,p+sides,n+sides});}
        }
        void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
