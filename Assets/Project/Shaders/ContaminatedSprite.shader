Shader "Fiore/Hazards/ContaminatedSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}

        [Header(Cores da Contaminacao)]
        _ColorA ("Cor A (mais escura)",  Color) = (0.18, 0.05, 0.20, 1)
        _ColorB ("Cor B (intermediaria)",Color) = (0.42, 0.12, 0.38, 1)
        _ColorC ("Cor C (clara)",        Color) = (0.65, 0.22, 0.30, 1)
        _ColorD ("Cor D (mais viva)",    Color) = (0.85, 0.32, 0.20, 1)

        [Header(Padrao Organico)]
        _NoiseScale ("Escala do Ruido",     Range(0.1, 20.0)) = 8.0
        _WarpAmount ("Intensidade Warp",    Range(0.0, 3.0))  = 1.0
        _TimeSpeed  ("Velocidade Animacao", Range(0.0, 2.0))  = 0.15
        _Steps      ("Quantizacao Cores",   Range(2.0, 16.0)) = 5

        [Header(Mix de Cores)]
        _MixAB ("Limite Cor A para B", Range(0.0, 1.0)) = 0.30
        _MixBC ("Limite Cor B para C", Range(0.0, 1.0)) = 0.55
        _MixCD ("Limite Cor C para D", Range(0.0, 1.0)) = 0.78

        [Header(Anchoragem)]
        [Toggle] _UseWorldSpace ("Padrao em Espaco Mundial", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float2 worldPos   : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

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
            float _UseWorldSpace;

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

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

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                o.color = v.color;
                // Usar posicao local do vertice em vez de mundial, e dividir por uma escala
                // (eh estavel entre frames porque o quad do sprite tem tamanho fixo)
                o.worldPos = v.positionOS.xy;
                return o;
            }

            float4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                float4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if (spriteColor.a < 0.01) discard;

                // Sempre usa posicao local do sprite (estavel entre frames),
                // ignorando UV do atlas (que muda a cada frame da animacao)
                float2 noiseCoord = i.worldPos;
                float2 p = noiseCoord * _NoiseScale;
                float t = _Time.y * _TimeSpeed;

                float2 q = float2(
                    fbm(p + float2(0.0, 0.0) + t),
                    fbm(p + float2(5.2, 1.3) + t)
                );
                float n = fbm(p + _WarpAmount * q);

                n = floor(n * _Steps) / _Steps;

                float3 col;
                if      (n < _MixAB) col = _ColorA.rgb;
                else if (n < _MixBC) col = _ColorB.rgb;
                else if (n < _MixCD) col = _ColorC.rgb;
                else                 col = _ColorD.rgb;

                return float4(col, spriteColor.a * i.color.a);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "NormalsRendering" }
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            float4 frag(Varyings i) : SV_Target { return float4(0,0,0,0); }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}