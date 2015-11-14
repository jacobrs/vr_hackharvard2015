Shader "Hidden/FilmicDepthOfField"
{
	Properties
	{
		_MainTex ("_MainTex (RGB)", 2D) = "black"
		_SecondTex ("_SecondTex (RGB)", 2D) = "black"
		_ThirdTex ("_ThirdTex (RGB)", 2D) = "black"
	}

	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _SecondTex;
		sampler2D _ThirdTex;
		sampler2D _CameraDepthTexture;
		uniform half4 _MainTex_TexelSize;
		uniform half4 _Delta;
		uniform half2 _sampleOffset[16];
		uniform half4 _BlurCoe;
		uniform half4 _BlurParams;
		uniform half4 _Boost;
		uniform half4 _Param0;
		uniform half _Param1;

		uniform half4 _MainTex_ST;
		uniform half4 _SecondTex_ST;
		uniform half4 _ThirdTex_ST;

		struct v2fDepth
		{
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
		};

		v2fDepth vert(appdata_img v)
		{
			v2fDepth o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord.xy;
			return o;
		}


#define SAMPLE_NUM_L	6
#define SAMPLE_NUM_L1	5.0h
#define SAMPLE_NUM_M	11
#define SAMPLE_NUM_M1	10.0h
#define SAMPLE_NUM_H	16
#define SAMPLE_NUM_H1	15.0h

		half4 fragShape0(v2fDepth i) : COLOR
		{
			half4 output = half4(0.0h, 0.0h, 0.0h, 0.0h);
			half totalWeight = 0.00000001h;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half2 radius = (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y)) * _MainTex_TexelSize.xy;
			for (int k = 0; k < SAMPLE_NUM_L; k++)
			{
				half t = (half)k / SAMPLE_NUM_L1;
				half2 kVal = lerp(_Delta.xy, -_Delta.xy, t);
				half2 offset = kVal * radius;
				half2 texCoord = i.uv + offset;
				half blur = tex2D (_SecondTex, texCoord).y;
				half weight = tex2D (_SecondTex, texCoord).x >= centerDepth ? 1.0h:abs(blur);
				weight = blur * blurriness >= 0.0h ? weight:0.0h;
				output += half4(weight, weight, weight, weight) * tex2D (_MainTex, texCoord);
				totalWeight += weight;
			}
			output *= (1.0h/totalWeight);

			return half4(output.xyz, 1.0h);

		}

		half4 fragShape1(v2fDepth i) : COLOR
		{
			half4 output = half4(0.0h, 0.0h, 0.0h, 0.0h);
			half totalWeight = 0.00000001h;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half2 radius = (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y)) * _MainTex_TexelSize.xy;
			for (int k = 0; k < SAMPLE_NUM_L; k++)
			{
				half t = (half)k / SAMPLE_NUM_L1;
				half2 kVal = lerp(_Delta.xy, -_Delta.xy, t);
				half2 offset = kVal * radius;
				half2 texCoord = i.uv + offset;
				half blur = tex2D (_SecondTex, texCoord).y;
				half weight = tex2D (_SecondTex, texCoord).x >= centerDepth ? 1.0h:abs(blur);
				weight = blur * blurriness >= 0.0h ? weight:0.0h;
				output += half4(weight, weight, weight, weight) * tex2D (_MainTex, texCoord);
				totalWeight += weight;
			}

			output *= (1.0h/totalWeight);
			output = min(output, tex2D (_ThirdTex, i.uv));
			return half4(output.xyz, 1.0h);

		}

		half4 fragShape2(v2fDepth i) : COLOR
		{
			half4 output = half4(0.0h, 0.0h, 0.0h, 0.0h);
			half totalWeight = 0.00000001h;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half2 radius = (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y)) * _MainTex_TexelSize.xy;
			for (int k = 0; k < SAMPLE_NUM_M; k++)
			{
				half t = (half)k / SAMPLE_NUM_M1;
				half2 kVal = lerp(_Delta.xy, -_Delta.xy, t);
				half2 offset = kVal * radius;
				half2 texCoord = i.uv + offset;
				half blur = tex2D (_SecondTex, texCoord).y;
				half weight = tex2D (_SecondTex, texCoord).x >= centerDepth ? 1.0h:abs(blur);
				weight = blur * blurriness >= 0.0h ? weight:0.0h;
				output += half4(weight, weight, weight, weight) * tex2D (_MainTex, texCoord);
				totalWeight += weight;
			}
			output *= (1.0h/totalWeight);

			return half4(output.xyz, 1.0h);

		}

		half4 fragShape3(v2fDepth i) : COLOR
		{
			half4 output = half4(0.0h, 0.0h, 0.0h, 0.0h);
			half totalWeight = 0.00000001h;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half2 radius = (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y)) * _MainTex_TexelSize.xy;
			for (int k = 0; k < SAMPLE_NUM_M; k++)
			{
				half t = (half)k / SAMPLE_NUM_M1;
				half2 kVal = lerp(_Delta.xy, -_Delta.xy, t);
				half2 offset = kVal * radius;
				half2 texCoord = i.uv + offset;
				half blur = tex2D (_SecondTex, texCoord).y;
				half weight = tex2D (_SecondTex, texCoord).x >= centerDepth ? 1.0h:abs(blur);
				weight = blur * blurriness >= 0.0h ? weight:0.0h;
				output += half4(weight, weight, weight, weight) * tex2D (_MainTex, texCoord);
				totalWeight += weight;
			}

			output *= (1.0h/totalWeight);
			output = min(output, tex2D (_ThirdTex, i.uv));
			return half4(output.xyz, 1.0h);

		}

		half4 fragShape4(v2fDepth i) : COLOR
		{
			half4 output = half4(0.0h, 0.0h, 0.0h, 0.0h);
			half totalWeight = 0.00000001h;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half2 radius = (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y)) * _MainTex_TexelSize.xy;
			for (int k = 0; k < SAMPLE_NUM_H; k++)
			{
				half t = (half)k / SAMPLE_NUM_H1;
				half2 kVal = lerp(_Delta.xy, -_Delta.xy, t);
				half2 offset = kVal * radius;
				half2 texCoord = i.uv + offset;
				half blur = tex2D (_SecondTex, texCoord).y;
				half weight = tex2D (_SecondTex, texCoord).x >= centerDepth ? 1.0h:abs(blur);
				weight = blur * blurriness >= 0.0h ? weight:0.0h;
				output += half4(weight, weight, weight, weight) * tex2D (_MainTex, texCoord);
				totalWeight += weight;
			}
			output *= (1.0h/totalWeight);

			return half4(output.xyz, 1.0h);

		}

		half4 fragShape5(v2fDepth i) : COLOR
		{
			half4 output = half4(0.0h, 0.0h, 0.0h, 0.0h);
			half totalWeight = 0.00000001h;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half2 radius = (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y)) * _MainTex_TexelSize.xy;
			for (int k = 0; k < SAMPLE_NUM_H; k++)
			{
				half t = (half)k / SAMPLE_NUM_H1;
				half2 kVal = lerp(_Delta.xy, -_Delta.xy, t);
				half2 offset = kVal * radius;
				half2 texCoord = i.uv + offset;
				half blur = tex2D (_SecondTex, texCoord).y;
				half weight = tex2D (_SecondTex, texCoord).x >= centerDepth ? 1.0h:abs(blur);
				weight = blur * blurriness >= 0.0h ? weight:0.0h;
				output += half4(weight, weight, weight, weight) * tex2D (_MainTex, texCoord);
				totalWeight += weight;
			}

			output *= (1.0h/totalWeight);
			output = min(output, tex2D (_ThirdTex, i.uv));
			return half4(output.xyz, 1.0h);

		}


