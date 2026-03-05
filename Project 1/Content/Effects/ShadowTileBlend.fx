#if OPENGL
#define PS_SHADERMODEL ps_3_0
#else
#define PS_SHADERMODEL ps_4_0
#endif

sampler2D TextureSampler : register(s0);

float2 shadowTextureSize;
float tilePixelSize;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinates;
    float2 texelPos = uv * shadowTextureSize;
    float2 cell = floor(texelPos);
    float2 inCell = frac(texelPos);

    float minDistance = inCell.x;
    float2 direction = float2(-1, 0);

    if (1.0f - inCell.x < minDistance)
    {
        minDistance = 1.0f - inCell.x;
        direction = float2(1, 0);
    }
    if (inCell.y < minDistance)
    {
        minDistance = inCell.y;
        direction = float2(0, -1);
    }
    if (1.0f - inCell.y < minDistance)
    {
        minDistance = 1.0f - inCell.y;
        direction = float2(0, 1);
    }

    // Distance-to-border normalized over half a tile (0 at border, 1 at centre).
    float distanceNorm = saturate(minDistance / 0.5f);
    float centreWeight = 1.0f / max(tilePixelSize, 1.0f);
    float neighbourWeight = lerp(0.5f, centreWeight, distanceNorm);

    float2 centerUv = (cell + 0.5f) / shadowTextureSize;
    float2 neighbourCell = clamp(cell + direction, float2(0, 0), shadowTextureSize - 1.0f);
    float2 neighbourUv = (neighbourCell + 0.5f) / shadowTextureSize;

    float4 center = tex2D(TextureSampler, centerUv) * input.Color;
    float4 neighbour = tex2D(TextureSampler, neighbourUv) * input.Color;
    return lerp(center, neighbour, neighbourWeight);
}

technique BasicColorDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
