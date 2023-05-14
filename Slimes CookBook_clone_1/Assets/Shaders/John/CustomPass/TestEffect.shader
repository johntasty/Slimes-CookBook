Shader "Custom/TestEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DinstanceAccuracy("ACcuracy", float) = 0
        _Max_steps("max_steps", float) = 0
        _Max_Distance("_Max_Distance", float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite On ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
             
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
           
            uniform float4x4 _FrustumCornersES;            
            uniform float4x4 _CameraToWorld;
            uniform float3 _CameraToWorldPosition;
            uniform float _Max_Distance;
            uniform float _Max_steps;
            half4 Sphere1;
            half _DinstanceAccuracy;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 rayDirection : TEXCOORD1;
                float3 ray: TEXCOORD2;
                float3 TextCordStereo : TEXCOORD3;
            };
            float2 TransformTriangleVertexToUV(float2 vertex)
            {
                float2 uv = (vertex ) ;
                return uv;
            }
            v2f vert (appdata v)
            {
                v2f o;
                
                v.vertex.z = 0.1;

                o.vertex = half4(v.vertex.xy,0.0,1.0);

                o.uv = v.uv;//TransformTriangleVertexToUV(v.vertex.xy);
             
                //o.TextCordStereo = TransformStereoScreenSpaceTex(o.uv, 1.0);
                int index = (o.uv.x / 2) + o.uv.y;
                o.ray = _FrustumCornersES[index];
                return o;
            }

            
            float sdSphere(half3 p, half3 pos, half s) {

                half d = distance(p, pos) - s;

                return d;
            }
          /*  half GetDist(half3 p)
            {               
                half d = sdSphere(p, Sphere1.w);
             
                return d;
            }*/
       /*     float3 calcNormal(in float3 pos)
            {
                const float2 eps = float2(0.001, 0.0);               
                float3 nor = float3(
                    GetDist(pos + eps.xyy).x - GetDist(pos - eps.xyy).x,
                    GetDist(pos + eps.yxy).x - GetDist(pos - eps.yxy).x,
                    GetDist(pos + eps.yyx).x - GetDist(pos - eps.yyx).x);
                return normalize(nor);
            }*/
            half4 raymarching(float3 ro, float3 rd) {
               
                half4 result = half4(1,1,1,1);
                float t = 0.01;//distance
                for (int i = 0; i < _Max_steps; i++)
                {
                    if (t > _Max_Distance) {                       
                        result = half4(rd, 1);
                        break;
                    }
                    half3 p = ro + rd * t;
                    float d = sdSphere(p, half3(1,0,0), 2);
                    if (d <= _DinstanceAccuracy) {
                        //float3 n = calcNormal(p);
                        result = half4(1,1,1,1);                                        
                        break;
                    }
                    t += d;
                }


                return result;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                
                //float nonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r;
                //float sceneEyeDepthtest = LinearEyeDepth(nonLinear, _ZBufferParams);
                ////sceneEyeDepthtest *= length(i.ray);

                half3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                //half3 result = raymarching(rayOrigin, rayDirection, sceneEyeDepthtest);
                half3 rayOr = _CameraToWorldPosition;
                half3 direction = normalize(i.ray);
                half4 result = raymarching(rayOr, direction);

                return half4(1 - col, 1);
            }
                ENDHLSL
        }
    }
}
