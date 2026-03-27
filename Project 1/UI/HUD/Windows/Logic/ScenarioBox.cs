using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.SelectBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows.Logic
{
    internal class ScenarioBox : ScrollableBox<UIElement> //TODO: I dont think this UIE is right xdd
    {
        public enum ScenarioType
        {
            Attacking,
            Fleeing,
            OutOfRangeOfBind
        }

        public ScenarioBox() : base(10, new UITexture("WhiteBackground", Color.Chartreuse), Color.GreenYellow, new RelativeScreenPosition(0.01f, 0.51f), new RelativeScreenPosition(0.48f, 0.48f))
        {
        }
    }
}
