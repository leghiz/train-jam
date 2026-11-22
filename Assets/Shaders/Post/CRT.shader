Shader "FPS/CRT"
{
    Properties
    {
        _PixelHeight ("Пикселей PSX по высоте экрана — как в PSX.mat. Полоса и цветная маска ложатся ровно на каждый пиксель PSX, поэтому кадр одинаковый на любом мониторе", Range(60, 720)) = 360
        _LineStrength ("Насколько темнеют промежутки между полосами: 0 — полос нет, 1 — чёрные", Range(0, 1)) = 0.55
        _LineSharpness ("Резкость полос: 0 — мягкая волна, 1 — чёткие края", Range(0, 1)) = 0.35
        _BrightLines ("Насколько яркие места заливают промежутки, как луч кинескопа: 0 — полосы везде одинаковые", Range(0, 1)) = 0.4

        _Mask ("Сила цветной маски кинескопа: каждый пиксель PSX делится на красную, зелёную и синюю полоски", Range(0, 1)) = 0.12

        _RollStrength ("Яркость бегущей по экрану полосы, 0 — её нет", Range(0, 0.5)) = 0.05
        _RollSpeed ("Скорость бегущей полосы: экранов в секунду, минус — вверх", Range(-2, 2)) = 0.12
        _RollWidth ("Ширина бегущей полосы: доля высоты экрана", Range(0.01, 0.5)) = 0.12

        _Brightness ("Компенсация яркости: полосы и маска затемняют кадр, этим его можно вернуть", Range(1, 2)) = 1.25
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "CRT"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _PixelHeight;
            float _LineStrength;
            float _LineSharpness;
            float _BrightLines;
            float _Mask;
            float _RollStrength;
            float _RollSpeed;
            float _RollWidth;
            float _Brightness;
            float _PostEffectsOff;

            static const int LineSamples = 6;
            static const float TextAlpha = 0.7;

            float Overlap(float from, float to, float low, float high)
            {
                return max(0.0, min(to, high) - max(from, low));
            }

            float3 Triad(float from, float width)
            {
                float to = from + width;
                float3 share = 0.0;
                for (int k = 0; k < 2; k++)
                {
                    share.r += Overlap(from, to, k, k + 1.0 / 3.0);
                    share.g += Overlap(from, to, k + 1.0 / 3.0, k + 2.0 / 3.0);
                    share.b += Overlap(from, to, k + 2.0 / 3.0, k + 1.0);
                }
                share /= width;
                return share.r * float3(1.0, 0.4, 0.4) + share.g * float3(0.4, 1.0, 0.4) + share.b * float3(0.4, 0.4, 1.0);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord);
                if (_PostEffectsOff > 0.5 || color.a < TextAlpha) return color;

                float luma = saturate(dot(LinearToSRGB(saturate(color.rgb)), float3(0.299, 0.587, 0.114)));
                float width = min(1.0, _PixelHeight / _ScreenParams.y);
                float2 grid = float2(_ScreenParams.x / _ScreenParams.y, 1.0) * _PixelHeight;
                float2 inside = frac((input.texcoord - 0.5) * grid - width * 0.5);

                float start = lerp(0.0, 0.85, _LineSharpness);
                float gap = 0.0;
                for (int i = 0; i < LineSamples; i++)
                {
                    float edge = abs(frac(inside.y + width * (i + 0.5) / LineSamples) - 0.5) * 2.0;
                    gap += smoothstep(start, 1.0, edge);
                }
                gap /= LineSamples;
                float scan = 1.0 - _LineStrength * gap * (1.0 - luma * _BrightLines);

                float3 mask = lerp(float3(1.0, 1.0, 1.0), Triad(inside.x, width), _Mask);

                float away = abs(frac(input.texcoord.y - _Time.y * _RollSpeed) - 0.5);
                float roll = 1.0 + _RollStrength * (1.0 - smoothstep(0.0, _RollWidth, away));

                return float4(color.rgb * scan * mask * roll * _Brightness, color.a);
            }
            ENDHLSL
        }
    }
}
