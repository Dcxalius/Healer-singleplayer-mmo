using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Rendering;
using Project_1.Textures;
using System;
using System.Collections.Generic;

namespace Project_1.Tiles
{
    internal static class WorldBlockRenderer
    {
        enum CubeFace
        {
            Up,
            Down,
            North,
            West,
            South,
            East
        }

        sealed class TileRenderMaterial
        {
            public TileRenderMaterial(Texture2D aTexture, Color[] aFaceTints)
            {
                Texture = aTexture;
                FaceTints = aFaceTints ?? Array.Empty<Color>();
            }

            public Texture2D Texture { get; }
            public Color[] FaceTints { get; }
        }

        sealed class TileBatchBuilder
        {
            public TileBatchBuilder(TileRenderMaterial aMaterial)
            {
                Material = aMaterial;
            }

            public TileRenderMaterial Material { get; }
            public List<VertexPositionColorTexture> Vertices { get; } = new List<VertexPositionColorTexture>(1024);
            public List<short> Indices { get; } = new List<short>(1536);

            public void Clear()
            {
                Vertices.Clear();
                Indices.Clear();
            }
        }

        public static int RenderDistanceInChunks
        {
            get => renderDistanceInChunks;
            set => renderDistanceInChunks = Math.Max(0, value);
        }

        public static Camera3D CurrentCamera => worldCamera;
        public static float CameraYawRadians => cameraYawRadians;
        public static float CameraZoom => cameraZoom;
        public static Vector2 CameraGroundForward => BuildGroundForward(cameraYawRadians);
        public static Vector2 CameraGroundRight
        {
            get
            {
                Vector2 forward = CameraGroundForward;
                return new Vector2(-forward.Y, forward.X);
            }
        }

        const int MaxVerticesPerDraw = 30000;
        const float BaseCameraOrbitHeight = 18f;
        const float BaseCameraOrbitRadius = 19.79899f;
        const float MinCameraZoom = 0.55f;
        const float MaxCameraZoom = 1.75f;
        const float CameraZoomStep = 0.08f;
        static readonly string[] compassNames = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        static readonly Vector2[] quadUvs =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };

        static readonly Dictionary<int, TileRenderMaterial> materialsByTileId = new Dictionary<int, TileRenderMaterial>();
        static readonly Dictionary<int, TileBatchBuilder> batchesByTileId = new Dictionary<int, TileBatchBuilder>();
        static readonly CubeFace[] allFaces =
        {
            CubeFace.Up,
            CubeFace.Down,
            CubeFace.North,
            CubeFace.West,
            CubeFace.South,
            CubeFace.East
        };
        static readonly RasterizerState rasterizerState = new RasterizerState { CullMode = CullMode.None };
        static readonly BlendState blendState = BlendState.AlphaBlend;
        static readonly SamplerState samplerState = SamplerState.LinearClamp;
        static readonly DepthStencilState depthStencilState = DepthStencilState.Default;
        static readonly Matrix identityWorld = Matrix.Identity;
        static Camera3D worldCamera;
        static BoundingFrustum worldFrustum;
        static int renderDistanceInChunks = 1;
        static volatile float cameraYawRadians = MathHelper.PiOver4;
        static volatile float cameraZoom = 1f;

        public static void PrepareFrame()
        {
            ThreadAffinity.AssertMainThread();
            EnsureCamera();
            ConfigureCamera();
            worldFrustum = new BoundingFrustum(worldCamera.ViewProjection);
        }

        public static void RotateCameraYaw(float aDeltaRadians)
        {
            cameraYawRadians = NormalizeAngle(cameraYawRadians + aDeltaRadians);
        }

        public static void AdjustCameraZoom(int aDirectionAndSteps)
        {
            if (aDirectionAndSteps == 0) return;

            float updatedZoom = cameraZoom + aDirectionAndSteps * CameraZoomStep;
            cameraZoom = MathHelper.Clamp(updatedZoom, MinCameraZoom, MaxCameraZoom);
        }

