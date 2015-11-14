Shader "Hidden/Volund/Motion Blur Tile Max" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
	Pass {
		ZWrite Off ZTest Always Cull Off
        
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#pragma target 3.0
#pragma only_renderers d3d11 ps4
//+opengl
#pragma fragmentoption ARB_precision_hint_fastest

#include "UnityCG.cginc"

struct a2v {
	float4 vertex	: POSITION;
	float2 texcoord	: TEXCOORD0;
};

struct v2f {
	float4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
};

uniform int u_TexelsInTile;
uniform sampler2D u_VelocityTexture;
uniform half4 u_VelocityTexture_TexelSize;

v2f vert(a2v v) {
	v2f o;
	
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv = v.texcoord;

	return o;
}

half4 frag(v2f i) : COLOR {
	half2 maxDirection;
	half maxMagnitude = -1.f;
		
	half2 rowUV = i.uv - u_VelocityTexture_TexelSize.xy * (float)u_TexelsInTile * 0.5f;
	for(int y = 0; y < u_TexelsInTile; ++y) {
		rowUV.y += u_VelocityTexture_TexelSize.y;
		
		half2 colUV = rowUV;
		for(int x = 0; x < u_TexelsInTile; ++x) {
			colUV.x += u_VelocityTexture_TexelSize.x;

			const half2 vec = tex2D(u_VelocityTexture, colUV).xy;
			const half mag2 = dot(vec, vec);
			maxDirection = mag2 > maxMagnitude ? vec : maxDirection;
			maxMagnitude = mag2 > maxMagnitude ? mag2 : maxMagnitude;
		}
	}
	
	return maxDirection.xyxy;
}
ENDCG

	}
}

Fallback off
}