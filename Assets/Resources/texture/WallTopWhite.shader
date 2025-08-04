Shader "Custom/WallTopWhite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _Threshold ("Up Dot Threshold", Float) = 0.95
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TopColor;
            float _Threshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Eğer normal yukarı bakıyorsa, beyaz yap (Y ekseni)
                float up = dot(normalize(i.normal), float3(0,1,0));
                if (up > _Threshold)
                    return _TopColor;
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
