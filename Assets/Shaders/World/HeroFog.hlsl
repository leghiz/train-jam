#ifndef FPS_HERO_FOG_INCLUDED
#define FPS_HERO_FOG_INCLUDED

float4 _HeroFogCenter;
half4 _HeroFogColor;
float4 _HeroFogRange;
float4 _HeroFogAim;
float _HeroFogOff;

half3 HeroFog(half3 color, float3 positionWS)
{
    if (_HeroFogRange.z <= 0.0 || _HeroFogOff > 0.5) return color;
    float2 offset = positionWS.xz - _HeroFogCenter.xz;
    float2 forward = _HeroFogAim.xy;
    float along = dot(offset, forward);
    float2 side = offset - along * forward;
    along /= along > 0.0 ? max(_HeroFogAim.z, 1.0) : 1.0;
    float distance = length(side + along * forward);
    float amount = smoothstep(_HeroFogRange.x, _HeroFogRange.y, distance) * _HeroFogRange.z;
    return lerp(color, _HeroFogColor.rgb, amount);
}

#endif
