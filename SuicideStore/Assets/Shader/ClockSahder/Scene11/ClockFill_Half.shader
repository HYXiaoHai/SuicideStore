Shader "Custom/ClockFill_Half"
{
    Properties
    {
        _MainTex ("未填充纹理 (左侧背景)", 2D) = "white" {}
        _FillTex ("填充纹理 (右侧已擦除)", 2D) = "white" {}
        _Progress ("填充进度 (0~1)", Range(0,1)) = 0
        _AspectRatio ("Aspect Ratio", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _FillTex;
            float _Progress;
            float _AspectRatio;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算相对于中心点的 UV（中心为 (0.5,0.5)）
                float2 centered = i.uv - float2(0.5, 0.5);
                centered.x *= _AspectRatio; // 修正宽高比

                // 计算角度（-90° 为 9点方向，+90° 为 3点方向）
                float angle = atan2(centered.y, centered.x) * 180 / UNITY_PI;
                // 将角度范围 -90~90 映射到 0~1
                float t = (angle + 90) / 180.0;
                t = clamp(t, 0, 1);

                // 根据进度决定用哪张图
                fixed4 col;
                if (t <= _Progress)
                    col = tex2D(_FillTex, i.uv) * i.color;
                else
                    col = tex2D(_MainTex, i.uv) * i.color;

                return col;
            }
            ENDCG
        }
    }
}