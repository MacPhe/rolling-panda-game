using UnityEngine;

namespace RollingPanda
{
    // Normalized children stay inside the tested box and follow its pooled configuration.
    public sealed class ObstacleArt : MonoBehaviour
    {
        readonly GameObject[] variants=new GameObject[3];
        int current=-1;
        public void Initialize(Material stone,Material wood,Material red,Material gold,Material purple,Material moss)
        {
            GetComponent<Renderer>().enabled=false;
            for(int kind=0;kind<3;kind++)
            {
                var root=new GameObject(kind==0 ? "Weathered ruin" : kind==1 ? "Timber jump barrier" : "Temple slide lintel");
                root.transform.SetParent(transform,false); variants[kind]=root;
                if(kind==0)
                {
                    for(int row=0;row<5;row++) for(int col=0;col<2;col++)
                        Cube(root.transform,new Vector3((col-.5f)*.5f,-.4f+row*.2f,0),new Vector3(.494f,.194f,.99f),stone);
                    Cube(root.transform,new Vector3(0,0,-.498f),new Vector3(.16f,.95f,.003f),red);
                    Cube(root.transform,new Vector3(-.27f,.39f,-.498f),new Vector3(.32f,.09f,.003f),moss);
                }
                else
                {
                    Cube(root.transform,Vector3.zero,new Vector3(.99f,.99f,.99f),wood);
                    for(int board=0;board<4;board++)
                        Cube(root.transform,new Vector3(0,-.37f+board*.245f,-.498f),new Vector3(.98f,.022f,.003f),stone);
                    foreach(float side in new[]{-1f,1f})
                        Cube(root.transform,new Vector3(side*.35f,0,-.499f),new Vector3(.08f,.98f,.002f),kind==1 ? gold : purple);
                    Cube(root.transform,new Vector3(0,kind==1 ? .38f : -.38f,-.499f),new Vector3(.98f,.12f,.002f),kind==1 ? gold : purple);
                }
                root.SetActive(false);
            }
            Refresh();
        }
        void OnEnable(){ if(variants[0]!=null) Refresh(); }
        void LateUpdate(){ if(variants[0]!=null) Refresh(); }
        void Refresh()
        {
            float height=transform.localScale.y;
            int kind=height>3 ? 0 : height<1 ? 1 : 2;
            if(kind==current) return;
            current=kind;
            for(int i=0;i<3;i++) variants[i].SetActive(i==kind);
        }
        static void Cube(Transform parent,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Obstacle detail";
            go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial=material;
        }
    }
}
