Shader "Unlit/Brush2D"
{
   Properties
    {
        _Color ("Brush Color", Color) = (1,1,1,1) // 铅笔用白，橡皮用黑
        _Size ("Brush Size", Float) = 0.05
        _Softness ("Softness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            // 关键：混合模式
            // 铅笔：颜色设为白，使用 Blend One One (线性减淡)，白色不断叠加
            // 橡皮：颜色设为黑，使用 Blend One Zero (强制覆盖)，强制把区域刷黑
            Blend One One 
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            float _Size;
            float _Softness;
            float4 _Color;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // 计算当前像素点到 Quad 中心点的距离 (中心点 UV 是 0.5, 0.5)
                float dist = distance(input.uv, float2(0.5, 0.5));
                
                // 使用 smoothstep 实现边缘羽化
                // 当 dist < 内圈半径，alpha=1；当 dist > 外圈半径，alpha=0；中间平滑过渡
                float edge1 = _Size;
                float edge2 = _Size * (1.0 - _Softness);
                float alpha = smoothstep(edge1, edge2, dist);

                // 最终颜色乘上计算出的 Alpha，实现圆形笔刷
                return _Color * alpha;
            }
            ENDHLSL
        }
    }
}