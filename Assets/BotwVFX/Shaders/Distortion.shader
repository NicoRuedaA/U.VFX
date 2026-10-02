// Distorsión de pantalla (solo URP): onda expansiva, calor y "succión" del portal.
// Lee la imagen ya renderizada (_CameraOpaqueTexture, hay que activar "Opaque Texture"
// en el asset de URP) y la vuelve a pintar desplazada.
//
// Modos:
//   0 = ruido (calor / aire caliente)
//   1 = radial hacia fuera (onda expansiva)
//   2 = radial hacia dentro (portal que lo absorbe todo)
// El alpha del color de la partícula escala la fuerza (fade natural).
// Se dibuja en la cola Transparent-10, antes que el resto de partículas,
// para no "borrar" las que tenga detrás.
Shader "BotwVFX/Distortion"
{
    Properties
    {
        _MainTex ("Mask (R)", 2D) = "white" {}
        _NoiseTex ("Noise (RG = dirección)", 2D) = "gray" {}
        _NoiseScroll ("Noise Scroll (XY)", Vector) = (0, 0.3, 0, 0)
        [Enum(Noise, 0, Outward, 1, Inward, 2)] _Mode ("Mode", Float) = 0
        _Strength ("Strength (UV de pantalla)", Range(0, 0.2)) = 0.03
        _MeshFade ("Mesh Center Fade (mallas)", Range(0, 8)) = 0
        _Tint ("Tint (A = cantidad)", Color) = (1, 1, 1, 0)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent-10" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull [_Cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "BotwVFX.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float4 _NoiseScroll;
            float _Mode;
            float _Strength;
            float _MeshFade;
            float4 _Tint;
            sampler2D _CameraOpaqueTexture;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
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
                float3 normalWS : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv0.xy;
                o.custom = float4(v.uv0.zw, v.uv1.xy);
                o.screenPos = ComputeScreenPos(o.pos);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.positionWS = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)).r;

                // En mallas (p. ej. el rayo) la distorsión se concentra en el centro.
                if (_MeshFade > 0)
                {
                    float3 v = normalize(_WorldSpaceCameraPos - i.positionWS);
                    mask *= pow(saturate(abs(dot(normalize(i.normalWS), v))), _MeshFade);
                }

                float2 nuv = i.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw + _NoiseScroll.xy * _Time.y + i.custom.y * float2(0.37, 0.71);
                float2 noise = tex2D(_NoiseTex, nuv).rg - 0.5;
                float2 radial = normalize(i.uv - 0.5 + 1e-5);
                float2 dir = _Mode < 0.5 ? noise * 2.0
                           : radial * (_Mode < 1.5 ? 1.0 : -1.0) * (0.7 + noise.x);

                float strength = _Strength * mask * i.color.a;
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float3 background = tex2D(_CameraOpaqueTexture, screenUV + dir * strength).rgb;
                background += _Tint.rgb * (_Tint.a * mask * i.color.a);

                return float4(background, mask > 0.002 ? 1.0 : 0.0);
            }
            ENDCG
        }
    }
}