#define DISC_SAMPLE_NUM0	12
static const half3 DiscKernel0[DISC_SAMPLE_NUM0] =
{
	half3(-0.326212h, -0.405810h, 0.520669h),
	half3(-0.840144h, -0.073580h, 0.843360h),
	half3(-0.695914h, 0.457137h, 0.832629h),
	half3(-0.203345h, 0.620716h, 0.653175h),
	half3(0.962340h, -0.194983h, 0.981894h),
	half3(0.473434h, -0.480026h, 0.674214h),
	half3(0.519456h, 0.767022h, 0.926368h),
	half3(0.185461h, -0.893124h, 0.912177h),
	half3(0.507431h, 0.064425h, 0.511504h),
	half3(0.896420h, 0.412458h, 0.986758h),
	half3(-0.321940h, -0.932615h, 0.986619h),
	half3(-0.791559h, -0.597710h, 0.991878h)
};

#define DISC_SAMPLE_NUM1	28
static const half3 DiscKernel1[DISC_SAMPLE_NUM1] =
{
	half3(0.62463h, 0.54337h, 0.82790h),
	half3(-0.13414h, -0.94488h, 0.95435h),
	half3(0.38772h, -0.43475h, 0.58253h),
	half3(0.12126h, -0.19282h, 0.22778h),
	half3(-0.20388h, 0.11133h, 0.23230h),
	half3(0.83114h, -0.29218h, 0.88100h),
	half3(0.10759h, -0.57839h, 0.58831h),
	half3(0.28285h, 0.79036h, 0.83945h),
	half3(-0.36622h, 0.39516h, 0.53876h),
	half3(0.75591h, 0.21916h, 0.78704h),
	half3(-0.52610h, 0.02386h, 0.52664h),
	half3(-0.88216h, -0.24471h, 0.91547h),
	half3(-0.48888h, -0.29330h, 0.57011h),
	half3(0.44014h, -0.08558h, 0.44838h),
	half3(0.21179h, 0.51373h, 0.55567h),
	half3(0.05483h, 0.95701h, 0.95858h),
	half3(-0.59001h, -0.70509h, 0.91938h),
	half3(-0.80065h, 0.24631h, 0.83768h),
	half3(-0.19424h, -0.18402h, 0.26757h),
	half3(-0.43667h, 0.76751h, 0.88304h),
	half3(0.21666h, 0.11602h, 0.24577h),
	half3(0.15696h, -0.85600h, 0.87027h),
	half3(-0.75821h, 0.58363h, 0.95682h),
	half3(0.99284h, -0.02904h, 0.99327h),
	half3(-0.22234h, -0.57907h, 0.62029h),
	half3(0.55052h, -0.66984h, 0.86704h),
	half3(0.46431h, 0.28115h, 0.54280h),
	half3(-0.07214h, 0.60554h, 0.60982h),
};

