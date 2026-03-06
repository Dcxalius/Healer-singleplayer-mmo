# Shader path

This is the current shader pipeline, in execution order.

## 1) Asset registration (effect gets built)

### `Project 1/Content/Content.mgcb`
```txt
#begin Effects/SuperSoftShadows.fx
/importer:EffectImporter
/processor:EffectProcessor
/processorParam:DebugMode=Auto
/build:Effects/SuperSoftShadows.fx
#end
```

## 2) Effect loading (runtime)

### `Project 1/Managers/EffectManager.cs`
```csharp
internal static class EffectManager
{
    static Dictionary<string, Effect> effects;

    public static Effect GetEffect(string aName)
    {
        ThreadAffinity.AssertMainThread();
        return effects[aName];
    }

    public static void Init()
    {
        ThreadAffinity.AssertMainThread();
        if (initialized) return;
        initialized = true;

        effects = new Dictionary<string, Effect>();
        rendertargets = new Dictionary<IEffects, RenderTarget2D>();
        spriteBatch = GraphicsManager.CreateSpriteBatch();

        string filePath = SaveManager.Effects;
        string[] files = Directory.GetFiles(filePath);
        string debug = "Effects loaded: ";
        for (int i = 0; i < files.Length; i++)
        {
            string name = SaveManager.TrimToNameOnly(files[i]);
            Effect e = Game1.ContentManager.Load<Effect>("Effects/" + name);
            effects.Add(name, e);
            debug += name + ", ";
        }

        DebugManager.Print(debug);
    }
}
```

## 3) Shader vertex format

### `Project 1/Textures/ShadowVertex.cs`
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct ShadowVertex : IVertexType
{
    public Vector4 Segment;     // POSITION0
    public Vector2 ShadowCoord; // TEXCOORD0

    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public ShadowVertex(Vector4 segment, Vector2 shadowCoord)
    {
        Segment = segment;
        ShadowCoord = shadowCoord;
    }
}
```

## 4) Light emitters and radii

### `Project 1/GameObjects/ILightEmitter.cs`
```csharp
namespace Project_1.GameObjects
{
    internal interface ILightEmitter
    {
        const float DefaultLightRadiusTiles = 3f;

        float LightRadiusTiles => DefaultLightRadiusTiles;
    }
}
```

### `Project 1/GameObjects/Entities/Friendlies/Players/Player.cs`
```csharp
internal class Player : Friendly, ILightEmitter
{
    public float LightRadiusTiles => 6f;
    ...
}
```

### `Project 1/GameObjects/Entities/Friendlies/GuildMembers/GuildMember.cs`
```csharp
internal class GuildMember : Friendly, ILightEmitter
{
    public float LightRadiusTiles => 5f;
    ...
}
```

## 5) Light collection snapshot (sim thread)

### `Project 1/GameObjects/ObjectManager.cs`
```csharp
static volatile LightSnapshot renderLightSnapshot = LightSnapshot.Empty;
static readonly int[] lightRenderIdScratch = new int[LightSnapshot.MaxLights];
static readonly WorldSpace[] lightPositionScratch = new WorldSpace[LightSnapshot.MaxLights];
static readonly float[] lightRadiusTilesScratch = new float[LightSnapshot.MaxLights];
static readonly bool[] lightCoreScratch = new bool[LightSnapshot.MaxLights];
static readonly float[] lightDistanceToPlayerScratch = new float[LightSnapshot.MaxLights];

public sealed class LightSnapshot
{
    public const int MaxLights = 16;
    public const float NearbyEmitterRangeTiles = 40f;
    public static readonly LightSnapshot Empty = new LightSnapshot(Array.Empty<WorldSpace>(), Array.Empty<float>(), Array.Empty<bool>(), Array.Empty<float>(), Point.Zero);

    LightSnapshot(WorldSpace[] positions, float[] radiusTiles, bool[] isCoreLights, float[] distanceToPlayerWorld, Point originTile)
    {
        Positions = positions ?? Array.Empty<WorldSpace>();
        RadiusTiles = radiusTiles ?? Array.Empty<float>();
        IsCoreLights = isCoreLights ?? Array.Empty<bool>();
        DistanceToPlayerWorld = distanceToPlayerWorld ?? Array.Empty<float>();
        OriginTile = originTile;
    }

    public int Count => Positions.Length;
    public WorldSpace[] Positions { get; }
    public float[] RadiusTiles { get; }
    public bool[] IsCoreLights { get; }
    public float[] DistanceToPlayerWorld { get; }
    public Point OriginTile { get; }

    public WorldSpace GetPosition(int index) { return Positions[index]; }
    public float GetRadiusTiles(int index) { return RadiusTiles[index]; }

    public bool IsCoreLight(int index)
    {
        if (index < 0 || index >= IsCoreLights.Length) return false;
        return IsCoreLights[index];
    }

    public float GetDistanceToPlayerWorld(int index)
    {
        if (index < 0 || index >= DistanceToPlayerWorld.Length) return float.MaxValue;
        return DistanceToPlayerWorld[index];
    }

