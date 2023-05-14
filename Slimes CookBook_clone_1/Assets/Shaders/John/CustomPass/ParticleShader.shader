// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ParticleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorLow("Color Slow Speed", Color) = (0, 0, 0.5, 0.3)
        _ColorHigh("Color High Speed", Color) = (1, 0, 0, 0.3)
        _HighSpeedValue("High speed Value", Range(0, 50)) = 25
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
              
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            // Particle's data
            struct Particle
            {
                float4x4 mat;
                float3 originPos;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Particle's data, shared with the compute shader
            StructuredBuffer<Particle> particleBuffers;

            // Properties variables
            uniform float4 _ColorLow;
            uniform float4 _ColorHigh;
            uniform float _HighSpeedValue;
            v2f vert (appdata v, uint instance_id : SV_InstanceID)
            {
                v2f o = (v2f)0;
                            
                float3 pos = float3(particleBuffers[instance_id].mat[0][3],
                    particleBuffers[instance_id].mat[1][3],
                    particleBuffers[instance_id].mat[2][3]);
                o.vertex = mul(UNITY_MATRIX_MVP, float4(pos, 1.0f));
                o.color = _ColorHigh;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
              /*  fixed4 col = tex2D(_MainTex, i.uv);*/
              
                return i.color;
            }
                ENDHLSL
        }
    }
}
