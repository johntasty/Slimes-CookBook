Shader "Unlit/TestShader"
{
    Properties
    {
       
        [MainTexture]_MainTex("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (0, 0, 0.5, 0.3)
                TestParam("TestParam", Float) = 1.0
        _SpecColorSize("_SpecColorSize", Float) = 1.0
        [HDR]_SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
      
         _EmmisionSize("EmmisionSize", Float) = 1.0
        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _Size("_Size", Float) = 1.0
        _Blend("_Blend", Float) = 1.0
        _Reflections("_Reflections", Range(0.0, 1.0)) = .5
       
       
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0

        _Roughness("_Roughness", Range(0.0, 1.0)) = 0.5
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _SpecPower("_SpecPower", Float) = 1.0
        _SpecPowerPreClamp("_SpecPowerPreClamp", Float) = 50.0

        _Max_Distance("_Max_Distance", Float) = 0.1
        _Max_steps("_Max_steps", Float) = 0.1
        _DinstanceAccuracy("_DinstanceAccuracy", Float) = 0.1

        _ShadowMin("_ShadowMin Distance", Float) = 0.1
        _ShadowMax("_ShadowMax_Distance", Float) = 100.0
        _ShadowIntensity("_ShadowIntensity", Float) = 5.0
        _ShadowPenumbra("_ShadowPenumbra", Float) = 5.0
    }

        HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"


        struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;        
        float4 vertex : SV_POSITION;
        float3 rayOrigin : TEXCOORD1;
        float3 rayDirection : TEXCOORD2;
        float3 viewVector : TEXCOORD3;
        float4 projParams : TEXCOORD4;

    };

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
   
    half4 _MainTex_ST;

    half4x4 _CameraInverseProjection;
    half4x4 _CameraWorld;
    half3 _CameraToWorldPosition;
    half _AspectRatio;
    half _OrthoSize;
    half TestParam;

    half _Max_Distance;
    half _Max_steps;
    half _DinstanceAccuracy;

    half _Size;
    half _Blend;
    half _Reflections;
    half _SpecColorSize;
    half _EmmisionSize;
    
    half3 _positions[20];
    half3 _Position;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half4 _LightColor;

    half _Smoothness;
    half _Roughness;
    half _Metallic;
    half _SpecPower;
    half _SpecPowerPreClamp;

    half _ShadowMin;
    half _ShadowMax;
    half _ShadowIntensity;
    half _ShadowPenumbra;



    float sdSphere(float3 eye, float3 center, float s) {
        half d = distance(eye, center) - s;

        return d;
    }
    float sdSphereUnion(float3 eye) {
        float m = 0.0;
        float p = 0.0;
        float dmin = 1e6;

        float h = 1.0;

        for (int i = 0; i < _positions.Length; i++)
        {
            float db = length(_positions[i] - eye);

            if (db < _Size) {
                float x = db / _Size;
                p += 1.0 - x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
                m += 1.0;
                h = max(h, .5 * _Size);
            }
            else {


                dmin = min(dmin, db - _Size);
            }


        }
        half d = dmin + .1;

        if (m > 0.5) {
            float th = .1;
            d = h * (th - p);
        }

        return d;
    }
    float unionSDF(float distA, float distB, float k) {
        float h = max(k - abs(distA - distB), 0.0) / k;
        return min(distA, distB) - h * h * k * (1.0 / 4.0);
    }
    float GetDist(float3 eye) {
        float d = sdSphere(eye, _positions[0], _Size);

        for (int i = 1; i < _positions.Length; i++)
        {
            float d2 = sdSphere(eye, _positions[i], _Size);
            d = unionSDF(d, d2, _Blend);
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
       
        float4 samplers = SAMPLE_TEXTURECUBE_LOD(
            unity_SpecCube0, samplerunity_SpecCube0, uvw, mip
        );
        float3 color = samplers.rgb;
        return color;
    }
    half3 RefractEnvironment(half3 pos, half3 normal) {
        half precRoug = 1.0 - _Smoothness;
        float3 reflectVector = refract(pos, normal, (1.0 / 1.1));
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
    half3 _Shading(half3 p, half3 n, half3 ro) {
        Light lights = GetMainLight();
        half3 result;
        half3 color = _BaseColor.rgb;
      

        half ligh = saturate(saturate(dot(n, p)));
        half3 light = (_MainLightColor.rgb * ligh);

        color *= light;

        half specular = lerp(_SpecPower, color, _Metallic);
        half reflectivity = lerp(_SpecPower, 1.0, _Metallic);
        half fresStrenght = saturate(_Smoothness + reflectivity);

        color *= 1.0 - _SpecPower;
        half shadow = _ShadowSoft(p, lights.direction, _ShadowMin, _ShadowMax, _ShadowPenumbra) * 0.5 + 0.5;
        shadow = max(0.0, pow(shadow, _ShadowIntensity));

        //half ao = AmbientOcclusion(p, n);

        half3 test = brdf(ro, -ro, n, lights.direction, color);
        result = test * shadow;
        float relNorm = dot(-ro, n);

        //half3 reflections = SampleEnvironment(p, n);
        //half3 refraction = RefractEnvironment(p, n);
        //      

        //half3 reflection = ReflectEnviroment(reflections, n, -ro, specular, fresStrenght);
        //half3 refract = ReflectEnviroment(refraction, n, ro, specular, fresStrenght);
                   
        float sec = pow(relNorm, _EmmisionSize);
        float sec2 = pow(relNorm, _SpecColorSize);

        half3 absorbCol = lerp(_EmissionColor.rgb, result, sec);
        
        //float absorbAmount = min(relNorm * 1., _Roughness) - seeThrough * .5;
        absorbCol = lerp(absorbCol, _SpecColor.rgb, sec2);
        //absorbCol += lerp(reflection, refract, _Reflections);

        //half3 col = lerp(absorbCol, reflect, absorbAmount);
        //col = lerp(col, reflect, ds);


        return (absorbCol);

    }

    struct Ray {
        float3 origin;
        float3 direction;
    };

    Ray CreateRay(float3 origin, float3 direction) {
        Ray ray;
        ray.origin = origin;
        ray.direction = direction;
        return ray;
    }

    Ray CreateCameraRay(float2 uv) {
      
        float3 origin = mul(_CameraWorld, float4(0, 0, 0, 1)).xyz;
        origin += mul(_CameraWorld, float4(uv * _OrthoSize, 0, 0)).xyz;

        float3 direction = mul(_CameraInverseProjection, float4(uv, 0, 1)).xyz;
       
        direction = mul(_CameraWorld, float4(direction, 0)).xyz;
        
        return CreateRay(origin, direction);
    }

    half4 raymarch(float3 ro, float3 rd, float depth) {

        half4 ret = half4(0, 0, 0, 0);
        float rayDst = 0; // current distance traveled along ray
        float3 rOrigin = ro;
        
       
        for (int i = 0; i < _Max_steps; i++)     
        {
           
            float dst = GetDist(rOrigin);
            half3 pointOnSurface = rOrigin + rd * dst;
           
            if (rayDst > _Max_Distance || depth < length(pointOnSurface - ro)) {
                ret = float4(rd, 0);
                return ret;
                break;
            }
            if (dst <= _DinstanceAccuracy) {
               
                half3 norm = Get_Norm(pointOnSurface);               
                half3 tes = _Shading(pointOnSurface, norm, rd);
                ret = float4(tes, _BaseColor.a);
                return ret;
                break;
            }
           
            rOrigin += rd * dst;
            rayDst += dst;

        }       
      
        return ret;
    }

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = TransformObjectToHClip(v.vertex);
       
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.projParams = ComputeScreenPos(o.vertex);
        float3 viewVector = mul(unity_CameraInvProjection, float4(o.uv * 2 - 1, 0, -1));
        o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));

        /*float2 uv = o.uv * 2.0 - 1.0;
       
        Ray ray = CreateCameraRay(uv);
        o.rayDirection = ray.direction;
        o.rayOrigin = ray.origin;*/
        
        
       
        return o;
    }

    half4 frag(v2f i) : SV_Target
    {       
        float2 uv = i.uv * 2.0 - 1.0;

        Ray ray = CreateCameraRay(uv);
       /* o.rayDirection = ray.direction;
        o.rayOrigin = ray.origin;*/

        half3 rDirection = normalize(ray.direction);
        half3 rOrigin = ray.origin;
        
        float viewDir = length(ray.direction);
       
        float nonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r;
        
        float sceneEyeDepthtest = LinearEyeDepth(nonLinear, _ZBufferParams) * viewDir;

        //comment this out for perspective depth
        float orthoLinearDepth = _ProjectionParams.x > 0 ? nonLinear : 1- nonLinear;
        sceneEyeDepthtest = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);

                    
       
        half4 add = raymarch(rOrigin, rDirection, sceneEyeDepthtest);
        // sample the texture
        half3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb;       
       

        return half4(col * (1.0 - add.w) + add.xyz * add.w, 1.0);
        
    }

        ENDHLSL



        SubShader
    {
        // UniversalPipeline needed to have this render in URP
        Tags{ "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
            Pass
        {

            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On           
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            // Signal this shader requires a compute buffer
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 4.5

            // Lighting and shadow keywords
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
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