    public static LightSnapshot CreateFromScratch(int count, WorldSpace[] positions, float[] radiusTiles, bool[] isCoreLights, float[] distanceToPlayerWorld, Point originTile)
    {
        int clampedCount = Math.Clamp(count, 0, MaxLights);
        if (clampedCount == 0) return Empty;

        WorldSpace[] positionCopy = new WorldSpace[clampedCount];
        float[] radiusCopy = new float[clampedCount];
        bool[] coreCopy = new bool[clampedCount];
        float[] distanceCopy = new float[clampedCount];
        Array.Copy(positions, positionCopy, clampedCount);
        Array.Copy(radiusTiles, radiusCopy, clampedCount);
        Array.Copy(isCoreLights, coreCopy, clampedCount);
        Array.Copy(distanceToPlayerWorld, distanceCopy, clampedCount);
        return new LightSnapshot(positionCopy, radiusCopy, coreCopy, distanceCopy, originTile);
    }
}

public static LightSnapshot RenderLightSnapshot => renderLightSnapshot;

static LightSnapshot BuildRenderLightSnapshot()
{
    ThreadAffinity.AssertSimThread();
    if (player == null) return LightSnapshot.Empty;

    int count = 0;
    float nearbyRangeWorld = LightSnapshot.NearbyEmitterRangeTiles * Tile.Size.X;
    WorldSpace playerFeet = player.FeetPosition;
    Point originTile = TileManager.GetGridPos(playerFeet);

    AddEmitterIfEligible(player, true, playerFeet, nearbyRangeWorld, ref count);

    for (int i = 0; i < guild.Count && count < LightSnapshot.MaxLights; i++)
    {
        GuildMember member = guild[i];
        if (member == null) continue;
        if (!player.Party.IsInParty(member)) continue;
        AddEmitterIfEligible(member, true, playerFeet, nearbyRangeWorld, ref count);
    }

    for (int i = 0; i < entities.Count && count < LightSnapshot.MaxLights; i++)
    {
        AddEmitterIfEligible(entities[i], false, playerFeet, nearbyRangeWorld, ref count);
    }

    for (int i = 0; i < guild.Count && count < LightSnapshot.MaxLights; i++)
    {
        AddEmitterIfEligible(guild[i], false, playerFeet, nearbyRangeWorld, ref count);
    }

    for (int i = 0; i < npcs.Count && count < LightSnapshot.MaxLights; i++)
    {
        AddEmitterIfEligible(npcs[i], false, playerFeet, nearbyRangeWorld, ref count);
    }

    return LightSnapshot.CreateFromScratch(count, lightPositionScratch, lightRadiusTilesScratch, lightCoreScratch, lightDistanceToPlayerScratch, originTile);
}

static void AddEmitterIfEligible(Entity entity, bool forceInclude, WorldSpace playerFeet, float nearbyRangeWorld, ref int count)
{
    ThreadAffinity.AssertSimThread();
    if (entity == null) return;
    if (count >= LightSnapshot.MaxLights) return;
    if (entity is not ILightEmitter emitter) return;

    if (!forceInclude && entity.FeetPosition.DistanceTo(playerFeet) > nearbyRangeWorld)
    {
        return;
    }

    int renderId = entity.RenderId;
    for (int i = 0; i < count; i++)
    {
        if (lightRenderIdScratch[i] != renderId) continue;
        if (forceInclude) lightCoreScratch[i] = true;
        return;
    }

    lightRenderIdScratch[count] = renderId;
    WorldSpace feet = entity.FeetPosition;
    lightPositionScratch[count] = feet;
    lightRadiusTilesScratch[count] = Math.Max(0.1f, emitter.LightRadiusTiles);
    lightCoreScratch[count] = forceInclude;
    lightDistanceToPlayerScratch[count] = feet.DistanceTo(playerFeet);
    count++;
}
```

## 6) Snapshot orchestration (sim thread)

### `Project 1/Managers/RenderSnapshotManager.cs`
```csharp
public static void BuildGameSnapshots()
{
    ThreadAffinity.AssertSimThread();
    TileManager.BuildRenderSnapshot();
    ObjectManager.BuildRenderSnapshot();
    ShadowSnapshotManager.BuildSnapshot(ObjectManager.RenderLightSnapshot);
    Camera.Camera.BuildMinimapSnapshot();
    ProjectileManager.BuildRenderSnapshot();
    TileManager.BuildDoodadRenderSnapshots();
    CorpseManager.BuildRenderSnapshot();
    SpawnerManager.BuildRenderSnapshot();
    MinimapSnapshotManager.BuildSnapshot();
    ...
}
```

## 7) Tile boundaries -> occluder segments -> mesh

### `Project 1/Tiles/TileShadowOccluderBuilder.cs`
```csharp
internal readonly struct TileShadowSegment
{
    public TileShadowSegment(Vector2 a, Vector2 b)
    {
        A = a;
        B = b;
    }

    public Vector2 A { get; }
    public Vector2 B { get; }
}

