Shader "Fiore/Hazards/ToxicResin"
{
    Properties
    {
        [Header(Cores)]
        _ColorA ("Cor A (mais escura)",  Color) = (0.18, 0.05, 0.20, 1)
        _ColorB ("Cor B (intermediaria)",Color) = (0.42, 0.12, 0.38, 1)
        _ColorC ("Cor C (clara)",        Color) = (0.65, 0.22, 0.30, 1)
        _ColorD ("Cor D (mais viva)",    Color) = (0.85, 0.32, 0.20, 1)

        [Header(Padrao Organico)]
        _NoiseScale ("Escala do Ruido",     Range(0.1, 10.0)) = 2.5
        _WarpAmount ("Intensidade Warp",    Range(0.0, 3.0))  = 1.0
        _TimeSpeed  ("Velocidade Animacao", Range(0.0, 2.0))  = 0.15
        _Steps      ("Quantizacao Cores",   Range(2.0, 16.0)) = 5

        [Header(Mix de Cores)]
        _MixAB ("Limite Cor A para B", Range(0.0, 1.0)) = 0.30
        _MixBC ("Limite Cor B para C", Range(0.0, 1.0)) = 0.55
        _MixCD ("Limite Cor C para D", Range(0.0, 1.0)) = 0.78
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector"= "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float2 positionOS  : TEXCOORD1;
            };

            float4 _ColorA;
            float4 _ColorB;
            float4 _ColorC;
            float4 _ColorD;
            float _NoiseScale;
            float _WarpAmount;
            float _TimeSpeed;
            float _Steps;
            float _MixAB;
            float _MixBC;
            float _MixCD;

            // Hash procedural (sem textura)
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Ruido com interpolacao bilinear
            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash(i + float2(0.0, 0.0));
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // FBM de 3 oitavas
            float fbm(float2 p)
            {
                float f = 0.0;
                float amp = 0.5;
                float2x2 mtx = float2x2(0.80, 0.60, -0.60, 0.80);

                f += amp * valueNoise(p);     p = mul(mtx, p) * 2.02; amp *= 0.5;
                f += amp * valueNoise(p);     p = mul(mtx, p) * 2.03; amp *= 0.5;
                f += amp * valueNoise(p);
                return f / 0.875;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.positionOS = IN.positionOS.xy;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Padrao organico com domain warping (1 camada)
                float2 p = IN.positionOS * _NoiseScale;
                float t = _Time.y * _TimeSpeed;

                float2 q = float2(
                    fbm(p + float2(0.0, 0.0) + t),
                    fbm(p + float2(5.2, 1.3) + t)
                );

                float n = fbm(p + _WarpAmount * q);

                // Quantizacao - hard step
                n = floor(n * _Steps) / _Steps;

                // Mix de 4 cores baseado no ruido quantizado
                float3 col;
                if      (n < _MixAB) col = _ColorA.rgb;
                else if (n < _MixBC) col = _ColorB.rgb;
                else if (n < _MixCD) col = _ColorC.rgb;
                else                 col = _ColorD.rgb;

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}