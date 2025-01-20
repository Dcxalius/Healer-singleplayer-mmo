using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.OptionMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class OptionMenu : State
    {
        public OptionMenu() => OptionManager.Init();

        public override void Update() => OptionManager.Update();

        public override bool Click(ClickEvent aClickEvent) => OptionManager.Click(aClickEvent);



        public override void OnEnter()
        {
        }

        public override void OnLeave()
        {
        }

        public override void Draw(SpriteBatch aBatch)
        {

            GraphicsManager.ClearScreen(Color.LightGray);
            OptionManager.Draw(aBatch);

            Camera.Camera.OptionDraw();
        }
    }
}
