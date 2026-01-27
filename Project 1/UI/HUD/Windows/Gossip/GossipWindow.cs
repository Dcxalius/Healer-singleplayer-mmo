using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_1.GameObjects.Entities.Friendlies.Npcs;

namespace Project_1.UI.HUD.Windows.Gossip
{
    internal class GossipWindow : Window
    {
        Label introduction;
        ScrollableBox options;

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
        }

        public Action<ChatGossipOption> GetSet()
        {
            return Set;
        }

        public void Set(GossipData aData)
        {
            ChatGossipOption start = BuildOptions(aData);
            Set(start);
        }

        void Set(ChatGossipOption aOption)
        {
            ResetOptions();
            introduction.Text = aOption.IntroText;
            AddOptions(aOption.GossipOptions);
        }

        ChatGossipOption BuildOptions(GossipData aData)
        {
            string[][] optionsData = aData.Options;
            int[][] links = aData.LinkTree;
            GossipOption[] built = new GossipOption[optionsData.Length];

            for (int i = 0; i < optionsData.Length; i++)
            {
                string type = optionsData[i][0];
                string header = optionsData[i][1];
                string data = optionsData[i][2];

                built[i] = type switch
                {
                    "C" => new ChatGossipOption(header, data),
                    "S" => new ShopGossipOption(header, data),
                    _ => throw new NotImplementedException()
                };
            }

            for (int i = 0; i < links.Length; i++)
            {
                if (built[i] is not ChatGossipOption chat) continue;
                for (int j = 0; j < links[i].Length; j++)
                {
                    chat.AddGossipOption(built[links[i][j]]);
                }
            }

            ChatGossipOption start = built[aData.StartIndex] as ChatGossipOption;
            Debug.Assert(start != null);
            return start;
        }

        public Action<ShopGossipOption> GetClose()
        {
            return CloseAndOpenShop;
        }

        void CloseAndOpenShop(ShopGossipOption aSO)
        {
            CloseWindow();
            Mailboxes.Ui.Publish(new ShopOpened(aSO.ItemIDsInShop));
        }

        public void SetIntro(string aIntro) => introduction.Text = aIntro;

        public void AddOption(GossipOption aOption) => options.AddScrollableElement(aOption);

        public void AddOptions(GossipOption[] aOptions) => options.AddScrollableElements(aOptions);

        public void ResetOptions() => options.RemoveAllScrollableElements();
    }
}
