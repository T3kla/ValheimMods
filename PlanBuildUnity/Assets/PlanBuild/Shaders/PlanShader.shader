Shader "MarcoPogo/PlanShader"
{
	Properties
    {
        [HDR] _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {} 

        _NoiseTex ("Noise", 2D) = "white" {}
        _MainMovementDirection ("Movement Direction Main", float) = (0, -1, 0, 1)
        _NoiseMovementDirection ("Movement Direction Noise", float) = (0, 1, 0, 1)
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags{ "RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha SrcAlpha
        LOD 100
        Cull [_Cull]
        Lighting Off
        ZWrite On
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex; 
        sampler2D _NoiseTex; 

        half2 _MainMovementDirection;
		half2 _NoiseMovementDirection;

        struct Input
        {
            float2 uv_MainTex; 
            float2 uv_NoiseTex;
        };

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        fixed4 pixel, alphaPixel;
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            IN.uv_NoiseTex += _NoiseMovementDirection * _Time.y / 2.0;
            IN.uv_MainTex += _MainMovementDirection * _Time.y; 

            alphaPixel = tex2D (_NoiseTex, IN.uv_NoiseTex);
            
            pixel = tex2D (_MainTex, IN.uv_MainTex) * _Color * alphaPixel.r;
            o.Albedo = pixel.rgb;
			o.Emission = pixel.rgb;
			
            o.Alpha = alphaPixel.r;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
