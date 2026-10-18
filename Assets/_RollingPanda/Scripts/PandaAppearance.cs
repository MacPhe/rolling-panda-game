using System.Collections.Generic;
using UnityEngine;

namespace RollingPanda
{
    // Cosmetic surface only. Never adds colliders or changes the movement rig.
    public sealed class PandaAppearance : MonoBehaviour
    {
        readonly List<Material> ownedMaterials = new List<Material>();
        readonly List<Mesh> ownedMeshes = new List<Mesh>();
        Texture2D fur;
        Transform accessories;
        public Transform HeadSocket { get; private set; }
        public Transform BackSocket { get; private set; }
        public Transform WaistSocket { get; private set; }
        public Transform LeftWristSocket { get; private set; }
        public Transform RightWristSocket { get; private set; }
        public void SetHeadbandVisible(bool visible) { if(accessories!=null) accessories.gameObject.SetActive(visible); }

        public void Initialize(Transform head, Transform leftArm, Transform rightArm, bool sleeping=false)
        {
            fur=BuildFur();
            var white=Surface("Ivory short fur",new Color(.96f,.935f,.86f),.12f,true);
            var black=Surface("Charcoal short fur",new Color(.075f,.085f,.095f),.13f,true);
            var skin=Surface("Soft charcoal nose",new Color(.025f,.03f,.035f),.36f,false);
            var eyeWhite=Surface("Warm eye whites",new Color(1,.975f,.9f),.5f,false);
            var iris=Surface("Amber brown iris",new Color(.26f,.12f,.035f),.65f,false);
            var pupil=Surface("Glossy pupils",new Color(.009f,.014f,.018f),.8f,false);
            var highlight=Surface("Eye catchlight",Color.white,.8f,false);
            var renderers=GetComponentsInChildren<MeshRenderer>(true);
            foreach(var renderer in renderers)
            {
                string n=renderer.name;
                bool face=n=="Eye" || n=="Pupil" || n=="Nose" || n=="Closed eyelid";
                if(n.Contains("headband") || n.Contains("Headband") || n.Contains("ribbon")) continue;
                if(face) { renderer.sharedMaterial=n=="Nose" ? skin : n=="Pupil" ? pupil : eyeWhite; continue; }
                var original=renderer.sharedMaterial;
                if(original==null) continue;
                bool pale=original.color.grayscale>.45f;
                renderer.sharedMaterial=pale ? white : black;
                int count=n=="Head" ? 96 : n=="Pear shaped body" || n=="Body" ? 64 : n.Contains("arm") || n.Contains("leg") || n=="Relaxed paw" ? 24 : n=="Round ear" || n=="Ear" || n=="Tail" ? 20 : 0;
                if(count>0) AddFurOutline(renderer.transform,count,pale ? white : black,n=="Head");
            }
            if(sleeping) return;
            HeadSocket=Socket(head,"Cosmetic socket - head",new Vector3(0,.38f,0));
            BackSocket=Socket(transform,"Cosmetic socket - back",new Vector3(0,1.02f,-.36f));
            WaistSocket=Socket(transform,"Cosmetic socket - waist",new Vector3(0,.76f,0));
            LeftWristSocket=Socket(leftArm,"Cosmetic socket - wrist",new Vector3(0,-.36f,.02f));
            RightWristSocket=Socket(rightArm,"Cosmetic socket - wrist",new Vector3(0,-.36f,.02f));
            accessories=Socket(head,"Removable headband cosmetic",Vector3.zero);
            var bandParts=new List<Transform>();
            foreach(Transform part in head) if(part.name=="Red headband" || part.name=="Headband knot") bandParts.Add(part);
            foreach(var part in bandParts) part.SetParent(accessories,true);
            foreach(Transform part in head)
            {
                if(part.name=="Eye") { part.localScale=new Vector3(.13f,.17f,.045f); part.localPosition=new Vector3(Mathf.Sign(part.localPosition.x)*.195f,.065f,.408f); }
                if(part.name=="Pupil") { part.localScale=new Vector3(.064f,.095f,.028f); part.localPosition=new Vector3(Mathf.Sign(part.localPosition.x)*.193f,.06f,.448f); }
            }
            foreach(float side in new[]{-1f,1f})
            {
                Piece(head,"Amber iris",new Vector3(side*.193f,.06f,.433f),new Vector3(.092f,.125f,.025f),iris);
                Piece(head,"Eye sparkle",new Vector3(side*.193f-.017f,.087f,.464f),new Vector3(.025f,.028f,.012f),highlight);
                Piece(head,"Inner ear",new Vector3(side*.36f,.35f,.115f),new Vector3(.16f,.18f,.045f),skin);
                var brow=Piece(head,"Expressive brow",new Vector3(side*.2f,.196f,.387f),new Vector3(.24f,.065f,.075f),black);
                brow.localRotation=Quaternion.Euler(0,0,side*18);
                Piece(head,"Soft cheek",new Vector3(side*.28f,-.13f,.28f),new Vector3(.3f,.25f,.21f),white);
            }
            Piece(head,"Smile",new Vector3(0,-.19f,.478f),new Vector3(.19f,.033f,.025f),skin);
        }
        static Transform Socket(Transform parent,string name,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }
        static Transform Piece(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Sphere);obj.name=name;
            var collider=obj.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=scale;
            obj.GetComponent<Renderer>().sharedMaterial=material;return obj.transform;
        }
        Material Surface(string name,Color tint,float gloss,bool textured)
        {
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};mat.color=tint;
            mat.SetFloat("_Smoothness",gloss);
            if(textured) { mat.SetTexture("_BaseMap",fur);mat.SetTextureScale("_BaseMap",new Vector2(2,2)); }
            ownedMaterials.Add(mat);return mat;
        }
        static Texture2D BuildFur()
        {
            const int size=128;var pixels=new Color[size*size];var rng=new System.Random(72419);
            for(int i=0;i<pixels.Length;i++) { float v=.98f+(float)rng.NextDouble()*.02f;pixels[i]=new Color(v,v,v,1); }
            // Short, tapered directional strokes; one small shared texture, no strand simulation.
            for(int i=0;i<2400;i++)
            {
                int x=rng.Next(size),y=rng.Next(size),length=rng.Next(3,9);
                float shade=.92f+(float)rng.NextDouble()*.05f;
                for(int j=0;j<length;j++) { float v=Mathf.Lerp(shade,1,j/(float)length);pixels[((y+j)%size)*size+(x+j/4)%size]=new Color(v,v,v,1); }
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,true){name="Shared short panda fur",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear};
            var softened=new Color[pixels.Length];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                Color sum=Color.clear;
                for(int dy=-1;dy<=1;dy++) for(int dx=-1;dx<=1;dx++) sum+=pixels[((y+dy+size)%size)*size+(x+dx+size)%size];
                softened[y*size+x]=sum/9;
            }
            texture.SetPixels(softened);texture.Apply(true,true);return texture;
        }
        void AddFurOutline(Transform parent,int count,Material material,bool face)
        {
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
            const int sides=8,rings=4;
            for(int i=0;i<count;i++)
            {
                float y=1-2*(i+.5f)/count,a=i*2.39996323f,r=Mathf.Sqrt(1-y*y);
                var normal=new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r);
                if(face && normal.z>.45f && normal.y<.65f) continue;
                var tangent=Vector3.Cross(normal,Mathf.Abs(normal.y)>.9f ? Vector3.right : Vector3.up).normalized;
                var bitangent=Vector3.Cross(normal,tangent);
                // Low rounded domes, embedded at the roots. No cones or elongated tips.
                int start=vertices.Count;float height=face ? .045f : .032f,width=face ? .065f : .055f;
                for(int ring=0;ring<rings;ring++) for(int k=0;k<sides;k++)
                {
                    float theta=ring*Mathf.PI*.5f/rings,angle=k*Mathf.PI*2/sides;
                    var across=tangent*Mathf.Cos(angle)+bitangent*Mathf.Sin(angle);
                    vertices.Add(normal*(.486f+Mathf.Sin(theta)*height)+across*(Mathf.Cos(theta)*width));
                    normals.Add((normal*(Mathf.Sin(theta)/height)+across*(Mathf.Cos(theta)/width)).normalized);
                    uv.Add(new Vector2(k/(float)sides,ring/(float)rings));
                }
                int cap=vertices.Count;vertices.Add(normal*(.486f+height));normals.Add(normal);uv.Add(new Vector2(.5f,1));
                for(int ring=0;ring<rings-1;ring++) for(int k=0;k<sides;k++)
                {
                    int lo=start+ring*sides+k,next=start+ring*sides+(k+1)%sides,hi=lo+sides,hiNext=next+sides;
                    triangles.AddRange(new[]{lo,next,hi,next,hiNext,hi});
                }
                for(int k=0;k<sides;k++) triangles.AddRange(new[]{start+(rings-1)*sides+k,start+(rings-1)*sides+(k+1)%sides,cap});
            }
            var mesh=new Mesh{name="Batched rounded short-fur clumps"};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv);mesh.RecalculateBounds();ownedMeshes.Add(mesh);
            var obj=new GameObject("Fur silhouette");obj.transform.SetParent(parent,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        void OnDestroy()
        {
            foreach(var material in ownedMaterials) if(material!=null) Destroy(material);
            foreach(var mesh in ownedMeshes) if(mesh!=null) Destroy(mesh);
            if(fur!=null) Destroy(fur);
        }
    }
}

