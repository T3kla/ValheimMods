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
        #pragma surface surf Lambert alpha noshadow 

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
            float4 screenPos;
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
            float2 textureCoordinate = IN.worldPos.xz;
            float2 emissionCoordinate = TRANSFORM_TEX(textureCoordinate, _EmissionTex);
            float2 noiseCoordinate = IN.worldPos.xy + IN.worldPos.z;
            noiseCoordinate = TRANSFORM_TEX(noiseCoordinate, _NoiseTex);

            noiseCoordinate = noiseCoordinate + _NoiseMovementDirection * _Time.y;
            emissionCoordinate = emissionCoordinate + _MainMovementDirection * _Time.y;
              
            float runeAlpha = tex2D(_EmissionTex, emissionCoordinate).r;
            float normalFactor = abs(dot(IN.worldNormal, _IgnoreNormal));
            
            float runeSelect = step(tex2D(_NoiseTex, noiseCoordinate).r * runeAlpha, 0.1);
            float normalSelect = step(normalFactor, 0.6);
            
            runeSelect = step(1, runeSelect + normalSelect);

            o.Albedo = lerp(_RuneColor, _Color, runeSelect); 
            o.Alpha = lerp(runeAlpha, _Color.a, runeSelect);
            o.Emission = lerp(o.Albedo, _EmissionColor, runeSelect) * o.Alpha;
           
            //o.Alpha = alphaPixel.r;
        }
        ENDCG
    }
    FallBack Off
}
