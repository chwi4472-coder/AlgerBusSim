Shader "AlgerBusSim/Post/BrightPass"
{
    Properties { _MainTex ("Source", 2D) = "white" {} _Threshold ("Threshold", Range(0, 2)) = 0.75 }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float _Threshold;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            v2f vert (appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float lum = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                float contrib = max(0, lum - _Threshold) / max(lum, 0.001);
                return col * contrib;
            }
            ENDCG
        }
    }
}
