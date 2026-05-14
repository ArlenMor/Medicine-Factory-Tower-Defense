// Shader для туториального оверлея:
// Затемняет весь экран, оставляя "дыры" (hole) в указанных прямоугольниках (до 4 штук).
// Дыры задаются как нормализованные (0..1) экранные координаты: (x, y, width, height).
// Если hole.z <= 0, дыра считается неактивной.
Shader "Tutorial/SpotlightOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OverlayColor ("Overlay Color", Color) = (0, 0, 0, 0.75)
        _Hole0 ("Hole 0 (x,y,w,h)", Vector) = (-1, -1, 0, 0)
        _Hole1 ("Hole 1 (x,y,w,h)", Vector) = (-1, -1, 0, 0)
        _Hole2 ("Hole 2 (x,y,w,h)", Vector) = (-1, -1, 0, 0)
        _Hole3 ("Hole 3 (x,y,w,h)", Vector) = (-1, -1, 0, 0)
        _EdgeSoftness ("Edge Softness", Float) = 0.005
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Overlay"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _OverlayColor;
                float4 _Hole0;
                float4 _Hole1;
                float4 _Hole2;
                float4 _Hole3;
                float  _EdgeSoftness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color      = IN.color;
                return OUT;
            }

            // Возвращает 1.0, если точка НЕ попадает в дыру; 0.0 — если попадает.
            // rect = (minX, minY, width, height) в нормализованных UV-координатах.
            float HoleAlphaMask(float2 uv, float4 rect, float softness)
            {
                if (rect.z <= 0.0) return 1.0; // дыра неактивна

                float2 inside = smoothstep(rect.xy - softness, rect.xy + softness, uv)
                              * (1.0 - smoothstep(rect.xy + rect.zw - softness,
                                                   rect.xy + rect.zw + softness, uv));
                // inside.x * inside.y == 1 внутри дыры, 0 снаружи
                return 1.0 - (inside.x * inside.y);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // UV полноэкранного Image совпадают с нормализованными экранными координатами
                float2 uv = IN.uv;

                float alpha = _OverlayColor.a;
                alpha *= HoleAlphaMask(uv, _Hole0, _EdgeSoftness);
                alpha *= HoleAlphaMask(uv, _Hole1, _EdgeSoftness);
                alpha *= HoleAlphaMask(uv, _Hole2, _EdgeSoftness);
                alpha *= HoleAlphaMask(uv, _Hole3, _EdgeSoftness);

                half4 col = half4(_OverlayColor.rgb, alpha) * IN.color;
                return col;
            }
            ENDHLSL
        }
    }
}
