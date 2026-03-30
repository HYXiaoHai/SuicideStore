Shader "Custom/ClockEraseWorld"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SecondTex ("Second Texture", 2D) = "white" {}
        _Angle ("Angle", Range(0,360)) = 0
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

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 dir = i.uv - float2(0.5,0.5);
                float ang = degrees(atan2(dir.x, dir.y));
                ang = (ang + 360) % 360;

                if (ang < _Angle)
                    return tex2D(_SecondTex, i.uv);   // 擦除区域显示第二张纹理
                else
                    return tex2D(_MainTex, i.uv);      // 未擦除区域显示原纹理
            }
            ENDCG
        }
    }
}