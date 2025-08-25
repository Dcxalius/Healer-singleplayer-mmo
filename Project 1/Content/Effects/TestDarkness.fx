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
    float2 TextureCoordinates : TEXCOORD0;
};


float2 lightPos[5];
float2 cameraWorldPos;
float2 cameraSize;
float minLength;
float maxBrightness;



sampler TextureSampler : register(s0);

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float minDistance = minLength;
    for (int i = 0; i < 5; i++)
    {
        bool2 a = (lightPos[i] == float2(0, 0));
        if (a.x && a.y) 
            continue;
        
        float d = distance(lightPos[i], (cameraWorldPos + input.Position.xy));
        //float distance = sqrt(pow(lightPos[i].x - (cameraWorldPos.x + input.TextureCoordinates.x * cameraSize), 2) + pow(lightPos[i].y - (cameraWorldPos.y + input.TextureCoordinates.y * cameraSize), 2));
        
        if (d < minDistance)
        {
            minDistance = d;
        }
    }
    
    float4 texColor = tex2D(TextureSampler, input.TextureCoordinates) * input.Color;
    
    if (minDistance < minLength)
    {
        if (minDistance < maxBrightness)
            return texColor;
        else
        {
            float a = ((minDistance - maxBrightness) / (minLength - maxBrightness));
            float r = texColor.r * (1 - a);
            float g = texColor.g * (1 - a);
            float b = texColor.b * (1 - a);
            return float4(r, g, b, texColor.a);
        }
        
    }
    else
        return float4(0, 0, 0, texColor.a);
}

technique BasicColorDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};