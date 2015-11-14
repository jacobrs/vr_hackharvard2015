Shader "Hidden/Volund/Motion Blur Skinned Velocity" {
SubShader {
	Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#pragma target 3.0
#pragma only_renderers d3d11 ps4
//+opengl
#pragma fragmentoption ARB_precision_hint_fastest

#pragma multi_compile _ MOBLUR_SKINNED_AS_RIGID

struct a2v {
	float4 pos		: POSITION;
#ifndef MOBLUR_SKINNED_AS_RIGID
	float4 prevPos	: NORMAL;
#endif
};

struct v2f {
	float4 pos : SV_POSITION;
	half4 nowPos : TEXCOORD0;
	half4 prevPos : TEXCOORD1;
};

uniform float4x4 u_PreviousMVP;
uniform half u_VelocityFramerateFraction;
uniform float3 u_RootMotionTranslationDelta;

v2f vert(a2v v) {
	v2f o;
	o.pos = o.nowPos = mul(UNITY_MATRIX_MVP, v.pos);
#ifndef MOBLUR_SKINNED_AS_RIGID
	o.prevPos = mul(u_PreviousMVP, v.prevPos);
#else
	o.prevPos = mul(u_PreviousMVP, float4(v.pos.xyz + u_RootMotionTranslationDelta, 1.f));
#endif
	return o;
}

half4 frag(v2f i) : COLOR {
	half4 o = i.nowPos / i.nowPos.w;
	o.xy -= i.prevPos.xy / i.prevPos.w;
	o.xy *= u_VelocityFramerateFraction;
	return o;
}
ENDCG

	}
}

Fallback Off
}