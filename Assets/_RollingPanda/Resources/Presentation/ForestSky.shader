Shader "Rolling Panda/Forest Sky"
{
    Properties
    {
        _Night("Night",Range(0,1))=0
        _Dusk("Dusk",Range(0,1))=0
        _Mist("Horizon mist",Range(0,1))=0
        _Clock("Animation clock",Float)=0
        _SunDirection("Sun direction",Vector)=(.32,.2,1,0)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Night,_Dusk,_Mist,_Clock;
            float4 _SunDirection;
            CBUFFER_END
            struct Attributes {float4 positionOS:POSITION;};
            struct Varyings {float4 positionCS:SV_POSITION;float3 direction:TEXCOORD0;};
            Varyings Vert(Attributes input)
            {
                Varyings output;output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.direction=input.positionOS.xyz;return output;
            }
            float Hash(float2 p) {return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p)
            {
                float2 cell=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(Hash(cell),Hash(cell+float2(1,0)),f.x),lerp(Hash(cell+float2(0,1)),Hash(cell+1),f.x),f.y);
            }
            half4 Frag(Varyings input):SV_Target
            {
                float3 d=normalize(input.direction);
                float elevation=saturate(d.y);
                float gradient=sqrt(elevation);
                float3 day=lerp(float3(.65,.85,.98),float3(.10,.39,.83),gradient);
                float3 sunset=lerp(float3(1,.48,.24),float3(.27,.20,.46),gradient);
                day=lerp(day,sunset,_Dusk*.8);
                float3 night=lerp(float3(.035,.07,.16),float3(.005,.012,.045),gradient);
                float3 color=lerp(day,night,_Night);
                // Forest fog never affects the sky; only this thin horizon haze does.
                float haze=(1-smoothstep(0,.15,elevation))*_Mist*.18;
                color=lerp(color,lerp(float3(.72,.82,.85),float3(.09,.14,.23),_Night),haze);
                float2 cloudUV=d.xz/max(.12,d.y+.2)*2+float2(_Clock*.012,_Clock*.004);
                float clouds=Noise(cloudUV)*.58+Noise(cloudUV*2.07)*.28+Noise(cloudUV*4.13)*.14;
                float cloudMask=smoothstep(.57,.76,clouds)*smoothstep(-.02,.12,d.y);
                color=lerp(color,lerp(float3(1,.97,.91),float3(.10,.14,.23),_Night),cloudMask*lerp(.86,.24,_Night));
                float sunDot=dot(d,normalize(_SunDirection.xyz));
                float sun=smoothstep(.9990,.9996,sunDot);
                float sunGlow=pow(saturate(sunDot),100)*.3;
                color+=(sun*float3(1,.86,.50)*2+sunGlow*float3(1,.65,.30))*(1-_Night);
                float moonDot=dot(d,normalize(float3(-.25,.24,1)));
                color+=smoothstep(.9993,.9996,moonDot)*float3(.72,.85,1)*_Night;
                float2 skyUV=float2(atan2(d.x,d.z)/6.2831853+.5,asin(clamp(d.y,-1,1))/3.1415927+.5);
                float2 grid=skyUV*float2(420,210),cell=floor(grid);
                float random=Hash(cell);
                float2 centre=float2(Hash(cell+43),Hash(cell+117))*.7+.15;
                float star=1-smoothstep(.025,.095,length(frac(grid)-centre));
                float twinkle=.8+.2*sin(_Clock*(.7+random)+random*60);
                color+=star*step(.979,random)*twinkle*_Night*smoothstep(-.02,.08,d.y)*lerp(float3(.6,.78,1),float3(1,.91,.74),Hash(cell+9))*1.7;
                // Low northern-light ribbons remain visible through openings above the forest.
                float azimuth=atan2(d.x,d.z);
                float ribbonHeight=.15+.045*sin(azimuth*4+_Clock*.06)+.02*sin(azimuth*11-_Clock*.09);
                float ribbon=exp(-abs(d.y-ribbonHeight)*42);
                float curtain=.45+.55*Noise(float2(azimuth*28,_Clock*.035));
                float upper=exp(-abs(d.y-ribbonHeight-.07)*22)*.3;
                color+=(ribbon*float3(.10,.65,.42)+upper*float3(.36,.17,.65))*curtain*_Night*_Night;
                return half4(max(color,0),1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
