Shader "Hidden/TestEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            
    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
           
            uniform float4x4 _FrustumCornersES;            
            uniform float4x4 _CameraToWorld;
            uniform float max_Distance;
            half4 Sphere1;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
                float3 rayOri : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                half index = v.vertex.z;
                v.vertex.z = 0;

                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv.xy;

                o.ray = _FrustumCornersES[(int)index].xyz;

                o.ray /= abs(o.ray.z);
                o.rayOri = GetCameraPositionWS();
                o.ray = mul(_CameraToWorld, o.ray);
                return o;
            }

           
            float sdSphere(half3 p, half s) {

                half d = length(p) - s;

                return d;
            }
            half GetDist(half3 p)
            {               
                half d = sdSphere(p - Sphere1.xyz, Sphere1.w);
             
                return d;
            }
            float3 calcNormal(in float3 pos)
            {
                const float2 eps = float2(0.001, 0.0);
                // The idea here is to find the "gradient" of the distance field at pos
                // Remember, the distance field is not boolean - even if you are inside an object
                // the number is negative, so this calculation still works.
                // Essentially you are approximating the derivative of the distance field at this point.
                float3 nor = float3(
                    GetDist(pos + eps.xyy).x - GetDist(pos - eps.xyy).x,
                    GetDist(pos + eps.yxy).x - GetDist(pos - eps.yxy).x,
                    GetDist(pos + eps.yyx).x - GetDist(pos - eps.yyx).x);
                return normalize(nor);
            }
            half4 raymarching(float3 ro, float3 rd, float s) {
                half4 result = half4(1, 1, 1, 1);
                const int max_steps = 64;
                float t = 0;//distance
                for (int i = 0; i < max_steps; i++)
                {
                    if (t >= s || t > max_Distance) {
                        //result = half4(rd, 1);
                        break;
                    }
                    float3 p = ro + rd * t;
                    float d = GetDist(p);
                    if (d < 0.001) {
                        float3 n = calcNormal(p);
                        result = half4(n, 1);
                        break;
                    }
                    t += d;
                }


                return result;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                float3 rayDirection = normalize(i.ray.xyz);
                float3 rayOrigin = i.rayOri;
                float2 duv = i.uv;

                float nonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r;
                float sceneEyeDepthtest = LinearEyeDepth(nonLinear, _ZBufferParams);
                sceneEyeDepthtest *= length(i.ray);

                half3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                half4 result = raymarching(rayOrigin, rayDirection, sceneEyeDepthtest);
                return half4(col * (1.0 - result.w) + result.xyz * result.w, 1.0);
            }
                ENDHLSL
        }
    }
}
