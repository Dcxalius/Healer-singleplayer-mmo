#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VertexShaderOutput
{
	float4 Color : COLOR0;
    float2 textCoord : TEXCOORD0;
};

sampler2D texSampler;

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 col = tex2D(texSampler, input.textCoord);
    //float avgC = (col.r + col.g + col.b) / 3;
    //float3 mulC = input.Color.rgb * avgC;
    //float4 r = float4(mulC, col.a);
    float4 mulC = col;

    float4 r = float4(col.r,0,0,1);
	
    return r;
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};