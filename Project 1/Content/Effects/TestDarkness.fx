#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#define TILE_SIZE float2(32, 32)
#define MAP_MAX_SIZE uint2(64, 64)
#define FMAP_MAX_SIZE float2(64, 64)
#define ZERO_LIGHT float2(0,0)

sampler2D TextureSampler : register(s0);

Texture2D transparentMap : register(t1);
sampler2D transpSamp : register(s1) = sampler_state
{
    Texture = <transparentMap>;
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


bool b2oCheck(bool2 aBool)
{
    return aBool.x || aBool.y;
}

bool b2aCheck(bool2 aBool) 
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
    
    if (b2oCheck((aEndPos - lightPos[0] / TILE_SIZE) / FMAP_MAX_SIZE + float2(0.5f, 0.5f) > 1 || (aEndPos - lightPos[0] / TILE_SIZE) / FMAP_MAX_SIZE + float2(0.5f, 0.5f) < 0))
    {
        aDebug = float4(1, 0, 1, 1);
        return false;
    }
      
    float2 dirVector = aEndPos - aStartPos;
    
    float dirX = sign(dirVector.x);
    float dirY = sign(dirVector.y);
    float m = (aEndPos.y - aStartPos.y) / (aEndPos.x - aStartPos.x);
    float c = aStartPos.y - m * aStartPos.x;
    
    float2 pointToCheck = aStartPos;
    float2 endPoint = aEndPos;
    
    float stepSize = 0.02f;
    
    for (uint i = 0; i <= (MAP_MAX_SIZE.x + MAP_MAX_SIZE.y)  / stepSize; i++)
    {
        float2 inMapSpace = (pointToCheck - lightPos[0] / TILE_SIZE) / FMAP_MAX_SIZE +float2(0.5, 0.5);
        
        
        
        //float2 inMapSpace = float2((pointToCheck.x - lightPos[0].x / TILE_SIZE.x) / FMAP_MAX_SIZE.x + 0.5f, (pointToCheck.y - lightPos[0].y / TILE_SIZE.y) / FMAP_MAX_SIZE.y + 0.5f);
        if (distance(pointToCheck, endPoint) < 1)
        //if (b2aCheck(pointToCheck == endPoint))
        {
            aDebug = float4(1, 1, 1, 1);
            
            return true;
        }
            
        if (b2oCheck(inMapSpace > 1 || inMapSpace < 0))
        {
            if(inMapSpace.x > 1 )
            {
                aDebug = float4(0, inMapSpace.x, 0, 1);
                return false;
                
            }
            if (inMapSpace.x < 0);
            {
                aDebug = float4(inMapSpace.x, inMapSpace.x, inMapSpace.x, 1);
                return false;
            }
            if (inMapSpace.y > 1)
            {
                //aDebug = float4(pointToCheck.y / 64 + 0.5f, inMapSpace.x, inMapSpace.y, 1);
                
                aDebug = float4(0, 0, inMapSpace.y, 1);
                return false;
            }
            
            aDebug = float4(inMapSpace.y, 0, inMapSpace.y, 1);
            return false;
        }
        
        
        float4 solidF = tex2Dlod(transpSamp, float4(inMapSpace, 0, 0));
        if (solidF.a > 0)
        {
            aDebug = float4(0, 0, 0, 1);
            return false;
        }
            
        
        //https://www.dcc.fc.up.pt/~mcoimbra/lectures/CG_1213/CG_1213_T5_Rasterization.pdf
//        Initially:
//        d = m(x + 1) + b - y
//–
//        Then:
//        d += m
        
        float borderInX;
        if (dirX <= 0)
            borderInX = pointToCheck.x - 0.5;
        else
            borderInX = pointToCheck.x +0.5;
        float yAtBorder = m * borderInX + c;
        
        
        if (yAtBorder > pointToCheck.y && yAtBorder < pointToCheck.y + 1)
            pointToCheck += float2(dirX * stepSize, 0);
        else
            pointToCheck += float2(0, dirY * stepSize);

    }
    
    aDebug = float4(1, 0, 0, 1);
    return false;
}



float4 MainPS(VertexShaderOutput input) : COLOR0
{   
    float minDistance = minLength;
    float4 texColor = tex2D(TextureSampler, input.TextureCoordinates) * input.Color;
    
    float2 pixelPos = cameraWorldPos + input.Position.xy;
    
    float2 centreOfMapSpaceInWorld = lightPos[0] / TILE_SIZE; 
    float4 DEBUG = float4(0, 0, 0, 0);
    bool behindWall = true;
    for (int i = 0; i < 5; i++)
    {
        bool2 a = (lightPos[i] == ZERO_LIGHT); //Possible change this to taking in a int with the highest index of light to use
        if (a.x && a.y) 
            continue;
        
        float d = distance(lightPos[i], pixelPos);
        
        float2 lightPosInTileSpace = lightPos[i] / TILE_SIZE;
        float2 pixelPosInTileSpace = pixelPos / TILE_SIZE;
        
        if(behindWall)
            behindWall = !LineOfSight(lightPosInTileSpace, pixelPosInTileSpace, DEBUG);
        
        if (d < minDistance)
            minDistance = d;
    }
    
    //float2 ppInMap = pixelPos / TILE_SIZE / FMAP_MAX_SIZE;
    //float2 lpInMap = lightPos[0] / TILE_SIZE / FMAP_MAX_SIZE;
    
    
    //float4 solidF = tex2D(transpSamp, lpInMap - ppInMap + float2(0.5f, 0.5f));
    
    if (behindWall)
        return DEBUG;
    texColor *= DEBUG;
    
        //return float4(0.5, 0.5, 0.5, xdd.a);
    if (minDistance >= minLength)
        return float4(0, 0, 0, 1);
    
    if (minDistance < maxBrightness)
        return texColor;
    
    return float4(texColor.rgb * (1 - ((minDistance - maxBrightness) / (minLength - maxBrightness))), texColor.a);
}


technique BasicColorDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};