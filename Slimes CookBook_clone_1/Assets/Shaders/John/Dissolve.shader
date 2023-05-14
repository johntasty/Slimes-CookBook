Shader "Custom/Displace_Shader"
{
	Properties
	{
		 _BaseMap("Base Texture", 2D) = "white" {}
		 _BaseColor("Example Colour", Color) = (0, 0.66, 0.73, 1)
		 _Smoothness("Smoothness", Float) = 0.5

		 [Header(Displace Variables)]
				
		_Radius("_Radius", Float) = 0.5
		_NoiseAmplitude("_NoiseAmplitude", Float) = 0.5
		_NoiseFrequency("_NoiseFrequency", Float) = 0.5
		_NoiseOffset("_NoiseOffset", Float) = 0.5

		 [Toggle(_ALPHATEST_ON)] _EnableAlphaTest("Enable Alpha Cutoff", Float) = 0.0
		 _Cutoff("Alpha Cutoff", Float) = 0.5

		 [Toggle(_NORMALMAP)] _EnableBumpMap("Enable Normal/Bump Map", Float) = 0.0
		 _BumpMap("Normal/Bump Texture", 2D) = "bump" {}
		 _BumpScale("Bump Scale", Float) = 1

		 [Toggle(_EMISSION)] _EnableEmission("Enable Emission", Float) = 0.0
		 _EmissionMap("Emission Texture", 2D) = "white" {}
		 _EmissionColor("Emission Colour", Color) = (0, 0, 0, 0)
	}
		SubShader{
			Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

			HLSLINCLUDE
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

				CBUFFER_START(UnityPerMaterial)
					float4 _BaseMap_ST;
					float4 _DissolveTex_ST;
					float4 _BaseColor;
					float _BumpScale;
					float4 _EmissionColor;
					float _Smoothness;
					float _Cutoff;
					float _Radius;
					float _NoiseAmplitude;
					float _NoiseFrequency;
					float _NoiseOffset;
				CBUFFER_END
				ENDHLSL

				Pass {
					Name "Example"
					Tags { "LightMode" = "UniversalForward" }

			
					HLSLPROGRAM

					// Required to compile gles 2.0 with standard SRP library
					// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
					#pragma prefer_hlslcc gles
					#pragma exclude_renderers d3d11_9x gles

					//#pragma target 4.5 // https://docs.unity3d.com/Manual/SL-ShaderCompileTargets.html

					#pragma vertex vert
					#pragma fragment frag

					// Material Keywords
					#pragma shader_feature _NORMALMAP
					#pragma shader_feature _ALPHATEST_ON
					#pragma shader_feature _ALPHAPREMULTIPLY_ON
					#pragma shader_feature _EMISSION
					//#pragma shader_feature _METALLICSPECGLOSSMAP
					//#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
					//#pragma shader_feature _OCCLUSIONMAP
					//#pragma shader_feature _ _CLEARCOAT _CLEARCOATMAP // URP v10+

					//#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
					//#pragma shader_feature _ENVIRONMENTREFLECTIONS_OFF
					//#pragma shader_feature _SPECULAR_SETUP
					#pragma shader_feature _RECEIVE_SHADOWS_OFF

					// URP Keywords
					#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
					#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
					#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
					#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
					#pragma multi_compile _ _SHADOWS_SOFT
					#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

					// Unity defined keywords
					#pragma multi_compile _ DIRLIGHTMAP_COMBINED
					#pragma multi_compile _ LIGHTMAP_ON
					#pragma multi_compile_fog

					// Includes
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

					struct Attributes {
						float4 positionOS   : POSITION;
						float3 normalOS		: NORMAL;
						float4 tangentOS	: TANGENT;
						float4 color		: COLOR;
						float2 uv           : TEXCOORD0;
						float2 lightmapUV   : TEXCOORD1;
					};

					struct Varyings {
						float4 positionCS				: SV_POSITION;
						float4 color					: COLOR;
						float2 uv					: TEXCOORD0;
						DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

						#ifdef REQUIRES_WORLD_SPACE_POS_INTERPOLATOR
							float3 positionWS			: TEXCOORD2;
						#endif

						float3 normalWS					: TEXCOORD3;
						#ifdef _NORMALMAP
							float4 tangentWS 			: TEXCOORD4;
						#endif

						float3 viewDirWS 				: TEXCOORD5;
						half4 fogFactorAndVertexLight	: TEXCOORD6; // x: fogFactor, yzw: vertex light

						#ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
							float4 shadowCoord			: TEXCOORD7;
						#endif
					};

					// Automatically defined with SurfaceInput.hlsl
					TEXTURE2D(_DissolveTex);
					SAMPLER(sampler_DissolveTex);

					#if SHADER_LIBRARY_VERSION_MAJOR < 9
					// This function was added in URP v9.x.x versions, if we want to support URP versions before, we need to handle it instead.
					// Computes the world space view direction (pointing towards the viewer).
					float3 GetWorldSpaceViewDir(float3 positionWS) {
						if (unity_OrthoParams.w == 0) {
							// Perspective
							return _WorldSpaceCameraPos - positionWS;
						}
					else {
							// Orthographic
							float4x4 viewMat = GetWorldToViewMatrix();
							return viewMat[2].xyz;
						}
					}
					#endif
					float3 mod289(float3 x)
					{
						return x - floor(x / 289.0) * 289.0;
					}

					float4 mod289(float4 x)
					{
						return x - floor(x / 289.0) * 289.0;
					}

					float4 permute(float4 x)
					{
						return mod289((x * 34.0 + 1.0) * x);
					}

					float4 taylorInvSqrt(float4 r)
					{
						return 1.79284291400159 - r * 0.85373472095314;
					}

					float3 snoise_grad(float3 v)
					{
						const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);

						// First corner
						float3 i = floor(v + dot(v, C.yyy));
						float3 x0 = v - i + dot(i, C.xxx);

						// Other corners
						float3 g = step(x0.yzx, x0.xyz);
						float3 l = 1.0 - g;
						float3 i1 = min(g.xyz, l.zxy);
						float3 i2 = max(g.xyz, l.zxy);

						// x1 = x0 - i1  + 1.0 * C.xxx;
						// x2 = x0 - i2  + 2.0 * C.xxx;
						// x3 = x0 - 1.0 + 3.0 * C.xxx;
						float3 x1 = x0 - i1 + C.xxx;
						float3 x2 = x0 - i2 + C.yyy;
						float3 x3 = x0 - 0.5;

						// Permutations
						i = mod289(i); // Avoid truncation effects in permutation
						float4 p =
							permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
								+ i.y + float4(0.0, i1.y, i2.y, 1.0))
								+ i.x + float4(0.0, i1.x, i2.x, 1.0));

						// Gradients: 7x7 points over a square, mapped onto an octahedron.
						// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
						float4 j = p - 49.0 * floor(p / 49.0);  // mod(p,7*7)

						float4 x_ = floor(j / 7.0);
						float4 y_ = floor(j - 7.0 * x_);  // mod(j,N)

						float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
						float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;

						float4 h = 1.0 - abs(x) - abs(y);

						float4 b0 = float4(x.xy, y.xy);
						float4 b1 = float4(x.zw, y.zw);

						//float4 s0 = float4(lessThan(b0, 0.0)) * 2.0 - 1.0;
						//float4 s1 = float4(lessThan(b1, 0.0)) * 2.0 - 1.0;
						float4 s0 = floor(b0) * 2.0 + 1.0;
						float4 s1 = floor(b1) * 2.0 + 1.0;
						float4 sh = -step(h, 0.0);

						float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
						float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

						float3 g0 = float3(a0.xy, h.x);
						float3 g1 = float3(a0.zw, h.y);
						float3 g2 = float3(a1.xy, h.z);
						float3 g3 = float3(a1.zw, h.w);

						// Normalise gradients
						float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
						g0 *= norm.x;
						g1 *= norm.y;
						g2 *= norm.z;
						g3 *= norm.w;

						// Compute gradient of noise function at P
						float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
						float4 m2 = m * m;
						float4 m3 = m2 * m;
						float4 m4 = m2 * m2;
						float3 grad =
							-6.0 * m3.x * x0 * dot(x0, g0) + m4.x * g0 +
							-6.0 * m3.y * x1 * dot(x1, g1) + m4.y * g1 +
							-6.0 * m3.z * x2 * dot(x2, g2) + m4.z * g2 +
							-6.0 * m3.w * x3 * dot(x3, g3) + m4.w * g3;
						return 42.0 * grad;
					}

					float3 _Displace(float3 p) 
					{
						float3 q = normalize(cross(p, float3(0, 1, 0)) + float3(0, 1e-5, 0));
						float3 r = cross(p, q);
						float3 n = snoise_grad(p * _NoiseFrequency + _NoiseOffset) * _NoiseAmplitude;

						return p * (_Radius + n.x) + q * n.y + r * n.z;
					}

					Varyings vert(Attributes IN) {
						Varyings OUT;

						VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
						OUT.positionCS = positionInputs.positionCS;
						OUT.uv = TRANSFORM_TEX(IN.uv, _DissolveTex);
						OUT.color = IN.color;

						#ifdef REQUIRES_WORLD_SPACE_POS_INTERPOLATOR
							OUT.positionWS = positionInputs.positionWS;
						#endif

						OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);

						VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
						OUT.normalWS = normalInputs.normalWS;
						float v1 = _Displace(OUT.positionWS.xyz);
						float v2 = _Displace(OUT.positionWS.xyz);
						float v3 = _Displace(OUT.positionWS.xyz);
						/*OUT.positionWS.xyz = v1;
						OUT.normalWS = normalize(cross(v2 - v1, v3 - v1));*/
						#ifdef _NORMALMAP
							real sign = IN.tangentOS.w * GetOddNegativeScale();
							OUT.tangentWS = half4(normalInputs.tangentWS.xyz, sign);
						#endif

						half3 vertexLight = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
						half fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

						OUT.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

						OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);
						OUTPUT_SH(OUT.normalWS.xyz, OUT.vertexSH);

						#ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
							OUT.shadowCoord = GetShadowCoord(positionInputs);
						#endif
							
						return OUT;
					}

					InputData InitializeInputData(Varyings IN, half3 normalTS) {
						InputData inputData = (InputData)0;

						#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
							inputData.positionWS = IN.positionWS;
						#endif

						half3 viewDirWS = SafeNormalize(IN.viewDirWS);
						#ifdef _NORMALMAP
							float sgn = IN.tangentWS.w; // should be either +1 or -1
							float3 bitangent = sgn * cross(IN.normalWS.xyz, IN.tangentWS.xyz);
							inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(IN.tangentWS.xyz, bitangent.xyz, IN.normalWS.xyz));
						#else
							inputData.normalWS = IN.normalWS;
						#endif

						inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
						inputData.viewDirectionWS = viewDirWS;

						#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
							inputData.shadowCoord = IN.shadowCoord;
						#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
							inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
						#else
							inputData.shadowCoord = float4(0, 0, 0, 0);
						#endif

						inputData.fogCoord = IN.fogFactorAndVertexLight.x;
						inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
						inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, inputData.normalWS);
						return inputData;
					}

					SurfaceData InitializeSurfaceData(Varyings IN) {
						SurfaceData surfaceData = (SurfaceData)0;
						// Note, we can just use SurfaceData surfaceData; here and not set it.
						// However we then need to ensure all values in the struct are set before returning.
						// By casting 0 to SurfaceData, we automatically set all the contents to 0.

						half4 albedoAlpha = SampleAlbedoAlpha(IN.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));												
						surfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
						surfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb * IN.color.rgb;

						// For the sake of simplicity I'm not supporting the metallic/specular map or occlusion map
						// for an example of that see : https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl

						surfaceData.smoothness = 0.5;
						surfaceData.normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
						surfaceData.emission = SampleEmission(IN.uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

						surfaceData.occlusion = 1;

						return surfaceData;
					}

					half4 frag(Varyings IN) : SV_Target {
						SurfaceData surfaceData = InitializeSurfaceData(IN);
						InputData inputData = InitializeInputData(IN, surfaceData.normalTS);

						// In URP v10+ versions we could use this :
						// half4 color = UniversalFragmentPBR(inputData, surfaceData);

						// But for other versions, we need to use this instead.
						// We could also avoid using the SurfaceData struct completely, but it helps to organise things.
						half4 color = UniversalFragmentPBR(inputData, surfaceData.albedo, surfaceData.metallic,
							surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion,
							surfaceData.emission, surfaceData.alpha);

						color.rgb = MixFog(color.rgb, inputData.fogCoord);

						// color.a = OutputAlpha(color.a);
						// Not sure if this is important really. It's implemented as :
						// saturate(outputAlpha + _DrawObjectPassData.a);
						// Where _DrawObjectPassData.a is 1 for opaque objects and 0 for alpha blended.
						// But it was added in URP v8, and versions before just didn't have it.
						// We could still saturate the alpha to ensure it doesn't go outside the 0-1 range though :
						color.a = saturate(color.a);

						return color; // float4(inputData.bakedGI,1);
					}
					ENDHLSL
				}
		}
}
