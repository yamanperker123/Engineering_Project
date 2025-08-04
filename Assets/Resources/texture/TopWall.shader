Shader "Custom/TopWall"
{
    Properties
    {
        _MainTex   ("Wall Texture", 2D) = "white" {}  
        _Threshold("Vertical Threshold", Range(0,1)) = 0.7  
        _Falloff   ("Smooth Falloff",     Range(0,1)) = 0.1  
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        float    _Threshold;
        float    _Falloff;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 1) Duvar dokusu
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

            // 2) Y ekseni normalini al, pozitif ve negatif için aynı olsun
            float ny = abs(IN.worldNormal.y);

            // 3) Eşiği baz alarak yumuşak mask oluştur
            float lower = _Threshold - _Falloff * 0.5;
            float upper = _Threshold + _Falloff * 0.5;
            float mask  = saturate((ny - lower) / (upper - lower));

            // 4) Mask’e göre karıştır: 0 → doku, 1 → beyaz
            o.Albedo = lerp(c.rgb, float3(1,1,1), mask);
            o.Metallic   = 0;
            o.Smoothness = 0.5;
            o.Alpha      = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}