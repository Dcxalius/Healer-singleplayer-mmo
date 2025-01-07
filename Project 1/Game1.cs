using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.AoE;
using Project_1.GameObjects.Spells.Projectiles;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Particles;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.OptionMenu;
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

        protected override void Initialize() //TODO: Split all Inits so that all calls that are independant are called first and only after that the dependant ones are called
        {
            DebugManager.Init();
            ItemFactory.Init(Content);
            LootFactory.Init(Content);
            TextureManager.Init(Content);
            SpellFactory.Init(Content);
            ProjectileFactory.Init(Content);
            KeyBindManager.Init(Content);
            GraphicsManager.Init();
            //Camera.Init();
            RandomManager.Init();
            ObjectFactory.Init(Content);
            ObjectManager.Init();
            SpawnerManager.Init(Content);
            TileManager.Init(Content);
            UIManager.Init();
            AreaOfEffectFactory.Init(Content);
            ParticleManager.Init();
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
            DebugManager.Update();

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
