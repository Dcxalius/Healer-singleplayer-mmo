#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#define TILE_SIZE float2(32, 32)
#define MAP_MAX_SIZE uint2(65, 65)
#define FMAP_MAX_SIZE float2(65, 65)
#define INV_FMAP_MAX_SIZE float2(1.0f / 65.0f, 1.0f / 65.0f)
#define ZERO_LIGHT float2(0,0)

sampler2D TextureSampler : register(s0);

Texture2D transparentMap : register(t1);
sampler2D transpSamp : register(s1) = sampler_state
{
    Texture = <transparentMap>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
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
float cameraScale;
float2 transparentMapOriginTile;
float minLength;
float maxBrightness;

//bool tileTransparent[4096];



float2 TileToUV(int2 tileIndex)
{
    float2 tileF = (float2) tileIndex;

    // Δtile in [-32, +32] → texel centers in [0, 1]
    return (tileF - transparentMapOriginTile + float2(32.5f, 32.5f)) * INV_FMAP_MAX_SIZE;
}

bool LineOfSight(float2 startPos, float2 endPos, out float4 aDebug)
{
    // Same tile → trivially visible
    int2 startTile = (int2) floor(startPos);
    int2 endTile = (int2) floor(endPos);
    int2 preTargetTile = endTile;
    if (startTile.x == endTile.x && startTile.y == endTile.y)
    {
        aDebug = float4(1, 1, 1, 1);
        return true;
    }

    // Ray origin/end at tile centres
    float2 start = startPos;
    float2 end = endPos;
    float2 dir = end - start;
    float2 dirStep = normalize(dir);
    float2 endNudged = end - dirStep * 0.01f;
    preTargetTile = (int2) floor(endNudged);

    // Avoid degenerate rays
    if (abs(dir.x) < 1e-4 && abs(dir.y) < 1e-4)
    {
        aDebug = float4(1, 1, 1, 1);
        return true;
    }

    int2 current = startTile;

    // Step direction per axis
    int2 step;
    step.x = (dir.x > 0.0f) ? 1 : ((dir.x < 0.0f) ? -1 : 0);
    step.y = (dir.y > 0.0f) ? 1 : ((dir.y < 0.0f) ? -1 : 0);

    // Distance between next vertical / horizontal crossings
    float2 deltaDist;
    deltaDist.x = (abs(dir.x) > 1e-4) ? abs(1.0f / dir.x) : 1e6;
    deltaDist.y = (abs(dir.y) > 1e-4) ? abs(1.0f / dir.y) : 1e6;

    // Distance from start to first vertical / horizontal crossing
    float2 sideDist;

    if (dir.x > 0.0f)
        sideDist.x = ((float) (current.x + 1) - start.x) * deltaDist.x;
    else
        sideDist.x = (start.x - (float) current.x) * deltaDist.x;

    if (dir.y > 0.0f)
        sideDist.y = ((float) (current.y + 1) - start.y) * deltaDist.y;
    else
        sideDist.y = (start.y - (float) current.y) * deltaDist.y;

    if (step.x == 0)
    {
        deltaDist.x = 1e6;
        sideDist.x = 1e6;
    }

    if (step.y == 0)
    {
        deltaDist.y = 1e6;
        sideDist.y = 1e6;
    }

    const float epsilon = 0.00001f;
    [loop]
    for (int i = 0; i < 256; ++i)  // safety cap
    {
        // Step to next tile along the ray
        if (abs(sideDist.x - sideDist.y) < epsilon)
        {
            sideDist.x += deltaDist.x;
            sideDist.y += deltaDist.y;
            current.x += step.x;
            current.y += step.y;
        }
        else if (sideDist.x < sideDist.y)
        {
            sideDist.x += deltaDist.x;
            current.x += step.x;
        }
        else
        {
            sideDist.y += deltaDist.y;
            current.y += step.y;
        }

        // Any solid tile blocks the ray before reaching the target
        float2 uv = TileToUV(current);

        // Outside the 65×65 window → treat as blocked
        if (any(uv < 0.0f) || any(uv > 1.0f))
        {
            aDebug = float4(1, 0, 0, 1);
            return false;
        }

        bool reachedTarget = (current.x == endTile.x && current.y == endTile.y);
        if (reachedTarget)
        {
            aDebug = float4(1, 1, 1, 1);
            return true;
        }

        float4 solidF = tex2Dlod(transpSamp, float4(uv, 0, 0));
        if (solidF.a > 0.0f)
        {
            // Solid tile blocks visibility (but the target tile itself should still be visible).
            // Also allow the tile directly before the target (in ray direction) to be visible.
            if (current.x == preTargetTile.x && current.y == preTargetTile.y)
            {
                aDebug = float4(1, 1, 1, 1);
                return true;
            }
            aDebug = float4(0, 0, 0, 1);
            return false;
        }
    }

    // Should not normally hit this
    aDebug = float4(1, 0, 0, 1);
    return false;
}

//bool NotThatOldLineOfSight(int2 startTile, int2 endTile, out float4 aDebug)
//{
//    // Ray from tile center to tile center
//    float2 start = (float2(startTile) + 0.5f);
//    float2 end = (float2(endTile) + 0.5f);

//    float2 delta = end - start;
//    float dist = length(delta);

//    if (dist < 0.001f)
//    {
//        aDebug = float4(1, 1, 1, 1);
//        return true;
//    }

//    float2 dir = delta / dist;
//    float step = 0.2f; // samples per tile along the ray
//    float tMax = dist + 0.5f; // small extra margin

//    [loop]
//    for (float t = 0.0f; t <= tMax; t += step)
//    {
//        float2 pos = start + dir * t; // tile-space float
//        int2 tile = (int2) floor(pos); // tile index
//        float2 uv = TileToUV(tile);

//        // Outside the 65×65 window → treat as blocked
//        if (any(uv < 0.0f) || any(uv > 1.0f))
//        {
//            aDebug = float4(1, 0, 0, 1);
//            return false;
//        }

//        float4 solidF = tex2Dlod(transpSamp, float4(uv, 0, 0));
        

//        // Once we've clearly passed the end tile, stop
//        if (length((float2) tile + 0.5f - end) < 0.25f)
//        {
//            aDebug = float4(1, 1, 1, 1);
//            return true;
//        }
//        // Solid tile that is not start or end → blocks view
//        if (solidF.a > 0.0f &&
//            !(tile.x == startTile.x && tile.y == startTile.y) &&
//            !(tile.x == endTile.x && tile.y == endTile.y))
//        {
//            aDebug = float4(0, 0, 0, 1);
//            return false;
//        }
//    }

//    aDebug = float4(1, 0, 0, 1);
//    return false;
//}

//bool OldLineOfSight(float2 aStartPos, float2 aEndPos, out float4 aDebug)
//{
//    //bool2 xdd = (aEndPos.xy >= 0 && aEndPos.xy <= MAP_MAX_SIZE.x);
//    //if (!(xdd.x && xdd.y))
//    //{
//    //    aDebug = float4(1, 0, 0, 1);
//    //    return false;
//    //}
    
//    if (b2oCheck((aEndPos - lightPos[0] / TILE_SIZE) / FMAP_MAX_SIZE + float2(0.5f, 0.5f) > 1 || (aEndPos - lightPos[0] / TILE_SIZE) / FMAP_MAX_SIZE + float2(0.5f, 0.5f) < 0))
//    {
//        aDebug = float4(1, 0, 1, 1);
//        return false;
//    }
      
//    float2 dirVector = aEndPos - aStartPos;
    
//    float dirX = sign(dirVector.x);
//    float dirY = sign(dirVector.y);
//    float m = (aEndPos.y - aStartPos.y) / (aEndPos.x - aStartPos.x);
//    float c = aStartPos.y - m * aStartPos.x;
    
//    float2 pointToCheck = aStartPos;
//    float2 endPoint = aEndPos;
    
//    float stepSize = 0.02f;
    
//    for (uint i = 0; i <= (MAP_MAX_SIZE.x + MAP_MAX_SIZE.y)  / stepSize; i++)
//    {
//        float2 inMapSpace = (pointToCheck - lightPos[0] / TILE_SIZE) / FMAP_MAX_SIZE +float2(0.5, 0.5);
        
        
        
//        //float2 inMapSpace = float2((pointToCheck.x - lightPos[0].x / TILE_SIZE.x) / FMAP_MAX_SIZE.x + 0.5f, (pointToCheck.y - lightPos[0].y / TILE_SIZE.y) / FMAP_MAX_SIZE.y + 0.5f);
//        if (distance(pointToCheck, endPoint) < 1)
//        //if (b2aCheck(pointToCheck == endPoint))
//        {
//            aDebug = float4(1, 1, 1, 1);
            
//            return true;
//        }
            
//        if (b2oCheck(inMapSpace > 1 || inMapSpace < 0))
//        {
//            if(inMapSpace.x > 1 )
//            {
//                aDebug = float4(0, inMapSpace.x, 0, 1);
//                return false;
                
//            }
//            if (inMapSpace.x < 0);
//            {
//                aDebug = float4(inMapSpace.x, inMapSpace.x, inMapSpace.x, 1);
//                return false;
//            }
//            if (inMapSpace.y > 1)
//            {
//                //aDebug = float4(pointToCheck.y / 64 + 0.5f, inMapSpace.x, inMapSpace.y, 1);
                
//                aDebug = float4(0, 0, inMapSpace.y, 1);
//                return false;
//            }
            
//            aDebug = float4(inMapSpace.y, 0, inMapSpace.y, 1);
//            return false;
//        }
        
        
//        float4 solidF = tex2Dlod(transpSamp, float4(inMapSpace, 0, 0));
//        if (solidF.a > 0)
//        {
//            aDebug = float4(0, 0, 0, 1);
//            return false;
//        }
            
        
//        //https://www.dcc.fc.up.pt/~mcoimbra/lectures/CG_1213/CG_1213_T5_Rasterization.pdf
////        Initially:
////        d = m(x + 1) + b - y
////–
////        Then:
////        d += m
        
//        float borderInX;
//        if (dirX <= 0)
//            borderInX = pointToCheck.x - 0.5;
//        else
//            borderInX = pointToCheck.x +0.5;
//        float yAtBorder = m * borderInX + c;
        
        
//        if (yAtBorder > pointToCheck.y && yAtBorder < pointToCheck.y + 1)
//            pointToCheck += float2(dirX * stepSize, 0);
//        else
//            pointToCheck += float2(0, dirY * stepSize);

//    }
    
//    aDebug = float4(1, 0, 0, 1);
//    return false;
//}


float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float4 texColor = tex2D(TextureSampler, input.TextureCoordinates) * input.Color;
    float2 pixelPos = cameraWorldPos + input.Position.xy / cameraScale;

    // Tile index for this pixel (tile-aligned LOS)
    float2 pixelTilePos = pixelPos / TILE_SIZE;
    int2 pixelTileIndex = (int2) floor(pixelTilePos);
    float2 pixelTileCenter = (float2) pixelTileIndex + 0.5f;
    float2 targetUv = TileToUV(pixelTileIndex);
    float4 targetSolidF = tex2Dlod(transpSamp, float4(targetUv, 0, 0));
    bool targetIsSolid = (targetSolidF.a > 0.0f);

    float minDistance = minLength;
    float4 DEBUG = float4(0, 0, 0, 0);
    bool anyLit = false;

    for (int i = 0; i < 5; i++)
    {
        if (all(lightPos[i] == ZERO_LIGHT))
            continue;

        float2 lightWorld = lightPos[i];
        float2 lightTilePos = lightWorld / TILE_SIZE;

        // Per-pixel gradient distance (reverted)
        float d = distance(lightWorld, pixelPos);

        float4 losDebug;
        float2 losTarget = targetIsSolid ? pixelTileCenter : pixelTilePos;
        if (LineOfSight(lightTilePos, losTarget, losDebug))
        {
            anyLit = true;
            DEBUG = losDebug; // keep last LOS debug color
            if (d < minDistance)
                minDistance = d;
        }
    }

    // If no light has LOS, whole tile is in shadow
    if (!anyLit)
        return float4(0, 0, 0, 1);

    // Optional: debug tint
    //texColor *= DEBUG;

    if (minDistance >= minLength)
        return float4(0, 0, 0, 1);

    if (minDistance < maxBrightness)
        return texColor;

    float atten = 1 - ((minDistance - maxBrightness) / (minLength - maxBrightness));
    // TODO: Soften/blur the hard lighting edge; current falloff is too harsh.
    return float4(texColor.rgb * atten, texColor.a);
}

