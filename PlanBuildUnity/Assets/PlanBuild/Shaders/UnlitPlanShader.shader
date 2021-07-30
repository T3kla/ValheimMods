 
Shader "Unlit/UnlitPlanShader"
{
    Properties
    {
        _Color("Color", Color) = (0,0,0,0)
        _RuneColor("Rune Color", Color) = (0,0,0,0)
        _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _MainTex("Albedo (RGB)", 2D) = "black" {}
        _EmissionTex("Emission (RGB)", 2D) = "white" {}

        _CellSize("Cell Size", Range(0, 10)) = 1

        _NoiseScale("Scale", float) = (1, 1, 1, 1)
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

            #include "Random.cginc"

            float _CellSize;

            float easeIn(float interpolator) {
                return interpolator * interpolator;
            }

            float easeOut(float interpolator) {
                return 1 - easeIn(1 - interpolator);
            }

            float easeInOut(float interpolator) {
                float easeInValue = easeIn(interpolator);
                float easeOutValue = easeOut(interpolator);
                return lerp(easeInValue, easeOutValue, interpolator);
            }

            float3 _NoiseMovementDirection;
            float3 _NoiseScale;

            float perlinNoise(float3 value) {
                float3 fraction = frac(value);

                float interpolatorX = easeInOut(fraction.x);
                float interpolatorY = easeInOut(fraction.y);
                float interpolatorZ = easeInOut(fraction.z);

                float cellNoiseZ[2];
                [unroll]
                for (int z = 0;z <= 1;z++) {
                    float cellNoiseY[2];
                    [unroll]
                    for (int y = 0;y <= 1;y++) {
                        float cellNoiseX[2];
                        [unroll]
                        for (int x = 0;x <= 1;x++) {
                            float3 cell = floor(value) + float3(x, y, z);
                            float3 cellDirection = rand3dTo3d(cell) * 2 - 1;
                            float3 compareVector = fraction - float3(x, y, z);
                            cellNoiseX[x] = dot(cellDirection, compareVector);
                        }
                        cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
                    }
                    cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
                }
                float noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);
                return noise;
            }


            float perlinNoise2d(float2 value) {
                //generate random directions
                float2 lowerLeftDirection = rand2dTo2d(float2(floor(value.x), floor(value.y))) * 2 - 1;
                float2 lowerRightDirection = rand2dTo2d(float2(ceil(value.x), floor(value.y))) * 2 - 1;
                float2 upperLeftDirection = rand2dTo2d(float2(floor(value.x), ceil(value.y))) * 2 - 1;
                float2 upperRightDirection = rand2dTo2d(float2(ceil(value.x), ceil(value.y))) * 2 - 1;

                float2 fraction = frac(value);

                //get values of cells based on fraction and cell directions
                float lowerLeftFunctionValue = dot(lowerLeftDirection, fraction - float2(0, 0));
                float lowerRightFunctionValue = dot(lowerRightDirection, fraction - float2(1, 0));
                float upperLeftFunctionValue = dot(upperLeftDirection, fraction - float2(0, 1));
                float upperRightFunctionValue = dot(upperRightDirection, fraction - float2(1, 1));

                float interpolatorX = easeInOut(fraction.x);
                float interpolatorY = easeInOut(fraction.y);

                //interpolate between values
                float lowerCells = lerp(lowerLeftFunctionValue, lowerRightFunctionValue, interpolatorX);
                float upperCells = lerp(upperLeftFunctionValue, upperRightFunctionValue, interpolatorX);

                float noise = lerp(lowerCells, upperCells, interpolatorY);
                return noise;
            }


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
            half2 _IgnoreNormal;

            sampler2D _EmissionTex;
            float4 _EmissionTex_ST; 

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
                float3 value = v.worldPos * _NoiseScale;
                //get noise and adjust it to be ~0-1 range
                float noise = perlinNoise(value  + _NoiseMovementDirection * _Time.y) + 0.5;

                float yOffset = value.y + _NoiseMovementDirection.y * _Time.y;
                float section = floor(yOffset);

              //  float noise = perlinNoise2d(v.worldPos.xz ) + value.y + _NoiseMovementDirection.y * _Time.y;

                noise = frac(noise * 6);
                noise = pow(noise, 3) / 2;
               // noise = 1;

              // float pixelNoiseChange = fwidth(noise);
              //
              // float heightLine = smoothstep(1 - pixelNoiseChange, 1, noise);
              // heightLine += smoothstep(pixelNoiseChange, 0, noise);
              //
              // noise = heightLine;

                float2 textureCoordinateX = v.worldPos.zy;
                float2 textureCoordinateY = v.worldPos.xz;
                float2 textureCoordinateZ = v.worldPos.xy;

                float2 xEmissionCoordinate = TRANSFORM_TEX(textureCoordinateX, _EmissionTex);
                float2 yEmissionCoordinate = TRANSFORM_TEX(textureCoordinateY, _EmissionTex);
                float2 zEmissionCoordinate = TRANSFORM_TEX(textureCoordinateZ, _EmissionTex);
                 
                xEmissionCoordinate = xEmissionCoordinate + _MainMovementDirection * _Time.y;
                yEmissionCoordinate = yEmissionCoordinate + _MainMovementDirection * _Time.y;
                zEmissionCoordinate = zEmissionCoordinate + _MainMovementDirection * _Time.y;

                float xRuneAlpha = tex2D(_EmissionTex, xEmissionCoordinate).r;
                float yRuneAlpha = tex2D(_EmissionTex, yEmissionCoordinate).r;
                float zRuneAlpha = tex2D(_EmissionTex, zEmissionCoordinate).r;

                float xNormalFactor = abs(dot(v.normal, float3(1, 0, 0)));
                float yNormalFactor = abs(dot(v.normal, float3(0, 1, 0)));
                float zNormalFactor = abs(dot(v.normal, float3(0, 0, 1)));
                 
                xRuneAlpha = noise * xRuneAlpha;
                yRuneAlpha = noise * yRuneAlpha;
                zRuneAlpha = noise * zRuneAlpha;

                float xRuneSelect = step(0.08, noise * step(0.01, xRuneAlpha));
                float yRuneSelect = step(0.08, noise * step(0.01, yRuneAlpha));
                float zRuneSelect = step(0.08, noise * step(0.01, zRuneAlpha));

                float xNormalSelect = smoothstep(0.7, 0.8, xNormalFactor);
                float yNormalSelect = smoothstep(0.7, 0.8, yNormalFactor);
                float zNormalSelect = smoothstep(0.7, 0.8, zNormalFactor);

                float xSelect = lerp(0, xRuneSelect, xNormalSelect);
                float ySelect = lerp(0, yRuneSelect, yNormalSelect);
                float zSelect = lerp(0, zRuneSelect, zNormalSelect);

                //o.Albedo = lerp(_RuneColor, 0, saturate(xNormalSelect * yNormalSelect * zNormalSelect));
                float runeSelect = step(0.01, xSelect)
                    + step(0.01, ySelect)
                    + step(0.01, zSelect);

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
