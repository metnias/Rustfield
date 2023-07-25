void GetCrossSampleUVs_float(float4 UV, float2 TexelSize, float OffsetMultiplier,
    out float2 UVOriginal, out float2 UVTopRight, out float2 UVBottomLeft, out float2 UVTopLeft, out float2 UVBottomRight)
{
    UVOriginal = UV;
    UVTopRight = UV.xy + float2(TexelSize.x, TexelSize.y) * OffsetMultiplier;
    UVBottomLeft = UV.xy - float2(TexelSize.x, TexelSize.y) * OffsetMultiplier;
    UVTopLeft = UV.xy + float2(-TexelSize.x, TexelSize.y) * OffsetMultiplier;
    UVBottomRight = UV.xy + float2(TexelSize.x, -TexelSize.y) * OffsetMultiplier;
}

void GetCrossSampleUVs_half(half4 UV, half2 TexelSize, half OffsetMultiplier,
    out half2 UVOriginal, out half2 UVTopRight, out half2 UVBottomLeft, out half2 UVTopLeft, out half2 UVBottomRight)
{
    UVOriginal = UV;
    UVTopRight = UV.xy + half2(TexelSize.x, TexelSize.y) * OffsetMultiplier;
    UVBottomLeft = UV.xy - half2(TexelSize.x, TexelSize.y) * OffsetMultiplier;
    UVTopLeft = UV.xy + half2(-TexelSize.x, TexelSize.y) * OffsetMultiplier;
    UVBottomRight = UV.xy + half2(TexelSize.x, -TexelSize.y) * OffsetMultiplier;
}