using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Npcs;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD.Windows.Gossip
{
    internal class GossipWindow : Window
    {
        Label introduction;
        ScrollableBox options;
        Npc npcTalkingTo;

        public GossipWindow() : base(new UITexture("WhiteBackground", Color.AntiqueWhite))
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.05f, Size);
            RelativeScreenPosition introSize = new RelativeScreenPosition((1 - spacing.X * 2), 0.4f);
            introduction = new Label("", spacing, introSize, Label.TextAllignment.TopLeft, Color.Black);
            AddChild(introduction);
            options = new ScrollableBox(10, UITexture.Null, Color.AliceBlue, introSize.OnlyY + spacing, RelativeScreenPosition.One - spacing - introSize.OnlyY);
            AddChild(options);
        }

        public override void Update()
        {
            base.Update();

            if (npcTalkingTo == null) return;
            if (!npcTalkingTo.InConversationRange(ObjectManager.Player.FeetPosition))
            {
                ResetOptions();
                npcTalkingTo = null;
                CloseWindow();
            }
        }

        public Action<ChatGossipOption> GetSet()
        {
            return Set;
        }

        public void Set(ChatGossipOption aOption, Npc aNpc)
        {
            npcTalkingTo = aNpc;
            Set(aOption);
        }

        void Set(ChatGossipOption aOption)
        {
            Debug.Assert(npcTalkingTo != null);
            ResetOptions();
            introduction.Text = aOption.IntroText;
            AddOptions(aOption.GossipOptions);
        }

        public Action<ShopGossipOption> GetClose()
        {
            return CloseAndOpenShop;
        }

        void CloseAndOpenShop(ShopGossipOption aSO)
        {
            CloseWindow();
            HUDManager.windowHandler.OpenShopWindow(aSO, npcTalkingTo);
            
        }

        public void SetIntro(string aIntro) => introduction.Text = aIntro;

        public void AddOption(GossipOption aOption) => options.AddScrollableElement(aOption);

        public void AddOptions(GossipOption[] aOptions) => options.AddScrollableElements(aOptions);

        public void ResetOptions() => options.RemoveAllScrollableElements();
    }
}
