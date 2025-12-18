using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Projectiles;
using Project_1.GameObjects.Spawners;
using Project_1.GameObjects.Spells;
using Project_1.GameObjects.Spells.AoE;
using Project_1.Input;
using Project_1.Items;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Particles;
using Project_1.Messaging;
using Project_1.Textures;
using Project_1.Tiles;
using Project_1.UI;
using Project_1.UI.CharacterCreator;
using Project_1.UI.HUD.Managers;
using Project_1.UI.OptionMenu;
using System.Runtime.InteropServices;

namespace Project_1
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static Microsoft.Xna.Framework.Game Instance { get; private set; }
        public static ContentManager ContentManager { get; private set; }
        public Game1()
        {
            GraphicsManager.SetManager(this);
            Content.RootDirectory = "Content";
            ContentManager = Content;
            IsMouseVisible = true;
            Instance = this;
            InitializeMainThreadSystems();
        }

        protected override void Initialize()
        {

            //DEBUG

            if (DebugManager.Mode(DebugMode.InstantlyContinue))
            {
                SaveManager.ContinueLastSave();
                StateManager.SetState(StateManager.States.Game);
            }
            base.Initialize();
        }

        protected override void LoadContent()
        {
            DebugManager.LoadContent();
            //GraphicsManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            TimeManager.Update(gameTime);

            GraphicsManager.Update();
            InputManager.Update();
            Mailboxes.Ui.DispatchAll();
            Mailboxes.Main.DispatchAll();
            StateManager.Update();
            Mailboxes.Ui.DispatchAll();
            Mailboxes.Main.DispatchAll();
            DebugManager.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.HotPink);

            EffectManager.EffectDraw();
            StateManager.Draw();
            

            base.Draw(gameTime);
        }

        static void InitializeMainThreadSystems()
        {
            ThreadAffinity.InitMainThread();
            ThreadAffinity.AssertMainThread();
            Mailboxes.InitMainThread();
            TextureManager.Init();
            EffectManager.Init();
            StateManager.Init();
            HUDManager.Init();
            InputEventBridge.Init();
            UIEventBridge.Init();
        }
    }
}
