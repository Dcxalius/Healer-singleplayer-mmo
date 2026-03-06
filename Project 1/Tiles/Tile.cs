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
        public static readonly Point Size = new Point(32, 32);
        [JsonProperty("id")]
        public int ID => tileData.ID;
        
        [JsonIgnore]
        public string Name => tileData.Name;
        [JsonIgnore]
        public WorldSpace Position { get => position; protected set => position = value; }
        WorldSpace position;
        [JsonIgnore]
        public WorldSpace Centre { get => position + new WorldSpace(Size.ToVector2()) / 2; }
        [JsonIgnore]
        public virtual Rectangle WorldRectangle { get => new Rectangle(position.ToPoint(), Size); }

        [JsonIgnore]
        public bool Walkable => tileData.Walkable;
        [JsonIgnore]
        public float DragCoeficient => tileData.DragCoeficient;

        [JsonIgnore]
        public bool Transparent
        {
            get
            {
                //DebugManager.debugShapes.Add(new DebugTools.DebugSquare(new Rectangle(Position.ToPoint(), Size)));
                return tileData.Transparent;
            }
        }

        [JsonIgnore]
        public Color MinimapColor => tileData.AvgColor;

        public bool IsAdjacent(Tile aPossibleNeighbour)
        {
            if (Math.Abs(aPossibleNeighbour.GridPos.X-GridPos.X) > 1 || Math.Abs(aPossibleNeighbour.GridPos.Y-GridPos.Y) > 1) return false;
            if (!(aPossibleNeighbour.GridPos.X == GridPos.X || aPossibleNeighbour.GridPos.Y == GridPos.Y)) return false;
            return true;
        }

        [JsonIgnore]
        public Point GridPos => tilePos;
        Point tilePos;
        Point textureOffset;
        bool debugTextInitialized;

        Text xText;
        Text yText;

        TileData tileData;
        //bool 

        public Tile(TileData aTileData, Point aPos, Point aTilePos)
        {
            tileData = aTileData;
            tilePos = aTilePos;
            textureOffset = tileData.GetRandomTextureOffset(new Point(aPos.X / Size.X, aPos.Y / Size.Y));
            Position = new WorldSpace(aPos);
        }

        public void AddDebugSquare()
        {
            DebugManager.AddDebugShape(new DebugTools.DebugSquare(new Rectangle(Position.ToPoint(), Size)));
        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            Textures.Texture.DrawSnapshot(aBatch, tileData.BuildRenderSnapshot(textureOffset), Position, Camera.Camera.WorldRectangle.Top);

            //xText.LeftAllignedDraw(aBatch, new WorldSpace(Position - Size.ToVector2() / 2).ToAbsoltueScreenPosition());
            if (DebugManager.Mode(DebugMode.TileCoords))
            {
                EnsureDebugText();
                xText.TopLeftDraw(aBatch, new WorldSpace(Position).ToAbsoltueScreenPosition());
                yText.TopLeftDraw(aBatch, new WorldSpace(Position + new WorldSpace(xText.Offset.X, 0)).ToAbsoltueScreenPosition());
            }
            //Camera.Camera.WorldPosToCameraSpace(Position), 0); 
        }

        void EnsureDebugText()
        {
            ThreadAffinity.AssertMainThread();
            if (debugTextInitialized) return;
            debugTextInitialized = true;
            xText = new Text("Gloryse", tilePos.X.ToString(), Color.Black);
            yText = new Text("Gloryse", tilePos.Y.ToString(), Color.Yellow);
        }

        internal Textures.Texture.TextureRenderSnapshot BuildRenderSnapshot()
        {
            ThreadAffinity.AssertSimThread();
            return tileData.BuildRenderSnapshot(textureOffset);
        }
    }
}