        public static WorldSpace3D ResolvePreviewWorldPosition(WorldSpace aFeetPosition)
        {
            return new WorldSpace3D(
                aFeetPosition.X / Tile.Size.X,
                ResolveSurfaceHeight(aFeetPosition),
                aFeetPosition.Y / Tile.Size.Y);
        }

        public static string ResolveCompassName(Vector2 aDirection)
        {
            if (aDirection.LengthSquared() <= float.Epsilon) return "N";

            Vector2 normalized = Vector2.Normalize(aDirection);
            float degrees = MathHelper.ToDegrees(MathF.Atan2(normalized.X, -normalized.Y));
            if (degrees < 0f) degrees += 360f;

            int sector = (int)MathF.Round(degrees / 45f) % compassNames.Length;
            return compassNames[sector];
        }

        public static Camera3D CreatePreviewCamera(WorldSpace aCameraCentre)
        {
            AbsoluteScreenPosition viewportSize = Camera.Camera.WindowSize;
            float centreX = aCameraCentre.X / Tile.Size.X;
            float centreZ = aCameraCentre.Y / Tile.Size.Y;
            float yaw = cameraYawRadians;
            float targetY = ResolvePreviewTargetHeight(aCameraCentre);
            float orbitRadius = BaseCameraOrbitRadius * cameraZoom;
            float orbitHeight = BaseCameraOrbitHeight * cameraZoom;

            WorldSpace3D cameraPosition = new WorldSpace3D(
                centreX + MathF.Sin(yaw) * orbitRadius,
                targetY + orbitHeight,
                centreZ + MathF.Cos(yaw) * orbitRadius);
            WorldSpace3D cameraTarget = new WorldSpace3D(centreX, targetY, centreZ);
            Camera3D previewCamera = new Camera3D(viewportSize, cameraPosition, cameraTarget);
            previewCamera.SetLens(MathHelper.ToRadians(38f), 0.1f, 2048f);
            return previewCamera;
        }

        public static void DrawBlocks(Point aChunkPosition, ChunkBlockRenderSnapshot[] aBlocks)
        {
            ThreadAffinity.AssertMainThread();
            if (aBlocks == null || aBlocks.Length == 0) return;

            if (worldCamera == null || worldFrustum == null)
            {
                PrepareFrame();
            }

            ClearBatches();

            for (int i = 0; i < aBlocks.Length; i++)
            {
                ChunkBlockRenderSnapshot block = aBlocks[i];
                if (!ShouldDrawBlock(block.WorldPosition)) continue;

                TileBatchBuilder batch = GetOrCreateBatch(block.TileId);
                TileRenderMaterial material = batch.Material;
                for (int faceIndex = 0; faceIndex < allFaces.Length; faceIndex++)
                {
                    CubeFace face = allFaces[faceIndex];
                    if (!IsFaceExposed(block.ExposedFaces, face)) continue;
                    if (!IsFaceVisibleFromCamera(block.WorldPosition, face)) continue;
                    EnsureBatchCapacity(batch);
                    AppendFace(batch, block.WorldPosition.ToVector3(), face, material.FaceTints[(int)face]);
                }
            }

            FlushAllBatches();
        }

        public static bool ShouldDrawChunk(Point aChunkPosition)
        {
            Point playerChunk = ResolvePlayerChunkPosition();
            int radius = RenderDistanceInChunks;
            bool insideRadius =
                aChunkPosition.X >= playerChunk.X - radius &&
                aChunkPosition.X <= playerChunk.X + radius &&
                aChunkPosition.Y >= playerChunk.Y - radius &&
                aChunkPosition.Y <= playerChunk.Y + radius;
            if (!insideRadius) return false;

            if (worldFrustum == null)
            {
                PrepareFrame();
            }

            BoundingBox chunkBounds = BuildChunkBounds(aChunkPosition);
            return worldFrustum.Contains(chunkBounds) != ContainmentType.Disjoint;
        }

