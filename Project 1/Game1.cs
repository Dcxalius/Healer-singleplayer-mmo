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
using Project_1.Managers.States;
using Project_1.Particles;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.CharacterCreator;
using Project_1.UI.OptionMenu;
using System.Runtime.InteropServices;

namespace Project_1
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static Microsoft.Xna.Framework.Game Instance { get; private set; }
        public Game1()
        {
            GraphicsManager.SetManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
        }

        protected override void Initialize() //TODO: Split all Inits so that all calls that are independant are called first and only after that the dependant ones are called
        {
            Camera.Camera.Init();
            ManagerInit();
            FactoryInit();
            ClassSelector.Init(Content);

            //DEBUG
            
            if(DebugManager.Mode(DebugMode.InstantlyContinue))
            {
                SaveManager.ContinueLastSave();
                StateManager.SetState(StateManager.States.Game);
            }
            base.Initialize();
        }

        void ManagerInit()
        {
            DebugManager.Init();
            SaveManager.Init(Content);
            RandomManager.Init();
            TextureManager.Init(Content);
            KeyBindManager.Init(Content);
            SpawnerManager.Init();
            ParticleManager.Init();
            TimeManager.Init();
            StateManager.Init(Content);
        }

        void FactoryInit()
        {
            SpellFactory.Init(Content);
            ItemFactory.Init(Content);
            TileFactory.Init(Content);
            ProjectileFactory.Init(Content);
            AreaOfEffectFactory.Init(Content);
        }

        protected override void LoadContent()
        {
            //GraphicsManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            TimeManager.Update(gameTime);

            GraphicsManager.Update();
            InputManager.Update();
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
