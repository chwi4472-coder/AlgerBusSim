Shader "AlgerBusSim/Unlit/Alpha"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _AlphaTex ("Alpha Map", 2D) = "white" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _Tint;
            float _Smoothness;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed a = tex2D(_AlphaTex, i.uv).a;
                col.a = a * _Smoothness;
                col *= _Tint;
                return col;
            }
            ENDCG
        }
    }
}
