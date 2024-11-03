Shader "PolBol/Flag"
{
    Properties
    {
        _MainTex ("Flag (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MaskTex;

        struct Input
		{
			float2 uv_MainTex;
            float2 uv_MaskTex;
		};

        void surf(Input i, inout SurfaceOutputStandard o)
		{
			fixed4 main = tex2D(_MainTex, i.uv_MainTex);
			fixed4 mask = tex2D(_MaskTex, i.uv_MaskTex);

            o.Alpha = mask.a;

            fixed4 color = main * mask.r;
            o.Albedo = color.rgb * 1.27079633;
		}
        ENDCG
    }
    FallBack "Transparent"
}
