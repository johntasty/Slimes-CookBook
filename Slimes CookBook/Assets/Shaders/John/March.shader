Shader "Custom/March"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ColorLow("Color Slow Speed", Color) = (0, 0, 0.5, 0.3)
        _ColorHigh("Color High Speed", Color) = (1, 0, 0, 0.3)
        _HighSpeedValue("High speed Value", Range(0, 50)) = 25
        _Size("Size", Float) = 10.0
        _Max_Steps("_Max_Steps", Float) = 80.0
        _Max_Distance("_Max_Distance", Float) = 100.0
        _Accuracy("_Accuracy", Float) = 0.001
    }

        HLSLINCLUDE


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    struct Test {

        float3 position;
    };

    struct v2f
    {
        half2 uv : TEXCOORD0;        
        half4 vertex : POSITION;
       
        half4 color : COLOR;
        half3 ro     : TEXCOORD3;
        half3 hitPos : TEXCOORD4;
    };

    sampler2D _MainTex;
    half4 _MainTex_ST;
    StructuredBuffer<Test> _normals;

    // Properties variables
    uniform half4 _ColorLow;
    uniform half4 _ColorHigh;
    uniform half _HighSpeedValue;
    uniform half _Size;
    uniform half _Max_Steps;
    uniform half _Max_Distance;
    uniform half _Accuracy;

    v2f vert(v2f input, uint instance_id : SV_InstanceID)
    {        
        v2f o = (v2f)0;
       
        
        /*half3 partic = _normals[instance_id].position;
        o.vertex = mul(UNITY_MATRIX_MVP, half4(partic, 1.0f));*/

        o.ro = TransformWorldToObject(GetCameraPositionWS());
        o.hitPos = input.vertex.xyz;

        // Color
        /*float3 speed = _normals[instance_id].position;
        float lerpValue = clamp(speed / _HighSpeedValue, 0.0f, 1.0f);
        o.color = lerp(_ColorLow, _ColorHigh, lerpValue);
        half3 partic = _normals[instance_id].position;
        o.vertex = mul(UNITY_MATRIX_MVP, half4(partic, 1.0f));*/
        
        o.uv = TRANSFORM_TEX(input.uv, _MainTex);
       
        return o;
    }
    half GetDist(half3 p, half s)
    {
        half d = length(p) - s;

        return d;
    }
    half DistanceField(half3 _Pos) {

        half sphere = GetDist(_Pos, _Size);
        /*for (int i = 1; i < _PositionArray.Length; i++) {
            half sphereAdd = GetDist(_Pos - _PositionArray[i].xyz / _Size, _PositionArray[0].w);
            sphere = unionSDF(sphere, sphereAdd, _SphereSmooth);
        }*/

        return sphere;
    }
    half3 Get_Norm(half3 p)
    {
        half2 e = half2(1e-2, 0);
        half3 n = DistanceField(p) - half3(
            DistanceField(p + e.xyy) - DistanceField(p - e.xyy),
            DistanceField(p + e.yxy) - DistanceField(p - e.yxy),
            DistanceField(p + e.yyx) - DistanceField(p - e.yyx));
        return normalize(n);
    }
    bool _RayMarch(half3 ro, half3 rd,inout half3 p, inout half ds)
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
    half4 frag(v2f i, uint instance_id : SV_InstanceID) : SV_Target
    {       
            half3 color = tex2D(_MainTex, i.uv).rgb;
            //half3 ro = i.ro;
            //half3 rd = normalize(i.hitPos - ro);
            //// sample the texture
            ////half4 col = tex2D(_MainTex, i.uv).rgb;
            //half3 posVert = _normals[instance_id].position;
            //half3 pos; 
            //half3 n;
            //half ds;
            //half4 result;
            //bool hit = _RayMarch(ro, rd,pos, ds);
            //if (hit) {
            //    half3 n = Get_Norm(pos);
            //    result = half4(n, 1);
            //   
            //}
            //else { discard; }
            //   
            //return half4(color * (1.0 - result.w) + result.xyz * result.w, 1.0);
        return i.color;
    }


    ENDHLSL

    SubShader {
        // UniversalPipeline needed to have this render in URP
        Tags{ "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

            // Forward Lit Pass
            Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off 
            Blend SrcAlpha OneMinusSrcAlpha 

            HLSLPROGRAM
            // Signal this shader requires a compute buffer
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 5.0

            // Lighting and shadow keywords
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

