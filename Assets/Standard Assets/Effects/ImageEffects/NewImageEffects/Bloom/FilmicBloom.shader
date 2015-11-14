Shader "Hidden/FilmicBloom"
{
	Properties 
	{
        _MainTex ("_MainTex (RGB)", 2D) = "black"
		_BlurBloom ("-", 2D) = "black" {}
		_SpreadBloom ("-", 2D) = "black" {}
		_DirtTex ("-", 2D) = "black" {}
	}

	CGINCLUDE

		#pragma multi_compile __ UNITY_COLORSPACE_GAMMA

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _BlurBloom;
		sampler2D _SpreadBloom;
		sampler2D _DirtTex;
		uniform half4 _MainTex_TexelSize;
		uniform half4 _Param0;
		uniform half4 _Param1;
		uniform half _Param2;

		struct v_blurCoords
		{
			float4 pos : SV_POSITION;
			half2 uv0 : TEXCOORD0;
			half2 uv1 : TEXCOORD1;
			half2 uv2 : TEXCOORD2;
			half2 uv3 : TEXCOORD3;
			half2 uv4 : TEXCOORD4;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		uniform float4 _MainTex_ST;

		v_blurCoords vertBlur(appdata_img v)
		{
			v_blurCoords o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv0 = v.texcoord;
			o.uv1 = v.texcoord + _MainTex_TexelSize.xy * _Param0.xy;
			o.uv2 = v.texcoord - _MainTex_TexelSize.xy * _Param0.xy;
			o.uv3 = v.texcoord + _MainTex_TexelSize.xy * _Param0.zw;
			o.uv4 = v.texcoord - _MainTex_TexelSize.xy * _Param0.zw;
			return o;
		}

		half4 fragBlurIter(v_blurCoords i) : COLOR
		{
			half4 color = half4(0.182h, 0.182h, 0.182h, 0.182h) * tex2D (_MainTex, i.uv0);
			color += half4(0.2045h, 0.2045h, 0.2045h, 0.2045h) * (tex2D (_MainTex, i.uv1) + tex2D (_MainTex, i.uv2) + tex2D (_MainTex, i.uv3) + tex2D (_MainTex, i.uv4));
			return color;
		}

		half4 fragBlurLerpIter(v_blurCoords i) : COLOR
		{
			half4 src0 = tex2D (_MainTex, i.uv0);
			half4 color = half4(0.182h, 0.182h, 0.182h, 0.182h) * src0;
			color += half4(0.2045h, 0.2045h, 0.2045h, 0.2045h) * (tex2D (_MainTex, i.uv1) + tex2D (_MainTex, i.uv2) + tex2D (_MainTex, i.uv3) + tex2D (_MainTex, i.uv4));
			return lerp(src0, color, _Param2);
		}

		v2f vert(appdata_img v)
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv =  v.texcoord.xy;
			return o;
		}

		half4 fragThresh(v2f i) : COLOR
		{
			half4 color = tex2D(_MainTex, i.uv);
#if defined(UNITY_COLORSPACE_GAMMA)
			color = color * color;
#endif
			half luma = dot(color, half4(0.3h, 0.59h, 0.11h, 0.0h));
			return luma < _Param2 ? half4(0.0h, 0.0h, 0.0h, 0.0h):color;
		}

		half4 fragAdd(v2f i) : COLOR
		{
			half4 color = tex2D(_MainTex, i.uv);
#if defined(UNITY_COLORSPACE_GAMMA)
			color = color * color;
#endif
			half4 bloom = tex2D(_BlurBloom, i.uv) * _Param0;
			bloom += tex2D(_SpreadBloom, i.uv) * _Param1;
			color = color + bloom;
#if defined(UNITY_COLORSPACE_GAMMA)
			color = sqrt(color);
#endif
			return color;
		}

		half4 fragScreen(v2f i) : COLOR
		{
			half4 color = tex2D(_MainTex, i.uv);
#if defined(UNITY_COLORSPACE_GAMMA)
			color = color * color;
#endif
			half4 bloom = tex2D(_BlurBloom, i.uv) * _Param0;
			bloom += tex2D(_SpreadBloom, i.uv) * _Param1;
			half4 mr = color + bloom - color * bloom;
			color = max(color, mr);
#if defined(UNITY_COLORSPACE_GAMMA)
			color = sqrt(color);
#endif
			return color;
		}

		half4 fragAddDirt(v2f i) : COLOR
		{
			half4 color = tex2D(_MainTex, i.uv);
#if defined(UNITY_COLORSPACE_GAMMA)
			color = color * color;
#endif
			half4 bloom = tex2D(_BlurBloom, i.uv) * _Param0;
			bloom += tex2D(_SpreadBloom, i.uv) * _Param1;
			half4 dirt = bloom * tex2D(_DirtTex, i.uv) * _Param2;
			color = color + dirt + bloom;
#if defined(UNITY_COLORSPACE_GAMMA)
			color = sqrt(color);
#endif
			return color;
		}

		half4 fragScreenDirt(v2f i) : COLOR
		{
			half4 color = tex2D(_MainTex, i.uv);
#if defined(UNITY_COLORSPACE_GAMMA)
			color = color * color;
#endif
			half4 bloom = tex2D(_BlurBloom, i.uv) * _Param0;
			bloom += tex2D(_SpreadBloom, i.uv) * _Param1;
			half4 mr = color + bloom - color * bloom;
			color = max(color, mr);
#if defined(UNITY_COLORSPACE_GAMMA)
			color = sqrt(color);
#endif
			return color;
		}

	ENDCG

	SubShader
	{

		ZTest Always Cull Off ZWrite Off Fog { Mode Off } Lighting Off Blend Off

		// 0
		Pass 
		{
		
			CGPROGRAM
			#pragma vertex vertBlur
			#pragma fragment fragBlurIter
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
			 
		}

		// 1
		Pass 
		{

			CGPROGRAM
			#pragma vertex vertBlur
			#pragma fragment fragBlurLerpIter
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG 

		}

		// 2
		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragThresh
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 3
		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragAdd
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 4
		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragScreen
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 5
		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragAddDirt
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 6
		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragScreenDirt
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
	}

	FallBack Off
}

