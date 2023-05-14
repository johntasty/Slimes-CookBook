#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// GLES2 has limited amount of interpolators
#if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

#define Surf_Dis 1e-3
// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct Varyings
{
    float2 uv                       : TEXCOORD0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD1;
#endif

    float3 normalWS                 : TEXCOORD2;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: sign
#endif
    float3 viewDirWS                : TEXCOORD4;

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight   : TEXCOORD5; // x: fogFactor, yzw: vertex light
#else
    half  fogFactor                 : TEXCOORD5;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                : TEXCOORD7;
#endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV : TEXCOORD9; // Dynamic lightmap UVs
#endif

    float4 positionCS               : SV_POSITION;
    half3 ro                        : TEXCOORD10;
    half3 hitPos                    : TEXCOORD11;
    half4 screenPos : TEXCOORD12;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

    #if defined(_NORMALMAP)
    inputData.tangentToWorld = tangentToWorld;
    #endif
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
#else
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
#else
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
#endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif

}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    
    
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
        fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
#else
    output.fogFactor = fogFactor;
#endif

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    
#endif
    output.positionWS = vertexInput.positionWS;
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

    output.ro = TransformWorldToObject(GetCameraPositionWS());
    output.hitPos = input.positionOS.xyz;
    output.screenPos = ComputeScreenPos(vertexInput.positionCS);
    return output;
}
half GetDist(half3 p , half s)
{
    half d = length(p) - s;
   
    return d;
}
half OpUs(half d1, half d2, half k){
    half h = clamp(0.5 + 0.5 * (d1 - d2) / k, 0.0, 0.1);
    return lerp(d2, d1, h) - k * h * (1.0 - h);

}
half3 _PosNew(half3 pos, half NewPos) {
    half rad = 0.0174532925 * NewPos;
    half cosY = cos(rad);
    half sinY = sin(rad);
    half3 _pos = half3(cosY * pos.x - sinY * pos.z, pos.y, sinY * pos.x + cosY * pos.z);
    return _pos;
}
float unionSDF(float distA, float distB, float k) {
    float h = max(k - abs(distA - distB), 0.0) / k;
    return min(distA, distB) - h * h * k *  (1.0 / 4.0);
}
half DistanceField(half3 _Pos) {
    
    half sphere = GetDist(_Pos - _TargetPos.xyz, _Radius);
    /*for (int i = 1; i < _PositionArray.Length; i++) {
        half sphereAdd = GetDist(_Pos - _PositionArray[i].xyz / _Size, _Radius);
        sphere = unionSDF(sphere, sphereAdd, _SphereSmooth);
    }*/
    
    return sphere;
}
half3 Get_Norm(half3 p)
{
    half2 e = half2(0.001, 0);
    half3 n = half3(
        DistanceField(p + e.xyy) - DistanceField(p - e.xyy),
        DistanceField(p + e.yxy) - DistanceField(p - e.yxy),
        DistanceField(p + e.yyx) - DistanceField(p - e.yyx));
    return normalize(n);
}
half _Shadow(half3 ro, half3 rd, half mint, half maxt, half k) {
    half result = 1.0;
    for (half t = mint; t < maxt;) {

        half h = DistanceField(ro + rd * t);
        if (h < 0.001) {
            return 0.0;
        }
        result = min(result, k * h / t);
        t += h;
    }
    return result;
}
half AmbientOcclusion(half3 p, half3 n) {

    half step = _StepSize;
    half Ambient = 0.0;
    half dist;
    for (int i = 1; i <= _Iterations; i++) {
        dist = step * i;
        Ambient += max(0.0, (dist - DistanceField(p + n * dist)) / dist);
    }
    return (1.0 - Ambient * _Intensity);

}

half3 _Shading(half3 p, half3 n) {
    half3 result;
    half3 color = _BaseColorRay.rgb;
    half3 light = (GetMainLight().color * dot(-GetMainLight().direction, n) * 0.5 + 0.5) * GetMainLight().shadowAttenuation;

    half shadow = _Shadow(p, -GetMainLight().direction, _ShadowDis.x, _ShadowDis.y, _ShadowInt) * 0.5 + 0.5;

    shadow = max(0.0, pow(shadow, _ShadowIntensity));

    half ambient = AmbientOcclusion(p, n);

    result = color * light * shadow * ambient;
    return result;
}
bool _RayMarch(half3 ro, half3 rd, inout half3 p, inout half ds) 
{
    bool hit;
    half result = half4(1, 1, 1, 1);
    half dO = 0;   
    for (int i = 0; i < _Max_Steps; i++) {
       
       
        if (dO > _Max_Distance) {
            hit = false;
            break;
        }
        p = ro + rd * dO;
        ds = DistanceField(p);
        dO += ds;

        if (ds < _Accuracy) {
           
            hit = true;           
            break;
        }
        
    }
    return hit;
}

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

   
    float rawDepth = SampleSceneDepth(input.screenPos.xy / input.screenPos.w);   
    float orthoLinearDepth = _ProjectionParams.x > 0 ? rawDepth : 1 - rawDepth;
    float sceneEyeDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, orthoLinearDepth);
    //color.rgb = MixFog(color.rgb, inputData.fogCoord);
    half2 uv = input.uv - 0.5;
    half3 ro = input.ro;
    half3 rd = normalize(input.hitPos - ro);


    half3 pos;
    half3 n;
    half ds;
    half4 result;
    bool hit = _RayMarch(ro, rd, pos, ds);

    if (hit && sceneEyeDepth > ds) {
        n = Get_Norm(pos);
        half3 sh = _Shading(pos, n);

        result = half4(sh, 1);
        //result += half4(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, sh).rgb, 0);
    }
    else { discard; }
    
#if defined(_PARALLAXMAP)
#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
#else
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
#endif
    ApplyPerPixelDisplacement(viewDirTS, input.uv);
#endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(result.xyz, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif
   
   
   
  
    half4 color = UniversalFragmentPBR(inputData, surfaceData);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(1, _Surface);

    return result;
}

#endif
