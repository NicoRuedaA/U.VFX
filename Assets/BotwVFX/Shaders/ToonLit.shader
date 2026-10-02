// Shader toon sencillo para el escenario de la demo (solo URP).
// Luz principal en dos tonos + sombras duras + luces puntuales (los flashes de las explosiones).
Shader "BotwVFX/Environment/Toon Lit"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _BaseMap ("Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Tint", Color) = (0.55, 0.62, 0.8, 1)
        _RampThreshold ("Ramp Threshold", Range(-1, 1)) = 0.05
        _RampSoftness ("Ramp Softness", Range(0.001, 0.5)) = 0.02
        _AmbientAmount ("Ambient", Range(0, 1)) = 0.35
        _RimColor ("Rim Color (A = strength)", Color) = (1, 1, 0.9, 0.12)

        [Header(Dissolve)]
        [Toggle(_DISSOLVE_ON)] _UseDissolve ("Dissolve", Float) = 0
        _Dissolve ("Dissolve Amount", Range(0, 1)) = 0
        [HDR] _DissolveEdgeColor ("Dissolve Edge", Color) = (1, 3, 5, 1)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 0.3)) = 0.08
        _DissolveScale ("Dissolve Noise Scale", Float) = 3
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _BaseMap_ST;
        float4 _ShadowColor;
        float _RampThreshold;
        float _RampSoftness;
        float _AmbientAmount;
        float4 _RimColor;
        float _Dissolve;
        float4 _DissolveEdgeColor;
        float _DissolveEdgeWidth;
        float _DissolveScale;
    CBUFFER_END

    float ToonHash(float3 p)
    {
        p = frac(p * 0.3183099 + 0.1);
        p *= 17.0;
        return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
    }

    float ToonNoise(float3 x)
    {
        float3 i = floor(x);
        float3 f = frac(x);
        f = f * f * (3.0 - 2.0 * f);
        return lerp(lerp(lerp(ToonHash(i), ToonHash(i + float3(1, 0, 0)), f.x),
                         lerp(ToonHash(i + float3(0, 1, 0)), ToonHash(i + float3(1, 1, 0)), f.x), f.y),
                    lerp(lerp(ToonHash(i + float3(0, 0, 1)), ToonHash(i + float3(1, 0, 1)), f.x),
                         lerp(ToonHash(i + float3(0, 1, 1)), ToonHash(i + float3(1, 1, 1)), f.x), f.y), f.z);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #pragma shader_feature_local _DISSOLVE_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                float3 positionOS : TEXCOORD4;
                float4 color : COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionOS = v.positionOS.xyz;
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 v = normalize(GetWorldSpaceViewDir(i.positionWS));
                float3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb * i.color.rgb;

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float ndl = dot(n, mainLight.direction);
                float ramp = smoothstep(_RampThreshold - _RampSoftness, _RampThreshold + _RampSoftness, ndl);
                float shadow = smoothstep(0.35, 0.65, mainLight.shadowAttenuation);
                float lit = ramp * shadow;

                float3 ambient = SampleSH(n) * _AmbientAmount;
                float3 color = albedo * lerp(_ShadowColor.rgb, mainLight.color, lit) + albedo * ambient;

                // Luces puntuales en bandas (flashes de explosión).
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                #if defined(_ADDITIONAL_LIGHTS)
                uint lightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(lightCount)
                    Light light = GetAdditionalLight(lightIndex, i.positionWS);
                    float atten = light.distanceAttenuation * saturate(dot(n, light.direction));
                    color += albedo * light.color * smoothstep(0.05, 0.12, atten) * min(atten * 2.0, 0.6);
                LIGHT_LOOP_END
                #endif

                float rim = pow(1.0 - saturate(dot(n, v)), 4.0) * lit;
                color += _RimColor.rgb * step(0.5, rim) * _RimColor.a;

                #if defined(_DISSOLVE_ON)
                    // Disolución con borde brillante (enemigo absorbido por la flecha ancestral).
                    float dn = ToonNoise(i.positionOS * _DissolveScale) * 0.7 + ToonNoise(i.positionOS * _DissolveScale * 2.3 + 7.1) * 0.3;
                    float cut = _Dissolve * 1.05;
                    clip(dn - cut);
                    color = lerp(color, _DissolveEdgeColor.rgb, step(dn, cut + _DissolveEdgeWidth) * step(0.001, _Dissolve));
                #endif

                color = MixFog(color, i.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };

            float4 vert(Attributes v) : SV_POSITION
            {
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDir = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDir = _LightDirection;
                #endif
                return ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDir)));
            }

            half4 frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 vert(float4 positionOS : POSITION) : SV_POSITION { return TransformObjectToHClip(positionOS.xyz); }
            half4 frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target { return half4(NormalizeNormalPerPixel(i.normalWS), 0.0); }
            ENDHLSL
        }
    }
}
