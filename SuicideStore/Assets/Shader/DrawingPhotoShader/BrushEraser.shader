Shader "Unlit/BrushEraser"
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
            // 【关键修改】：改用标准的 Alpha 混合
            // 结果 = (当前 Shader 颜色 * Alpha) + (背景颜色 * (1 - Alpha))
            Blend SrcAlpha OneMinusSrcAlpha 
            
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
                float dist = distance(input.uv, float2(0.5, 0.5));
                
                // 计算圆形遮罩
                float edge1 = _Size;
                float edge2 = _Size * (1.0 - _Softness);
                float alpha = smoothstep(edge1, edge2, dist);

                // 【关键修改】：返回颜色的 Alpha 通道决定了“擦除”的形状
                // 颜色部分使用 _Color (黑色)，Alpha 部分使用计算出的圆形遮罩
                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}