        static void EnsureCamera()
        {
            if (worldCamera != null) return;
            worldCamera = new Camera3D(new AbsoluteScreenPosition(1, 1));
        }

        static void ConfigureCamera()
        {
            Viewport viewport = GraphicsManager.GraphicsDevice.Viewport;
            worldCamera.Resize(new AbsoluteScreenPosition(viewport.Width, viewport.Height));
            Camera3D previewCamera = CreatePreviewCamera(Camera.Camera.CentreInWorldSpace);
            worldCamera.MoveTo(previewCamera.Position);
            worldCamera.LookAt(previewCamera.Target);
            worldCamera.SetUp(previewCamera.Up);
            worldCamera.SetLens(previewCamera.FieldOfViewRadians, previewCamera.NearPlane, previewCamera.FarPlane);
        }

        static Vector2 BuildGroundForward(float aYawRadians)
        {
            Vector2 forward = new Vector2(-MathF.Sin(aYawRadians), -MathF.Cos(aYawRadians));
            if (forward.LengthSquared() <= float.Epsilon)
            {
                return -Vector2.UnitY;
            }

            forward.Normalize();
            return forward;
        }

        static float NormalizeAngle(float aAngleRadians)
        {
            while (aAngleRadians <= -MathF.PI) aAngleRadians += MathHelper.TwoPi;
            while (aAngleRadians > MathF.PI) aAngleRadians -= MathHelper.TwoPi;
            return aAngleRadians;
        }

        static float ResolvePreviewTargetHeight(WorldSpace aCameraCentre)
        {
            WorldSpace playerFeet = ObjectManager.Player?.FeetPosition ?? aCameraCentre;
            return ResolveSurfaceHeight(playerFeet);
        }

        static float ResolveSurfaceHeight(WorldSpace aFeetPosition)
        {
            Chunk chunk = TileManager.GetChunk(aFeetPosition);
            if (chunk == null) return 0f;

            Point gridPosition = TileManager.GetGridPos(aFeetPosition);
            int localX = PositiveModulo(gridPosition.X, Chunk.ChunkSize.X);
            int localY = PositiveModulo(gridPosition.Y, Chunk.ChunkSize.Y);

            for (int z = Chunk.ChunkHeight - 1; z >= 0; z--)
            {
                if (chunk.GetBlock(localX, localY, z) != null)
                {
                    return z + 1f;
                }
            }

            return 0f;
        }

        static int PositiveModulo(int aValue, int aDivisor)
        {
            int result = aValue % aDivisor;
            return result < 0 ? result + aDivisor : result;
        }

        static Point ResolvePlayerChunkPosition()
        {
            WorldSpace playerPosition = ObjectManager.Player != null
                ? ObjectManager.Player.FeetPosition
                : WorldSpace.Zero;

            return new Point(
                (int)MathF.Floor(playerPosition.X / Tile.Size.X / Chunk.ChunkSize.X),
                (int)MathF.Floor(playerPosition.Y / Tile.Size.Y / Chunk.ChunkSize.Y));
        }

        static bool ShouldDrawBlock(WorldSpace3D aWorldPosition)
        {
            BoundingBox blockBounds = BuildBlockBounds(aWorldPosition);
            return worldFrustum.Contains(blockBounds) != ContainmentType.Disjoint;
        }

        static bool IsFaceExposed(BlockFaceMask aExposedFaces, CubeFace aFace)
        {
            return (aExposedFaces & ToFaceMask(aFace)) != 0;
        }

        static bool IsFaceVisibleFromCamera(WorldSpace3D aBlockCenter, CubeFace aFace)
        {
            Vector3 normal = GetFaceNormal(aFace);
            Vector3 faceCenter = aBlockCenter.ToVector3() + normal * 0.5f;
            Vector3 toCamera = worldCamera.Position.ToVector3() - faceCenter;
            return Vector3.Dot(normal, toCamera) >= -0.001f;
        }