internal static class TileShadowOccluderBuilder
{
    public static int BuildSegments(Rectangle tileBounds, List<TileShadowSegment> destination, int maxSegments)
    {
        return BuildSegments(tileBounds, destination, maxSegments, out _);
    }

    public static int BuildSegments(Rectangle tileBounds, List<TileShadowSegment> destination, int maxSegments, out bool capped)
    {
        ThreadAffinity.AssertSimThread();
        capped = false;
        if (destination == null || maxSegments <= 0) return 0;
        destination.Clear();

        int minX = tileBounds.Left;
        int minY = tileBounds.Top;
        int maxX = tileBounds.Right;
        int maxY = tileBounds.Bottom;

        // Horizontal runs: top and bottom edges.
        for (int y = minY; y < maxY; y++)
        {
            int x = minX;
            while (x < maxX)
            {
                if (HasTopBoundary(x, y))
                {
                    int runStart = x;
                    x++;
                    while (x < maxX && HasTopBoundary(x, y)) x++;
                    if (!TryAddHorizontalTop(destination, maxSegments, runStart, x - 1, y))
                    {
                        capped = true;
                        return destination.Count;
                    }
                    continue;
                }

                if (HasBottomBoundary(x, y))
                {
                    int runStart = x;
                    x++;
                    while (x < maxX && HasBottomBoundary(x, y)) x++;
                    if (!TryAddHorizontalBottom(destination, maxSegments, runStart, x - 1, y))
                    {
                        capped = true;
                        return destination.Count;
                    }
                    continue;
                }

                x++;
            }
        }

        // Vertical runs: left and right edges.
        for (int x = minX; x < maxX; x++)
        {
            int y = minY;
            while (y < maxY)
            {
                if (HasLeftBoundary(x, y))
                {
                    int runStart = y;
                    y++;
                    while (y < maxY && HasLeftBoundary(x, y)) y++;
                    if (!TryAddVerticalLeft(destination, maxSegments, x, runStart, y - 1))
                    {
                        capped = true;
                        return destination.Count;
                    }
                    continue;
                }

                if (HasRightBoundary(x, y))
                {
                    int runStart = y;
                    y++;
                    while (y < maxY && HasRightBoundary(x, y)) y++;
                    if (!TryAddVerticalRight(destination, maxSegments, x, runStart, y - 1))
                    {
                        capped = true;
                        return destination.Count;
                    }
                    continue;
                }

                y++;
            }
        }

        return destination.Count;
    }

    public static void BuildMesh(IReadOnlyList<TileShadowSegment> segments, ShadowVertex[] vertices, short[] indices, out int vertexCount, out int indexCount)
    {
        ThreadAffinity.AssertSimThread();
        vertexCount = 0;
        indexCount = 0;
        if (segments == null || vertices == null || indices == null) return;

        int segmentCount = segments.Count;
        int requiredVertices = segmentCount * 4;
        int requiredIndices = segmentCount * 6;
        if (vertices.Length < requiredVertices || indices.Length < requiredIndices)
        {
            return;
        }

        for (int i = 0; i < segmentCount; i++)
        {
            TileShadowSegment segment = segments[i];
            Vector4 packed = new Vector4(segment.B.X, segment.B.Y, segment.A.X, segment.A.Y);
            int v = i * 4;
            vertices[v + 0] = new ShadowVertex(packed, new Vector2(0f, 0f));
            vertices[v + 1] = new ShadowVertex(packed, new Vector2(1f, 0f));
            vertices[v + 2] = new ShadowVertex(packed, new Vector2(0f, 1f));
            vertices[v + 3] = new ShadowVertex(packed, new Vector2(1f, 1f));

            int ii = i * 6;
            short baseIndex = (short)v;
            indices[ii + 0] = (short)(baseIndex + 0);
            indices[ii + 1] = (short)(baseIndex + 1);
            indices[ii + 2] = (short)(baseIndex + 2);
            indices[ii + 3] = (short)(baseIndex + 2);
            indices[ii + 4] = (short)(baseIndex + 1);
            indices[ii + 5] = (short)(baseIndex + 3);
        }

        vertexCount = requiredVertices;
        indexCount = requiredIndices;
    }

    static bool IsOpen(int gridX, int gridY)
    {
        Tile neighbour = TileManager.GetTileAtGrid(new Point(gridX, gridY));
        return neighbour == null || neighbour.Transparent;
    }

    static bool IsSolid(int gridX, int gridY)
    {
        Tile tile = TileManager.GetTileAtGrid(new Point(gridX, gridY));
        return tile != null && !tile.Transparent;
    }

    static bool HasTopBoundary(int x, int y) => IsSolid(x, y) && IsOpen(x, y - 1);
    static bool HasBottomBoundary(int x, int y) => IsSolid(x, y) && IsOpen(x, y + 1);
    static bool HasLeftBoundary(int x, int y) => IsSolid(x, y) && IsOpen(x - 1, y);
    static bool HasRightBoundary(int x, int y) => IsSolid(x, y) && IsOpen(x + 1, y);

