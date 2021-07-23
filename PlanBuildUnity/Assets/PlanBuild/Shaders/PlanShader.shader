Shader "MarcoPogo/PlanShader"
{
	Properties
    {
        _Color("Color", Color) = (0,0,0,0)
        _RuneColor ("Rune Color", Color) = (0,0,0,0)
        _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _MainTex ("Albedo (RGB)", 2D) = "black" {} 
        _EmissionTex("Emission (RGB)", 2D) = "black" {}

        _NoiseTex ("Noise", 2D) = "white" {}
        _MainMovementDirection ("Movement Direction Main", float) = (0, -1, 0, 1)
        _NoiseMovementDirection ("Movement Direction Noise", float) = (0, 1, 0, 1)

        _IgnoreNormal ("Ignore normal", float) = (0,1,0,1 )
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" "IsEmissive" = "true" }
 
        Blend SrcAlpha SrcAlpha 
        LOD 100
        Cull [_Cull]
        Lighting Off
        ZWrite Off


        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert alpha:fade  //noshadow  nometa 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;  
        sampler2D _EmissionTex;
        float4 _EmissionTex_ST;
        sampler2D _NoiseTex; 
        float4 _NoiseTex_ST;
        

        half2 _MainMovementDirection;
		half2 _NoiseMovementDirection;
        half2 _IgnoreNormal;

        struct Input
        {  
            float3 worldPos; 
            float3 worldNormal;
        };
         

        fixed4 _Color;
        fixed4 _RuneColor;
        fixed4 _EmissionColor;
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
         
        void surf (Input IN, inout SurfaceOutput o)
        {  

            float2 textureCoordinateX = IN.worldPos.zy;
            float2 textureCoordinateY = IN.worldPos.xz;
            float2 textureCoordinateZ = IN.worldPos.xy;

            float2 xEmissionCoordinate = TRANSFORM_TEX(textureCoordinateX, _EmissionTex);
            float2 yEmissionCoordinate = TRANSFORM_TEX(textureCoordinateY, _EmissionTex);
            float2 zEmissionCoordinate = TRANSFORM_TEX(textureCoordinateZ, _EmissionTex);


            float2 noiseCoordinate = IN.worldPos.zy + IN.worldPos.z;
            noiseCoordinate = TRANSFORM_TEX(noiseCoordinate, _NoiseTex);

            noiseCoordinate = noiseCoordinate + _NoiseMovementDirection * _Time.y;
            xEmissionCoordinate = xEmissionCoordinate + _MainMovementDirection * _Time.y;
            yEmissionCoordinate = yEmissionCoordinate + _MainMovementDirection * _Time.y;
            zEmissionCoordinate = zEmissionCoordinate + _MainMovementDirection * _Time.y;

            float xRuneAlpha = tex2D(_EmissionTex, xEmissionCoordinate).r;
            float yRuneAlpha = tex2D(_EmissionTex, yEmissionCoordinate).r;
            float zRuneAlpha = tex2D(_EmissionTex, zEmissionCoordinate).r;
             
            float xNormalFactor = abs(dot(IN.worldNormal, float3(1,0,0)));
            float yNormalFactor = abs(dot(IN.worldNormal, float3(0,1,0)));
            float zNormalFactor = abs(dot(IN.worldNormal, float3(0,0,1)));

            float noiseAlpha = tex2D(_NoiseTex, noiseCoordinate).r;

            xRuneAlpha = noiseAlpha * xRuneAlpha;
            yRuneAlpha = noiseAlpha * yRuneAlpha;
            zRuneAlpha = noiseAlpha * zRuneAlpha;
            
            float xRuneSelect = step(0.08, noiseAlpha * step(0.01, xRuneAlpha));
            float yRuneSelect = step(0.08, noiseAlpha * step(0.01, yRuneAlpha));
            float zRuneSelect = step(0.08, noiseAlpha * step(0.01, zRuneAlpha));
             
            float xNormalSelect = 1- smoothstep(0.67, 0.8, xNormalFactor);
            float yNormalSelect = 1- smoothstep(0.67, 0.8, yNormalFactor);
            float zNormalSelect = 1- smoothstep(0.67, 0.8, zNormalFactor);
            
            float xSelect = lerp(xRuneSelect, 0, xNormalSelect);
            float ySelect = lerp(yRuneSelect, 0, yNormalSelect);
            float zSelect = lerp(zRuneSelect, 0, zNormalSelect);
             
            //o.Albedo = lerp(_RuneColor, 0, saturate(xNormalSelect * yNormalSelect * zNormalSelect));
            float runeSelect = step(0.001, xSelect)
                             + step(0.001, ySelect) 
                             + step(0.001, zSelect);

            float3 color = lerp( _Color, _RuneColor, step(0.001, runeSelect));
            o.Albedo = color;
            o.Alpha = max(_Color.a, 
                        lerp(0, xRuneAlpha, xSelect)
                      + lerp(0, yRuneAlpha, ySelect)
                      + lerp(0, zRuneAlpha, zSelect)
            );
            o.Emission = lerp(_EmissionColor, _RuneColor, step(0.001, runeSelect));
            UNITY_APPLY_FOG(i.fogCoord, color);
            //o.Alpha = alphaPixel.r;
        }
        ENDCG
    }
    FallBack Off
}
