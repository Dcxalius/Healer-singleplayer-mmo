using Project_1.Managers;
using Project_1.Managers.Saves;

namespace Project_1.Tiles
{
    internal static partial class TileManager
    {
        public static void SaveData(Save aSave)
        {
            ThreadAffinity.AssertSimThread();

            foreach (Chunk chunk in chunks.Values)
            {
                SaveManager.ExportData(aSave.Tiles + "\\" + chunk.Id + ".tilemap", chunk);
            }
        }
    }
}