    static bool TryAddHorizontalTop(List<TileShadowSegment> destination, int maxSegments, int startX, int endX, int y)
    {
        float wy = y * Tile.Size.Y;
        float ax = (endX + 1) * Tile.Size.X;
        float bx = startX * Tile.Size.X;
        return TryAdd(destination, maxSegments, new Vector2(ax, wy), new Vector2(bx, wy));
    }

    static bool TryAddHorizontalBottom(List<TileShadowSegment> destination, int maxSegments, int startX, int endX, int y)
    {
        float wy = (y + 1) * Tile.Size.Y;
        float ax = startX * Tile.Size.X;
        float bx = (endX + 1) * Tile.Size.X;
        return TryAdd(destination, maxSegments, new Vector2(ax, wy), new Vector2(bx, wy));
    }

    static bool TryAddVerticalLeft(List<TileShadowSegment> destination, int maxSegments, int x, int startY, int endY)
    {
        float wx = x * Tile.Size.X;
        float ay = startY * Tile.Size.Y;
        float by = (endY + 1) * Tile.Size.Y;
        return TryAdd(destination, maxSegments, new Vector2(wx, ay), new Vector2(wx, by));
    }

    static bool TryAddVerticalRight(List<TileShadowSegment> destination, int maxSegments, int x, int startY, int endY)
    {
        float wx = (x + 1) * Tile.Size.X;
        float ay = (endY + 1) * Tile.Size.Y;
        float by = startY * Tile.Size.Y;
        return TryAdd(destination, maxSegments, new Vector2(wx, ay), new Vector2(wx, by));
    }

    static bool TryAdd(List<TileShadowSegment> destination, int maxSegments, Vector2 a, Vector2 b)
    {
        if (destination.Count >= maxSegments) return false;
        destination.Add(new TileShadowSegment(a, b));
        return true;
    }
}
```

## 8) Shadow snapshot build (sim thread)

### `Project 1/Managers/ShadowSnapshotManager.cs`
```csharp
internal static class ShadowSnapshotManager
{
    const int SegmentPaddingTiles = 2;
    const int MaxSegmentsPerLight = 512;
    const int MaxTotalSegments = 2048;
    const int MaxTrackedLights = ObjectManager.LightSnapshot.MaxLights;

    static readonly List<TileShadowSegment> segmentScratch = new List<TileShadowSegment>(MaxSegmentsPerLight);
    static readonly PreparedShadowLight[] preparedLightScratch = new PreparedShadowLight[MaxTrackedLights];
    static readonly bool[] keepLightScratch = new bool[MaxTrackedLights];
    static readonly int[] dropCandidateScratch = new int[MaxTrackedLights];
    static readonly NonCoreDropComparer nonCoreDropComparer = new NonCoreDropComparer();
    static volatile ShadowFrameSnapshot snapshot = ShadowFrameSnapshot.Empty;

    public static ShadowFrameSnapshot Snapshot => snapshot;

