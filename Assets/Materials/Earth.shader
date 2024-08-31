Shader "PolBot/Earth"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _CityTex ("City Lights", 2D) = "black" {}
        _CloudTex("Cloud Texture", 2D) = "black" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _ParallaxMap("Parallax Map", 2D) = "black" {}
        _SpecularMap("Specular Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #pragma vertex vert
            #pragma fragment frag

            #define PI 3.141592653589793238462f
            #define PI2 6.283185307179586476924f       

            struct appdata
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                float3 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : POSITION;
                float2 uv : TEXTCOORD0;
                float3 light : TEXCOORD1;
                float3 view : TEXCOORD2;
                float3 tangent : TEXCOORD3;
                float3 binormal : TEXCOORD4;
                float3 normal : TEXCOORD5;
            };

            float4 _MainTex_ST;
            sampler2D _MainTex;
            sampler2D _CityTex;
            sampler2D _CloudTex;
            sampler2D _BumpMap;
            sampler2D _ParallaxMap;
            sampler2D _SpecularMap;

            float2 azimuthal(float2 uv) {                  
                float2 coord = (uv - .5) * 4; 

                float radius = length(coord);
                float angle = atan2(coord.y, coord.x) + PI + _Time.y * 0.001;
                float projection = 2 * acos(radius / 2.) - PI / 2;
                return float2(angle, projection);
            }          

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.position);
                o.uv = v.uv;

                float4 world = mul(unity_ObjectToWorld, v.position);
                float time = _Time.y * 0.005;
                float4 light = float4(sin(time), 0, cos(time), 0);
                o.light = normalize(world.xyz - light.xyz);
                o.view = normalize(world.xyz - float3(0, 0, 0));
                
                float3 binormal = mul(unity_ObjectToWorld, cross(v.normal, v.tangent.xyz));
                float3 normal = mul(unity_ObjectToWorld, v.normal);
                float3 tangent = mul(unity_ObjectToWorld, v.tangent);

				o.binormal = binormal;
                o.normal = normal;
                o.tangent = tangent;

                return o;
            } 

            fixed4 frag(v2f i) : SV_Target
            {
                float2 coord = azimuthal(i.uv);
                float x = coord.x;
                float y = log(tan(PI / 4. + coord.y / 2.));
                x = x / PI2;
                y = (y + PI) / PI2;       

                float time = sin(_Time.y * 0.1) * 0.00125;
                float2 uv = float2(x, y) + time;

                float height = tex2D(_ParallaxMap, uv).r;

                float3x3 TBN = transpose(float3x3(
                    normalize(i.tangent),
                    normalize(i.binormal),
                    normalize(i.normal)));
                float3 view = normalize(i.view);
                uv = uv + mul(TBN, view).xy * height * 0.025;

                float3 tangent = normalize(tex2D(_BumpMap, uv).rgb) * 2 - 1;
                float3 normal = mul(TBN, tangent);
                
                fixed4 albedo = tex2D(_MainTex, uv);
                float3 light = normalize(i.light);
                float3 diffuse = saturate(dot(normal, -light));
                diffuse = albedo.rgb * diffuse;
                
                float4 specular = tex2D(_SpecularMap, uv) * 0.1;
                float3 reflection = reflect(light, normal);
				float intensity = saturate(dot(reflection, view));
                specular = specular * pow(intensity, 16);

                float3 ambient = tex2D(_CityTex, uv);
                ambient *= 1 - saturate(dot(i.normal, -i.light));

                fixed4 col = fixed4(diffuse + ambient, 1);

                float4 cloud1 = tex2D(_CloudTex, uv + 0.1 * time);
                float4 cloud2 = tex2D(_CloudTex, uv + 0.2 * time);
                col += (cloud1 * cloud1.r) + (cloud2 * cloud2.r * 0.2);

                col = length(i.uv * 2 - 1) > 1 ? 0 : col;

                return col;
            }
            ENDCG
        }
    }
}