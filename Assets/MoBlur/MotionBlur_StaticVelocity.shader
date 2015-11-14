Shader "Hidden/Volund/Motion Blur Static Velocity" {
SubShader {
	Pass {
	ZWrite Off
	ZTest LEqual
	Cull Off
	         
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#pragma target 3.0
#pragma only_renderers d3d11 ps4
//+opengl
#pragma fragmentoption ARB_precision_hint_fastest

#include "UnityCG.cginc"

struct v2f {
	float4	pos					: SV_POSITION;
	half2	uv					: TEXCOORD0;
	half4	clp					: TEXCOORD1;
};

uniform sampler2D _CameraDepthTexture;

uniform half4x4 u_CurrentClipToPreviousHPos;
uniform half u_VelocityFramerateFraction;

v2f vert(appdata_img v) {
	v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.pos.z = o.pos.w;
	o.uv.xy = v.texcoord;
	o.clp = half4(o.pos.x, o.pos.y, 0.f, 1.f);
	return o;
}

half4 frag(v2f i) : COLOR  {
	const half rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
	const half4 clipPos = i.clp.zzwz * rawDepth + i.clp;
	const half4 prevHPos = mul(u_CurrentClipToPreviousHPos, clipPos);

	half4 o;
	o.xy = u_VelocityFramerateFraction *(clipPos.xy - prevHPos.xy / prevHPos.w);
	o.zw = 0;
	return o;
}
ENDCG
	}
}

Fallback Off
}