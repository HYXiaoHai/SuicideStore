// Sprite_EdgeBlueGlow.shader
// URP 专用：Sprite/UGUI 蓝色外发光边缘，配合Bloom出光晕
Shader "Custom/Sprite_EdgeBlueGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR]_Color ("Tint", Color) = (1,1,1,1)
        
        _EdgeWidth ("Edge Width", Range(1, 10)) = 3.0
        [HDR]_EdgeColor ("Edge Blue Glow", Color) = (0, 0.6, 1.0, 1)
        _EdgeIntensity ("Edge Intensity", Range(0, 5)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "Sprite Edge Glow"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            // 关键修复：显式声明纹理像素尺寸
            float4 _MainTex_TexelSize;
            float4 _Color;

            float _EdgeWidth;
            half4 _EdgeColor;
            float _EdgeIntensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 mainCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                mainCol *= IN.color * _Color;
                half alpha = mainCol.a;

                // 取像素步长
                float2 texelSize = _MainTex_TexelSize.xy;
                float2 offset = texelSize * _EdgeWidth;

                float aUp    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, offset.y)).a;
                float aDown  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(0, offset.y)).a;
                float aLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(offset.x, 0)).a;
                float aRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(offset.x, 0)).a;

                float edge = saturate((aUp + aDown + aLeft + aRight) * 0.25 - alpha);
                half3 edgeGlow = _EdgeColor.rgb * edge * _EdgeIntensity;
                half3 finalRGB = mainCol.rgb + edgeGlow;

                finalRGB *= alpha;
                return half4(finalRGB, alpha);
            }
            ENDHLSL
        }
    }
}