Shader "AlgerBusSim/Post/Composite"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _BloomTex ("Bloom", 2D) = "black" {}
        _BloomIntensity ("Bloom Intensity", Range(0, 2)) = 0.35
        _Contrast ("Contrast", Range(0.5, 2)) = 1.12
        _Saturation ("Saturation", Range(0, 2)) = 1.15
        _Temperature ("Temperature", Range(-0.5, 0.5)) = 0.06
        _VignetteIntensity ("Vignette", Range(0, 1)) = 0.32
        _VignetteSmooth ("Vignette Smooth", Range(0.1, 1)) = 0.45
        _Exposure ("Exposure", Range(0.5, 2)) = 1.05
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _BloomTex;
            float _BloomIntensity, _Contrast, _Saturation, _Temperature, _VignetteIntensity, _VignetteSmooth, _Exposure;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            v2f vert (appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 bloom = tex2D(_BloomTex, i.uv);
                col.rgb += bloom.rgb * _BloomIntensity;
                col.rgb *= _Exposure;
                // contrast
                col.rgb = (col.rgb - 0.5) * _Contrast + 0.5;
                // saturation
                float lum = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                col.rgb = lerp(float3(lum, lum, lum), col.rgb, _Saturation);
                // temperature (warm)
                col.r += _Temperature * 0.1;
                col.b -= _Temperature * 0.08;
                // vignette
                float2 d = i.uv - 0.5;
                float vig = 1.0 - dot(d, d) * _VignetteIntensity * 4.0;
                vig = smoothstep(0, _VignetteSmooth, vig);
                col.rgb *= vig;
                col.rgb = saturate(col.rgb);
                return col;
            }
            ENDCG
        }
    }
}
