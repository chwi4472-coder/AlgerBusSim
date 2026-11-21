Shader "AlgerBusSim/Unlit/Tilable"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _TileSize ("Tile Size (UV repeat)", Vector) = (4, 4, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _Tint;
            float4 _TileSize;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target {
                float2 t = i.uv * _TileSize.xy;
                fixed4 col = tex2D(_MainTex, t);
                col *= _Tint;
                return col;
            }
            ENDCG
        }
    }
}
