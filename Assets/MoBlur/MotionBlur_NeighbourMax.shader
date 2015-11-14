Shader "Hidden/Volund/Motion Blur Neighbour Max" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
	Pass {
		ZWrite Off ZTest Always Cull Off Fog { Mode Off }

        
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#pragma target 3.0
#pragma only_renderers d3d11 ps4
//+opengl
#pragma fragmentoption ARB_precision_hint_fastest

struct a2v {
	float4 vertex	: POSITION;
	float2 texcoord	: TEXCOORD0;
};

struct v2f {
	float4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
};

uniform sampler2D u_TileMaxTexture;
uniform half4 u_TileMaxTexture_TexelSize;

v2f vert(a2v v) {
	v2f o;
	
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv = v.texcoord;

	return o;
}

half4 frag(v2f i) : COLOR {
	half2 maxDirection;
	half maxMagnitude = -1.f;

	const half2 baseUV = i.uv;
	for(int y = -1; y <= 1; ++y) {
		for(int x = -1; x <= 1; ++x) {
			const half2 off = half2(x, y);
			const half2 uv = baseUV + off * u_TileMaxTexture_TexelSize.xy;
			const half2 vec = tex2D(u_TileMaxTexture, uv).xy;
			const half mag2 = dot(vec, vec);

			if(mag2 > maxMagnitude) {
				const half neighbourDist = abs(off.x) + abs(off.y);
				const half facingCenter = dot(off, vec);
				
				if(neighbourDist < 1.5f || facingCenter < 0.f) {
					maxDirection = vec;
					maxMagnitude = mag2;
				}
			}
		}
	}

	return maxDirection.xyxy;
}
ENDCG

	}
}

Fallback off
}