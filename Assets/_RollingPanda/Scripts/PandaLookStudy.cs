using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RollingPanda
{
    // Shared approved character surfaces, with an optional isolated look-development preview.
    public sealed class PandaLookStudy : MonoBehaviour
    {
        readonly List<Mesh> meshes=new List<Mesh>();readonly List<Material> materials=new List<Material>();
        readonly List<Material> darkCoat=new List<Material>();
        Material[] headCoat;
        Texture2D fibres;Transform subject,accessory;float yaw=180;int palette;
        bool built;Font font;GUIStyle caption,button;
#if !UNITY_EDITOR && !UNITY_STANDALONE
        int pressed=-1;Vector2 pressPosition;
#endif
        bool gameplay;
        void Start(){if(!built)Build(false);}
        public bool BuildGameplay(Transform head,Transform body)
        {
            gameplay=true;Build(true);if(!built)return false;
            var shoulders=subject.Find("Shoulders");if(shoulders!=null){shoulders.gameObject.SetActive(false);Destroy(shoulders.gameObject);}
            subject.SetParent(head,false);subject.localScale=Vector3.one*.74f;subject.localPosition=new Vector3(0,-1.11f,0);subject.localRotation=Quaternion.identity;
            var shader=Shader.Find("Rolling Panda/Study Short Fur");
            var pale=FurMaterials(shader,new Color(.94f,.9f,.8f),false);var dark=FurMaterials(shader,new Color(.19f,.195f,.21f),true);
            var originals=body.GetComponentsInChildren<MeshRenderer>(true);
            foreach(var renderer in originals)
            {
                if(renderer.transform.IsChildOf(head))continue;
                string n=renderer.name;if(n!="Pear shaped body" && n!="Shoulders" && n!="Left arm" && n!="Right arm" && n!="Left leg" && n!="Right leg" && n!="Tail")continue;
                var coat=n=="Pear shaped body" || n=="Tail" ? pale : dark;renderer.sharedMaterial=coat[0];
                // Three outer layers for the smaller body surfaces; no extra colliders.
                var filter=renderer.GetComponent<MeshFilter>();if(filter==null)continue;
                for(int i=2;i<coat.Length;i+=2)
                {
                    var shell=new GameObject("Short body fur "+i);shell.transform.SetParent(renderer.transform,false);
                    shell.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
                    var surface=shell.AddComponent<MeshRenderer>();surface.sharedMaterial=coat[i];surface.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
            enabled=false;return true;
        }
        void Build(bool forGameplay)
        {
            var lit=Shader.Find("Universal Render Pipeline/Lit");var furShader=Shader.Find("Rolling Panda/Study Short Fur");
            if(lit==null || furShader==null){Debug.LogError("Panda study shaders unavailable. Check the Console for shader import errors.");return;}
            if(!forGameplay)
            {
            var key=GameObject.Find("Soft studio key");
            if(key!=null) {key.transform.rotation=Quaternion.Euler(35,-35,0);var light=key.GetComponent<Light>();light.intensity=1.15f;light.color=new Color(1,.96f,.91f);}
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.62f,.64f,.68f);
            }
            fibres=FibreTexture();font=Resources.Load<Font>("Presentation/alagard");
            subject=new GameObject("Original panda bust").transform;subject.SetParent(transform,false);
            var ivory=FurMaterials(furShader,new Color(.94f,.9f,.8f),false);
            var charcoal=FurMaterials(furShader,new Color(.19f,.195f,.21f),true);
            headCoat=ivory;
            foreach(var m in headCoat){m.SetFloat("_FacePattern",1);m.SetFloat("_BandOn",1);m.SetColor("_PatchColour",new Color(.19f,.195f,.21f));}
            var nose=Solid(lit,new Color(.045f,.035f,.03f),.3f);
            var eyeWhite=Solid(lit,new Color(1,.975f,.9f),.55f);
            var iris=Solid(lit,new Color(.31f,.13f,.035f),.7f);
            var pupil=Solid(lit,new Color(.008f,.012f,.018f),.8f);
            var sparkle=Solid(lit,Color.white,.6f);
            Coat("Shoulders",new Vector3(0,.82f,0),new Vector3(.56f,.35f,.33f),charcoal,false);
            Coat("Continuous cheek and forehead mesh",new Vector3(0,1.5f,0),new Vector3(.63f,.54f,.47f),ivory,true);
            foreach(float side in new[]{-1f,1f})
            {
                Coat("Rounded ear",new Vector3(side*.48f,1.96f,-.01f),new Vector3(.17f,.18f,.12f),charcoal,false);
                Plain("Inner ear",new Vector3(side*.48f,1.96f,.1f),new Vector3(.095f,.11f,.022f),nose);
                Plain("Eye white",new Vector3(side*.235f,1.57f,.455f),new Vector3(.108f,.114f,.03f),eyeWhite);
                Plain("Amber iris",new Vector3(side*.222f,1.565f,.482f),new Vector3(.083f,.093f,.019f),iris);
                Plain("Pupil",new Vector3(side*.22f,1.565f,.502f),new Vector3(.052f,.066f,.012f),pupil);
                Plain("Eye catchlight",new Vector3(side*.22f-.022f,1.6f,.516f),new Vector3(.017f,.021f,.008f),sparkle);
                var lidPoints=new List<Vector3>();
                for(int i=0;i<=20;i++)
                {
                    float angle=Mathf.Lerp(.08f,Mathf.PI-.08f,i/20f);
                    lidPoints.Add(new Vector3(side*.235f+Mathf.Cos(angle)*.108f,1.57f+Mathf.Sin(angle)*.106f,.463f));
                }
                Tube("Soft upper eyelid",lidPoints,.013f,charcoal[0]);
                Coat("Friendly brow",new Vector3(side*.23f,1.742f,.401f),new Vector3(.115f,.022f,.021f),charcoal,false).localRotation=Quaternion.Euler(0,0,side*5);
            }
            var nosePart=Plain("Rounded triangular nose",new Vector3(0,1.36f,FaceDepth(0,1.36f)+.012f),new Vector3(.1f,.063f,.045f),nose);
            var noseMesh=nosePart.GetComponent<MeshFilter>().sharedMesh;var noseVertices=noseMesh.vertices;
            for(int i=0;i<noseVertices.Length;i++)noseVertices[i].x*=1+.28f*(noseVertices[i].y/.063f);
            noseMesh.vertices=noseVertices;noseMesh.RecalculateNormals();noseMesh.RecalculateBounds();
            Smile(nose);
            accessory=new GameObject("Removable red headband").transform;accessory.SetParent(subject,false);
            var red=Solid(lit,new Color(.73f,.045f,.025f),.15f);
            Band(red);
            built=true;
        }
        Material Solid(Shader shader,Color colour,float smoothness)
        {
            var m=new Material(shader);m.color=colour;m.SetFloat("_Smoothness",smoothness);materials.Add(m);return m;
        }
        Material[] FurMaterials(Shader shader,Color colour,bool recolour)
        {
            // Six shells plus an opaque base; the same mesh is reused across layers.
            var set=new Material[7];for(int i=0;i<set.Length;i++)
            {
                var m=new Material(shader);m.SetColor("_BaseColor",colour);m.SetTexture("_BaseMap",fibres);
                m.SetTextureScale("_BaseMap",new Vector2(4,2));m.SetFloat("_Layer",i/7f);m.SetFloat("_FurLength",.014f);
                materials.Add(m);set[i]=m;if(recolour)darkCoat.Add(m);
            }
            return set;
        }
        Transform Coat(string name,Vector3 position,Vector3 radii,Material[] coat,bool cheeks)
        {
            var root=new GameObject(name).transform;root.SetParent(subject,false);root.localPosition=position;
            var mesh=Ellipsoid(radii,cheeks);
            int layers=name=="Eye patch" || name=="Soft muzzle" || name=="Friendly brow" ? 1 : coat.Length;
            for(int i=0;i<layers;i++)
            {
                var shell=new GameObject(i==0 ? "Base surface" : "Short fibre layer "+i);shell.transform.SetParent(root,false);
                shell.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=shell.AddComponent<MeshRenderer>();renderer.sharedMaterial=coat[i];
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return root;
        }
        Transform Plain(string name,Vector3 position,Vector3 radii,Material material)
        {
            var root=new GameObject(name).transform;root.SetParent(subject,false);root.localPosition=position;
            root.gameObject.AddComponent<MeshFilter>().sharedMesh=Ellipsoid(radii,false);root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;return root;
        }
        Mesh Ellipsoid(Vector3 radii,bool cheeks)
        {
            int columns=cheeks ? 80 : 40,rows=cheeks ? 56 : 28;var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {
                float v=y/(float)rows,u=x/(float)columns,theta=v*Mathf.PI,a=u*Mathf.PI*2;
                var n=new Vector3(Mathf.Sin(theta)*Mathf.Cos(a),Mathf.Cos(theta),Mathf.Sin(theta)*Mathf.Sin(a));
                float fullness=cheeks ? 1+.1f*Mathf.Exp(-Mathf.Pow((n.y+.32f)/.42f,2)) : 1;
                var point=Vector3.Scale(n,new Vector3(radii.x*fullness,radii.y,radii.z));
                if(cheeks && n.z>0)point.z+=MuzzleBlend(point.x,point.y)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.2f,.8f,n.z));
                vertices.Add(point);
                normals.Add(new Vector3(n.x/(radii.x*fullness),n.y/radii.y,n.z/radii.z).normalized);uv.Add(new Vector2(u,v));
                if(y<rows && x<columns){int p=y*(columns+1)+x;indices.AddRange(new[]{p,p+1,p+columns+1,p+1,p+columns+2,p+columns+1});}
            }
            var mesh=new Mesh{name=cheeks ? "Continuous soft cheek silhouette" : "Smooth study surface"};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);
            if(cheeks)
            {
                mesh.RecalculateNormals();var smoothed=mesh.normals;
                for(int row=0;row<=rows;row++){int lo=row*(columns+1),hi=lo+columns;var average=(smoothed[lo]+smoothed[hi]).normalized;smoothed[lo]=smoothed[hi]=average;}
                for(int col=0;col<=columns;col++){smoothed[col]=Vector3.up;smoothed[rows*(columns+1)+col]=Vector3.down;}
                mesh.normals=smoothed;
            }
            mesh.RecalculateBounds();meshes.Add(mesh);return mesh;
        }
        static float MuzzleBlend(float x,float localY)
        {
            return .15f*Mathf.Exp(-Mathf.Pow(x/.28f,2)-Mathf.Pow((localY+.2f)/.18f,2));
        }
        static float FaceDepth(float x,float worldY)
        {
            float y=(worldY-1.5f)/.54f,fullness=1+.1f*Mathf.Exp(-Mathf.Pow((y+.32f)/.42f,2));
            float z=Mathf.Sqrt(Mathf.Max(0,1-y*y-Mathf.Pow(x/(.63f*fullness),2)));
            return .47f*z+MuzzleBlend(x,worldY-1.5f)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.2f,.8f,z));
        }
        void Smile(Material material)
        {
            var smile=new List<Vector3>();
            for(int i=0;i<=32;i++)
            {
                float x=Mathf.Lerp(-.18f,.18f,i/32f),y=1.245f+.045f*Mathf.Pow(x/.18f,2);
                smile.Add(new Vector3(x,y,FaceDepth(x,y)+.007f));
            }
            Tube("Continuous gentle smile",smile,.006f,material);
            var stem=new List<Vector3>();for(int i=0;i<=8;i++){float y=Mathf.Lerp(1.31f,1.245f,i/8f);stem.Add(new Vector3(0,y,FaceDepth(0,y)+.007f));}
            Tube("Nose to smile crease",stem,.005f,material);
        }
        void Tube(string name,List<Vector3> points,float radius,Material material)
        {
            const int sides=8;var vertices=new List<Vector3>();var normals=new List<Vector3>();var indices=new List<int>();
            for(int i=0;i<points.Count;i++)
            {
                var tangent=(points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(i-1,0)]).normalized;
                var side=Vector3.Cross(tangent,Vector3.forward).normalized;var other=Vector3.Cross(tangent,side).normalized;
                for(int k=0;k<sides;k++)
                {
                    float a=k*Mathf.PI*2/sides;var normal=side*Mathf.Cos(a)+other*Mathf.Sin(a);vertices.Add(points[i]+normal*radius);normals.Add(normal);
                    if(i<points.Count-1){int p=i*sides+k,n=i*sides+(k+1)%sides;indices.AddRange(new[]{p,n,p+sides,n,n+sides,p+sides});}
                }
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();meshes.Add(mesh);
            var obj=new GameObject(name);obj.transform.SetParent(subject,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        void Band(Material material)
        {
            const int count=64,bandRows=6;var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
            for(int i=0;i<=count;i++)for(int row=0;row<=bandRows;row++)
            {
                float a=i*Mathf.PI*2/count,y=1.79f+row/(float)bandRows*.095f;
                float radius=Mathf.Sqrt(1-Mathf.Pow((y-1.5f)/.54f,2));
                float fold=.004f*Mathf.Sin(row/(float)bandRows*Mathf.PI*4+a*.65f);
                vertices.Add(new Vector3(Mathf.Cos(a)*(.63f*radius+.015f+fold),y,Mathf.Sin(a)*(.47f*radius+.015f+fold)));uv.Add(new Vector2(i/(float)count,row/(float)bandRows));
                if(i<count && row<bandRows){int p=i*(bandRows+1)+row,next=p+bandRows+1;indices.AddRange(new[]{p,p+1,next,p+1,next+1,next});}
            }
            var mesh=new Mesh{name="Separate curved cloth band"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();meshes.Add(mesh);
            accessory.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;accessory.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
            for(int i=0;i<2;i++)
            {
                var tail=Plain("Headband cloth tail",new Vector3(.57f+i*.045f,1.77f-i*.09f,-.12f),new Vector3(.22f,.045f,.025f),material);
                tail.localRotation=Quaternion.Euler(0,15,-25-i*25);tail.SetParent(accessory,true);
            }
        }
        static Texture2D FibreTexture()
        {
            const int size=256;var data=new Color[size*size];var rng=new System.Random(741);
            for(int i=0;i<data.Length;i++) data[i]=new Color(.97f,0,0,1);
            // Fine, curving upright fibres. Density stores strand height for shell clipping.
            for(int i=0;i<3000;i++)
            {
                int x=rng.Next(size),y=rng.Next(size),length=rng.Next(10,26);float height=.25f+(float)rng.NextDouble()*.75f;
                for(int k=0;k<length;k++)
                {
                    int px=(x+(int)(Mathf.Sin(k*.15f)*1.5f))%size,py=(y+k)%size;int p=py*size+px;
                    float density=height*Mathf.Sin((k+.5f)/length*Mathf.PI);
                    if(density>data[p].g)data[p]=new Color(.82f+.16f*density,density,0,1);
                }
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,true,true){name="Original short fibre coverage",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear};texture.SetPixels(data);texture.Apply(true,true);return texture;
        }
        Rect ButtonRect(int index)
        {
            var safe=Screen.safeArea;float width=Mathf.Min(150,(safe.width-32)/4);
            return new Rect(safe.x+16+index*width,Screen.height-safe.y-62,width-6,46);
        }
        void Action(int index)
        {
            if(index==0) yaw-=30;else if(index==1)yaw+=30;
            else if(index==2){accessory.gameObject.SetActive(!accessory.gameObject.activeSelf);foreach(var m in headCoat)m.SetFloat("_BandOn",accessory.gameObject.activeSelf ? 1 : 0);}
            else {palette=(palette+1)%3;Color[] colours={new Color(.19f,.195f,.21f),new Color(.19f,.09f,.18f),new Color(.27f,.105f,.055f)};foreach(var m in darkCoat)m.SetColor("_BaseColor",colours[palette]);foreach(var m in headCoat)m.SetColor("_PatchColour",colours[palette]);}
        }
        void Update()
        {
            if(!built || gameplay)return;subject.localRotation=Quaternion.Euler(0,yaw,0);
#if !UNITY_EDITOR && !UNITY_STANDALONE
            var touch=Touchscreen.current;if(touch==null)return;var t=touch.primaryTouch;var p=t.position.ReadValue();p.y=Screen.height-p.y;
            if(t.press.wasPressedThisFrame){pressed=-1;pressPosition=p;for(int i=0;i<4;i++)if(ButtonRect(i).Contains(p))pressed=i;}
            if(t.press.wasReleasedThisFrame){if(pressed>=0 && ButtonRect(pressed).Contains(p) && Vector2.Distance(p,pressPosition)<24)Action(pressed);pressed=-1;}
#endif
        }
        void OnGUI()
        {
            if(!built || gameplay)return;if(caption==null){caption=new GUIStyle(GUI.skin.label){font=font,fontSize=20};button=new GUIStyle(GUI.skin.button){font=font,fontSize=18};}
            GUI.Label(new Rect(Screen.safeArea.x+16,Screen.height-Screen.safeArea.yMax+12,Screen.safeArea.width-32,60),"Panda look study - face / short fur / removable band",caption);
            string[] labels={"Turn left","Turn right","Headband","Coat colour"};for(int i=0;i<4;i++)
            {
                bool clicked=GUI.Button(ButtonRect(i),labels[i],button);
#if UNITY_EDITOR || UNITY_STANDALONE
                if(clicked)Action(i);
#endif
            }
        }
        void OnDestroy(){foreach(var m in meshes)if(m!=null)Destroy(m);foreach(var m in materials)if(m!=null)Destroy(m);if(fibres!=null)Destroy(fibres);}
    }
}

