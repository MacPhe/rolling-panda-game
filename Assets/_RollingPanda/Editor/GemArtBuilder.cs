#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RollingPanda
{
    // Resolves user-imported assets locally; no vendor source assets are redistributed.
    [InitializeOnLoad]
    static class GemArtBuilder
    {
        const string Model="Assets/BTM_Assets/BTM_Items_Gems/Models/Diamondo.fbx";
        const string Folder="Assets/_RollingPanda/Resources/Presentation";
        static GemArtBuilder(){EditorApplication.delayCall+=Build;EditorApplication.playModeStateChanged+=OnPlayMode;}
        static void OnPlayMode(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall+=Build;}
        [MenuItem("Rolling Panda/Refresh Diamondo HUD art")]
        static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            if(model==null) return;
            var importer=AssetImporter.GetAtPath(Model) as ModelImporter;
            if(importer!=null && !importer.isReadable)
            {
                importer.isReadable=true;importer.SaveAndReimport();EditorApplication.delayCall+=Build;return;
            }
            var filter=model.GetComponentInChildren<MeshFilter>();
            if(filter==null || filter.sharedMesh==null) return;
            Directory.CreateDirectory(Folder);
            var data=AssetDatabase.LoadAssetAtPath<GameArt>(Folder+"/GameArt.asset");
            if(data!=null && data.Diamond==filter.sharedMesh && data.BlueIcon!=null && data.RedIcon!=null && data.PouchIcon!=null) return;
            if(data==null){data=ScriptableObject.CreateInstance<GameArt>();AssetDatabase.CreateAsset(data,Folder+"/GameArt.asset");}
            data.Diamond=filter.sharedMesh;
            data.BlueIcon=Bake(filter.sharedMesh,new Color(.05f,.55f,1),"BlueGem");
            data.RedIcon=Bake(filter.sharedMesh,new Color(1,.06f,.15f),"RedGem");
            data.PouchIcon=Bake(filter.sharedMesh,new Color(.1f,1,.25f),"GreenPouch");
            EditorUtility.SetDirty(data);AssetDatabase.SaveAssets();
            Debug.Log("Rolling Panda: Diamondo mesh and three matching HUD icons prepared.");
        }
        static Texture2D Bake(Mesh mesh,Color tint,string name)
        {
            const int size=96;
            var pixels=new Color[size*size];var depth=new float[size*size];
            for(int i=0;i<depth.Length;i++) depth[i]=float.NegativeInfinity;
            var source=mesh.vertices;var projected=new Vector3[source.Length];
            var turn=Quaternion.Euler(12,35,0);
            Vector3 min=Vector3.one*float.MaxValue,max=Vector3.one*float.MinValue;
            for(int i=0;i<source.Length;i++){projected[i]=turn*(source[i]-mesh.bounds.center);min=Vector3.Min(min,projected[i]);max=Vector3.Max(max,projected[i]);}
            float scale=80/Mathf.Max(.001f,Mathf.Max(max.x-min.x,max.y-min.y));
            Vector3 centre=(min+max)*.5f;
            for(int i=0;i<projected.Length;i++) projected[i]=(projected[i]-centre)*scale+new Vector3(48,48,0);
            var tris=mesh.triangles;
            for(int tri=0;tri<tris.Length;tri+=3)
            {
                Vector3 a=projected[tris[tri]],b=projected[tris[tri+1]],c=projected[tris[tri+2]];
                float area=Edge(a,b,c);if(Mathf.Abs(area)<.00001f) continue;
                Vector3 normal=Vector3.Cross(b-a,c-a).normalized;
                Color colour=tint*Mathf.Lerp(.4f,1.1f,Mathf.Abs(Vector3.Dot(normal,new Vector3(-.4f,.7f,1).normalized)));colour.a=1;
                int x0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(b.x,c.x))),0,95),x1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x,Mathf.Max(b.x,c.x))),0,95);
                int y0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y,Mathf.Min(b.y,c.y))),0,95),y1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y,Mathf.Max(b.y,c.y))),0,95);
                for(int y=y0;y<=y1;y++) for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector3(x+.5f,y+.5f,0);
                    float u=Edge(b,c,p)/area,v=Edge(c,a,p)/area,w=1-u-v;
                    if(u<0 || v<0 || w<0) continue;
                    float z=u*a.z+v*b.z+w*c.z;int index=y*size+x;
                    if(z<=depth[index]) continue;depth[index]=z;pixels[index]=colour;
                }
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.SetPixels(pixels);texture.Apply();
            string path=Folder+"/"+name+".png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static float Edge(Vector3 a,Vector3 b,Vector3 p)=>(p.x-a.x)*(b.y-a.y)-(p.y-a.y)*(b.x-a.x);
    }
}
#endif

