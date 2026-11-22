Shader "FPS/HeroFogPost"
{
    Properties
    {
        _NearSkip ("Near Skip", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "HeroFog"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Assets/Shaders/World/HeroFog.hlsl"

            float _NearSkip;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, input.texcoord);

                #if UNITY_REVERSED_Z
                float depth = SampleSceneDepth(input.texcoord);
                #else
                float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(input.texcoord));
                #endif

                if (LinearEyeDepth(depth, _ZBufferParams) < _NearSkip)
                    return color;

                float3 positionWS = ComputeWorldSpacePosition(input.texcoord, depth, UNITY_MATRIX_I_VP);
                color.rgb = HeroFog(color.rgb, positionWS);
                return color;
            }
            ENDHLSL
        }
    }
}
