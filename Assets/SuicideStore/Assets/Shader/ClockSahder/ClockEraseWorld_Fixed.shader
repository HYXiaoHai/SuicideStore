Shader "Custom/ClockEraseWorld_Fixed"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SecondTex ("Second Texture", 2D) = "white" {}
        _Angle ("Angle", Range(0,360)) = 0
        _CenterX ("Center X", Range(0,1)) = 0.5
        _CenterY ("Center Y", Range(0,1)) = 0.5
        _AspectRatio ("Aspect Ratio", Float) = 1 
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

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
            sampler2D _SecondTex;
            float _Angle;
            float _CenterX;
            float _CenterY;
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
                // 1. 相对于可调节中心点的偏移
                float2 centeredUV = i.uv - float2(_CenterX, _CenterY);

                // 2. 修正长宽比（用于圆形擦除的形状修正）
                float2 correctedUV = centeredUV;
                correctedUV.x *= _AspectRatio;

                // 3. 计算角度（12点方向为0度，顺时针为正）
                float angleRad = atan2(correctedUV.x, correctedUV.y);
                float ang = degrees(angleRad);
                if (ang < 0) ang += 360;

                // 4. 根据角度决定显示哪张纹理，并乘以顶点颜色（支持UI色调）
                fixed4 col;
                if (ang < _Angle)
                {
                    col = tex2D(_SecondTex, i.uv) * i.color;
                }
                else
                {
                    col = tex2D(_MainTex, i.uv) * i.color;
                }

                return col;
            }
            ENDCG
        }
    }
}