Shader "AlgerBusSim/Sky/Dome"
{
    Properties
    {
        _TopColor ("Zenith", Color) = (0.12, 0.31, 0.48, 1)
        _HorizonColor ("Horizon", Color) = (0.79, 0.72, 0.60, 1)
        _GroundColor ("Ground", Color) = (0.28, 0.26, 0.23, 1)
        _SunDir ("Sun Direction", Vector) = (0, 1, 0, 0)
        _SunColor ("Sun Color", Color) = (1, 0.95, 0.8, 1)
        _SunSize ("Sun Size", Range(0, 0.5)) = 0.04
        _Exposure ("Exposure", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Background" "Queue" = "Background" }
        LOD 100
        Cull Off
        ZWrite Off
        Fog { Mode Off }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float3 worldDir : TEXCOORD0; };
            float3 _SunDir;
            v2f vert (appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.worldDir = normalize(v.vertex.xyz); return o; }
            fixed4 _TopColor, _HorizonColor, _GroundColor, _SunColor;
            float _SunSize, _Exposure;
            fixed4 frag (v2f i) : SV_Target
            {
                float y = i.worldDir.y;
                fixed4 col;
                if (y >= 0)
                {
                    float t = pow(y, 0.45);
                    col = lerp(_HorizonColor, _TopColor, t);
                }
                else
                {
                    float t = pow(-y, 0.6);
                    col = lerp(_HorizonColor, _GroundColor, t);
                }
                // sun
                float sunDot = saturate(dot(i.worldDir, _SunDir));
                float sun = pow(sunDot, 1.0 / max(_SunSize, 0.001));
                col.rgb += _SunColor.rgb * sun * 0.8;
                col.rgb *= _Exposure;
                return col;
            }
            ENDCG
        }
    }
}
