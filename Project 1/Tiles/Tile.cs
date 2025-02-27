using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using Project_1.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.Camera;
using Project_1.Managers;
using Newtonsoft.Json;

namespace Project_1.Tiles
{
    internal class Tile
    {
        readonly static Point tileSize = new Point(32);
        [JsonProperty("id")]
        public int ID => tileData.ID;
        
        [JsonIgnore]
        public string Name => tileData.Name;
        [JsonIgnore]
        public WorldSpace Position { get => position; protected set => position = value; }
        WorldSpace position;
        [JsonIgnore]
        public WorldSpace Centre { get => position + new WorldSpace(tileSize.ToVector2()) / 2; }
        [JsonIgnore]
        public virtual Rectangle WorldRectangle { get => new Rectangle(position.ToPoint(), tileSize); }

        [JsonIgnore]
        public bool Walkable => tileData.Walkable;
        [JsonIgnore]
        public float DragCoeficient => tileData.DragCoeficient;

        [JsonIgnore]
        public bool Transparent
        {
            get
            {
                //DebugManager.debugShapes.Add(new DebugTools.DebugSquare(new Rectangle(Position.ToPoint(), tileSize)));
                return tileData.Transparent;
            }
        }

        public bool IsAdjacent(Tile aPossibleNeighbour)
        {
            if (Math.Abs(aPossibleNeighbour.GridPos.X-GridPos.X) > 1 || Math.Abs(aPossibleNeighbour.GridPos.Y-GridPos.Y) > 1) return false;
            if (!(aPossibleNeighbour.GridPos.X == GridPos.X || aPossibleNeighbour.GridPos.Y == GridPos.Y)) return false;
            return true;
        }

        [JsonIgnore]
        public Point GridPos => tilePos;
        Point tilePos;

        Text xText;
        Text yText;

        UITexture debugTexture;
        TileData tileData;

        Textures.Texture gfx;
        //bool 

        public Tile(TileData aTileData, Point aPos, Point aTilePos)
        {
            gfx = new Textures.RandomlyGeneratedTexture(true, tileSize, new GfxPath(GfxType.Tile, aTileData.Name));
            debugTexture = new UITexture(new GfxPath(GfxType.Debug, "Debug"), Color.White);
            tileData = aTileData;
            tilePos = aTilePos;
            if (DebugManager.Mode(DebugMode.TileCoords))
            {
                xText = new Text("Gloryse", tilePos.X.ToString(), Color.Black);
                yText = new Text("Gloryse", tilePos.Y.ToString(), Color.Yellow);
            }
            Position = new WorldSpace(aPos);
        }

        public void AddDebugSquare()
        {
            DebugManager.AddDebugShape(new DebugTools.DebugSquare(new Rectangle(Position.ToPoint(), tileSize)));
        }

        public void Draw(SpriteBatch aBatch)
        {
            //base.Draw(aBatch); Skip draw as a gameobject to make all tiles appear in the background
            gfx.Draw(aBatch, Position.ToAbsoltueScreenPosition(), Color.White, 0);

            //xText.LeftAllignedDraw(aBatch, new WorldSpace(Position - Size.ToVector2() / 2).ToAbsoltueScreenPosition());
            if (DebugManager.Mode(DebugMode.TileCoords))
            {
                xText.TopLeftDraw(aBatch, new WorldSpace(Position).ToAbsoltueScreenPosition());
                yText.TopLeftDraw(aBatch, new WorldSpace(Position + new WorldSpace(xText.Offset.X, 0)).ToAbsoltueScreenPosition());
            }
            //Camera.Camera.WorldPosToCameraSpace(Position), 0); 
        }
    }
}
