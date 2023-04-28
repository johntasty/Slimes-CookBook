Shader "Hidden/Ball"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DinstanceAccuracy("ACcuracy", float) = 0
        _Max_steps("max_steps", float) = 0
        _Max_Distance("_Max_Distance", float) = 0
        
        _Positions("Position", Vector) = (0,0,0,0)
        _Width("_Width", float) = 0
        _Height("_Height", float) = 0
    }
    SubShader
    {
        // No culling or depth
      
        Tags{ "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
         
        Pass
        {
             Tags { "LightMode" = "UniversalForward" }
               Cull Off
               ZWrite On
               Blend Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
             TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 rayOrigin : TEXCOORD1;
                float3 rayDirection : TEXCOORD2;
                float3 posWS : TEXCOORD3;
                float2 Puv : TEXCOORD4;
            };

            float4x4 _CameraInverseProjection;
            float4x4 _CameraWorld;

            float3 _CameraToWorldPosition;
            float _Max_Distance;
            float _Max_steps;
            half _DinstanceAccuracy;

            half _Width;
            half _Height;

            float4 _Positions;

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
                float3 direction = mul(_CameraInverseProjection, float4(uv, 0, 1)).xyz;
                direction = mul(_CameraWorld, float4(direction, 0)).xyz;
                //direction = normalize(direction);
                return CreateRay(origin, direction);
            }

            v2f vert (appdata v)
            {
                v2f o;
              
                o.pos = TransformObjectToHClip(v.vertex);
                o.posWS = TransformObjectToWorld(v.vertex);
                o.uv = v.vertex.xy;
                uint width, height;
                _MainTex.GetDimensions(width, height);
                float2 uv = o.uv / float2(width, height) * 2 - 1;
                Ray ray = CreateCameraRay(uv);
                o.rayOrigin = ray.origin;
                o.rayDirection = ray.direction;
               
                return o;
            }

            float sdSphere(float3 eye, float3 center,float s) {
                half d = distance(eye, center) - s;

                return d;
           }
            float GetDist(float3 eye) {
                float d = sdSphere(eye, _Positions.xyz, _Positions.w);
                return d;
            }
           /* float map(float3 p) {
                return sdSphere(p, 1.0);
            }*/
            half3 Get_Norm(half3 p)
            {
                half2 e = half2(0.001, 0);
                half3 n = half3(
                    GetDist(p + e.xyy) - GetDist(p - e.xyy),
                    GetDist(p + e.yxy) - GetDist(p - e.yxy),
                    GetDist(p + e.yyx) - GetDist(p - e.yyx));
                return normalize(n);
            }
            half4 raymarch(float3 ro, float3 rd) {

                half4 ret = half4(0, 0, 0, 0);              
                                
                float t = 0; // current distance traveled along ray
                for (int i = 0; i < _Max_steps; ++i) {
                   
                    float dst = GetDist(ro);
                    if (t > _Max_Distance)
                    {           
                        //ret = half4(rd, 1);
                        break;
                    }                   
                    float3 pointOnSurface = ro + rd * t; // World space position of sample
                    if (dst <= 0.001) {
                       
                        float3 norm = Get_Norm(pointOnSurface);
                        ret = half4(norm, 1);
                        break;
                    }
                    ro += rd * dst;
                    t += dst;
                }

                return ret;
            }
            half4 frag(v2f i) : SV_Target
            {
              
                float3 rd = normalize(i.rayDirection);
                float3 ro = i.rayOrigin;
               
                // just invert the colors
                half3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb; // Color of the scene before this shader was run
                half4 add = raymarch(ro, rd);

                // Returns final color using alpha blending
                return half4(col * (1.0 - add.w) + add.xyz * add.w, 1.0);
                //return half4(rd, 1);

            }
                ENDHLSL
        }
    }
}
