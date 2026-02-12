using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Doodads
{
    internal static class DoodadManager
    {
        static List<Doodad> doodads;
        static readonly RenderCache<WorldObjectRenderSnapshot> renderDoodads = new RenderCache<WorldObjectRenderSnapshot>();
        static readonly HashSet<int> knownDoodadIds = new HashSet<int>();
        static readonly HashSet<int> currentDoodadIds = new HashSet<int>();
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            doodads = new List<Doodad>();

            doodads.Add(new Chest(new Camera.WorldSpace(600, 600))); //DEBUG
        }


        public static void Update()
        {
            ThreadAffinity.AssertSimThread();
            for (int i = 0; i < doodads.Count; i++)
            {
                doodads[i].Update();
            }
        }

        public static bool TryGetDoodadAt(WorldSpace worldPos, out Doodad doodad)
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

        public static bool TryGetDoodadByRenderId(int renderId, out Doodad doodad)
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

        internal static void DrawSnapshots(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            // Snapshot-only draw path. Do not read live sim doodad list here.
            renderDoodads.ApplyUpdates();
            foreach (WorldObjectRenderSnapshot snapshot in renderDoodads.Values)
            {
                snapshot.Draw(aBatch);
            }
        }

        internal static void BuildRenderSnapshot()
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

        static void PublishRemovals()
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
