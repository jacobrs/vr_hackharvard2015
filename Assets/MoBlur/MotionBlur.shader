Shader "Hidden/Volund/Motion Blur" {

// -------------------------------------------------------------------------------------------
// This is currently a mix of [Guerin14] and [McGuire12], with a various other custom
// experiments and modifications intended to follow.
//
// Some additional experiments are planned, the primary one being an experiment involving
// rendering the scene twice per visible to frame (render@1/48, present@1/24). The idea is
// to enhance blur quality and accuracy while acentuating strobing from the low fps of
// standard film cameras.
//
// Guerin14:  http://graphics.cs.williams.edu/papers/MotionBlurHPG14/Guertin2014MotionBlur.pdf
// McGuire12: http://graphics.cs.williams.edu/papers/MotionBlurI3D12/McGuire12Blur.pdf
//
// Currently only developed and tested on DX11, but there's nothing here that won't
// work on any other platform.
//

Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

CGINCLUDE
#include "UnityCG.cginc"

struct v2f {
	float4	pos	: SV_POSITION;
	half2	uv	: TEXCOORD0;
};

v2f vert(appdata_img i) {
	v2f o;	
	o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
	o.uv = i.texcoord;
	return o;
}

uniform sampler2D	_MainTex;
uniform half4		_MainTex_TexelSize;
uniform sampler2D	_CameraDepthTexture;

uniform sampler2D	u_VelocityTexture;
uniform half4		u_VelocityTexture_TexelSize;
uniform sampler2D	u_TileMaxTexture;
uniform half4		u_TileMaxTexture_TexelSize;
uniform sampler2D	u_NeighbourMaxTexture;
uniform half4		u_NeighbourMaxTexture_TexelSize;

uniform half2		u_MinMaxBlurRadiusPixels;
uniform half4		u_ExposureFraction;		// (shutter angle / 360, x/2, threshold/x, 0)
uniform half		u_JitterStrength;
uniform half2		u_VelocityScale;
uniform half		u_CenterVelocityThreshold;

#define VARIANCETHRESHOLD 1.5
//#define VARIANCETHRESHOLD 0.75

#define DBG_MODE_NONE				0
#define DBG_MODE_PASS_THROUGH		1
#define DBG_MODE_VELOCITY			2
#define DBG_MODE_TILEMAX			3
#define DBG_MODE_NEIGHBOURMAX		4
#define DBG_MODE_NEIGHBOURJITTER	5
uniform int	u_DebugRenderMode;

//#define DBG_IF(x) if(false)
#define DBG_IF(x) if(x)
#define DBG_IF_MODE(x) if(u_DebugRenderMode == x)

//TODO: Try using Halton sequence for jitter distribution.

half randomZeroOne(half2 i) {
	//TODO: Will this be good enough at half precision?
	const half2 r = half2(
		23.1406926327792690f,	// e^pi (Gelfond's constant)
		2.6651441426902251f		// 2^sqrt(2) (Gelfond–Schneider constant)
	);
	return frac(cos(fmod(123456789.f, 1e-7f + 256.f * dot(i, r))));
}

half randomPlusMinusHalf(half2 i) {
	//TODO: Will this be good enough at half precision?
	return randomZeroOne(i) - 0.5f;
}

half cone(half d, half r) {
    return saturate(1.f - abs(d) / r);
}

half coneRcp(half d, half rRcp) {
    return saturate(1.0 - abs(d) * rRcp);
}

half cylinder(half d, half r) {
    return sign(r - abs(d)) * 0.5f + 0.5f;
}

half depthCompare(half za, half zb) {
	return min(max(0.f, 1.f - (za - zb) / min(za, zb)), 1.f);
}

half3 fetchVelocity(sampler2D s, half2 uv) {
	const half2 vel = tex2Dlod(s, float4(uv, 0.f, 0.f)).xy * u_VelocityTexture_TexelSize.zw * u_VelocityScale;
	const half mag = length(vel);
	const bool almostZero = mag < u_ExposureFraction.z;
	
	const half radius = clamp(mag * u_ExposureFraction.y, u_MinMaxBlurRadiusPixels.x, u_MinMaxBlurRadiusPixels.y);
	
	return almostZero
		? half3(vel, radius)
		: half3(vel * (radius / mag), radius);
}


half2 neighbourMaxEdgeJitter(const half2 uv) {
	half2 baseUV = uv + u_NeighbourMaxTexture_TexelSize.xy * 0.5f;
#if 1
	const half jitter = randomPlusMinusHalf(uv);
	const half2 texelRect = baseUV * u_NeighbourMaxTexture_TexelSize.zw;
	const half2 jitterFactor = frac(texelRect) * 2.f - 1.f;
	const half2 jitterFactorAbs = abs(jitterFactor);
	const half2 jitterDirFactor = jitterFactorAbs.x > jitterFactorAbs.y ? half2(jitterFactor.x, 0.f) : half2(0.f, jitterFactor.y);	
	baseUV += ((1-jitterFactorAbs) < jitter ? u_NeighbourMaxTexture_TexelSize.xy * jitterDirFactor : 0.f);
#endif
	return baseUV;
}

half4 debug(v2f i) {
	const half4 scale = 1.f / 20.f;
	const half4 bias = 0.f;
	
	DBG_IF_MODE(DBG_MODE_PASS_THROUGH)
		return tex2D(_MainTex, i.uv);
		
	DBG_IF_MODE(DBG_MODE_VELOCITY) {
		const half3 baseVelocity = fetchVelocity(u_VelocityTexture, i.uv);
		return half4(abs(baseVelocity.x), abs(baseVelocity.y), 0, 0) * (scale / 10.0f) + bias;
	}
	
	DBG_IF_MODE(DBG_MODE_TILEMAX) {
		const half3 tileMax = fetchVelocity(u_TileMaxTexture, i.uv + u_TileMaxTexture_TexelSize.xy * 0.5f);
		return half4(abs(tileMax.x), abs(tileMax.y), 0, 0) * scale + bias;
	}

	DBG_IF_MODE(DBG_MODE_NEIGHBOURMAX) {
		const half3 neighbourMax = fetchVelocity(u_NeighbourMaxTexture, i.uv + u_NeighbourMaxTexture_TexelSize.xy * 0.5f);
		return half4(abs(neighbourMax.x), abs(neighbourMax.y), 0, 0) * scale + bias;
	}

	DBG_IF_MODE(DBG_MODE_NEIGHBOURJITTER) {
		const half3 nj = fetchVelocity(u_NeighbourMaxTexture, neighbourMaxEdgeJitter(i.uv));
		return half4(abs(nj.x), abs(nj.y), 0, 1) * scale + bias;
	}
	
	return 0;
}

half4 filter(v2f i, const int SAMPLES) : SV_Target {
	DBG_IF(u_DebugRenderMode != DBG_MODE_NONE)
		return debug(i)*5;// * 0.5f + tex2D(_MainTex, i.uv);
	
	const half2 baseUV = i.uv;
	
	const half4 baseColor = tex2D(_MainTex, baseUV);
	const half3 neighbourMaxVelocity = fetchVelocity(u_NeighbourMaxTexture, neighbourMaxEdgeJitter(i.uv));

	if(abs(neighbourMaxVelocity.z) <= u_MinMaxBlurRadiusPixels.x)
		return baseColor;

#if 1
	const half jitter = randomPlusMinusHalf(baseUV) * 0.066f;
#else
	const half jitter = randomPlusMinusHalf(baseUV) * u_VelocityTexture_TexelSize.xy * u_JitterStrength;
#endif

	const half3 baseVelocity = fetchVelocity(u_VelocityTexture, baseUV);
	const half baseDepth = -Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, baseUV));

	const half2 dirNeighbourhood = normalize(neighbourMaxVelocity.xy);
	const half2 dirCenter = normalize(baseVelocity.xy);

