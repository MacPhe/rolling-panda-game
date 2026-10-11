Shader "Rolling Panda/Study Short Fur"
{
    Properties
    {
        _BaseMap("Fibre texture",2D)="white"{}
        _BaseColor("Coat colour",Color)=(1,1,1,1)
        _Layer("Shell fraction",Range(0,1))=0
        _FurLength("Short coat length",Float)=0.018
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            Cull Back ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;half4 _BaseColor;float _Layer;float _FurLength;
            CBUFFER_END
            struct Input {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
            struct Output {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float2 uv:TEXCOORD1;};
            Output Vert(Input v)
            {
                Output o;
                float3 p=v.positionOS.xyz+v.normalOS*(_Layer*_FurLength);
                // Slight downward grooming, strongest at the ends, never inflated clumps.
                p.y-=_Layer*_Layer*_FurLength*.22;
                o.positionCS=TransformObjectToHClip(p);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.uv=TRANSFORM_TEX(v.uv,_BaseMap);return o;
            }
            half4 Frag(Output i):SV_Target
            {
                half4 fibre=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
                if(_Layer>0.001) clip(fibre.g-_Layer);
                half3 n=normalize(i.normalWS);Light key=GetMainLight();
                half diffuse=saturate(dot(n,key.direction));
                half3 ambient=max(SampleSH(n),half3(.16,.17,.19));
                half ao=lerp(.83,1.0,_Layer);
                half3 colour=_BaseColor.rgb*lerp(.93,1.02,fibre.r)*ao*(ambient+key.color*diffuse*.8);
                return half4(colour,1);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
