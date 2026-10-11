Shader "Rolling Panda/Study Short Fur"
{
    Properties
    {
        _BaseMap("Fibre texture",2D)="white"{}
        _BaseColor("Coat colour",Color)=(1,1,1,1)
        _PatchColour("Eye patch colour",Color)=(.19,.195,.21,1)
        _FacePattern("Painted face patches",Float)=0
        _BandOn("Mask fur under headband",Float)=0
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
                float4 _BaseMap_ST;half4 _BaseColor;half4 _PatchColour;float _FacePattern;float _BandOn;float _Layer;float _FurLength;
            CBUFFER_END
            struct Input {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
            struct Output {float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float2 uv:TEXCOORD1;float3 positionOS:TEXCOORD2;};
            Output Vert(Input v)
            {
                Output o;
                float3 p=v.positionOS.xyz+v.normalOS*(_Layer*_FurLength);
                // Slight downward grooming, strongest at the ends, never inflated clumps.
                p.y-=_Layer*_Layer*_FurLength*.22;
                o.positionCS=TransformObjectToHClip(p);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.uv=TRANSFORM_TEX(v.uv,_BaseMap);o.positionOS=v.positionOS.xyz;return o;
            }
            half4 Frag(Output i):SV_Target
            {
                half4 fibre=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
                if(_Layer>0.001)
                {
                    clip(fibre.g-_Layer);
                    if(_FacePattern>.5 && _BandOn>.5 && i.positionOS.y>.285 && i.positionOS.y<.39) clip(-1);
                }
                float2 patch=float2((abs(i.positionOS.x)-.235+(i.positionOS.y-.06)*.12)/.175,(i.positionOS.y-.06)/.215);
                half pattern=(1-smoothstep(.9,1.04,length(patch)))*step(.15,i.positionOS.z)*_FacePattern;
                half3 coat=lerp(_BaseColor.rgb,_PatchColour.rgb,pattern);
                half3 n=normalize(i.normalWS);Light key=GetMainLight();
                half diffuse=saturate(dot(n,key.direction));
                half3 ambient=max(SampleSH(n),half3(.16,.17,.19));
                half ao=lerp(.83,1.0,_Layer);
                half3 colour=coat*lerp(.93,1.02,fibre.r)*ao*(ambient+key.color*diffuse*.8);
                return half4(colour,1);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
