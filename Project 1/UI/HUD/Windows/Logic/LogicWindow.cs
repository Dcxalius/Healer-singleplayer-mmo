using Microsoft.Xna.Framework;
using Project_1.Messaging.Events;
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
        readonly NodeViewer nodeViewer;
        ScenarioBox scenarioBox;
        public LogicWindow() : base(new UITexture("WhiteBackground", Color.CadetBlue))
        {
            nodeViewer = new NodeViewer();
            scenarioBox = new ScenarioBox();
            AddChild(nodeViewer);
            AddChild(scenarioBox);
        }

        public void SetData(int memberRenderId)
        {
            nodeViewer.SetCurrentTarget(memberRenderId);
        }

        public void SetSnapshot(int memberRenderId, LogicNodeUiSnapshot[] nodes)
        {
            nodeViewer.SetSnapshot(memberRenderId, nodes);
        }

    }
}
