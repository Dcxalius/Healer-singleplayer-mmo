using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Tiles;
using System.Collections.Generic;

namespace Project_1.GameObjects.Doodads
{
    internal sealed class DoodadManager
    {
        readonly List<Doodad> doodads = new List<Doodad>();
        readonly RenderCache<WorldObjectRenderSnapshot> renderDoodads = new RenderCache<WorldObjectRenderSnapshot>();
        readonly HashSet<int> knownDoodadIds = new HashSet<int>();
        readonly HashSet<int> currentDoodadIds = new HashSet<int>();
        bool structuresApplied;

        public void EnsureStructureDoodads(Chunk chunk)
        {
            if (chunk == null) return;
            if (structuresApplied) return;
            structuresApplied = true;

            StructureSpawnSystem.DoodadSpawn[] spawns = StructureSpawnSystem.GetDoodadSpawns(chunk.Id);
            for (int i = 0; i < spawns.Length; i++)
            {
                WorldSpace worldPos = chunk.Position + new WorldSpace(
                    (spawns[i].LocalTile.X + 0.5f) * Tile.Size.X,
                    (spawns[i].LocalTile.Y + 0.5f) * Tile.Size.Y);

                switch (spawns[i].Type)
                {
                    case StructureSpawnSystem.StructureDoodadType.CookingFire:
                        doodads.Add(new CookingFire(worldPos));
                        break;
                }
            }
        }


        public void Update()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < doodads.Count; i++)
            {
                doodads[i].Update();
            }
        }

        public bool TryGetDoodadAt(WorldSpace worldPos, out Doodad doodad)
        {
            ThreadAffinity.AssertSimThread();
            doodad = null;
            for (int i = 0; i < doodads.Count; i++)
            {
                if (!doodads[i].CanInteract(worldPos)) continue;
                doodad = doodads[i];
                return true;
            }
            return false;
        }

        public bool TryGetDoodadByRenderId(int renderId, out Doodad doodad)
        {
            ThreadAffinity.AssertSimThread();
            doodad = null;
            if (renderId <= 0) return false;
            for (int i = 0; i < doodads.Count; i++)
            {
                if (doodads[i].RenderId != renderId) continue;
                doodad = doodads[i];
                return true;
            }

            return false;
        }

        internal void DrawSnapshots(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            renderDoodads.ApplyUpdates();
            foreach (WorldObjectRenderSnapshot snapshot in renderDoodads.Values)
            {
                snapshot.Draw(aBatch);
            }
        }

        internal void BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            currentDoodadIds.Clear();
            for (int i = 0; i < doodads.Count; i++)
            {
                WorldObjectRenderSnapshot snapshot = doodads[i].BuildRenderSnapshot();
                renderDoodads.EnqueueUpdate(snapshot);
                currentDoodadIds.Add(snapshot.RenderId);
            }
            PublishRemovals();
        }

        void PublishRemovals()
        {
            foreach (int id in knownDoodadIds)
            {
                if (!currentDoodadIds.Contains(id))
                {
                    renderDoodads.EnqueueRemove(id);
                }
            }
            knownDoodadIds.Clear();
            foreach (int id in currentDoodadIds)
            {
                knownDoodadIds.Add(id);
            }
        }
    }
}
