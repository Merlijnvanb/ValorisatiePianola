Shader "Custom/Skybox"
{
    Properties
    {
        _SkyColor ("Sky Color", Color) = (0.25, 0.5, 0.85, 1)
        _HorizonColor ("Horizon Color", Color) = (0.75, 0.85, 0.9, 1)
        _GroundColor ("Ground Color", Color) = (0.3, 0.28, 0.25, 1)
        _HorizonHeight ("Horizon Height", Range(-1, 1)) = 0
        _SkyExponent ("Sky Blend Exponent", Range(0.1, 10)) = 1.5
        _GroundExponent ("Ground Blend Exponent", Range(0.1, 10)) = 1.5

        _SunDirection ("Sun Direction", Vector) = (0, 1, 0, 0)
        _SunColor ("Sun Color", Color) = (1, 1, 0.95, 1)
        _SunSize ("Sun Size", Range(0.001, 0.5)) = 0.04
        _SunSharpness ("Sun Sharpness", Range(1, 64)) = 16
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "RenderPipeline" = "UniversalPipeline" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _SkyColor;
                half4 _HorizonColor;
                half4 _GroundColor;
                float _HorizonHeight;
                float _SkyExponent;
                float _GroundExponent;

                float4 _SunDirection;
                half4 _SunColor;
                float _SunSize;
                float _SunSharpness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.viewDir = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = normalize(IN.viewDir);
                float height = dir.y - _HorizonHeight;

                half3 sky = lerp(_HorizonColor.rgb, _SkyColor.rgb, pow(saturate(height), _SkyExponent));
                half3 ground = lerp(_HorizonColor.rgb, _GroundColor.rgb, pow(saturate(-height), _GroundExponent));
                half3 color = height >= 0 ? sky : ground;

                float3 sunDir = normalize(_SunDirection.xyz);
                float sunDot = saturate(dot(dir, sunDir));
                float sunDisc = pow(sunDot, _SunSharpness / _SunSize);
                color += _SunColor.rgb * sunDisc;

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
