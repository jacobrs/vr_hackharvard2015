Shader "Hidden/Volund/Motion Blur Rigid Velocity" {
SubShader {
	Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#pragma target 3.0
#pragma only_renderers d3d11 ps4
//+opengl
#pragma fragmentoption ARB_precision_hint_fastest

struct v2f {
	float4 pos : SV_POSITION;
	half4 nowPos : TEXCOORD0;
	half4 prevPos : TEXCOORD1;
};

uniform float4x4 u_PreviousMVP;
uniform half u_VelocityFramerateFraction;

v2f vert(float4 pos : POSITION) {
	v2f o;
	o.pos = o.nowPos = mul(UNITY_MATRIX_MVP, pos);
	o.prevPos = mul(u_PreviousMVP, pos);
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