    public static void BuildSnapshot(ObjectManager.LightSnapshot lightSnapshot)
    {
        ThreadAffinity.AssertSimThread();
        if (lightSnapshot == null || lightSnapshot.Count == 0)
        {
            snapshot = ShadowFrameSnapshot.Empty;
            ShadowRenderTelemetry.RecordBuild(0, 0, 0, 0, 0, 0);
            return;
        }

        int lightCount = Math.Clamp(lightSnapshot.Count, 0, MaxTrackedLights);
        int candidateLights = 0;
        int candidateSegments = 0;
        int cappedLights = 0;

        for (int i = 0; i < lightCount; i++)
        {
            WorldSpace lightPosition = lightSnapshot.GetPosition(i);
            float radiusTiles = Math.Max(ILightEmitter.DefaultLightRadiusTiles, lightSnapshot.GetRadiusTiles(i));
            bool isCoreLight = lightSnapshot.IsCoreLight(i);
            float distanceToPlayer = lightSnapshot.GetDistanceToPlayerWorld(i);

            int radius = (int)MathF.Ceiling(radiusTiles) + SegmentPaddingTiles;
            Point centerTile = TileManager.GetGridPos(lightPosition);
            Rectangle tileBounds = new Rectangle(centerTile.X - radius, centerTile.Y - radius, radius * 2 + 1, radius * 2 + 1);

            int segmentCount = TileShadowOccluderBuilder.BuildSegments(tileBounds, segmentScratch, MaxSegmentsPerLight, out bool cappedByLightLimit);
            if (cappedByLightLimit) cappedLights++;

            preparedLightScratch[i] = new PreparedShadowLight(lightPosition, radiusTiles, tileBounds, isCoreLight, distanceToPlayer, segmentCount);
            bool keep = segmentCount > 0;
            keepLightScratch[i] = keep;
            if (!keep) continue;

            candidateLights++;
            candidateSegments += segmentCount;
        }

        int droppedLights = 0;
        if (candidateSegments > MaxTotalSegments)
        {
            int dropCandidateCount = 0;
            for (int i = 0; i < lightCount; i++)
            {
                if (!keepLightScratch[i]) continue;
                if (preparedLightScratch[i].IsCoreLight) continue;
                dropCandidateScratch[dropCandidateCount++] = i;
            }

            if (dropCandidateCount > 1)
            {
                Array.Sort(dropCandidateScratch, 0, dropCandidateCount, nonCoreDropComparer);
            }

            int keptSegmentBudget = candidateSegments;
            for (int i = 0; i < dropCandidateCount && keptSegmentBudget > MaxTotalSegments; i++)
            {
                int index = dropCandidateScratch[i];
                if (!keepLightScratch[index]) continue;
                keepLightScratch[index] = false;
                keptSegmentBudget -= preparedLightScratch[index].SegmentCount;
                droppedLights++;
            }
        }

        int activeLights = candidateLights - droppedLights;
        if (activeLights <= 0)
        {
            snapshot = ShadowFrameSnapshot.Empty;
            ShadowRenderTelemetry.RecordBuild(candidateLights, 0, droppedLights, 0, 0, cappedLights);
            return;
        }

        ShadowLightSnapshot[] lights = new ShadowLightSnapshot[activeLights];
        int write = 0;
        int totalSegments = 0;
        int maxSegments = 0;
        for (int i = 0; i < lightCount; i++)
        {
            if (!keepLightScratch[i]) continue;

            PreparedShadowLight prepared = preparedLightScratch[i];
            int segmentCount = TileShadowOccluderBuilder.BuildSegments(prepared.TileBounds, segmentScratch, MaxSegmentsPerLight, out _);
            if (segmentCount <= 0) continue;

            ShadowVertex[] vertices = new ShadowVertex[segmentCount * 4];
            short[] indices = new short[segmentCount * 6];
            TileShadowOccluderBuilder.BuildMesh(segmentScratch, vertices, indices, out int vertexCount, out int indexCount);
            lights[write++] = new ShadowLightSnapshot(prepared.Position, prepared.RadiusTiles, vertices, vertexCount, indices, indexCount);
            totalSegments += segmentCount;
            if (segmentCount > maxSegments) maxSegments = segmentCount;
        }

        if (write != lights.Length)
        {
            Array.Resize(ref lights, write);
            activeLights = write;
        }

        if (activeLights <= 0)
        {
            snapshot = ShadowFrameSnapshot.Empty;
            ShadowRenderTelemetry.RecordBuild(candidateLights, 0, droppedLights, 0, 0, cappedLights);
            return;
        }

        snapshot = new ShadowFrameSnapshot(lights, Camera.Camera.WorldRectangle);
        ShadowRenderTelemetry.RecordBuild(candidateLights, activeLights, droppedLights, totalSegments, maxSegments, cappedLights);
    }

    readonly struct PreparedShadowLight
    {
        public PreparedShadowLight(WorldSpace position, float radiusTiles, Rectangle tileBounds, bool isCoreLight, float distanceToPlayer, int segmentCount)
        {
            Position = position;
            RadiusTiles = radiusTiles;
            TileBounds = tileBounds;
            IsCoreLight = isCoreLight;
            DistanceToPlayer = distanceToPlayer;
            SegmentCount = segmentCount;
        }

        public WorldSpace Position { get; }
        public float RadiusTiles { get; }
        public Rectangle TileBounds { get; }
        public bool IsCoreLight { get; }
        public float DistanceToPlayer { get; }
        public int SegmentCount { get; }
    }

    sealed class NonCoreDropComparer : IComparer<int>
    {
        public int Compare(int left, int right)
        {
            PreparedShadowLight lhs = preparedLightScratch[left];
            PreparedShadowLight rhs = preparedLightScratch[right];

            int distanceCompare = rhs.DistanceToPlayer.CompareTo(lhs.DistanceToPlayer);
            if (distanceCompare != 0) return distanceCompare;

            int segmentCompare = rhs.SegmentCount.CompareTo(lhs.SegmentCount);
            if (segmentCompare != 0) return segmentCompare;

            return right.CompareTo(left);
        }
    }
}

internal sealed class ShadowFrameSnapshot
{
    public static readonly ShadowFrameSnapshot Empty = new ShadowFrameSnapshot(Array.Empty<ShadowLightSnapshot>(), Rectangle.Empty);

    public ShadowFrameSnapshot(ShadowLightSnapshot[] lights, Rectangle worldBounds)
    {
        Lights = lights ?? Array.Empty<ShadowLightSnapshot>();
        WorldBounds = worldBounds;
    }

    public ShadowLightSnapshot[] Lights { get; }
    public Rectangle WorldBounds { get; }
    public int Count => Lights.Length;
}

