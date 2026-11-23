Shader "FPS/MenuBlur"
{
    Properties
    {
        _Radius ("Blur radius in pixels at full strength", Range(0, 12)) = 5
        _Dim ("Brightness behind the menu at full strength", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "MenuBlur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Radius;
            float _Dim;
            float _MenuBlur;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 own = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                if (_MenuBlur <= 0.001) return own;

                float2 texel = _Radius * _MenuBlur / _ScreenParams.xy;
                float3 sum = 0.0;
                float weight = 0.0;

                for (int x = -3; x <= 3; x++)
                {
                    for (int y = -3; y <= 3; y++)
                    {
                        float w = exp(-(x * x + y * y) / 8.0);
                        sum += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord + float2(x, y) * texel).rgb * w;
                        weight += w;
                    }
                }

                float3 color = sum / weight;
                color *= lerp(1.0, _Dim, _MenuBlur);
                return float4(color, own.a);
            }
            ENDHLSL
        }
    }
}
