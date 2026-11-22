Shader "FPS/PSX"
{
    Properties
    {
        _PixelHeight ("Пикселей PSX по высоте экрана, 0 — без пикселизации. Интерфейс нарисован под 360: при другом числе буквы рассыпаются. CRT.mat — то же число", Range(0, 720)) = 360
        _ColorBits ("Бит на канал цвета: у PlayStation 5", Range(2, 8)) = 5
        _Dither ("Сила дизеринга", Range(0, 1)) = 1
        _DitherSize ("Размер точки дизеринга, пикселей PSX. 1 — как на приставке, больше — узор крупнее", Range(1, 4)) = 1
        _Wobble ("Дрожание кадра, пикселей PSX", Range(0, 4)) = 0.5
        _WobbleSpeed ("Скорость дрожания", Range(0, 5)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "PSX"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _PixelHeight;
            float _ColorBits;
            float _Dither;
            float _DitherSize;
            float _Wobble;
            float _WobbleSpeed;
            float _PostEffectsOff;

            static const float TextAlpha = 0.7;

            static const float Pattern[16] = { 0, 4, 1, 5, 6, 2, 7, 3, 1, 5, 0, 4, 7, 3, 6, 2 };

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 own = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord);
                if (_PostEffectsOff > 0.5 || own.a < TextAlpha) return own;

                float time = _Time.y * _WobbleSpeed;
                float wave = sin(input.texcoord.y * 23.0 + time * 2.1) + 0.6 * sin(input.texcoord.y * 61.0 - time * 3.7) + 0.4 * sin(input.texcoord.x * 17.0 + time * 1.3);
                float cell = _PixelHeight > 0.0 ? max(1.0, _ScreenParams.y / _PixelHeight) : 1.0;
                float2 texcoord = input.texcoord + float2(wave, wave * 0.35) * (_Wobble * cell / _ScreenParams.xy);

                float2 grid = _ScreenParams.xy / cell;
                float2 center = floor((texcoord - 0.5) * grid) + 0.5;
                float2 texel = floor(_ScreenParams.xy * 0.5 + center * cell + sign(center) * 0.01);
                float2 uv = (texel + 0.5) / _ScreenParams.xy;
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);
                if (color.a < TextAlpha) return own;

                float2 spot = floor(floor(center) / max(1.0, round(_DitherSize)));
                uint2 cellIndex = (uint2)(spot + 4096.0) & 3u;
                float threshold = (Pattern[cellIndex.y * 4 + cellIndex.x] + 0.5) / 8.0 - 0.5;
                float levels = exp2(round(_ColorBits)) - 1.0;
                float3 gamma = LinearToSRGB(saturate(color.rgb));
                gamma = floor(gamma * levels + 0.5 + threshold * _Dither) / levels;
                return float4(SRGBToLinear(saturate(gamma)), color.a);
            }
            ENDHLSL
        }
    }
}
