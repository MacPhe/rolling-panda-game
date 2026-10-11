using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace RollingPanda
{
    // One shared sparkle mesh/material and one polished material per pickup colour.
    public sealed class GemPresentation : MonoBehaviour
    {
        readonly Dictionary<Material,Material> polished=new Dictionary<Material,Material>();
        int phase;
        public float NextPhase() => (++phase*2.39996f)%(Mathf.PI*2);
        public Material Sparkle {get;private set;}
        public Mesh Star {get;private set;}
        void Awake()
        {
            Sparkle=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Shared gem glint"};
            Sparkle.SetFloat("_Surface",1);Sparkle.SetFloat("_ZWrite",0);Sparkle.SetFloat("_Cull",0);
            Sparkle.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);Sparkle.SetFloat("_DstBlend",(float)BlendMode.One);
            Sparkle.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");Sparkle.renderQueue=3000;
            Star=new Mesh{name="Four point gem glint"};
            Star.vertices=new[]{Vector3.zero,new Vector3(0,.5f,0),new Vector3(.065f,.065f,0),new Vector3(.5f,0,0),new Vector3(.065f,-.065f,0),new Vector3(0,-.5f,0),new Vector3(-.065f,-.065f,0),new Vector3(-.5f,0,0),new Vector3(-.065f,.065f,0)};
            var triangles=new int[24];for(int i=0;i<8;i++){triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%8+1;}
            Star.triangles=triangles;Star.RecalculateNormals();Star.RecalculateBounds();
        }
        public Material Polished(Material source)
        {
            if(source==null) return null;
            if(polished.TryGetValue(source,out var material)) return material;
            material=new Material(source){name="Polished "+source.name};
            material.SetFloat("_Smoothness",.85f);material.SetFloat("_Metallic",.12f);
            Color color=source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : source.color;
            material.SetColor("_EmissionColor",color*.18f);material.EnableKeyword("_EMISSION");
            polished[source]=material;return material;
        }
        void OnDestroy()
        {
            foreach(var material in polished.Values) Destroy(material);
            if(Sparkle!=null) Destroy(Sparkle);if(Star!=null) Destroy(Star);
        }
    }
}
