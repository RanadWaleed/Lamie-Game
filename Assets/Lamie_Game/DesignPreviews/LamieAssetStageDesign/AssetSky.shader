Shader "Lamie/Asset Gradient Sky"
{
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Background"} Pass {
 Tags {"LightMode"="Universal2D"} ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.uv=a.uv;return o;}
 half4 frag(V i):SV_Target {float3 peach=float3(1,.77,.57);float3 rose=float3(.91,.63,.75);float3 violet=float3(.54,.52,.76);float t=saturate(i.uv.y);return half4(lerp(lerp(peach,rose,smoothstep(0,.65,t)),violet,smoothstep(.4,1,t)),1);}
 ENDHLSL
 } }
}