//float4 OldMainPS(VertexShaderOutput input) : COLOR0
//{   
//    float minDistance = minLength;
//    float4 texColor = tex2D(TextureSampler, input.TextureCoordinates) * input.Color;
    
//    float2 pixelPos = cameraWorldPos + input.Position.xy;
    
//    float2 centreOfMapSpaceInWorld = lightPos[0] / TILE_SIZE; 
//    float4 DEBUG = float4(0, 0, 0, 0);
//    bool behindWall = true;
//    for (int i = 0; i < 5; i++)
//    {
//        bool2 a = (lightPos[i] == ZERO_LIGHT); //Possible change this to taking in a int with the highest index of light to use
//        if (a.x && a.y) 
//            continue;
        
//        float d = distance(lightPos[i], pixelPos);
        
//        float2 lightPosInTileSpace = lightPos[i] / TILE_SIZE;
//        float2 pixelPosInTileSpace = pixelPos / TILE_SIZE;
        
//        if(behindWall)
//            behindWall = !LineOfSight(lightPosInTileSpace, pixelPosInTileSpace, DEBUG);
        
//        if (d < minDistance)
//            minDistance = d;
//    }
    
//    //float2 ppInMap = pixelPos / TILE_SIZE / FMAP_MAX_SIZE;
//    //float2 lpInMap = lightPos[0] / TILE_SIZE / FMAP_MAX_SIZE;
    
    
//    //float4 solidF = tex2D(transpSamp, lpInMap - ppInMap + float2(0.5f, 0.5f));
    
//    if (behindWall)
//        return DEBUG;
//    texColor *= DEBUG;
    
//        //return float4(0.5, 0.5, 0.5, xdd.a);
//    if (minDistance >= minLength)
//        return float4(0, 0, 0, 1);
    
//    if (minDistance < maxBrightness)
//        return texColor;
    
//    return float4(texColor.rgb * (1 - ((minDistance - maxBrightness) / (minLength - maxBrightness))), texColor.a);
//}


technique BasicColorDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
