#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#define TILE_SIZE uint2(32, 32)
#define MAP_MAX_SIZE uint2(63, 63)
#define ZERO_LIGHT float2(0,0)

Texture2D Texture : register(t1);

sampler2D TextureSampler : register(s2);
sampler2D transpSamp : register(s1) = sampler_state
{
    Texture = <Texture>;
};

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




bool b2Check(bool2 aBool) 
{
    return aBool.x && aBool.y;
}

bool LineOfSight(float2 aStartPos, float2 aEndPos, out float4 aDebug)
{
    //bool2 xdd = (aEndPos.xy >= 0 && aEndPos.xy <= MAP_MAX_SIZE.x);
    //if (!(xdd.x && xdd.y))
    //{
    //    aDebug = float4(1, 0, 0, 1);
    //    return false;
    //}
      
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
        if (distance(pointToCheck, endPoint) < 5)
        //if (b2Check(pointToCheck == endPoint))
        {
            aDebug = float4(0, 0, 1, 0.2);
            return true;
        }
            
        float4 solidF = tex2D(transpSamp, pointToCheck / MAP_MAX_SIZE);
        if (solidF.a > 0)
        {
            aDebug = float4(0, 1, 0, 0.2);
            return false;
        }
            
        
        
//        Initially:
//        d = m(x + 1) + b - y
//–
//        Then:
//        d += m
        
        float borderInX;
        if (dirX < 0)
            borderInX = pointToCheck.x;
        else
            borderInX = pointToCheck.x + 1;
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
    
    aDebug = float4(1, 0, 0, 0.2);
    return false;
}



float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float minDistance = minLength;
    float4 texColor = tex2D(TextureSampler, input.TextureCoordinates) * input.Color;
    
    float2 centreOfMapSpaceInWorld = lightPos[0] / TILE_SIZE; 
    float4 DEBUG = float4(0, 0, 0, 0);
    bool behindWall = true;
    for (int i = 0; i < 5; i++)
    {
        bool2 a = (lightPos[i] == ZERO_LIGHT); //Possible change this to taking in a int with the highest index of light to use
        if (a.x && a.y) 
            continue;
        
        float d = distance(lightPos[i], (cameraWorldPos + input.Position.xy));
        
        float2 lightPosInMapSpace = lightPos[i] / TILE_SIZE - centreOfMapSpaceInWorld;
        float2 inMapSpace = (cameraWorldPos / TILE_SIZE + input.Position.xy / TILE_SIZE) - lightPosInMapSpace;
        
        if(behindWall)
            behindWall = !LineOfSight(lightPosInMapSpace, inMapSpace, DEBUG);
        
        if (d < minDistance)
            minDistance = d;
    }
    
    
    
    //float4 solidF = tex2D(transpSamp, input.TextureCoordinates);
    
    ;
        //return float4(0.5, 0.5, 0.5, xdd.a);
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
            return float4(r, g, b, texColor.a + texColor.a);
        }
        
    }
    else if (!behindWall)
        return DEBUG;
        return float4(0, 0, 0, 1);
}


technique BasicColorDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};