        static BoundingBox BuildChunkBounds(Point aChunkPosition)
        {
            Vector3 min = new Vector3(
                aChunkPosition.X * Chunk.ChunkSize.X,
                0f,
                aChunkPosition.Y * Chunk.ChunkSize.Y);
            Vector3 max = new Vector3(
                (aChunkPosition.X + 1) * Chunk.ChunkSize.X,
                Chunk.ChunkHeight,
                (aChunkPosition.Y + 1) * Chunk.ChunkSize.Y);
            return new BoundingBox(min, max);
        }

        static BoundingBox BuildBlockBounds(WorldSpace3D aWorldPosition)
        {
            Vector3 center = aWorldPosition.ToVector3();
            Vector3 halfExtent = new Vector3(0.5f);
            return new BoundingBox(center - halfExtent, center + halfExtent);
        }

        static TileBatchBuilder GetOrCreateBatch(int aTileId)
        {
            if (batchesByTileId.TryGetValue(aTileId, out TileBatchBuilder existing)) return existing;

            TileBatchBuilder created = new TileBatchBuilder(GetOrCreateMaterial(aTileId));
            batchesByTileId[aTileId] = created;
            return created;
        }

        static TileRenderMaterial GetOrCreateMaterial(int aTileId)
        {
            if (materialsByTileId.TryGetValue(aTileId, out TileRenderMaterial existing)) return existing;

            TileData tileData = TileFactory.GetTileData(aTileId);
            GfxPath texturePath = new GfxPath(GfxType.Tile, tileData.Name);
            TileRenderMaterial created = new TileRenderMaterial(
                TextureManager.GetTexture(texturePath),
                new[]
                {
                    Color.White,
                    new Color(190, 190, 190),
                    new Color(230, 230, 230),
                    new Color(215, 215, 215),
                    new Color(245, 245, 245),
                    new Color(225, 225, 225)
                });
            materialsByTileId[aTileId] = created;
            return created;
        }

        static void ClearBatches()
        {
            foreach (TileBatchBuilder batch in batchesByTileId.Values)
            {
                batch.Clear();
            }
        }

        static void EnsureBatchCapacity(TileBatchBuilder aBatch)
        {
            if (aBatch.Vertices.Count <= MaxVerticesPerDraw - 4) return;
            FlushBatch(aBatch);
        }

        static void AppendFace(TileBatchBuilder aBatch, Vector3 aCenter, CubeFace aFace, Color aTint)
        {
            Vector3[] corners = BuildFaceCorners(aCenter, aFace);
            short baseIndex = (short)aBatch.Vertices.Count;
            aBatch.Vertices.Add(new VertexPositionColorTexture(corners[0], aTint, quadUvs[0]));
            aBatch.Vertices.Add(new VertexPositionColorTexture(corners[1], aTint, quadUvs[1]));
            aBatch.Vertices.Add(new VertexPositionColorTexture(corners[2], aTint, quadUvs[2]));
            aBatch.Vertices.Add(new VertexPositionColorTexture(corners[3], aTint, quadUvs[3]));
            aBatch.Indices.Add(baseIndex);
            aBatch.Indices.Add((short)(baseIndex + 1));
            aBatch.Indices.Add((short)(baseIndex + 2));
            aBatch.Indices.Add((short)(baseIndex + 2));
            aBatch.Indices.Add((short)(baseIndex + 1));
            aBatch.Indices.Add((short)(baseIndex + 3));
        }

        static BlockFaceMask ToFaceMask(CubeFace aFace)
        {
            return aFace switch
            {
                CubeFace.Up => BlockFaceMask.Up,
                CubeFace.Down => BlockFaceMask.Down,
                CubeFace.North => BlockFaceMask.North,
                CubeFace.West => BlockFaceMask.West,
                CubeFace.South => BlockFaceMask.South,
                _ => BlockFaceMask.East
            };
        }

