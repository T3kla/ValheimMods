 
Shader "Unlit/UnlitPlanShader"
{
    Properties
    {
        _Color("Color", Color) = (0,0,0,0)
        _RuneColor("Rune Color", Color) = (0,0,0,0)
        _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _MainTex("Albedo (RGB)", 2D) = "black" {}
        _EmissionTex("Emission (RGB)", 2D) = "black" {}

        _NoiseTex("Noise", 2D) = "white" {}
        _MainMovementDirection("Movement Direction Main", float) = (0, -1, 0, 1)
        _NoiseMovementDirection("Movement Direction Noise", float) = (0, 1, 0, 1)

        _IgnoreNormal("Ignore normal", float) = (0,1,0,1)

        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }
    SubShader
    {
        Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" "IsEmissive" = "true" }

        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull[_Cull]
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM  
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
      
            struct appdata
            { 
               float4 vertex : POSITION; // vertex position
               float4 normal : NORMAL; // vertex position
            };

            struct v2f
            { 
                float4 pos  : POSITION;
                UNITY_FOG_COORDS(1)
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL; // vertex position
            };


            half2 _MainMovementDirection;
            half2 _NoiseMovementDirection;
            half2 _IgnoreNormal;

            sampler2D _EmissionTex;
            float4 _EmissionTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            fixed4 _Color;
            fixed4 _RuneColor;
            fixed4 _EmissionColor;

            v2f vert (appdata v)
            {
                v2f o;  
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f v) : SV_Target
            { 
                float2 textureCoordinateX = v.worldPos.zy;
                float2 textureCoordinateY = v.worldPos.xz;
                float2 textureCoordinateZ = v.worldPos.xy;

                float2 xEmissionCoordinate = TRANSFORM_TEX(textureCoordinateX, _EmissionTex);
                float2 yEmissionCoordinate = TRANSFORM_TEX(textureCoordinateY, _EmissionTex);
                float2 zEmissionCoordinate = TRANSFORM_TEX(textureCoordinateZ, _EmissionTex);


                float2 noiseCoordinate =  TRANSFORM_TEX(textureCoordinateZ, _NoiseTex);

               
                xEmissionCoordinate = xEmissionCoordinate + _MainMovementDirection * _Time.y;
                yEmissionCoordinate = yEmissionCoordinate + _MainMovementDirection * _Time.y;
                zEmissionCoordinate = zEmissionCoordinate + _MainMovementDirection * _Time.y;

                float xRuneAlpha = tex2D(_EmissionTex, xEmissionCoordinate).r;
                float yRuneAlpha = tex2D(_EmissionTex, yEmissionCoordinate).r;
                float zRuneAlpha = tex2D(_EmissionTex, zEmissionCoordinate).r;

                float xNormalFactor = abs(dot(v.normal, float3(1, 0, 0)));
                float yNormalFactor = abs(dot(v.normal, float3(0, 1, 0)));
                float zNormalFactor = abs(dot(v.normal, float3(0, 0, 1)));

                noiseCoordinate = noiseCoordinate + _NoiseMovementDirection * _Time.y;
                float noiseAlpha = tex2D(_NoiseTex, noiseCoordinate).r;

                xRuneAlpha = noiseAlpha * xRuneAlpha;
                yRuneAlpha = noiseAlpha * yRuneAlpha;
                zRuneAlpha = noiseAlpha * zRuneAlpha;

                float xRuneSelect = step(0.08, noiseAlpha * step(0.01, xRuneAlpha));
                float yRuneSelect = step(0.08, noiseAlpha * step(0.01, yRuneAlpha));
                float zRuneSelect = step(0.08, noiseAlpha * step(0.01, zRuneAlpha));

                float xNormalSelect = smoothstep(0.67, 0.8, xNormalFactor);
                float yNormalSelect = smoothstep(0.67, 0.8, yNormalFactor);
                float zNormalSelect = smoothstep(0.67, 0.8, zNormalFactor);

                float xSelect = lerp(0, xRuneSelect, xNormalSelect);
                float ySelect = lerp(0, yRuneSelect, yNormalSelect);
                float zSelect = lerp(0, zRuneSelect, zNormalSelect);

                //o.Albedo = lerp(_RuneColor, 0, saturate(xNormalSelect * yNormalSelect * zNormalSelect));
                float runeSelect = step(0.1, xSelect)
                    + step(0.1, ySelect)
                    + step(0.1, zSelect);

                float4 color = lerp(_EmissionColor, _RuneColor, runeSelect); 
                color.a = max(_EmissionColor.a * _EmissionColor.a,
                      lerp(0, xRuneAlpha, xSelect)
                    + lerp(0, yRuneAlpha, ySelect)
                    + lerp(0, zRuneAlpha, zSelect)
                );
        
                return color;
            }
            ENDCG
        }
    }
}
