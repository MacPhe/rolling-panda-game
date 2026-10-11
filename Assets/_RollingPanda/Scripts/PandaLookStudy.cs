using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace RollingPanda
{
    // Isolated look development. Not attached to the runner or any gameplay scene.
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
        void Awake()
        {
            var lit=Shader.Find("Universal Render Pipeline/Lit");var furShader=Shader.Find("Rolling Panda/Study Short Fur");
            if(lit==null || furShader==null){Debug.LogError("Panda study shaders unavailable. Check the Console for shader import errors.");return;}
            var key=GameObject.Find("Soft studio key");
            if(key!=null) {key.transform.rotation=Quaternion.Euler(35,-35,0);var light=key.GetComponent<Light>();light.intensity=1.15f;light.color=new Color(1,.96f,.91f);}
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.62f,.64f,.68f);
            fibres=FibreTexture();font=Resources.Load<Font>("Presentation/alagard");
            subject=new GameObject("Original panda bust").transform;subject.SetParent(transform,false);
            var ivory=FurMaterials(furShader,new Color(.94f,.9f,.8f),false);
            var charcoal=FurMaterials(furShader,new Color(.19f,.195f,.21f),true);
            headCoat=ivory;
            foreach(var m in headCoat){m.SetFloat("_FacePattern",1);m.SetFloat("_BandOn",1);m.SetColor("_PatchColour",new Color(.19f,.195f,.21f));}
            var cream=FurMaterials(furShader,new Color(.97f,.93f,.84f),false);
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
                Plain("Eye white",new Vector3(side*.235f,1.57f,.455f),new Vector3(.105f,.13f,.03f),eyeWhite);
                Plain("Amber iris",new Vector3(side*.222f,1.565f,.482f),new Vector3(.073f,.091f,.022f),iris);
                Plain("Pupil",new Vector3(side*.22f,1.565f,.502f),new Vector3(.044f,.063f,.014f),pupil);
                Plain("Eye catchlight",new Vector3(side*.22f-.022f,1.6f,.516f),new Vector3(.017f,.021f,.008f),sparkle);
                Coat("Soft muzzle",new Vector3(side*.105f,1.285f,.436f),new Vector3(.19f,.145f,.15f),cream,false);
                Coat("Friendly brow",new Vector3(side*.23f,1.742f,.401f),new Vector3(.13f,.028f,.025f),charcoal,false).localRotation=Quaternion.Euler(0,0,side*12);
            }
            var nosePart=Plain("Rounded triangular nose",new Vector3(0,1.36f,.585f),new Vector3(.115f,.074f,.055f),nose);
            nosePart.localRotation=Quaternion.Euler(0,0,180);
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
                m.SetTextureScale("_BaseMap",new Vector2(5,3));m.SetFloat("_Layer",i/7f);m.SetFloat("_FurLength",.014f);
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
            const int columns=40,rows=28;var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {
                float v=y/(float)rows,u=x/(float)columns,theta=v*Mathf.PI,a=u*Mathf.PI*2;
                var n=new Vector3(Mathf.Sin(theta)*Mathf.Cos(a),Mathf.Cos(theta),Mathf.Sin(theta)*Mathf.Sin(a));
                float fullness=cheeks ? 1+.1f*Mathf.Exp(-Mathf.Pow((n.y+.32f)/.42f,2)) : 1;
                vertices.Add(Vector3.Scale(n,new Vector3(radii.x*fullness,radii.y,radii.z)));
                normals.Add(new Vector3(n.x/(radii.x*fullness),n.y/radii.y,n.z/radii.z).normalized);uv.Add(new Vector2(u,v));
                if(y<rows && x<columns){int p=y*(columns+1)+x;indices.AddRange(new[]{p,p+1,p+columns+1,p+1,p+columns+2,p+columns+1});}
            }
            var mesh=new Mesh{name=cheeks ? "Continuous soft cheek silhouette" : "Smooth study surface"};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();meshes.Add(mesh);return mesh;
        }
        void Smile(Material material)
        {
            Plain("Nose to smile",new Vector3(0,1.28f,.59f),new Vector3(.008f,.045f,.009f),material);
            for(int i=0;i<15;i++)
            {
                float x=Mathf.Lerp(-.15f,.15f,i/14f),y=1.235f+.045f*Mathf.Pow(x/.15f,2);
                Plain("Smile detail",new Vector3(x,y,.582f-.015f*Mathf.Abs(x/.15f)),new Vector3(.018f,.008f,.009f),material);
            }
        }
        void Band(Material material)
        {
            const int count=64;var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
            for(int i=0;i<=count;i++)for(int row=0;row<2;row++)
            {
                float a=i*Mathf.PI*2/count,y=1.79f+row*.095f;
                float radius=Mathf.Sqrt(1-Mathf.Pow((y-1.5f)/.54f,2));
                vertices.Add(new Vector3(Mathf.Cos(a)*(.63f*radius+.012f),y,Mathf.Sin(a)*(.47f*radius+.012f)));uv.Add(new Vector2(i/(float)count,row));
                if(i<count && row==0){int p=i*2;indices.AddRange(new[]{p,p+1,p+2,p+1,p+3,p+2});}
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
            for(int i=0;i<4200;i++)
            {
                int x=rng.Next(size),y=rng.Next(size),length=rng.Next(5,18);float height=.25f+(float)rng.NextDouble()*.75f;
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
            if(!built)return;subject.localRotation=Quaternion.Euler(0,yaw,0);
#if !UNITY_EDITOR && !UNITY_STANDALONE
            var touch=Touchscreen.current;if(touch==null)return;var t=touch.primaryTouch;var p=t.position.ReadValue();p.y=Screen.height-p.y;
            if(t.press.wasPressedThisFrame){pressed=-1;pressPosition=p;for(int i=0;i<4;i++)if(ButtonRect(i).Contains(p))pressed=i;}
            if(t.press.wasReleasedThisFrame){if(pressed>=0 && ButtonRect(pressed).Contains(p) && Vector2.Distance(p,pressPosition)<24)Action(pressed);pressed=-1;}
#endif
        }
        void OnGUI()
        {
            if(!built)return;if(caption==null){caption=new GUIStyle(GUI.skin.label){font=font,fontSize=20};button=new GUIStyle(GUI.skin.button){font=font,fontSize=18};}
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
