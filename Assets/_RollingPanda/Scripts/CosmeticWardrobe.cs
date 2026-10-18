using System.Collections.Generic;
using UnityEngine;
namespace RollingPanda
{
    // Per-renderer overrides keep previews independent of shared character materials.
    public sealed class CosmeticWardrobe : MonoBehaviour
    {
        struct Surface {public Renderer Renderer;public bool Dark,Face,Band;}
        readonly List<Surface> surfaces=new List<Surface>();readonly List<Material> materials=new List<Material>();
        readonly List<Transform> roots=new List<Transform>();
        MaterialPropertyBlock block;Material beltMaterial,outfitMaterial,trim,straw;
        Transform belt,kimono,armour,helmet,hat;Transform[] kimonoSleeves,armourShoulders;
        Mesh hatMesh;int revision=-1;string preview;bool built;
        public static CosmeticWardrobe Attach(Transform model)
        {
            foreach(var old in model.GetComponents<CosmeticWardrobe>()){old.enabled=false;Destroy(old);}
            foreach(var part in model.GetComponentsInChildren<Transform>(true))if(part.name.StartsWith("Shop cosmetic root")){part.gameObject.SetActive(false);Destroy(part.gameObject);}
            var view=model.gameObject.AddComponent<CosmeticWardrobe>();view.Initialize();return view;
        }
        Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};m.SetFloat("_Smoothness",.18f);materials.Add(m);return m;}
        Transform Root(Transform parent,string name){var root=new GameObject("Shop cosmetic root "+name).transform;root.SetParent(parent,false);roots.Add(root);return root;}
        Transform Part(Transform parent,Vector3 pos,Vector3 scale,Material material,PrimitiveType type=PrimitiveType.Cube)
        {
            var go=GameObject.CreatePrimitive(type);go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;
            var c=go.GetComponent<Collider>();c.enabled=false;Destroy(c);var r=go.GetComponent<Renderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return go.transform;
        }
        void Initialize()
        {
            block=new MaterialPropertyBlock();
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))
            {
                var m=renderer.sharedMaterial;if(m==null)continue;
                bool fur=m.shader.name=="Rolling Panda/Study Short Fur",band=false;
                for(Transform t=renderer.transform;t!=null && t!=transform;t=t.parent)if(t.name=="Removable red headband")band=true;
                if(fur || band)surfaces.Add(new Surface{Renderer=renderer,Dark=fur && m.GetColor("_BaseColor").r<.5f,Face=fur && m.GetFloat("_FacePattern")>.5f,Band=band});
            }
            var head=transform.Find("Head tuck pivot");if(head==null)head=transform.Find("Head");if(head==null)head=transform;
            beltMaterial=Mat("Karate belt cloth",Color.white);outfitMaterial=Mat("Cosmetic cloth",new Color(.3f,.14f,.35f));trim=Mat("Costume warm trim",new Color(.65f,.43f,.16f));straw=Mat("Woven bamboo straw",new Color(.61f,.43f,.21f));
            belt=Root(transform,"belt");Part(belt,new Vector3(0,.73f,0),new Vector3(.88f,.12f,.7f),beltMaterial,PrimitiveType.Cylinder);
            Part(belt,new Vector3(0,.74f,.37f),new Vector3(.15f,.13f,.08f),beltMaterial);
            for(int side=-1;side<=1;side+=2)Part(belt,new Vector3(side*.07f,.59f,.38f),new Vector3(.085f,.25f,.045f),beltMaterial).localRotation=Quaternion.Euler(0,0,side*15);
            kimono=Root(transform,"kimono");Part(kimono,new Vector3(0,1,.29f),new Vector3(.84f,.53f,.15f),outfitMaterial);Part(kimono,new Vector3(0,1,-.28f),new Vector3(.83f,.55f,.15f),outfitMaterial);
            for(int side=-1;side<=1;side+=2)Part(kimono,new Vector3(side*.13f,1.12f,.39f),new Vector3(.12f,.44f,.035f),trim).localRotation=Quaternion.Euler(0,0,side*30);
            armour=Root(transform,"samurai armour");for(int row=0;row<4;row++)Part(armour,new Vector3(0,1.2f-row*.14f,.34f),new Vector3(.78f,.11f,.14f),outfitMaterial);
            Part(armour,new Vector3(0,1,-.34f),new Vector3(.82f,.6f,.12f),outfitMaterial);
            kimonoSleeves=new Transform[2];armourShoulders=new Transform[2];
            for(int i=0;i<2;i++)
            {
                var arm=transform.Find(i==0 ? "Left arm pivot" : "Right arm pivot");if(arm==null)arm=transform;
                kimonoSleeves[i]=Root(arm,"kimono sleeve");Part(kimonoSleeves[i],new Vector3(0,-.16f,.015f),new Vector3(.32f,.36f,.34f),outfitMaterial);
                armourShoulders[i]=Root(arm,"samurai shoulder");for(int row=0;row<2;row++)Part(armourShoulders[i],new Vector3(i==0 ? -.06f : .06f,-row*.1f,0),new Vector3(.38f,.08f,.38f),outfitMaterial);
            }
            helmet=Root(head,"samurai crest");Part(helmet,new Vector3(0,.43f,.02f),new Vector3(.96f,.3f,.72f),outfitMaterial,PrimitiveType.Sphere);
            for(int side=-1;side<=1;side+=2)Part(helmet,new Vector3(side*.18f,.59f,.26f),new Vector3(.06f,.23f,.06f),trim).localRotation=Quaternion.Euler(0,0,-side*32);
            hat=Root(head,"straw hat");var hatObject=new GameObject("Woven conical hat");hatObject.transform.SetParent(hat,false);hatObject.transform.localPosition=new Vector3(0,.43f,0);
            const int sides=32;var vertices=new Vector3[sides+1];var indices=new int[sides*3];vertices[0]=new Vector3(0,.26f,0);
            for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;vertices[i+1]=new Vector3(Mathf.Cos(a)*.72f,0,Mathf.Sin(a)*.72f);indices[i*3]=0;indices[i*3+1]=(i+1)%sides+1;indices[i*3+2]=i+1;}
            hatMesh=new Mesh{name="Original bamboo straw hat"};hatMesh.vertices=vertices;hatMesh.triangles=indices;hatMesh.RecalculateNormals();hatMesh.RecalculateBounds();hatMesh.UploadMeshData(true);
            straw.SetFloat("_Cull",0);hatObject.AddComponent<MeshFilter>().sharedMesh=hatMesh;hatObject.AddComponent<MeshRenderer>().sharedMaterial=straw;
            built=true;Refresh();
        }
        public void Preview(string id){preview=id;Refresh();}
        void LateUpdate(){if(built && revision!=PlayerProfile.Revision)Refresh();}
        int Variant(CosmeticSlot slot){var candidate=ShopCatalog.Find(preview);var item=candidate!=null && candidate.Slot==slot ? candidate : ShopCatalog.Find(PlayerProfile.Inventory.Equipped(slot));return item==null ? 0 : item.Variant;}
        void Refresh()
        {
            if(!built)return;revision=PlayerProfile.Revision;
            int coat=Variant(CosmeticSlot.Coat),band=Variant(CosmeticSlot.Headband),b=Variant(CosmeticSlot.Belt),outfit=Variant(CosmeticSlot.Outfit),h=Variant(CosmeticSlot.Hat);
            Color ink=coat==1 ? new Color(.13f,.23f,.36f) : coat==2 ? new Color(.31f,.14f,.29f) : coat==3 ? new Color(.4f,.2f,.1f) : new Color(.19f,.195f,.21f);
            Color ribbon=band==1 ? new Color(.08f,.42f,.9f) : band==2 ? new Color(.08f,.6f,.35f) : band==3 ? new Color(.95f,.65f,.12f) : new Color(.73f,.045f,.025f);
            foreach(var surface in surfaces)
            {
                if(surface.Renderer==null)continue;
                surface.Renderer.GetPropertyBlock(block);
                if(surface.Band)block.SetColor("_BaseColor",ribbon);else{if(surface.Dark)block.SetColor("_BaseColor",ink);if(surface.Face)block.SetColor("_PatchColour",ink);}
                surface.Renderer.SetPropertyBlock(block);block.Clear();
            }
            belt.gameObject.SetActive(b>0);beltMaterial.color=b==2 ? new Color(.055f,.06f,.065f) : new Color(.9f,.86f,.75f);
            kimono.gameObject.SetActive(outfit==1);armour.gameObject.SetActive(outfit==2);helmet.gameObject.SetActive(outfit==2 && h==0);hat.gameObject.SetActive(h==1);
            outfitMaterial.color=outfit==2 ? new Color(.13f,.22f,.2f) : new Color(.34f,.16f,.35f);
            foreach(var sleeve in kimonoSleeves)sleeve.gameObject.SetActive(outfit==1);foreach(var shoulder in armourShoulders)shoulder.gameObject.SetActive(outfit==2);
        }
        public static void Sleeping(Transform root,bool sleep)
        {
            foreach(var part in root.GetComponentsInChildren<Transform>(true))
            {
                string n=part.name;if(n=="Eye white" || n=="Amber iris" || n=="Pupil" || n=="Eye catchlight" || n=="Soft upper eyelid")part.gameObject.SetActive(!sleep);
                if(n=="Sleeping eyelid")part.gameObject.SetActive(sleep);
            }
        }
        void OnDestroy(){foreach(var root in roots)if(root!=null)Destroy(root.gameObject);foreach(var material in materials)if(material!=null)Destroy(material);if(hatMesh!=null)Destroy(hatMesh);}
    }
}
