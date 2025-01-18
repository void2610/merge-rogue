Shader"UI/SimpleUIBloom"
{
    Properties
    {
        _Luminance ("Luminance", Range(0.0, 10.0)) = 0.0
        _BlurColor("Blur Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _BlurDistance("Blur Distance", Range(0.0, 20.0)) = 8
        _BlurPower("Blur Power", Range(0.0, 1.0)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata
        {
            fixed2 uv : TEXCOORD0;
            fixed4 color : COLOR;
            fixed4 vertex : POSITION;
        };

        struct v2f
        {
            fixed2 uv : TEXCOORD0;
            fixed4 color : COLOR;
            fixed4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;

        fixed _Luminance;
        fixed3 _BlurColor;
        fixed _BlurDistance;
        fixed _BlurPower;

        v2f vert(appdata v)
        {
            v2f o;
            o.uv = v.uv;
            o.color = v.color;
            o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
        }

        ENDCG

        Pass
        {
            CGPROGRAM

            fixed4 blur(fixed2 uv, fixed w, fixed kx, fixed ky)
            {
                fixed2 shiftUv = fixed2(uv.x + kx * _BlurDistance, uv.y + ky * _BlurDistance);
                fixed4 tex = tex2D(_MainTex, shiftUv);

                tex.a = tex.a * w * _BlurPower * _Luminance;
                return tex;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = 0;

                //x軸 
                col += blur(i.uv, 0.01, 0, -0.0075);
                col += blur(i.uv, 0.02, 0, -0.006);
                col += blur(i.uv, 0.03, 0, -0.0045);
                col += blur(i.uv, 0.04, 0, -0.003);
                col += blur(i.uv, 0.05, 0, -0.002);
                col += blur(i.uv, 0.06, 0, -0.001);
                col += blur(i.uv, 0.06, 0, +0.001);
                col += blur(i.uv, 0.05, 0, +0.002);
                col += blur(i.uv, 0.04, 0, +0.003);
                col += blur(i.uv, 0.03, 0, +0.0045);
                col += blur(i.uv, 0.02, 0, +0.006);
                col += blur(i.uv, 0.01, 0, +0.0075);

                //y軸 
                col += blur(i.uv, 0.01, -0.0075, 0);
                col += blur(i.uv, 0.02, -0.006, 0);
                col += blur(i.uv, 0.03, -0.0045, 0);
                col += blur(i.uv, 0.04, -0.003, 0);
                col += blur(i.uv, 0.05, -0.002, 0);
                col += blur(i.uv, 0.06, -0.001, 0);
                col += blur(i.uv, 0.06, +0.001, 0);
                col += blur(i.uv, 0.05, +0.002, 0);
                col += blur(i.uv, 0.04, +0.003, 0);
                col += blur(i.uv, 0.03, +0.0045, 0);
                col += blur(i.uv, 0.02, +0.006, 0);
                col += blur(i.uv, 0.01, +0.0075, 0);

                //y軸を大きめに-にズラした上で、x軸を大きめに-から+に描画 
                col += blur(i.uv, 0.01, -0.006, -0.006);
                col += blur(i.uv, 0.02, -0.0045, -0.0045);
                col += blur(i.uv, 0.03, -0.003, -0.003);
                col += blur(i.uv, 0.04, -0.002, -0.002);
                col += blur(i.uv, 0.05, -0.001, -0.001);
                col += blur(i.uv, 0.05, +0.001, -0.001);
                col += blur(i.uv, 0.04, +0.002, -0.002);
                col += blur(i.uv, 0.03, +0.003, -0.003);
                col += blur(i.uv, 0.02, +0.0045, -0.0045);
                col += blur(i.uv, 0.01, +0.006, -0.006);

                //y軸を大きめに+にズラした上で、x軸を大きめに-から+に描画 
                col += blur(i.uv, 0.01, -0.006, 0.006);
                col += blur(i.uv, 0.02, -0.0045, 0.0045);
                col += blur(i.uv, 0.03, -0.003, 0.003);
                col += blur(i.uv, 0.04, -0.002, 0.002);
                col += blur(i.uv, 0.05, -0.001, 0.001);
                col += blur(i.uv, 0.05, +0.001, 0.001);
                col += blur(i.uv, 0.04, +0.002, 0.002);
                col += blur(i.uv, 0.03, +0.003, 0.003);
                col += blur(i.uv, 0.02, +0.0045, 0.0045);
                col += blur(i.uv, 0.01, +0.006, 0.006);

                //y軸を小さめに-にズラした上で、x軸を大きめに-から+に描画 
                col += blur(i.uv, 0.01, -0.006, -0.003);
                col += blur(i.uv, 0.02, -0.0045, -0.0022);
                col += blur(i.uv, 0.03, -0.003, -0.0015);
                col += blur(i.uv, 0.04, -0.002, -0.001);
                col += blur(i.uv, 0.05, -0.001, -0.0005);
                col += blur(i.uv, 0.05, +0.001, -0.0005);
                col += blur(i.uv, 0.04, +0.002, -0.001);
                col += blur(i.uv, 0.03, +0.003, -0.0015);
                col += blur(i.uv, 0.02, +0.0045, -0.0022);
                col += blur(i.uv, 0.01, +0.006, -0.003);

                 //y軸を小さめに+にズラした上で、x軸を大きめに-から+に描画 
                col += blur(i.uv, 0.01, -0.006, 0.003);
                col += blur(i.uv, 0.02, -0.0045, 0.0022);
                col += blur(i.uv, 0.03, -0.003, 0.0015);
                col += blur(i.uv, 0.04, -0.002, 0.001);
                col += blur(i.uv, 0.05, -0.001, 0.0005);
                col += blur(i.uv, 0.05, +0.001, 0.0005);
                col += blur(i.uv, 0.04, +0.002, 0.001);
                col += blur(i.uv, 0.03, +0.003, 0.0015);
                col += blur(i.uv, 0.02, -0.0045, 0.0022);
                col += blur(i.uv, 0.01, -0.006, 0.003);

                //y軸を大きめに-にズラした上で、x軸を小さめに-から+に描画 
                col += blur(i.uv, 0.01, -0.003, -0.006);
                col += blur(i.uv, 0.02, -0.0022, -0.0045);
                col += blur(i.uv, 0.03, -0.0015, -0.003);
                col += blur(i.uv, 0.04, -0.001, -0.002);
                col += blur(i.uv, 0.05, -0.0005, -0.001);
                col += blur(i.uv, 0.05, +0.0005, -0.001);
                col += blur(i.uv, 0.04, +0.001, -0.002);
                col += blur(i.uv, 0.03, +0.0015, -0.003);
                col += blur(i.uv, 0.02, +0.0022, -0.0045);
                col += blur(i.uv, 0.01, +0.003, -0.006);

                //y軸を大きめに+にズラした上で、x軸を小さめに-から+に描画 
                col += blur(i.uv, 0.01, -0.003, 0.006);
                col += blur(i.uv, 0.02, -0.0022, 0.0045);
                col += blur(i.uv, 0.03, -0.0015, 0.003);
                col += blur(i.uv, 0.04, -0.001, 0.002);
                col += blur(i.uv, 0.05, -0.0005, 0.001);
                col += blur(i.uv, 0.05, +0.0005, 0.001);
                col += blur(i.uv, 0.04, +0.001, 0.002);
                col += blur(i.uv, 0.03, +0.0015, 0.003);
                col += blur(i.uv, 0.02, +0.0022, 0.0045);
                col += blur(i.uv, 0.01, +0.003, 0.006);

                col.r = _BlurColor.r;
                col.g = _BlurColor.g;
                col.b = _BlurColor.b;

                return col;
            }

            ENDCG
        }

        Pass
        {
            CGPROGRAM

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                col.r = col.r + ((i.color.r - 0.5) * col.r * _Luminance);
                col.g = col.g + ((i.color.g - 0.5) * col.g * _Luminance);
                col.b = col.b + ((i.color.b - 0.5) * col.b * _Luminance);
                col.a *= i.color.a;
                return col;
            }

            ENDCG
        }
    }
}