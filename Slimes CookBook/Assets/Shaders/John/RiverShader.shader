Shader "Unlit/RiverShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTex2 ("Texture2", 2D) = "white" {}
        _BaseColor("Color", Color) = (0, 0, 0.5, 0.3)

        [Header(Spec Layer 1)]       
        _SpecColor1("Spec Color", Color) = (1,1,1,1)
        _SpecDirection1("Spec Direction", Vector) = (0, 1, 0, 0)
        [Header(Spec Layer 2)]
        _SpecColor2("Spec Color 2", Color) = (1,1,1,1)
        _SpecDirection2("Spec Direction 2", Vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
         Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM            

            #pragma target 3.0
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #pragma vertex vert
            #pragma fragment frag
            

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _MainTex2;
            float4 _MainTex2_ST;

            float4 _BaseColor;

            float4 _SpecColor1;
            float2 _SpecDirection1;
            float4 _SpecColor2;
            float2 _SpecDirection2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv, _MainTex2);
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = _BaseColor;
                // sample the texture
                float2 specCoordinates1 = i.uv + _SpecDirection1 * _Time.y;
                half4 scroll = tex2D(_MainTex, specCoordinates1) * _SpecColor1;

                col.rgb = lerp(col.rgb, scroll.rgb, scroll.a);
                col.a = lerp(col.a, 1, scroll.a);

                //add second layer of moving specs
                float2 specCoordinates2 = i.uv2 + _SpecDirection2 * _Time.y;
                half4 specLayer2 = tex2D(_MainTex2, specCoordinates2) * _SpecColor2;
                col.rgb = lerp(col.rgb, specLayer2.rgb, specLayer2.a);
                col.a = lerp(col.a, 1, specLayer2.a);

                return col;
            }
                ENDHLSL
        }
    }
}
