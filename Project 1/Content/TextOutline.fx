#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//sampler2D texSampler : register(S0);

//float2 texelSize;
Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};


struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 col = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    
    float2 texelSize = (1, 1);
    //SpriteTexture.GetDimensions(texelSize.x, texelSize.y);
    //if (texelSize.x == 0)
    //    return float4(1, 1, 1, 1);
    float red = col.r * input.Color.r;
    float4 r;
    if (col.a > 0)
    {
        r = float4(1, 0, 0, col.a);
    }
    else
    {
        
        float2 xOffset = (texelSize.x, 0);
        float2 yOffset = (0, texelSize.y);
        
        float alpha = col.a;
        
        for (int i = 0; i < 4; i++)
        {
            max(alpha, tex2D(SpriteTextureSampler, input.TextureCoordinates + xOffset).a);
            max(alpha, tex2D(SpriteTextureSampler, input.TextureCoordinates - xOffset).a);
            max(alpha, tex2D(SpriteTextureSampler, input.TextureCoordinates + yOffset).a);
            max(alpha, tex2D(SpriteTextureSampler, input.TextureCoordinates - yOffset).a);
            
            xOffset += xOffset;
            yOffset += yOffset;
        }
        //r = float4(0, 1, 1, 1);
        if (alpha > 0)
        {
            r = float4(0, 0, 0, 1);

        }
        else
        {
            r = float4(0, 0, 0, 0);
        }
    }
	
    //return col * input.Color;
    return r;
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};