        static Vector3 GetFaceNormal(CubeFace aFace)
        {
            return aFace switch
            {
                CubeFace.Up => Vector3.Up,
                CubeFace.Down => Vector3.Down,
                CubeFace.North => Vector3.Forward,
                CubeFace.West => Vector3.Left,
                CubeFace.South => Vector3.Backward,
                _ => Vector3.Right
            };
        }

        static Vector3[] BuildFaceCorners(Vector3 aCenter, CubeFace aFace)
        {
            const float half = 0.5f;
            return aFace switch
            {
                CubeFace.Up => new[]
                {
                    aCenter + new Vector3(-half, half, -half),
                    aCenter + new Vector3(half, half, -half),
                    aCenter + new Vector3(-half, half, half),
                    aCenter + new Vector3(half, half, half)
                },
                CubeFace.Down => new[]
                {
                    aCenter + new Vector3(-half, -half, half),
                    aCenter + new Vector3(half, -half, half),
                    aCenter + new Vector3(-half, -half, -half),
                    aCenter + new Vector3(half, -half, -half)
                },
                CubeFace.South => new[]
                {
                    aCenter + new Vector3(-half, half, half),
                    aCenter + new Vector3(half, half, half),
                    aCenter + new Vector3(-half, -half, half),
                    aCenter + new Vector3(half, -half, half)
                },
                CubeFace.North => new[]
                {
                    aCenter + new Vector3(half, half, -half),
                    aCenter + new Vector3(-half, half, -half),
                    aCenter + new Vector3(half, -half, -half),
                    aCenter + new Vector3(-half, -half, -half)
                },
                CubeFace.West => new[]
                {
                    aCenter + new Vector3(-half, half, -half),
                    aCenter + new Vector3(-half, half, half),
                    aCenter + new Vector3(-half, -half, -half),
                    aCenter + new Vector3(-half, -half, half)
                },
                _ => new[]
                {
                    aCenter + new Vector3(half, half, half),
                    aCenter + new Vector3(half, half, -half),
                    aCenter + new Vector3(half, -half, half),
                    aCenter + new Vector3(half, -half, -half)
                }
            };
        }

        static void FlushAllBatches()
        {
            foreach (TileBatchBuilder batch in batchesByTileId.Values)
            {
                FlushBatch(batch);
            }
        }

        static void FlushBatch(TileBatchBuilder aBatch)
        {
            if (aBatch.Vertices.Count == 0 || aBatch.Indices.Count == 0) return;

            Effect effect = EffectManager.GetEffect("ModelRender");
            GraphicsDevice device = GraphicsManager.GraphicsDevice;
            device.BlendState = blendState;
            device.DepthStencilState = depthStencilState;
            device.RasterizerState = rasterizerState;
            device.SamplerStates[0] = samplerState;

            effect.Parameters["World"]?.SetValue(identityWorld);
            effect.Parameters["View"]?.SetValue(worldCamera.View);
            effect.Parameters["Projection"]?.SetValue(worldCamera.Projection);
            effect.Parameters["TintColor"]?.SetValue(Color.White.ToVector4());
            effect.Parameters["DiffuseTexture"]?.SetValue(aBatch.Material.Texture);

            EffectTechnique technique = effect.Techniques["ModelTextured"] ?? effect.CurrentTechnique;
            for (int passIndex = 0; passIndex < technique.Passes.Count; passIndex++)
            {
                EffectPass pass = technique.Passes[passIndex];
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    aBatch.Vertices.ToArray(),
                    0,
                    aBatch.Vertices.Count,
                    aBatch.Indices.ToArray(),
                    0,
                    aBatch.Indices.Count / 3);
            }

            aBatch.Clear();
        }
    }
}
