using Microsoft.Xna.Framework;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows.Logic
{
    internal class LogicWindow : Window
    {
        ScenarioBox scenarioBox;
        public LogicWindow() : base(new UITexture("WhiteBackground", Color.CadetBlue))
        {
            scenarioBox = new ScenarioBox();
            AddChild(scenarioBox);
            visibleKey = Input.KeyBindManager.KeyListner.LogicWindow;
        }


    }
}
