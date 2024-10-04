using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
using System.Runtime.InteropServices;

namespace Project_1
{
    public class Game1 : Game
    {
        public static Game Instance { get; private set; }
        public Game1()
        {
            GraphicsManager.SetManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
        }

        protected override void Initialize()
        {
            DebugManager.Init();
            TextureManager.Init(Content);
            GraphicsManager.Init();
            Camera.Init();
            RandomManager.Init();
            ObjectManager.Init(Content);
            TileManager.Init(Content);
            UIManager.Init();
            KeyBindManager.Init(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //GraphicsManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {

            GraphicsManager.Update();
            InputManager.Update();
            TimeManager.Update(gameTime);
            StateManager.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.HotPink);

            StateManager.Draw();
            

            base.Draw(gameTime);
        }
    }
}
