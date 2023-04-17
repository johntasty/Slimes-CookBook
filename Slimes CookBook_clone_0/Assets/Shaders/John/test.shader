Shader "Unlit/test"
{
    Properties
    {
        
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor ("Color", Color) = (0, 0, 0.5, 0.3)
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0
        _Roughness("_Roughness", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}
        _SpecMultiply("_SpecMultiply", Float) = 1.0
        _SpecPower("_SpecPower", Float) = 1.0
        TestDist("TestDist", Float) = 1.0
        _SpecPowerPreClamp("_SpecPowerPreClamp", Float) = 50.0

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _StrenghLight("_StrenghLight", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax("Scale", Range(0.005, 0.08)) = 0.005
        _ParallaxMap("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}


        _Size("Size", Float) = 10.0
        _SizeSphere("_SizeSphere", Float) = 0.5
        _TestFloat("_TestFloat", Float) = 0.5
        _SphereSmooth("_SphereSmooth", Float) = 0.1
        _Max_Steps("_Max_Steps", Float) = 100.0
        _Max_Distance("_Max_Distance", Float) = 100.0
        _ShadowMin("_ShadowMin Distance", Float) = 0.1
        _ShadowMax("_ShadowMax_Distance", Float) = 100.0
        _ShadowIntensity("_ShadowIntensity", Float) = 5.0
        _ShadowPenumbra("_ShadowPenumbra", Float) = 5.0
        _Accuracy("_Accuracy", Float) = 0.001

        _Bounds("Bounds", Vector) = (0,0,0)
        _BoundsDifuse("Bounds", Vector) = (0,0,0)
        _BoundsTest("Bounds", Vector) = (0,0,0)

    }
        HLSLINCLUDE
#pragma vertex vert
#pragma fragment frag
            // make fog work
#pragma multi_compile_fog

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"


        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            half4 color : COLOR;

        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            half3 positionWS               : TEXCOORD1;
            half3 ro     : TEXCOORD3;
            half3 roWS     : TEXCOORD2;
            half3 hitPos : TEXCOORD4;
            half4 screenPos : TEXCOORD5;
            half3 viewVector : TEXCOORD6;
            half3 viewVectorWS : TEXCOORD7;
            half4 color : COLOR;
            float4 vertex : SV_POSITION;
        };
        //GetAbsolutePositionWS(input.positionWS)
        float4 _BaseMap_ST;
        float4 _BaseColor;
        float _Cutoff;
        float _StrenghLight;
        float _OcclusionStrength;
        half4 _SpecColor;
        half4 _EmissionColor;
        half _SpecMultiply;
        half _SpecPower;
        half _SpecPowerPreClamp;
        half _Smoothness;
        half _Roughness;
        half _Metallic;
        half TestDist;

        uniform half _Size;
        uniform half _SizeSphere;
        uniform half _Max_Steps;
        uniform half _Max_Distance;
        uniform half _Accuracy;
        uniform half _SphereSmooth;
        uniform half _TestFloat;

        uniform half _ShadowMin;
        uniform half _ShadowMax;
        uniform half _ShadowIntensity;
        uniform half _ShadowPenumbra;

        float3 _positions[20];
        half4 _Bounds;
        half4 _BoundsDifuse;
        half4 _BoundsTest;

        struct RmSphere {
            float unionSDF(float distA, float distB, float k) {
                float h = max(k - abs(distA - distB), 0.0) / k;
                return min(distA, distB) - h * h * k * (1.0 / 4.0);
            }
            half GetDist(half3 p)
            {
                half3 pos = (p - _positions[0].xyz);
                half d = length(pos) - _SizeSphere;
                for (int i = 1; i < _positions.Length; i++) {
                    half3 pos2 = (p - _positions[i].xyz );
                    half d2 = length(pos2) - _SizeSphere ;
                    d = unionSDF(d, d2, _SphereSmooth);
                }
                return d;
            }

            half3 Get_Norm(half3 p)
            {
                half2 e = half2(0.001, 0);
                half3 n = half3(
                    GetDist(p + e.xyy) - GetDist(p - e.xyy),
                    GetDist(p + e.yxy) - GetDist(p - e.yxy),
                    GetDist(p + e.yyx) - GetDist(p - e.yyx));
                return normalize(n);
            }
            half _ShadowSoft(half3 ro, half3 rd, half mint, half maxt, half k) {
                half result = 1.0;
                for (half t = mint; t < maxt;) {

                    half h = GetDist(ro + rd * t);
                    if (h < 0.001) {
                        return 0.0;
                    }
                    result = min(result, k * h / t);
                    t += h;
                }
                return result;
            }
            half3 brdf(half3 ro, half3 pos, half3 normal, half3 lightDir, inout half3 col) {
                half percepinalrough = 1.0 - _Smoothness;
                half roughness = percepinalrough * percepinalrough;
                float3 halfDir = SafeNormalize(lightDir + pos);
                float nh = saturate(dot(normal, halfDir));
                float lh = saturate(dot(lightDir, halfDir));
                float d = nh * nh * (roughness * roughness - 1.0) + 1.00001;
                float normalizationTerm = roughness * 4.0 + 2.0;
                float specularTerm = roughness * roughness;
                specularTerm /= (d * d) * max(0.1, lh * lh) * normalizationTerm;
                col += specularTerm * _SpecPower;
              
                return col;
           }
            half3 SampleEnvironment(half3 pos, half3 normal) {
                half precRoug = 1.0 - _Smoothness;
                float3 reflectVector = reflect(-pos, normal);
                float mip = PerceptualRoughnessToMipmapLevel(precRoug);

                float3 uvw = reflectVector;
                float4 sample = SAMPLE_TEXTURECUBE_LOD(
                    unity_SpecCube0, samplerunity_SpecCube0, uvw, mip
                );
                float3 color = sample.rgb;
                return color;
            }
            half3 RefractEnvironment(half3 pos, half3 normal) {
                half precRoug = 1.0 - _Smoothness;
                float3 reflectVector = refract(pos, normal,  _SpecPowerPreClamp);
                float mip = PerceptualRoughnessToMipmapLevel(precRoug);

                float3 uvw = reflectVector;
                float3 sample = SAMPLE_TEXTURECUBE_LOD(
                    unity_SpecCube0, samplerunity_SpecCube0, uvw, mip
                );
                float3 color = sample.rgb;
                return color;
            }
            half3 ReflectEnviroment(inout half3 enviroment, half3 normal, half3 pos, half specular, half fresStr) {
                half percepinalrough = 1.0 - _Smoothness;
                half roughness = percepinalrough * percepinalrough;

                float fresnel = Pow4(1.0 - saturate(dot(normal, pos)));
               
                enviroment *= lerp(specular, fresStr, fresnel);
                enviroment /= roughness * roughness + 1.0;
                return  enviroment;
            }

            half AmbientOcclusion(half3 pos , half3 norm) {
                float dlt = 0.5;
                float oc = 0.0, d = 1.0;
                for (float i = 0.0; i < 6.; i++) {
                    oc += (i * dlt - GetDist(pos + norm * i * dlt)) / d;
                    d *= 2.0;
                }

                return 1.0 - oc;
            }
            half3 _Shading(half3 p, half3 n, half3 ro, half ds) {
                Light lights = GetMainLight();
                half3 result; 
                half3 color = _BaseColor.rgb;
               
                half3 light = (_MainLightColor * saturate(dot(n,p)) * 0.5 + 0.5) * lights.distanceAttenuation;
                
                color *= light;

                half specular = lerp(_SpecPower, color, _Metallic);
                half reflectivity = lerp(_SpecPower, 1.0, _Metallic);
                half fresStrenght = saturate(_Smoothness + reflectivity);

                color *= 1.0 - _SpecPower;
                half shadow = _ShadowSoft(p, _MainLightPosition.xyz, _ShadowMin, _ShadowMax, _ShadowPenumbra) * 0.5 + 0.5;
                shadow = max(0.0, pow(shadow, _ShadowIntensity));

                //half ao = AmbientOcclusion(p, n);
               
                half3 test = brdf(ro, p, n, lights.direction, color);
                result = test * shadow;
                float relNorm = dot(-ro, n);

                half3 reflections = SampleEnvironment(-ro, n);
                half3 refraction = RefractEnvironment(p, n);
                               
                float seeThrough = clamp(3. - abs(p.y * 4.2 + 6.5), 0., 1.);
               
                
                half3 reflect = ReflectEnviroment(reflections, n, -ro, specular, fresStrenght);
                half3 refract = ReflectEnviroment(refraction, n, _MainLightPosition.xyz, specular, fresStrenght);
                result += reflect;
                half3 absorbCol = lerp(result, _SpecColor.rgb, relNorm);
                
                float absorbAmount = min(relNorm * 1., _Roughness) - seeThrough * .5;
                absorbCol = lerp(_EmissionColor.rgb, absorbCol, absorbAmount);
                half3 col = lerp(absorbCol, refract, absorbAmount);
                col = lerp(col, reflect, ds);
                                

                return (absorbCol);

            }
        };

       
        RmSphere _sphereFun;
        v2f vert(appdata v)
        {
            v2f o = (v2f)0;
            VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
            o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
            o.ro = GetCameraPositionWS();//TransformWorldToObject();
            o.roWS = GetCameraPositionWS();
            o.hitPos = v.vertex.xyz;
           
            o.vertex = TransformObjectToHClip(v.vertex);
            o.screenPos = ComputeScreenPos(v.vertex);

            o.positionWS = TransformObjectToWorld(v.vertex.xyz);
            o.viewVectorWS = GetWorldSpaceViewDir(GetCameraPositionWS());

            float3 viewVectors = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
            o.viewVector = mul(unity_CameraToWorld, float4(viewVectors, 0));

            return o;
        }
       
     

        half3 opRepLim(in half3 p, in float c, in half3 l)
        {
            half3 q = p - c * clamp(round(p / c), -l, l);
            return q;
        }

        half DistanceField(half3 _Pos) {

            half sphere;
            sphere = _sphereFun.GetDist(_Pos);
            //sphere = unionSDF(sphere, disT, _SphereSmooth);
            
           
            return sphere;
        }
       
        bool _RayMarch(half3 ro, half3 rd, inout half3 p, inout half ds, half depth)
        {
            bool hit;
            half result = half4(1, 1, 1, 1);
            half dO = 0;
            for (int i = 0; i < _Max_Steps; i++) {

               
                           
                if ( dO > _Max_Distance || dO >= depth) {
                    hit = false;
                    break;
                }        
              
                p = ro + rd * dO;
                
        
                float d1 = _sphereFun.GetDist(p);

                ds = d1;
                dO += ds;
                if (ds < _Accuracy) {

                    hit = true;
                    
                    break;
                }
               

            }
            return hit;
        }
       

        half4 frag(v2f i) : SV_Target
        {
           
            // sample the texture
            half2 uv = i.uv - 0.5;
            half3 color = _BaseColor.rgb;//tex2D(_MainTex, uv).rgb;
            // apply fog
            half3 ro = (i.ro);
            half3 rd = normalize(i.positionWS - ro);

           
           
            float rawDepth = SampleSceneDepth(i.screenPos.xy / i.screenPos.w);
            //float sceneEyeDepthInterect = LinearEyeDepth(rawDepth, _ZBufferParams);
            //float test = 1 - saturate(rawDepth - i.screenPos.w);
         
            float orthoLinearDepth = _ProjectionParams.x > 0 ? rawDepth : 1 - rawDepth;
            float sceneEyeDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);

            //depth test
           

            half3 pos = float3(i.positionWS);

            //float nonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
            //float sceneEyeDepthtest = LinearEyeDepth(nonLinear, _ZBufferParams);

            float len = sceneEyeDepth * length(i.viewVector);

            /*float3 testCamera = i.viewVector * -1.0;
            float dotCam = dot(i.viewVector, rd);*/
            //float depth = sceneEyeDepthtest / dotCam;

            half3 n;
            half ds;
            half4 result;
            //float lenDis = length(fragmentEyeDepth - ro);
            bool hit = _RayMarch(ro, rd, pos, ds, sceneEyeDepth);

            

            if (hit) {   
                //result = half4(n, 1);
                float3 normal = _sphereFun.Get_Norm(pos);
                
                half3 tes = _sphereFun._Shading(pos, normal, rd, ds);
                result = half4(tes, 1);
                return result;
            }
            
            clip(-1);
            return half4(0, 0, 0, 1);//half4(color * (1.0 - result.w) + result.xyz * result.w, 1.0);
            
        }
        ENDHLSL

            SubShader {
            // UniversalPipeline needed to have this render in URP
            Tags{ "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

                // Forward Lit Pass
                Pass
            {
                Name "ForwardLit"
                Tags { "LightMode" = "UniversalForward" }
                Cull Off
                ZWrite Off
                Blend Off
                ZTest Always
                HLSLPROGRAM
                // Signal this shader requires a compute buffer
                #pragma prefer_hlslcc gles
                #pragma exclude_renderers d3d11_9x
                #pragma target 5.0

                // Lighting and shadow keywords
                #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
                #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
                #pragma multi_compile _ _ADDITIONAL_LIGHTS
                #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
                #pragma multi_compile _ _SHADOWS_SOFT
                #pragma multi_compile_fog
                #pragma shader_feature FADE
                #pragma multi_compile_instancing
                // Register our functions
                #pragma vertex vert
                #pragma fragment frag

                // Include vertex and fragment functions

                ENDHLSL
            }
        }
}
