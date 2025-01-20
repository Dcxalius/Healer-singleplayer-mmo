using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.GameObjects.Spawners;
using Project_1.Input;
using Project_1.Particles;
using Project_1.UI;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class Game : State
    {
        public Game() => HUDManager.Init();

        public override void Update()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                StateManager.SetState(StateManager.States.Pause);
            }
            Camera.Camera.Update();
            ObjectManager.Update();
            SpawnerManager.Update();
            HUDManager.Update();
            ParticleManager.Update();
        }
        public override bool Click(ClickEvent aClickEvent) => HUDManager.Click(aClickEvent);

                    
        public override bool Release(ReleaseEvent aReleaseEvent) => HUDManager.Release(aReleaseEvent);

        public override void OnEnter() => TimeManager.StopPause();

        public override void OnLeave()
        {
            TimeManager.StartPause();
            HUDManager.PauseMenuActivated();
        }

        public override void Draw(SpriteBatch aBatch)
        {

            Camera.Camera.GameDraw();
            Camera.Camera.DrawGameToCamera();
        }

        public override bool Scroll(ScrollEvent aScrollEvent)
        {
            return HUDManager.Scroll(aScrollEvent);

        }
    }
}
