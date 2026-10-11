Shader "Custom/GrassShaderIndirect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TopColor ("Top Color", Color) = (0.2, 0.8, 0.2, 1)
        _BottomColor ("Bottom Color", Color) = (0.1, 0.5, 0.1, 1)
        _WindStrength ("Wind Strength", Range(0, 1)) = 1
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutOut"
            "IgnoreProjector" = "True"
        }

        LOD 150
        Cull Off
        ZWrite On
        Offset 0, 1

        Pass
        {
            Name "Forward"

            Tags
            {
                "LightMode" = "UniversalForwardOnly"
            }

            HLSLPROGRAM

            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW_SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _TopColor;
                float4 _BottomColor;
                float _WindStrength;
                float _WindSpeed;
                float _Cutoff;
            CBUFFER_END

            StructureBuffer<float4> _Positions;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

           struct Varyings 
           {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float height : TEXCOORD1;
            float3 positionWS : TEXCOORD2;
            half fogFactor : TEXCOORD3;
           };

           Varyings vert(Attributes input)
           {
                Varyings output = (Varyings)0;

                float4 instanceData = _Positions[input.instanceID];
                float3 basePositionWS = instanceData.xyz;

                float s;
                float c;
                sincos(radians(instanceData.w), s, c);

                float3 localPosition = input.positionOS.xyz;

                float3 rotatedPosition;
                rotatedPosition.x = localPosition.x * c + localPosition.z * s;
                rotatedPosition.y = localPosition.y;
                rotatedPosition.z = -localPosition.x * s + localPosition.z * c;

                float wind = sin(
                    basePositionWS.x +
                    basePositionWS.z +
                    _Time.y * _WindSpeed
                ) * _WindStrength * localPosition.y;

                rotatedPosition.x += wind * 0.1;
                rotatedPosition.z += wind * 0.05;

                output.positionWS = basePositionWS + rotatedPosition;
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.height = saturate(localPosition.y);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
           }

           half4 frag(Varyings input) : SV_Target
           {
            half4 tex = SAMPLE_TEXTURE(_MainTex, sampler_MainTex, input.uv);
            clip(tex.a - _Cutoff);

            half4 gradient = lerp(_BottomColor, _TopColor, input.height);

            float4 shadowCoord;

            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                shadowCoord = ComputeScreenPos(
                    TransformWorldToHClip(input.positionWS)
                );
            #else
                shadowCoord = TransformWorldToShadowCoord(input.positionWS);
            #endif

            Light mainLight = GetMainLight(
                shadowCoord,
                input.positionWS,
                half4(1.0, 1.0, 1.0, 1.0)
            );

            half ndotl = saturate(mainLight.direction.y);

            half3 lighting = SampleSH(half3(0.0, 1.0, 0.0));
            lighting += mainLight.color
                * ndotl
                * mainLight.distanceAttenuation
                * mainLight.shadowAttenuation;
            
            half4 color = tex * gradient;
            color.rgb *= lighting;
            color.rgb = MixFog(color.rgb, input.fogFactor);

            return color;
           }

           ENDHLSL
        }
    }

    FallBack Off
}