using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.OptionMenu;
using Project_1.UI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class Pause : State
    {
        static PauseBox pauseBox;

        public Pause() => pauseBox = new PauseBox();

        public override void Update()
        {
            if (InputManager.GetPress(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                StateManager.SetState(StateManager.States.Game);
            }
            pauseBox.Update(null);
        }

        public override bool Click(ClickEvent aClickEvent) => pauseBox.ClickedOn(aClickEvent);

        public override void OnEnter()
        {
            
        }

        public override void OnLeave()
        {
            
        }

        public override void Draw(SpriteBatch aBatch)
        {
            GraphicsManager.ClearScreen(Color.Purple);
            Camera.Camera.PauseDraw();
            pauseBox.Draw(aBatch);
        }
    }
}
