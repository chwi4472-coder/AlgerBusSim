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
                // procedural clouds
                float2 cuv = i.worldDir.xz / max(i.worldDir.y + 0.15, 0.05) * 0.6;
                float cloud = 0;
                float2 p = cuv * 1.2;
                cloud += 0.5 * (sin(p.x * 1.7 + sin(p.y * 2.3)) * 0.5 + 0.5);
                p = cuv * 2.5 + float2(3.1, 1.7);
                cloud += 0.3 * (sin(p.x * 2.1 + sin(p.y * 1.9 + 2.0)) * 0.5 + 0.5);
                p = cuv * 5.0 + float2(7.3, 4.1);
                cloud += 0.2 * (sin(p.x * 1.3 + sin(p.y * 2.7 + 1.0)) * 0.5 + 0.5);
                cloud = smoothstep(0.55, 0.85, cloud) * saturate(i.worldDir.y * 3.0);
                col.rgb = lerp(col.rgb, float3(1, 1, 1) * _Exposure, cloud * 0.55);
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
