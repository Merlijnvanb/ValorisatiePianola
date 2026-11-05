#ifndef PIANOLA_CUTAWAY_INCLUDED
#define PIANOLA_CUTAWAY_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Random.hlsl"

// Obstruction cutaway: removes the part of a surface that lies inside a cylinder running from the camera
// to what it looks at, so the shell of the pianola opens up and the internals (normal materials) show.
// The cut is in world space, so the shadow and depth passes cut the same hole: light falls in through it
// and post effects (SSAO, fog) see the opening too.
//
// Set from C# (ObstructionController) as global shader properties:
float3 _CutawayFrom;     // usually the camera position
float3 _CutawayTo;       // the focus point; the shell behind it stays
float  _CutawayRadius;   // 0 = no cut
float  _CutawaySoftness; // width of the dithered edge, in world units

// 1 where the surface stays, 0 where it is cut away.
float CutawayMask(float3 positionWS, float2 pixelCoord)
{
    if (_CutawayRadius <= 0.0001)
        return 1.0;

    float3 axis = _CutawayTo - _CutawayFrom;
    float t = dot(positionWS - _CutawayFrom, axis) / max(dot(axis, axis), 1e-5);
    if (t < 0.0 || t > 1.0)
        return 1.0;

    float distanceToAxis = length(positionWS - (_CutawayFrom + axis * t));

    // Dithered edge instead of a hard circle: per pixel the threshold moves inward by up to the softness.
    float noise = InterleavedGradientNoise(pixelCoord, 0);
    return distanceToAxis >= _CutawayRadius - _CutawaySoftness * noise ? 1.0 : 0.0;
}

void CutawayClip(float3 positionWS, float2 pixelCoord)
{
    clip(CutawayMask(positionWS, pixelCoord) - 0.5);
}

// Shader Graph hook: Custom Function node (File mode, this file, name "CutawayAlpha").
// Feed Position (World) and Screen Position (Pixel).xy, connect Alpha to the master Alpha,
// turn on Alpha Clipping with threshold 0.5.
void CutawayAlpha_float(float3 PositionWS, float2 PixelCoord, out float Alpha)
{
    Alpha = CutawayMask(PositionWS, PixelCoord);
}

#endif
