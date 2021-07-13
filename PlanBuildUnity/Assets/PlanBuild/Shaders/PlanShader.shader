Shader "MarcoPogo/PlanShader"
{
	Properties
    {
        [HDR] _Color ("Color", Color) = (0,0,0,0)
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _MainTex ("Albedo (RGB)", 2D) = "black" {} 
        _EmissionTex("Emission (RGB)", 2D) = "black" {}

        _NoiseTex ("Noise", 2D) = "white" {}
        _MainMovementDirection ("Movement Direction Main", float) = (0, -1, 0, 1)
        _NoiseMovementDirection ("Movement Direction Noise", float) = (0, 1, 0, 1)
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags{ "RenderType" = "Fade" "IgnoreProjector" = "True" "Queue" = "Transparent" "IsEmissive" = "true" }
        Cull Off
        Blend SrcAlpha SrcAlpha
        LOD 100
        Cull [_Cull]
        Lighting Off
        ZWrite Off


        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex; 
        sampler2D _EmissionTex;
        sampler2D _NoiseTex; 

        half2 _MainMovementDirection;
		half2 _NoiseMovementDirection;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_EmissionTex;
            float2 uv_NoiseTex;
            float3 worldPos;
        };

        fixed4 _Color;
        fixed4 _EmissionColor;
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        fixed4 pixel, alphaPixel;
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            
            IN.uv_NoiseTex += _NoiseMovementDirection * _Time.y;
            IN.uv_MainTex += _MainMovementDirection * _Time.y; 
            IN.uv_EmissionTex += _MainMovementDirection * _Time.y;

            alphaPixel = tex2D (_NoiseTex, IN.uv_NoiseTex);
            
            pixel = tex2D (_EmissionTex, IN.uv_EmissionTex) * _EmissionColor * alphaPixel.r;
            clip(alphaPixel.r - 0.5);
            o.Albedo = pixel.rgb;
            o.Alpha = alphaPixel.r;
			o.Emission = pixel.rgb;
            o.Occlusion = 0;
            o.Smoothness = 1;
            o.Metallic = 0;
            //o.Alpha = alphaPixel.r;
        }
        ENDCG
    }
    FallBack Off
}
