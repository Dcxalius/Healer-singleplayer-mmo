using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.UI.HUD.Chat;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.OptionMenu
{
    internal class ChatOptionsPanel : Box
    {
        const float HeaderHeight = 0.06f;
        const float RowHeight = 0.055f;
        const float RowSpacing = 0.01f;

        float nextRowY;

        public ChatOptionsPanel(RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : base(new Project_1.Textures.UITexture("GrayBackground", Color.WhiteSmoke), aPos, aSize)
        {
            capturesClick = false;
            capturesRelease = false;

            AddChild(new Label("Chat Message Filters", new RelativeScreenPosition(0.03f, 0.02f), new RelativeScreenPosition(0.94f, HeaderHeight), Label.TextAllignment.CentreLeft, Color.Black));

            nextRowY = 0.1f;
            AddMessageTypeToggle("Say", ChatMessageType.Say);
            AddMessageTypeToggle("Loot", ChatMessageType.Loot);
            AddMessageTypeToggle("Experience", ChatMessageType.Experience);
            AddMessageTypeToggle("Guild", ChatMessageType.Guild);
            AddMessageTypeToggle("Party", ChatMessageType.Party);
            AddMessageTypeToggle("System", ChatMessageType.System);
        }

        void AddMessageTypeToggle(string aLabel, ChatMessageType aType)
        {
            bool startState = ChatSettings.IsVisible(aType);
            AddChild(new ChatToggleRow(
                aLabel,
                startState,
                () => SetMessageTypeVisibility(aType, true),
                () => SetMessageTypeVisibility(aType, false),
                new RelativeScreenPosition(0.03f, nextRowY),
                new RelativeScreenPosition(0.94f, RowHeight)));
            nextRowY += RowHeight + RowSpacing;
        }

        void SetMessageTypeVisibility(ChatMessageType aType, bool aVisible)
        {
            bool oldValue = ChatSettings.IsVisible(aType);
            if (oldValue == aVisible) return;

            ChatSettings.SetVisible(aType, aVisible);
            MailboxManager.PublishUiEvent(new ChatFiltersChanged());

            OptionManager.AddActionToDoAtExitOfOptionMenu(
                () =>
                {
                    ChatSettings.SetVisible(aType, oldValue);
                    MailboxManager.PublishUiEvent(new ChatFiltersChanged());
                },
                ChatSettings.ExportSettings);
        }
    }

    internal sealed class ChatToggleRow : UIElement
    {
        public ChatToggleRow(string aLabel, bool aStartState, Action aOnTicked, Action aOnUnticked, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : base(new Project_1.Textures.UITexture("WhiteBackground", Color.Wheat), aPos, aSize)
        {
            capturesClick = false;
            capturesRelease = false;

            CheckBox checkBox = new CheckBox(aStartState, aOnTicked, aOnUnticked, new RelativeScreenPosition(0.01f, 0.14f), new RelativeScreenPosition(0.075f, 0.72f));
            Label label = new Label(aLabel, new RelativeScreenPosition(0.11f, 0), new RelativeScreenPosition(0.88f, 1), Label.TextAllignment.CentreLeft, Color.Black);

            AddChild(checkBox);
            AddChild(label);
        }
    }
}
