Shader "UI/WhiteChromaKey"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ColorKey ("Color to Remove", Color) = (1,1,1,1) // Por defeito, a cor a remover é o Branco
        _Tolerance ("Tolerance", Range(0.0, 1.0)) = 0.1 // O quão rigoroso ele é a apagar a cor
        _Smoothing ("Smoothing", Range(0.0, 1.0)) = 0.05 // Suaviza as bordas para não ficar "pixelizado"
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // Configurações para permitir transparência na Interface (UI)
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _ColorKey;
            float _Tolerance;
            float _Smoothing;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Pega a cor do píxel atual do vídeo
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Calcula a distância matemática entre a cor do píxel e a cor que queremos apagar (branco)
                float colorDistance = distance(c.rgb, _ColorKey.rgb);
                
                // Aplica a transparência baseada na tolerância e suavização
                float alpha = smoothstep(_Tolerance, _Tolerance + _Smoothing, colorDistance);
                
                // Multiplica a opacidade original pela opacidade do Chroma Key
                c.a *= alpha;
                
                return c;
            }
            ENDCG
        }
    }
}