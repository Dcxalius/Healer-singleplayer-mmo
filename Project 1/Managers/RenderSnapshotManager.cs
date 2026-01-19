using Project_1.GameObjects;
using Project_1.GameObjects.Doodads;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Spawners;
using Project_1.Particles;
using Project_1.Tiles;

namespace Project_1.Managers
{
    internal static class RenderSnapshotManager
    {
        public static void BuildGameSnapshots()
        {
            ThreadAffinity.AssertSimThread();
            TileManager.BuildRenderSnapshot();
            ObjectManager.BuildRenderSnapshot();
            ProjectileManager.BuildRenderSnapshot();
            DoodadManager.BuildRenderSnapshot();
            CorpseManager.BuildRenderSnapshot();
            SpawnerManager.BuildRenderSnapshot();
            ParticleManager.BuildRenderSnapshot();
            FloatingTextManager.BuildRenderSnapshot();
        }
    }
}
