using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.Spawners;
using Project_1.Tiles;
using Project_1.GameObjects.Entities.Friendlies.Players;

namespace Project_1.Managers
{
    internal static class RenderSnapshotManager
    {
        public static void BuildGameSnapshots()
        {
            ThreadAffinity.AssertSimThread();
            TileManager.BuildRenderSnapshot();
            ObjectManager.BuildRenderSnapshot();
            ObjectManager.PartyLightSnapshot lightSnapshot = ObjectManager.RenderLightSnapshot;
            WorldSpace lightOrigin = lightSnapshot.Positions.Length > 0 ? lightSnapshot.Positions[0] : WorldSpace.Zero;
            TileRenderCache.BuildTransparencySnapshot(lightOrigin);
            Camera.Camera.BuildMinimapSnapshot();
            ProjectileManager.BuildRenderSnapshot();
            DoodadManager.BuildRenderSnapshot();
            CorpseManager.BuildRenderSnapshot();
            SpawnerManager.BuildRenderSnapshot();
            MinimapSnapshotManager.BuildSnapshot();
        }
    }
}
