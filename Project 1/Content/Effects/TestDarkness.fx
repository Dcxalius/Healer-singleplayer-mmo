#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#define TILE_SIZE uint2(32, 32)
#define MAP_MAX_SIZE uint2(63, 63)


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
//bool tileTransparent[4096];
texture FirstTexture;


sampler TextureSampler : register(s0) = sampler_state
{
    Texture = <FirstTexture>;
};

Texture2D transparentMap;

sampler2D transpSamp : register(s1) = sampler_state
{
    Texture = <transparentMap>;
};


bool b2Check(bool2 aBool) 
{
    return aBool.x && aBool.y;
}

bool LineOfSight(float2 aStartPos, float2 aEndPos)
{
    bool2 xdd = (aEndPos.xy >= 0 && aEndPos.xy <= MAP_MAX_SIZE.x);
    if (!(xdd.x && xdd.y))
    {
        return false;
    }
      
    float2 dirVector = normalize(aEndPos - aStartPos);
    
    float dirX = sign(dirVector.x);
    float dirY = sign(dirVector.y);
    float m = (aEndPos.y - aStartPos.y) / (aEndPos.x - aStartPos.x);
    float c = aStartPos.y - m * aStartPos.x;
    
    float2 posToCheck = aStartPos / MAP_MAX_SIZE;
    int2 pointToCheck = aStartPos;
    int2 endPoint = aEndPos;
    
    float2 lastPoint;
    uint step = 0;
    for (uint i = 0; i <= MAP_MAX_SIZE.x + MAP_MAX_SIZE.y; i++)
    {
        lastPoint = pointToCheck;
        if (b2Check(pointToCheck == endPoint))
            return true;
        float4 solidF = tex2D(transpSamp, pointToCheck / MAP_MAX_SIZE);
        //float4 solidF = tex2Dlod(transpSamp, float4(pointToCheck / MAP_MAX_SIZE, 0, 0));
        if (solidF.g > 0)
            return false;
        
        
//        Initially:
//        d = m(x + 1) + b - y
//–
//        Then:
//        d += m
        
        float borderInX;
        if (dirX < 0)
            borderInX = pointToCheck.x;
        else
            borderInX = pointToCheck.x + solidF.a / solidF.b;
        float yAtBorder = m * borderInX + c;
        
        
        if (yAtBorder > pointToCheck.y && yAtBorder < pointToCheck.y + 1)
        {
            pointToCheck += int2(1, 0);
        }
        else
        {
            pointToCheck += int2(0, 1);
        }

    }
    
            return false;
}



float4 MainPS(VertexShaderOutput input) : COLOR
{
    float minDistance = minLength;
    
    float2 centreOfMapSpaceInWorld = lightPos[0] / TILE_SIZE;
    
    bool behindWall = true;
    for (int i = 0; i < 5; i++)
    {
        bool2 a = (lightPos[i] == float2(0, 0));
        if (a.x && a.y) 
            continue;
        
        float d = distance(lightPos[i], (cameraWorldPos + input.Position.xy));
        
        float2 lightPosInMapSpace = lightPos[i] / TILE_SIZE - centreOfMapSpaceInWorld;
        float2 inMapSpace = (cameraWorldPos / TILE_SIZE + input.Position.xy / TILE_SIZE) - lightPosInMapSpace;
        
        if(behindWall)
            behindWall = !LineOfSight(lightPosInMapSpace, inMapSpace);
        
        if (d < minDistance)
            minDistance = d;
    }
    
    
    
    float4 texColor = tex2D(TextureSampler, input.TextureCoordinates) * input.Color;
    float4 xdd = tex2D(transpSamp, input.TextureCoordinates);
    if (behindWall)
        return float4(0.5, 0.5, 0.5, xdd.a);
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