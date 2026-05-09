Shader "Unlit/SpriteReveal2D"
{
 Properties
    {
        [MainTexture] _MainTex("Empty Photo (Base)", 2D) = "white" {} // 纯红图
        _RevealMap("Text Photo (Reveal)", 2D) = "white" {}          // 纯绿图
        _MaskTex("Mask Render Texture", 2D) = "black" {}              // 绘制生成的 RT
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // 标准的 Sprite 混合模式

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_RevealMap);
            SAMPLER(sampler_RevealMap);
            
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color; // 接收 SpriteRenderer 的颜色
                return output;
            }

            half4 frag(Varyings input) : SV_Target
{
    // 1. 采样底图：使用对应的 sampler_MainTex
    half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
    
    // 2. 采样揭晓图
    half4 revealColor = SAMPLE_TEXTURE2D(_RevealMap, sampler_RevealMap, input.uv);
    
    // 3. 采样蒙版 RT
    half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv);

    half4 finalColor = lerp(baseColor, revealColor, mask.r);
    return finalColor * input.color;
}
            ENDHLSL
        }
    }
}