internal readonly struct ShadowLightSnapshot
{
    public ShadowLightSnapshot(WorldSpace position, float radiusTiles, ShadowVertex[] vertices, int vertexCount, short[] indices, int indexCount)
    {
        Position = position;
        RadiusTiles = radiusTiles;
        Vertices = vertices ?? Array.Empty<ShadowVertex>();
        VertexCount = Math.Max(0, vertexCount);
        Indices = indices ?? Array.Empty<short>();
        IndexCount = Math.Max(0, indexCount);
    }

    public WorldSpace Position { get; }
    public float RadiusTiles { get; }
    public ShadowVertex[] Vertices { get; }
    public int VertexCount { get; }
    public short[] Indices { get; }
    public int IndexCount { get; }

    public static ShadowLightSnapshot Empty(WorldSpace position, float radiusTiles)
    {
        return new ShadowLightSnapshot(position, radiusTiles, Array.Empty<ShadowVertex>(), 0, Array.Empty<short>(), 0);
    }
}
```

## 9) GPU render + combine + composite (main thread)

### `Project 1/Managers/SuperSoftShadowRenderer.cs`
```csharp
internal static class SuperSoftShadowRenderer
{
    const string ShadowEffectName = "SuperSoftShadows";

    static readonly BlendState maxBlendState = new BlendState
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.Max,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Max
    };

    static readonly BlendState darkenByMaskBlendState = new BlendState
    {
        ColorSourceBlend = Blend.Zero,
        ColorDestinationBlend = Blend.InverseSourceColor,
        ColorBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.One,
        AlphaBlendFunction = BlendFunction.Add
    };

    static SpriteBatch spriteBatch;
    static Effect shadowEffect;
    static RenderTarget2D lightMaskTarget;
    static RenderTarget2D combinedMaskTarget;
    static Point renderSize;
    static bool initialized;

    static float LightPenetrationWorld => Tile.Size.X * 0.5f;

    public static void DrawAndComposite(RenderTarget2D destination)
    {
        ThreadAffinity.AssertMainThread();
        long startTicks = Stopwatch.GetTimestamp();
        if (destination == null)
        {
            ShadowRenderTelemetry.RecordRender(0, 0, 0);
            return;
        }

        ShadowFrameSnapshot frameSnapshot = ShadowSnapshotManager.Snapshot;
        if (frameSnapshot == null || frameSnapshot.Count == 0)
        {
            ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, 0, 0);
            return;
        }

        EnsureInitialized();
        EnsureTargets(new Point(destination.Width, destination.Height));
        if (lightMaskTarget == null || combinedMaskTarget == null || shadowEffect == null)
        {
            ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, 0, 0);
            return;
        }

        EffectParameter matrixParam = shadowEffect.Parameters["u_matrix"];
        EffectParameter lightParam = shadowEffect.Parameters["u_light"];
        EffectParameter penetrationParam = shadowEffect.Parameters["LightPenetration"];
        if (matrixParam == null || lightParam == null || penetrationParam == null)
        {
            ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, 0, 0);
            return;
        }

        Rectangle worldBounds = frameSnapshot.WorldBounds;
        if (worldBounds.Width <= 0 || worldBounds.Height <= 0)
        {
            worldBounds = Camera.Camera.WorldRectangle;
        }

        Matrix worldToClip = Matrix.CreateOrthographicOffCenter(
            worldBounds.Left,
            worldBounds.Right,
            worldBounds.Bottom,
            worldBounds.Top,
            0f,
            1f);

        GraphicsDevice device = Game1.Instance.GraphicsDevice;

        GraphicsManager.SetRenderTarget(combinedMaskTarget);
        GraphicsManager.ClearScreen(Color.Black);

        int renderedLights = 0;
        int drawCalls = 0;
        for (int i = 0; i < frameSnapshot.Count; i++)
        {
            ShadowLightSnapshot light = frameSnapshot.Lights[i];
            if (light.VertexCount <= 0 || light.IndexCount <= 0) continue;

            GraphicsManager.SetRenderTarget(lightMaskTarget);
            GraphicsManager.ClearScreen(Color.Black);

            device.BlendState = maxBlendState;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;

            matrixParam.SetValue(worldToClip);
            lightParam.SetValue(new Vector3(light.Position.X, light.Position.Y, light.RadiusTiles * Tile.Size.X));
            penetrationParam.SetValue(LightPenetrationWorld);

            EffectTechnique technique = shadowEffect.Techniques["SoftShadow"] ?? shadowEffect.CurrentTechnique;
            for (int passIndex = 0; passIndex < technique.Passes.Count; passIndex++)
            {
                EffectPass pass = technique.Passes[passIndex];
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    light.Vertices,
                    0,
                    light.VertexCount,
                    light.Indices,
                    0,
                    light.IndexCount / 3);
                drawCalls++;
            }

            GraphicsManager.SetRenderTarget(combinedMaskTarget);
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                maxBlendState,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone);
            spriteBatch.Draw(lightMaskTarget, new Rectangle(0, 0, renderSize.X, renderSize.Y), Color.White);
            spriteBatch.End();
            drawCalls++;
            renderedLights++;
        }

        if (renderedLights <= 0)
        {
            GraphicsManager.SetRenderTarget(destination);
            ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, drawCalls, 0);
            return;
        }

        GraphicsManager.SetRenderTarget(destination);
        spriteBatch.Begin(
            SpriteSortMode.Immediate,
            darkenByMaskBlendState,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);
        spriteBatch.Draw(combinedMaskTarget, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
        spriteBatch.End();
        drawCalls++;
        ShadowRenderTelemetry.RecordRender(Stopwatch.GetTimestamp() - startTicks, drawCalls, renderedLights);
    }

    static void EnsureInitialized()
    {
        ThreadAffinity.AssertMainThread();
        if (initialized) return;
        initialized = true;

        spriteBatch = GraphicsManager.CreateSpriteBatch();
        shadowEffect = EffectManager.GetEffect(ShadowEffectName);
    }

    static void EnsureTargets(Point size)
    {
        ThreadAffinity.AssertMainThread();
        if (size.X <= 0 || size.Y <= 0) return;
        if (size == renderSize && lightMaskTarget != null && combinedMaskTarget != null) return;

        lightMaskTarget?.Dispose();
        combinedMaskTarget?.Dispose();
        lightMaskTarget = GraphicsManager.CreateRenderTarget(size);
        combinedMaskTarget = GraphicsManager.CreateRenderTarget(size);
        renderSize = size;
    }
}
```

## 10) Draw ordering (where shadows are applied)

### `Project 1/Managers/States/State.cs`
```csharp
public virtual void PrepRender(...)
{
    GraphicsManager.SetRenderTarget(renderTarget);

    spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
    GraphicsManager.ClearScreen(aClearColor);
}