#define SCATTER_OVERLAP_SMOOTH (-0.265)
		inline half BokehWeightDisc(half sampleDepth, half sampleDistance, half centerDepth)
		{
			return smoothstep(SCATTER_OVERLAP_SMOOTH, 0.0, sampleDepth - centerDepth*sampleDistance);
		}
		inline half2 BokehWeightDisc2(half sampleADepth, half sampleBDepth, half2 sampleDistance2, half centerSampleDepth)
		{
			return smoothstep(half2(SCATTER_OVERLAP_SMOOTH, SCATTER_OVERLAP_SMOOTH), half2(0.0,0.0), half2(sampleADepth, sampleBDepth) - half2(centerSampleDepth, centerSampleDepth)*sampleDistance2);
		}

		half4 fragCircle0(v2fDepth i) : COLOR
		{
			half4 centerTap = tex2D(_MainTex, i.uv.xy);
			half4 sum = centerTap;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half radius = 0.5h * (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y));
			half2 poissonScale = radius * _MainTex_TexelSize.xy;
			blurriness = abs(blurriness);
			half2 weights;

			half sampleCount = max(blurriness * 0.25h, 0.025h);
			sum *= sampleCount;


			for (int l = 0; l < DISC_SAMPLE_NUM0; l++)
			{
				half4 sampleUV = i.uv.xyxy + DiscKernel0[l].xyxy * poissonScale.xyxy / half4(1.2h, 1.2h, DiscKernel0[l].zz);
				half4 sample0 = tex2D(_MainTex, sampleUV.xy);
				half4 sample1 = tex2D(_MainTex, sampleUV.zw);
				half sample0Blur = abs(tex2D (_SecondTex, sampleUV.xy).y);
				half sample1Blur = abs(tex2D (_SecondTex, sampleUV.zw).y);

				if (sample0Blur + sample1Blur != 0.0)
				{
					weights = BokehWeightDisc2(sample0Blur, sample1Blur, half2(DiscKernel0[l].z/1.2h, 1.0h), blurriness);
					sum += sample0 * weights.x + sample1 * weights.y;
					sampleCount += dot(weights, 1);
				}
			}

			half4 returnValue = sum / sampleCount;

			return returnValue;
		}

		half4 fragCircle1(v2fDepth i) : COLOR
		{
			half4 blur = tex2D(_ThirdTex, i.uv.xy);
			half4 centerTap = tex2D(_MainTex, i.uv.xy);
			half4 sum = centerTap;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half radius = 0.5h * (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y));
			half2 poissonScale = radius * _MainTex_TexelSize.xy;
			blurriness = abs(blurriness);
			half weights;

			half sampleCount = max(blurriness * 0.25h, 0.025h);
			sum *= sampleCount;


			for (int l = 0; l < DISC_SAMPLE_NUM0; l++)
			{
				half2 sampleUV = i.uv.xy + DiscKernel0[l].xy * poissonScale.xy;
				half4 sample = tex2D(_MainTex, sampleUV);
				half sampleBlur = abs(tex2D (_SecondTex, sampleUV).y);

				if (sampleBlur != 0.0 )
				{
					weights = BokehWeightDisc(sampleBlur, DiscKernel0[l].z, blurriness);
					sum += sample * weights;
					sampleCount += weights;
				}
			}

			half4 returnValue = sum / sampleCount;
			returnValue = lerp(returnValue, blur, smoothstep(0.0, 0.85, blurriness));
			return (blurriness < 1e-2f ? centerTap:returnValue);
		}

		half4 fragCircle2(v2fDepth i) : COLOR
		{
			half4 centerTap = tex2D(_MainTex, i.uv.xy);
			half4 sum = centerTap;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half radius = 0.5h * (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y));
			half2 poissonScale = radius * _MainTex_TexelSize.xy;
			blurriness = abs(blurriness);
			half2 weights;

			half sampleCount = max(blurriness * 0.25h, 0.025h);
			sum *= sampleCount;


			for (int l = 0; l < DISC_SAMPLE_NUM1; l++)
			{
				half4 sampleUV = i.uv.xyxy + DiscKernel1[l].xyxy * poissonScale.xyxy / half4(1.2h, 1.2h, DiscKernel1[l].zz);
				half4 sample0 = tex2D(_MainTex, sampleUV.xy);
				half4 sample1 = tex2D(_MainTex, sampleUV.zw);
				half sample0Blur = abs(tex2D (_SecondTex, sampleUV.xy).y);
				half sample1Blur = abs(tex2D (_SecondTex, sampleUV.zw).y);

				if (sample0Blur + sample1Blur != 0.0)
				{
					weights = BokehWeightDisc2(sample0Blur, sample1Blur, half2(DiscKernel1[l].z/1.2h, 1.0h), blurriness);
					sum += sample0 * weights.x + sample1 * weights.y;
					sampleCount += dot(weights, 1);
				}
			}

			half4 returnValue = sum / sampleCount;

			return returnValue;
		}

		half4 fragCircle3(v2fDepth i) : COLOR
		{
			half4 blur = tex2D(_ThirdTex, i.uv.xy);
			half4 centerTap = tex2D(_MainTex, i.uv.xy);
			half4 sum = centerTap;
			const half centerDepth = tex2D (_SecondTex, i.uv).x;
			half blurriness = tex2D (_SecondTex, i.uv).y;
			half radius = 0.5h * (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y));
			half2 poissonScale = radius * _MainTex_TexelSize.xy;
			blurriness = abs(blurriness);
			half weights;

			half sampleCount = max(blurriness * 0.25h, 0.025h);
			sum *= sampleCount;


			for (int l = 0; l < DISC_SAMPLE_NUM1; l++)
			{
				half2 sampleUV = i.uv.xy + DiscKernel1[l].xy * poissonScale.xy;
				half4 sample = tex2D(_MainTex, sampleUV);
				half sampleBlur = abs(tex2D (_SecondTex, sampleUV).y);

				if (sampleBlur != 0.0 )
				{
					weights = BokehWeightDisc(sampleBlur, DiscKernel1[l].z, blurriness);
					sum += sample * weights;
				sampleCount += weights;
				}
			}

			half4 returnValue = sum / sampleCount;
			returnValue = lerp(returnValue, blur, smoothstep(0.0, 0.85, blurriness));
			return (blurriness < 1e-2f ? centerTap:returnValue);
		}

		half4 fragBlurinessAmount0(v2fDepth i) : COLOR
		{
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half f = _BlurParams.x * abs(d - _BlurParams.z) / (d + 1e-5f) - _BlurParams.w;
			f = f * (d < _BlurParams.z ? _BlurCoe.z:_BlurCoe.w);
			f = clamp(f, 0.0f, 1.0f);
			return half4(f, f, f, 1.0);
		}

		half4 fragBlurinessAmount1(v2fDepth i) : COLOR
		{
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half f = _BlurParams.x * abs(d - _BlurParams.z) / (d + 1e-5f) - _BlurParams.w;
			f = (d < _BlurParams.z ? -1.0h:1.0h) * clamp(f, 0.0f, 1.0f);
			return half4(d, f, 1.0, 1.0);
		}

		half4 fragBlurinessAmount2(v2fDepth i) : COLOR
		{
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half f = d < _BlurCoe.z ? (_BlurParams.x * d + _BlurParams.y):(_BlurParams.z * d + _BlurParams.w);
			f = clamp(f, 0.0f, 1.0f);
			return half4(f, f, f, 1.0);
		}

		half4 fragBlurinessAmount3(v2fDepth i) : COLOR
		{
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half f = d < _BlurCoe.z ? clamp((_BlurParams.x * d + _BlurParams.y), -1.0f, 0.0f):clamp((_BlurParams.z * d + _BlurParams.w), 0.0f, 1.0f);
			return half4(d, f, 1.0, 1.0);
		}


		half fragOverBlurinessAmount0(v2fDepth i) : COLOR
		{
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half f = _BlurParams.x * abs(d - _BlurParams.z) / (d + 1e-5f) - _BlurParams.w;
			f = f * (d < _BlurParams.z ? _BlurCoe.z:0.0h);
			f = clamp(f, 0.0f, 1.0f);
			return f;
		}

		half4 fragOverMergeBlurinessAmount0(v2fDepth i) : COLOR
		{
			half f = tex2D(_MainTex, i.uv);
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half b = _BlurParams.x * abs(d - _BlurParams.z) / (d + 1e-5f) - _BlurParams.w;
			b = b * (d < _BlurParams.z ? 0.0h:_BlurCoe.w);
			b = clamp(b, 0.0f, 1.0f);
			f = max(f, b);
			return half4(f, f, f, 1.0);
		}

		half4 fragOverMergeBlurinessAmount1(v2fDepth i) : COLOR
		{
			half f = tex2D(_MainTex, i.uv);
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half b = _BlurParams.x * abs(d - _BlurParams.z) / (d + 1e-5f) - _BlurParams.w;
			b = b * (d < _BlurParams.z ? 0.0h:_BlurCoe.w);
			b = clamp(b, 0.0f, 1.0f);
			f = f > b ? -f:b;
			return half4(d, f, 1.0, 1.0);
		}

		half fragOverBlurinessAmount2(v2fDepth i) : COLOR
		{
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half f = d < _BlurCoe.z ? (_BlurParams.x * d + _BlurParams.y):0.0h;
			f = clamp(f, 0.0f, 1.0f);
			return f;
		}

		half4 fragOverMergeBlurinessAmount2(v2fDepth i) : COLOR
		{
			half f = tex2D(_MainTex, i.uv);
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half b = d < _BlurCoe.z ? 0.0h:(_BlurParams.z * d + _BlurParams.w);
			b = clamp(b, 0.0f, 1.0f);
			f = max(f, b);
			return half4(f, f, f, 1.0);
		}

		half4 fragOverMergeBlurinessAmount3(v2fDepth i) : COLOR
		{
			half f = tex2D(_MainTex, i.uv);
			half d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
			d = Linear01Depth (d);
			half b = d < _BlurCoe.z ? 0.0h:(_BlurParams.z * d + _BlurParams.w);
			b = clamp(b, 0.0f, 1.0f);
			f = f > b ? -f:b;
			return half4(d, f, 1.0, 1.0);
		}

		half4 fragBoostThresh(v2fDepth i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			half blurriness = tex2D(_SecondTex, i.uv).y;
			half blur = (blurriness < 0.0f ? -(blurriness * _BlurCoe.x):(blurriness * _BlurCoe.y));
			half luma = dot(color, half4(0.3h, 0.59h, 0.11h, 0.0h));
			return luma < _Param1 ? half4(0.0h, 0.0h, 0.0h, 0.0h):color * blur;
		}

		half4 fragBoost(v2fDepth i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			half blurriness = tex2D(_SecondTex, i.uv).y;
			half4 blur = blurriness < 0.0f ? tex2D(_ThirdTex, i.uv) * _Param0.x:tex2D(_ThirdTex, i.uv) * _Param0.y;
			return color + blur;
		}

	ENDCG

	SubShader
	{

		ZTest Always Cull Off ZWrite Off Fog { Mode Off } Lighting Off Blend Off

		// 0
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlurinessAmount0
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 1
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlurinessAmount1
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 2
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlurinessAmount2
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 3
		Pass
		{


			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBlurinessAmount3
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 4
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragOverBlurinessAmount0
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 5
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragOverMergeBlurinessAmount0
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 6
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragOverMergeBlurinessAmount1
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 7
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragOverBlurinessAmount2
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 8
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragOverMergeBlurinessAmount2
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 9
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragOverMergeBlurinessAmount3
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 10
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragShape0
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 11
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragShape1
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 12
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragShape2
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 13
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragShape3
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 14
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragShape4
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 15
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragShape5
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 16
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCircle0
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 17
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCircle1
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 18
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCircle2
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 19
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragCircle3
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}
		// 20
		Pass
		{

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragBoostThresh
			#pragma fragmentoption ARB_precision_hint_fastest
			ENDCG

		}

		// 21
		Pass
		{

		CGPROGRAM
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment fragBoost
		#pragma fragmentoption ARB_precision_hint_fastest
		ENDCG

		}

	}

	FallBack Off
}
