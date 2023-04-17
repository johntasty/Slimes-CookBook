Shader "Custom/BounceShader"
{
	Properties
	{
		 _BaseMap("Base Texture", 2D) = "white" {}
		 _BaseColor("Example Colour", Color) = (0, 0.66, 0.73, 1)
		 _BaseColor2("Example Colour", Color) = (0, 0.66, 0.73, 1)
		 _Smoothness("Smoothness", Float) = 0.5
			
			 [Header(Metaballs)]
		_SquishAmount("Squshish Amount" , Float) = 1.0
		_SquishLimit("Squish Limit" , Float) = 0.0
		_Squish("Squshish" , Float) = 0.25
		_SquishScalar("Squshish Scalar" , Float) = 8
		
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
					float4 _CameraDepthTexture_ST;
					float4 _DissolveTex_ST;
					float4 _BaseColor;
					float4 _BaseColor2;
					float _BumpScale;
					float4 _EmissionColor;
					float _Smoothness;
					float _Cutoff;

					half _SquishAmount;
					half _SquishLimit;
					half _Squish;
					half _SquishScalar;
					
				CBUFFER_END
				ENDHLSL

				Pass {
					Name "Example"
					Tags { "LightMode" = "UniversalForward" }
								
					ZWrite On

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
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"

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
							float4 viewDis		: TEXCOORD8;
					};

					// Automatically defined with SurfaceInput.hlsl
					TEXTURE2D(_DissolveTex);
					SAMPLER(sampler_DissolveTex);

					TEXTURE2D_X_FLOAT(_CameraDepthTexture);
					SAMPLER(sampler_CameraDepthTexture);

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
					float SampleSceneDepth(float2 uv)
					{
						return SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv)).r;
					}

					float LoadSceneDepth(uint2 uv)
					{
						return LOAD_TEXTURE2D_X(_CameraDepthTexture, uv).r;
					}

					
					Varyings vert(Attributes IN) {
						Varyings OUT;
						
						float3 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
						float squish = min(worldPos.y, 0) * _SquishAmount;


						VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
						OUT.positionCS = positionInputs.positionCS;
						OUT.uv = TRANSFORM_TEX(IN.uv, _DissolveTex);
						OUT.color = IN.color;

						#ifdef REQUIRES_WORLD_SPACE_POS_INTERPOLATOR
							
						#endif
						OUT.positionWS = positionInputs.positionWS;
						OUT.viewDirWS = TransformWorldToView(positionInputs.positionWS);

						VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
						OUT.normalWS = normalInputs.normalWS;
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
							
							OUT.viewDis = ComputeScreenPos(positionInputs.positionCS);

						/*float squish = 0;
						float3 worldPos = mul(unity_ObjectToWorld, IN.positionOS);
							 if (worldPos.y < _SquishLimit) {
								   squish = 1;
							 }
							 else if (worldPos.y < _SquishLimit + _Squish) {
								   squish = 1 - ((worldPos.y - _SquishLimit) / (_SquishLimit + _Squish));
							 }
							 squish = pow(squish, _SquishScalar);
							 OUT.normalWS.y = 0;
							 if (!(OUT.normalWS.x == 0 && OUT.normalWS.z == 0)) {
								 OUT.normalWS = normalize(OUT.normalWS);
							 }
							 OUT.positionCS.xyz += OUT.normalWS * _SquishAmount;
							 OUT.positionCS.y += squish * 0.5f;*/
						return OUT;
					}

					InputData InitializeInputData(Varyings IN, half3 normalTS) {
						InputData inputData = (InputData)0;

						#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
							
						#endif
						inputData.positionWS = IN.positionWS;
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
						
						//float isVisible = (dissolve);
						

						half4 albedoAlpha = SampleAlbedoAlpha(IN.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
						
						surfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);
						surfaceData.albedo = albedoAlpha.rgb  *  _BaseColor.rgb * IN.color.rgb;

						// For the sake of simplicity I'm not supporting the metallic/specular map or occlusion map
						// for an example of that see : https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl

						surfaceData.smoothness = 0.5;
						surfaceData.normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
						surfaceData.emission = SampleEmission(IN.uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
												
						surfaceData.emission = surfaceData.emission ;
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
