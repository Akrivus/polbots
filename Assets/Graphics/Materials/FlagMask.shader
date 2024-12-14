Shader "PolBol/Flag"
{
    Properties
    {
        _MainTex ("Flag (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            sampler2D _MaskTex;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uvMain : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvMain : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uvMain = IN.uvMain;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 main = tex2D(_MainTex, IN.uvMain);
                half4 mask = tex2D(_MaskTex, IN.uvMain);

                half alpha = mask.a;
                half3 color = main.rgb * mask.r;

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}