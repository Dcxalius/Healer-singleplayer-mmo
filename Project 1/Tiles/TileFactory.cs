using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Project_1.DebugTools;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Tiles
{
    internal static class TileFactory
    {
        static TileData[] tileData;

        public static void Init(ContentManager aContentManager)
        {

            ImportData(aContentManager.RootDirectory, aContentManager);
                //DEBUG:
            TileManager.GenerateTiles(new Point(0), TileManager.debugSize);

        }

        public static TileData GetTileData(int aid) => tileData[aid];
        public static TileData GetTileData(string aName) => tileData.Where(tile =>  tile.Name == aName).First();

        static void ImportData(string aPathToData, ContentManager aContentManager)
        {
            List<TileData> tiles = new List<TileData>();
            string[] dataAsString = System.IO.File.ReadAllLines(aPathToData + "\\Data\\TileData.json");

            for (int i = 0; i < dataAsString.Length; i++)
            {
                TileData data = JsonConvert.DeserializeObject<TileData>(dataAsString[i]);
                tiles.Add(data);
            }
            tiles.Sort();

            tileData = tiles.ToArray();

            Debug.Assert(tileData[tileData.Count() - 1].ID == tileData.Count() - 1); 
            //TODO: Add checks to insure no missing gaps;
        }
    }
}
