#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 BorderColor;
float BorderWidth;

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

float2 TextureSize;
float PxRange;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float Median(float r, float g, float b)
{
    return max(min(r, g), min(max(r, g), b));
}

float ScreenPxRange(float2 uv)
{
    float2 unitRange = float2(PxRange, PxRange) / max(TextureSize, float2(1.0, 1.0));
    float2 screenTexSize = 1.0 / fwidth(uv);
    return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    
    float4 sample = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    float sd = Median(sample.r, sample.g, sample.b);
    float screenPxDistance = ScreenPxRange(input.TextureCoordinates) * (sd - 0.5);
    
    float fillOpacity = saturate(screenPxDistance + 0.5);

    if (BorderWidth > 0.0)
    {
        float outerOpacity = saturate(screenPxDistance + BorderWidth + 0.5);

        float borderOpacity = saturate(outerOpacity - fillOpacity) * BorderColor.a;

        float3 fillRgb = input.Color.rgb;
        float3 borderRgb = BorderColor.rgb;

        float3 finalRgb =
        borderRgb * borderOpacity +
        fillRgb * fillOpacity * input.Color.a;

        float finalA = max(fillOpacity, borderOpacity);
        
        return float4(finalRgb, finalA);
    }

    return float4(input.Color.rgb * input.Color.a * fillOpacity, input.Color.a * fillOpacity);
    //return float4(input.Color.rgb * input.Color.a * opacity, input.Color.a * opacity);
}

technique BasicColorDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
