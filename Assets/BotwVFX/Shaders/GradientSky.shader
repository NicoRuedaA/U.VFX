// Cielo degradado con sol suave, al estilo de los cielos limpios de Hyrule.
Shader "BotwVFX/Environment/Gradient Sky"
{
    Properties
    {
        _TopColor ("Top", Color) = (0.25, 0.5, 0.9, 1)
        _HorizonColor ("Horizon", Color) = (0.78, 0.9, 0.98, 1)
        _BottomColor ("Bottom", Color) = (0.55, 0.62, 0.6, 1)
        _Exponent ("Gradient Exponent", Range(0.1, 4)) = 0.6
        _SunDirection ("Sun Direction", Vector) = (-0.4, 0.5, -0.6, 0)
        [HDR] _SunColor ("Sun Color", Color) = (1.6, 1.4, 1.1, 1)
        _SunSize ("Sun Size", Range(0.9, 1)) = 0.997
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _TopColor;
            float4 _HorizonColor;
            float4 _BottomColor;
            float _Exponent;
            float4 _SunDirection;
            float4 _SunColor;
            float _SunSize;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert(float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.dir = vertex.xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float h = d.y;
                float3 col = h >= 0
                    ? lerp(_HorizonColor.rgb, _TopColor.rgb, pow(saturate(h), _Exponent))
                    : lerp(_HorizonColor.rgb, _BottomColor.rgb, pow(saturate(-h * 4.0), 0.5));

                float sun = dot(d, normalize(_SunDirection.xyz));
                col += _SunColor.rgb * smoothstep(_SunSize, _SunSize + 0.0008, sun);
                col += _SunColor.rgb * 0.15 * pow(saturate(sun), 64.0);
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}
