Shader "Custom/ClockEraseWorld"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SecondTex ("Second Texture", 2D) = "white" {}
        _Angle ("Angle", Range(0,360)) = 0
        _CenterX ("Center X", Range(0,1)) = 0.5
        _CenterY ("Center Y", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SecondTex;
            float _Angle;
            float _CenterX;
            float _CenterY;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 使用可调节的中心点
                float2 center = float2(_CenterX, _CenterY);
                float2 dir = i.uv - center;

                float ang = degrees(atan2(dir.x, dir.y));
                ang = (ang + 360) % 360;

                if (ang < _Angle)
                    return tex2D(_SecondTex, i.uv);
                else
                    return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}