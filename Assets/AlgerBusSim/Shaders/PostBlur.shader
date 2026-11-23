Shader "AlgerBusSim/Post/Blur"
{
    Properties { _MainTex ("Source", 2D) = "white" {} _Direction ("Direction", Vector) = (1, 0, 0, 0) _TexelSize ("Texel", Vector) = (0.001, 0.001, 0, 0) }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _Direction; float4 _TexelSize;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            v2f vert (appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 off = _Direction.xy * _TexelSize.xy;
                fixed4 col = tex2D(_MainTex, i.uv) * 0.227027;
                col += tex2D(_MainTex, i.uv + off * 1.3846) * 0.316216;
                col += tex2D(_MainTex, i.uv - off * 1.3846) * 0.316216;
                col += tex2D(_MainTex, i.uv + off * 3.2308) * 0.070270;
                col += tex2D(_MainTex, i.uv - off * 3.2308) * 0.070270;
                return col;
            }
            ENDCG
        }
    }
}
