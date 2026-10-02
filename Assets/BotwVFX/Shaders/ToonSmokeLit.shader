// Humo/fuego toon V2 (solo URP).
// Mejoras respecto a "Toon Smoke":
//  - Atlas 2x2 con 4 formas distintas (Texture Sheet Animation con frame aleatorio).
//  - Iluminado por el sol REAL de la escena (normales en el atlas + _MainLightPosition de URP),
//    con sombra fría en lugar de gris.
//  - Fuego con rampa de color (blanco -> amarillo -> naranja -> rojo -> marrón).
//  - Se erosiona al tocar el suelo (Depth Texture) en vez de cortarse en línea recta.
//
// Atlas: RG = normal (0..1), B = altura, A = máscara.
// Custom1: x = erosión del fuego, y = corte de sombra, z = erosión del alpha, w = semilla de ruido.
Shader "BotwVFX/Toon Smoke Lit"
{
    Properties
    {
        _SmokeAtlas ("Atlas 2x2 (RG normal, B height, A mask)", 2D) = "white" {}
        _Tiles ("Atlas Tiles", Float) = 2
        _NoiseTex ("Noise (R fbm, G cells)", 2D) = "gray" {}
        _DetailTiling ("Detail Tiling", Float) = 1.3

        [Header(Fire)]
        _FireRamp ("Fire Ramp (x: 0 = borde, 1 = núcleo)", 2D) = "white" {}
        _FireIntensity ("Fire Intensity (HDR)", Float) = 1.6
        _FireRange ("Fire Ramp Range", Range(0.05, 1)) = 0.35
        _FireErosion ("Fire Erosion", Range(0, 1)) = 0

        [Header(Smoke)]
        _SmokeLight ("Smoke Light", Color) = (0.62, 0.58, 0.55, 1)
        _SmokeShadow ("Smoke Shadow", Color) = (0.27, 0.27, 0.36, 1)
        _ShadeCutout ("Shade Cutout", Range(0, 1)) = 0.5
        _NoiseAmount ("Shade Noise", Range(0, 1)) = 0.3
        _DefaultLightDir ("Light Dir (si no hay sol)", Vector) = (0.4, 0.8, -0.3, 0)

        [Header(Alpha)]
        _AlphaErosion ("Alpha Erosion", Range(0, 1)) = 0
        _ErosionStrength ("Erosion Pattern Strength", Range(0, 1)) = 0.6
        _VertexAlphaErosion ("Vertex Alpha -> Erosion", Range(0, 1)) = 0
        _Softness ("Edge Softness", Range(0, 0.5)) = 0
        _DepthFade ("Ground Erosion Distance", Float) = 0.7
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "BotwVFX.cginc"

            sampler2D _SmokeAtlas;
            float _Tiles;
            sampler2D _NoiseTex;
            float _DetailTiling;
            sampler2D _FireRamp;
            float _FireIntensity;
            float _FireRange;
            float _FireErosion;
            float4 _SmokeLight;
            float4 _SmokeShadow;
            float _ShadeCutout;
            float _NoiseAmount;
            float4 _DefaultLightDir;
            float _AlphaErosion;
            float _ErosionStrength;
            float _VertexAlphaErosion;
            float _Softness;
            float _DepthFade;
            float4 _MainLightPosition; // global de URP: dirección hacia el sol
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 custom : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv0.xy;
                o.custom = float4(v.uv0.zw, v.uv1.xy);
                o.screenPos = ComputeScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.screenPos.z);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 atlas = tex2D(_SmokeAtlas, i.uv);

                // UV local de la celda del atlas, para rotar el ruido alrededor de su centro.
                float2 tile = floor(i.uv * _Tiles - 1e-4);
                float2 local = i.uv * _Tiles - tile;
                float2 ruv = BotwRotateUV(local, i.custom.w * BOTW_TAU);
                float4 noise = tex2D(_NoiseTex, ruv * _DetailTiling + i.custom.w * float2(0.37, 0.71));

                // --- Luz del sol real. El billboard está alineado con la vista,
                // así que la normal del atlas está en espacio de vista.
                float3 n;
                n.xy = atlas.rg * 2.0 - 1.0;
                n.z = sqrt(saturate(1.0 - dot(n.xy, n.xy)));
                float3 lightWS = dot(_MainLightPosition.xyz, _MainLightPosition.xyz) > 0.01 ? _MainLightPosition.xyz : _DefaultLightDir.xyz;
                float3 lightVS = normalize(mul((float3x3)UNITY_MATRIX_V, lightWS));
                float shade = dot(n, lightVS) * 0.5 + 0.5 + (noise.r - 0.5) * _NoiseAmount;
                float lit = BotwToonStep(saturate(_ShadeCutout + i.custom.y), _Softness, shade);
                float3 smoke = lerp(_SmokeShadow.rgb, _SmokeLight.rgb, lit);

                // --- Fuego con rampa: cuanto más lejos del borde, más caliente.
                float heat = saturate(atlas.b * 0.6 + noise.r * 0.4);
                float fireEdge = lerp(0.0, 1.05, saturate(_FireErosion + i.custom.x));
                float fire = BotwToonStep(fireEdge, _Softness, heat);
                float rampT = saturate((heat - fireEdge) / _FireRange);
                float3 fireCol = tex2Dlod(_FireRamp, float4(rampT, 0.5, 0, 0)).rgb * _FireIntensity;

                float3 col = lerp(smoke, fireCol, fire) * i.color.rgb;

                // --- Alpha: patrón de celdas + erosión al acercarse al suelo.
                float value = atlas.a * lerp(1.0, noise.g, _ErosionStrength);
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                value *= saturate((sceneDepth - i.screenPos.z) / max(_DepthFade, 1e-3));

                float erosion = saturate(_AlphaErosion + i.custom.z + (1.0 - i.color.a) * _VertexAlphaErosion);
                float alpha = BotwToonStep(BotwErosionEdge(erosion), _Softness, value);
                alpha *= saturate((1.0 - erosion) * 50.0);
                alpha *= lerp(i.color.a, 1.0, _VertexAlphaErosion);

                return float4(col, alpha);
            }
            ENDCG
        }
    }
}