public virtual void CleanRender()
{
    spriteBatch.End();
    GraphicsManager.SetRenderTarget(null);
}
```

### `Project 1/Managers/States/Game.cs`
```csharp
public override void Update()
{
    ...
    RenderSnapshotManager.BuildGameSnapshots();
}

public RenderTarget2D CleanGameDraw()
{
    ...
    PrepRender(Color.White, SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

    DrawList(spriteBatch);

    spriteBatch.End();
    SuperSoftShadowRenderer.DrawAndComposite(renderTarget);
    spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

    CleanRender();
    ...
}

public override RenderTarget2D Draw()
{
    ...
    PrepRender(Color.White, SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp);
    DrawList(spriteBatch);
    spriteBatch.End();

    SuperSoftShadowRenderer.DrawAndComposite(renderTarget);

    // NOTE: non-shader but relevant: this happens AFTER shadows.
    spriteBatch.Begin(SpriteSortMode.Deferred);
    StateManager.DrawGroundSpellEffects(spriteBatch);
    StateManager.DrawGroundTargetPreview(spriteBatch);
    spriteBatch.Draw(plateTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
    spriteBatch.Draw(uITarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

    CleanRender();
    ...
}

void DrawList(SpriteBatch aBatch)
{
    RenderSnapshotManager.DrawGameSnapshots(aBatch);

    // NOTE: non-shader but relevant: particles/floating text are drawn before shadow composite.
    ParticleManager.Draw(aBatch);
    FloatingTextManager.Draw(aBatch);
}
```

### `Project 1/Game1.cs`
```csharp
protected override void Draw(GameTime gameTime)
{
    ThreadAffinity.AssertMainThread();
    Camera.Camera.BeginMainThreadRenderFrame();
    try
    {
        GraphicsDevice.Clear(Color.HotPink);

        EffectManager.EffectDraw();
        StateManager.Draw();

        base.Draw(gameTime);
    }
    finally
    {
        Camera.Camera.EndMainThreadRenderFrame();
    }
}
```

### `Project 1/Managers/States/StateManager.cs`
```csharp
public static void Draw()
{
    ThreadAffinity.AssertMainThread();
    if (pendingRedrawGame && ThreadAffinity.IsMainThread)
    {
        pendingRedrawGame = false;
        finalGameFrame = game.Draw();
    }
    RenderTarget2D target = currentState.Draw();

    finalBatch.Begin();
    finalBatch.Draw(target, renderTargetPosition, Color.White);
    finalBatch.End();
}
```

## 11) Render target helpers used by pipeline

### `Project 1/Managers/GraphicsManager.cs`
```csharp
public static SpriteBatch CreateSpriteBatch()
{
    ThreadAffinity.AssertMainThread();
    return new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
}

public static RenderTarget2D CreateRenderTarget(Point aSize)
{
    ThreadAffinity.AssertMainThread();
    return new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, aSize.X, aSize.Y);
}

public static void SetRenderTarget(RenderTarget2D aRenderTarget)
{
    ThreadAffinity.AssertMainThread();
    graphicsDeviceManager.GraphicsDevice.SetRenderTarget(aRenderTarget);
}
```

## 12) Shader source itself

### `Project 1/Content/Effects/SuperSoftShadows.fx`
```hlsl
#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

cbuffer LightParams : register(b0)
{
    float4x4 u_matrix; // world-view-projection (or light-space → clip)
    float3 u_light; // xy = position, z = radius
    float LightPenetration; // ~0.01 recommended
}

float2x2 Mat2Cols(float2 c0, float2 c1)
{
    return float2x2(
        c0.x, c1.x,
        c0.y, c1.y
    );
}

float2x2 Adjugate(float2x2 m)
{
    return float2x2(
         m._22, -m._12,
        -m._21, m._11
    );
}

float2 MulM2x2(float2x2 m, float2 v)
{
    return float2(
        m._11 * v.x + m._12 * v.y,
        m._21 * v.x + m._22 * v.y
    );
}

struct VSInput
{
    float4 Segment : POSITION0; // (b.x, b.y, a.x, a.y)
    float2 ShadowCoord : TEXCOORD0; // (x = endpoint selector, y = near/far)
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Penumbras : TEXCOORD0;
    float3 Edges : TEXCOORD1;
    float3 ProjPos : TEXCOORD2;
    float4 Endpoints : TEXCOORD3;
};

VSOutput VS_SoftShadow(VSInput input)
{
    VSOutput o;

    float2 endpoint_a = input.Segment.zw;
    float2 endpoint_b = input.Segment.xy;
    float2 endpoint = lerp(endpoint_a, endpoint_b, input.ShadowCoord.x);

    float light_radius = u_light.z;
    float2 light_pos = u_light.xy;

    float2 delta_a = endpoint_a - light_pos;
    float2 delta_b = endpoint_b - light_pos;
    float2 delta = endpoint - light_pos;

    float2 offset_a = float2(-light_radius, light_radius) * normalize(delta_a).yx;
    float2 offset_b = float2(light_radius, -light_radius) * normalize(delta_b).yx;
    float2 offset = lerp(offset_a, offset_b, input.ShadowCoord.x);

    float w = input.ShadowCoord.y;
    float2 proj_xy = lerp(delta - offset, endpoint - light_pos, w);

    float4 clipPos = mul(float4(proj_xy + light_pos, 0.0f, 1.0f), u_matrix);
    o.Position = clipPos;

    float2x2 mA = Mat2Cols(offset_a, -delta_a);
    float2x2 mB = Mat2Cols(-offset_b, delta_b);

    float2 vA = delta - lerp(offset, delta_a, w);
    float2 vB = delta - lerp(offset, delta_b, w);

    float2 penumbra_a = MulM2x2(Adjugate(mA), vA);
    float2 penumbra_b = MulM2x2(Adjugate(mB), vB);

    if (light_radius > 0.0f)
    {
        o.Penumbras = float4(penumbra_a, penumbra_b);
    }
    else
    {
        o.Penumbras = float4(0.0f, 1.0f, 0.0f, 1.0f);
    }

    float2 seg_delta = endpoint_b - endpoint_a;
    float2 seg_normal = seg_delta.yx * float2(-1.0f, 1.0f);

    o.Edges.z = dot(seg_normal, delta - offset) * (1.0f - w);

    float2x2 mPen = Mat2Cols(seg_delta, delta_a + delta_b);
    float2 edges_xy = -MulM2x2(Adjugate(mPen), (delta - offset * (1.0f - w)));
    edges_xy.y *= 2.0f;

    o.Edges.xy = edges_xy;

    float lp = LightPenetration;
    if (lp <= 0.0f)
        lp = 0.01f;

    o.ProjPos = float3(proj_xy, max(w * lp, 1e-4f));
    o.Endpoints = float4(endpoint_a, endpoint_b) / lp;

    return o;
}

float4 PS_SoftShadow(VSOutput input) : SV_Target
{
    float2 grad = input.Penumbras.xz / input.Penumbras.yw;

    float2 pen = smoothstep(-1.0f, 1.0f, grad);

    float2 mask = step(float2(0.0f, 0.0f), input.Penumbras.yw);
    float penumbra = dot(pen, mask);

    penumbra -= 1.0f / 64.0f;

    float intersection_t =
        clamp(input.Edges.x / abs(input.Edges.y), -0.5f, 0.5f);

    float2 intersection_point =
        (0.5f - intersection_t) * input.Endpoints.xy +
        (0.5f + intersection_t) * input.Endpoints.zw;

    float2 pixel_pos = input.ProjPos.xy / max(abs(input.ProjPos.z), 1e-4f);

    float2 penetration_delta = intersection_point - pixel_pos;

    float bleed = min(dot(penetration_delta, penetration_delta), 1.0f);

    float shadow = bleed * (1.0f - penumbra) * step(input.Edges.z, 0.0f);
    shadow = saturate(shadow);

    return float4(shadow, shadow, shadow, 1.0f);
}

technique SoftShadow
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VS_SoftShadow();
        PixelShader = compile PS_SHADERMODEL PS_SoftShadow();
    }
}
```
