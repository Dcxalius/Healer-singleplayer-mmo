#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif


struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

sampler TextureSampler : register(s0);

float4 MainVS(float duration : FLOAT, float2 texCoord : TEXCOORD) : COLOR
{
	
    //float4 texColor = tex2D(TextureSampler, texCoord);
    //if (texColor.r >= duration)
        return float4(0, 0, 0, 0);
	//else
 //       return float4(80, 80, 80, 80);

}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};