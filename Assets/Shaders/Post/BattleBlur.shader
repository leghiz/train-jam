Shader "FPS/BattleBlur"
{
    Properties
    {
        _Zoom ("Насколько далеко размазывается кадр к центру на пике, доля расстояния до центра", Range(0, 1)) = 0.4
        _Twist ("Насколько закручивается кадр вокруг центра на пике, радианы. Сильнее всего у центра, к краям слабеет", Range(0, 3)) = 0.7
        _Samples ("Сколько выборок на пиксель: больше — глаже размытие, дороже", Range(4, 24)) = 14
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "BattleBlur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Zoom;
            float _Twist;
            float _Samples;
            float _BattleBlur;
            float _BattleFlash;
            float4 _BattleCenter;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 own = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                if (_BattleBlur <= 0.001 && _BattleFlash <= 0.001) return own;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 center = _BattleCenter.xy;
                float2 offset = input.texcoord - center;
                float distance = length(offset * float2(aspect, 1.0));
                float angle = _Twist * _BattleBlur * saturate(1.0 - distance);
                float s = sin(angle);
                float c = cos(angle);
                float2 turned = float2(offset.x * aspect * c - offset.y * s, offset.x * aspect * s + offset.y * c);
                turned.x /= aspect;

                int count = max(2, (int)_Samples);
                float3 sum = 0.0;
                for (int i = 0; i < count; i++)
                {
                    float step = 1.0 - _Zoom * _BattleBlur * (i / (float)(count - 1));
                    sum += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, center + turned * step).rgb;
                }
                float3 color = sum / count;
                color = lerp(color, 1.0, saturate(_BattleFlash));
                return float4(color, own.a);
            }
            ENDHLSL
        }
    }
}
