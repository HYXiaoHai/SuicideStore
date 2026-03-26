Shader "Custom/ClockEraseWorld_Fixed"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Angle ("Angle", Range(0,360)) = 0
        // 新增：长宽比 (Width / Height)，默认为 1
        _AspectRatio("Aspect Ratio", Float) = 1 
    }

    SubShader
    {
        // 确保使用 UI 常用的渲染标签
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
            // 标准半透明混合
            Blend SrcAlpha OneMinusSrcAlpha
            // 关闭深度写入
            ZWrite Off
            // 关闭剔除
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                // 新增：UI 颜色，通常 UI 组件会传递这个
                float4 color : COLOR; 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float _Angle;
            float _AspectRatio; // 接收 C# 传来的长宽比

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // 传递顶点颜色
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 将 UV 居中，范围从 [0, 1] 变为 [-0.5, 0.5]
                float2 centeredUV = i.uv - float2(0.5, 0.5);

                // 2. *** 核心修正 ***
                // 如果是 16:9，则 centeredUV.x 的范围在视觉上被拉长了。
                // 我们通过乘以长宽比，将 X 轴“复原”到与 Y 轴相同的视觉尺度。
                float2 correctedUV = centeredUV;
                correctedUV.x *= _AspectRatio; 

                // 3. 计算角度 (atan2(x, y) 正北方为0度，顺时针为正)
                // 此时使用的是修正后的 UV，计算的是正圆的角度。
                float angleRad = atan2(correctedUV.x, correctedUV.y); 
                float ang = degrees(angleRad); // 转换为 -180 到 180
                
                // 将其映射到 0 到 360
                if (ang < 0) ang += 360;

                // 4. 采样原图 (使用原始 UV)
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // 5. 根据角度进行裁切/白色填充
                // 这里我默认您的 WhiteMask 是通过 Alpha 混合的。
                // 如果角度小于当前指针角度，使其变为完全不透明白色。
                if (ang <= _Angle)
                {
                    // 方案A：直接返回纯白，不带透明度 (即扫过的地方变成实体白块)
                    // return fixed4(1, 1, 1, 1); 

                    // 方案B (推荐)：让原图的透明度变为 1，颜色变为白。
                    // 这样可以保留原图的形状（如果原图是圆的）。
                    return fixed4(1, 1, 1, col.a); 
                }

                return col;
            }
            ENDCG
        }
    }
}