#if 1
	const half2 dirAlternative = baseVelocity.z < u_CenterVelocityThreshold ? dirNeighbourhood : dirCenter;
#else
	const half2 dirPerp = normalize(half2(dirNeighbourhood.y, -dirNeighbourhood.x));
	const half2 dirPerp2 = dot(dirPerp, dirCenter) > 0.f ? -dirPerp : dirPerp;
	const half2 dirAlternative = baseVelocity.z < u_CenterVelocityThreshold ? dirPerp2 : dirCenter;
#endif
	
	const half rcpBaseRadius = 1.f / baseVelocity.z; 
	
	half lastSampleVelocity = baseVelocity.z;
	
	const half fSAMPLES = (half)SAMPLES;
	half weight = rcpBaseRadius *  fSAMPLES / 40.f;
	half4 sum = baseColor * weight;
	
#if 1
	for(int j = 0; j < SAMPLES; ++j) {
		const half t = lerp( -1.f, 1.f, saturate(1.2f * ((half)j + 1.f + jitter) / (fSAMPLES + 1.f)) );		
		const half d = t * neighbourMaxVelocity.z;
		const half2 dir = (j&1) == 1 ? dirAlternative : dirNeighbourhood;
		const half2 off = d * dir;
#else
	const half maxCenterAngle = dot(dirNeighbourhood, dirCenter);
	const half centerRatio = (1.f - max(0.f, maxCenterAngle)) * 0.5f;

	const int SAMPLEHALF = SAMPLES / 2;
	const half fSAMPLEHALF = SAMPLES / 2;
	for(int j = -SAMPLEHALF; j <= SAMPLEHALF; ++j) {
		const float sampleRatio = j / fSAMPLEHALF;
		const bool sampleOdd = (j & 1) == 1;
		const bool sampleSplit = sampleRatio <= centerRatio;
		const bool useAlt = sampleOdd && sampleSplit;

		const half t = sampleRatio + jitter;
		const half d = t * (useAlt ? lastSampleVelocity : neighbourMaxVelocity.z);
		const half2 dir = useAlt ? dirAlternative : dirNeighbourhood;
		const half2 off = d * dir;
#endif
		
		const half2 sampleUV = baseUV + (off + 0.0f) * _MainTex_TexelSize.xy;

		const half4 sampleColor = tex2Dlod(_MainTex, float4(sampleUV, 0.f, 0.f));
		const half3 sampleVelocity = fetchVelocity(u_VelocityTexture, sampleUV);
		const half sampleDepth = -Linear01Depth(SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(sampleUV, 0.f, 0.f)));
		
#if 0
		const half fW = 1.f;
#else
		const half fW = saturate(0.25f + dot(normalize(sampleVelocity.xy), dir));
#endif

		const half front = depthCompare(baseDepth, sampleDepth) * fW;
		const half behind = depthCompare(sampleDepth, baseDepth);

		const half w 
			= front * coneRcp(d, sampleVelocity.z)
			+ behind * coneRcp(d, rcpBaseRadius)
			+ cylinder(d, min(baseVelocity.z, sampleVelocity.z)) * 2.f;
		
		lastSampleVelocity = sampleVelocity.z;
		
		weight += w;
		sum += sampleColor * w;
	}

	return sum / weight;
}

half4 frag17(v2f i) : SV_Target { return filter(i, 17); }
half4 frag25(v2f i) : SV_Target { return filter(i, 25); }
half4 frag35(v2f i) : SV_Target { return filter(i, 35); }
half4 frag51(v2f i) : SV_Target { return filter(i, 51); }

#pragma vertex vert
#pragma target 3.0
#pragma only_renderers d3d11 ps4
//+opengl
#pragma fragmentoption ARB_precision_hint_nicest

ENDCG


SubShader {
	ZWrite Off ZTest Always Cull Off
	
	Pass {
		CGPROGRAM
			#pragma fragment frag17
		ENDCG
	}
	
	Pass {
		CGPROGRAM
			#pragma fragment frag25
		ENDCG
	}
	
	Pass {
		CGPROGRAM
			#pragma fragment frag35
		ENDCG
	}
	
	Pass {
		CGPROGRAM
			#pragma fragment frag51
		ENDCG
	}
}

Fallback off
}

