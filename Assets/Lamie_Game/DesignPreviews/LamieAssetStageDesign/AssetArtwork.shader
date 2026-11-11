Shader "Lamie/Independent Asset Artwork"
{
 Properties{_AssetTexture("Source asset",2D)="white"{} _Tint("Tint",Color)=(1,1,1,1) _WashColor("Wash",Color)=(1,1,1,0)}
 SubShader{Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"} Pass{
 Tags{"LightMode"="Universal2D"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_AssetTexture);SAMPLER(sampler_AssetTexture);
 CBUFFER_START(UnityPerMaterial)
 float4 _Tint;float4 _WashColor;
 CBUFFER_END
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.uv=a.uv;return o;}
 half4 frag(V i):SV_Target{half4 c=SAMPLE_TEXTURE2D(_AssetTexture,sampler_AssetTexture,i.uv);c.rgb=lerp(c.rgb,_WashColor.rgb,_WashColor.a);return c*_Tint;}
 ENDHLSL
 } }
}
