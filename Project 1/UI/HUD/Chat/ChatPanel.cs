using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project_1.UI.HUD.Chat
{
    internal sealed class ChatPanel : Box
    {
        const int MaxMessages = 100;
        const float VisibleRows = 9f;
        const float ChatTextSize = Text.DefaultTextSize;

        readonly ScrollableBox messageLog;
        readonly InputBox inputBox;
        readonly ChatMessageBuffer messageBuffer = new ChatMessageBuffer(MaxMessages);

        bool suppressOpenUntilEnterReleased;

        public ChatPanel(RelativeScreenPosition aPos, RelativeScreenPosition aSize)
            : base(new UITexture("GrayBackground", new Color(50, 50, 50, 165)), aPos, aSize)
        {
            AlwaysOnScreen = true;

            RelativeScreenPosition outerPadding = RelativeScreenPosition.GetSquareFromX(0.015f, Size);
            RelativeScreenPosition inputHeight = RelativeScreenPosition.GetSquareFromY(0.12f, Size).OnlyY;
            RelativeScreenPosition logSize = new RelativeScreenPosition(
                1f - outerPadding.X * 2,
                1f - outerPadding.Y * 3 - inputHeight.Y);

            messageLog = new ScrollableBox(
                VisibleRows,
                UITexture.Null,
                new Color(120, 120, 120, 200),
                outerPadding,
                logSize);
            AddChild(messageLog);

            RelativeScreenPosition inputPos = new RelativeScreenPosition(
                outerPadding.X,
                1f - outerPadding.Y - inputHeight.Y);
            RelativeScreenPosition inputSize = new RelativeScreenPosition(1f - outerPadding.X * 2, inputHeight.Y);
            inputBox = new InputBox(
                "",
                Size,
                new[] { InputBox.ValidInputs.Any },
                Color.White,
                "",
                new Color(20, 20, 20, 215),
                false,
                Color.LightGray,
                Color.White,
                inputPos,
                inputSize);
            inputBox.Visible = false;
            inputBox.SetEnter(new List<Action> { SubmitInput });
            AddChild(inputBox);
        }

        public override void Update()
        {
            base.Update();
            HandleEnterToggle();
        }

        public override void Rescale()
        {
            base.Rescale();
            RebuildDisplayedMessages(scrollToBottom: false);
        }

        public override void LeavingGameState()
        {
            base.LeavingGameState();
            suppressOpenUntilEnterReleased = false;
            CloseInput();
        }

        void HandleEnterToggle()
        {
            if (suppressOpenUntilEnterReleased)
            {
                if (!UiKeyboardStateCache.GetHold(Keys.Enter))
                {
                    suppressOpenUntilEnterReleased = false;
                }
                return;
            }

            if (UiTextInputManager.IsActive) return;
            if (StateManager.CurrentState != StateManager.States.Game
                && StateManager.CurrentState != StateManager.States.MoveHUD)
            {
                return;
            }
            if (!UiKeyboardStateCache.IsNewlyPressed(Keys.Enter)) return;

            OpenInput();
        }

        void OpenInput()
        {
            inputBox.Input = string.Empty;
            inputBox.Visible = true;
            UiTextInputManager.Begin(inputBox);
            HUDManager.InvalidateUi();
        }

        void CloseInput()
        {
            inputBox.Input = string.Empty;
            inputBox.Visible = false;
            if (UiTextInputManager.ActiveInput == inputBox)
            {
                UiTextInputManager.Clear();
            }
            HUDManager.InvalidateUi();
        }

        void SubmitInput()
        {
            ThreadAffinity.AssertUiThread();
            string text = (inputBox.Input ?? string.Empty).Trim();

            suppressOpenUntilEnterReleased = UiKeyboardStateCache.GetHold(Keys.Enter);
            CloseInput();

            if (string.IsNullOrWhiteSpace(text)) return;

            if (text.StartsWith("/"))
            {
                MailboxManager.PublishSimCommand(new ChatCommandRequested(text));
                return;
            }

            MailboxManager.PublishSimCommand(new ChatSayRequested(text));
        }

        public void AddMessage(ChatMessage aMessage)
        {
            ThreadAffinity.AssertUiThread();
            if (string.IsNullOrWhiteSpace(aMessage.Content)) return;

            messageBuffer.Add(new ChatMessageEntry(aMessage));
            RebuildDisplayedMessages(scrollToBottom: true);
        }

        public void AddMessage(ChatMessageType aType, string aMessage)
        {
            AddMessage(new ChatMessage(aType, aMessage));
        }

        public void ClearMessages()
        {
            ThreadAffinity.AssertUiThread();
            messageBuffer.Clear();
            RebuildDisplayedMessages(scrollToBottom: false);
        }

        public void RefreshFilters()
        {
            ThreadAffinity.AssertUiThread();
            RebuildDisplayedMessages(scrollToBottom: false);
        }

        void RebuildDisplayedMessages(bool scrollToBottom)
        {
            messageLog.RemoveAllScrollableElements();

            float maxWidth = CalculateLineWidthPx();
            foreach (ChatMessageEntry entry in messageBuffer.EnumerateChronological())
            {
                if (!ChatSettings.IsVisible(entry.Message.Type)) continue;

                List<string> wrapped = WrapText(entry.Message.DisplayText, maxWidth, "Comfortaa-msdf", ChatTextSize);
                Color textColor = ResolveMessageColor(entry.Message.Type);
                for (int i = 0; i < wrapped.Count; i++)
                {
                    messageLog.AddScrollableElement(new ChatLineElement(wrapped[i], textColor));
                }
            }

            if (scrollToBottom)
            {
                messageLog.SetScrollValue(1f);
            }
        }

        float CalculateLineWidthPx()
        {
            AbsoluteScreenPosition elementAbsolute = messageLog.ElementSize.ToAbsoluteScreenPos(messageLog.Size);
            float maxWidth = elementAbsolute.X;
            if (maxWidth <= 0f) return 0f;

            float horizontalPadding = 8f;
            return Math.Max(1f, maxWidth - horizontalPadding);
        }

        static List<string> WrapText(string aText, float aMaxWidthPx, string aFontName, float aTextSize)
        {
            List<string> lines = new List<string>();
            if (string.IsNullOrEmpty(aText))
            {
                lines.Add(string.Empty);
                return lines;
            }
            if (aMaxWidthPx <= 0f)
            {
                lines.Add(aText);
                return lines;
            }

            string[] sourceLines = aText.Replace("\r", string.Empty).Split('\n');
            for (int i = 0; i < sourceLines.Length; i++)
            {
                WrapSingleLine(sourceLines[i], aMaxWidthPx, aFontName, aTextSize, lines);
            }
            return lines;
        }

        static void WrapSingleLine(string aLine, float aMaxWidthPx, string aFontName, float aTextSize, List<string> aOutput)
        {
            if (string.IsNullOrEmpty(aLine))
            {
                aOutput.Add(string.Empty);
                return;
            }

            string[] words = aLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                aOutput.Add(string.Empty);
                return;
            }

            StringBuilder current = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (!FitsWithinWidth(word, aMaxWidthPx, aFontName, aTextSize))
                {
                    if (current.Length > 0)
                    {
                        aOutput.Add(current.ToString());
                        current.Clear();
                    }

                    List<string> brokenWord = BreakLongWord(word, aMaxWidthPx, aFontName, aTextSize);
                    for (int j = 0; j < brokenWord.Count; j++)
                    {
                        if (j + 1 < brokenWord.Count)
                        {
                            aOutput.Add(brokenWord[j]);
                        }
                        else
                        {
                            current.Append(brokenWord[j]);
                        }
                    }
                    continue;
                }

                string candidate = current.Length == 0 ? word : $"{current} {word}";
                if (FitsWithinWidth(candidate, aMaxWidthPx, aFontName, aTextSize))
                {
                    current.Clear();
                    current.Append(candidate);
                    continue;
                }

                aOutput.Add(current.ToString());
                current.Clear();
                current.Append(word);
            }

            if (current.Length > 0)
            {
                aOutput.Add(current.ToString());
            }
        }

        static List<string> BreakLongWord(string aWord, float aMaxWidthPx, string aFontName, float aTextSize)
        {
            List<string> parts = new List<string>();
            StringBuilder current = new StringBuilder();
            for (int i = 0; i < aWord.Length; i++)
            {
                current.Append(aWord[i]);
                if (FitsWithinWidth(current.ToString(), aMaxWidthPx, aFontName, aTextSize)) continue;

                if (current.Length == 1)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                char overflow = current[current.Length - 1];
                current.Length--;
                parts.Add(current.ToString());
                current.Clear();
                current.Append(overflow);
            }

            if (current.Length > 0)
            {
                parts.Add(current.ToString());
            }

            return parts;
        }

        static bool FitsWithinWidth(string aText, float aMaxWidthPx, string aFontName, float aTextSize)
        {
            return Text.CalculateOffset(aText, aFontName, aTextSize).X <= aMaxWidthPx;
        }

        static Color ResolveMessageColor(ChatMessageType aType)
        {
            return aType switch
            {
                ChatMessageType.Say => Color.White,
                ChatMessageType.Loot => Color.Gold,
                ChatMessageType.Experience => Color.Aquamarine,
                ChatMessageType.Guild => Color.LightSkyBlue,
                ChatMessageType.Party => Color.LightGreen,
                ChatMessageType.System => Color.Orange,
                _ => Color.White
            };
        }

        readonly struct ChatMessageEntry
        {
            public ChatMessageEntry(ChatMessage message)
            {
                Message = message;
            }

            public ChatMessage Message { get; }
        }

        sealed class ChatMessageBuffer
        {
            readonly ChatMessageEntry[] entries;
            int start;
            int count;

            public ChatMessageBuffer(int aCapacity)
            {
                entries = new ChatMessageEntry[Math.Max(1, aCapacity)];
            }

            public void Add(ChatMessageEntry aEntry)
            {
                int capacity = entries.Length;
                if (count < capacity)
                {
                    entries[(start + count) % capacity] = aEntry;
                    count++;
                    return;
                }

                entries[start] = aEntry;
                start = (start + 1) % capacity;
            }

            public void Clear()
            {
                start = 0;
                count = 0;
            }

            public IEnumerable<ChatMessageEntry> EnumerateChronological()
            {
                int capacity = entries.Length;
                for (int i = 0; i < count; i++)
                {
                    yield return entries[(start + i) % capacity];
                }
            }
        }

        sealed class ChatLineElement : Box
        {
            readonly Label label;

            public ChatLineElement(string aText, Color aTextColor)
                : base(UITexture.Null, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
            {
                capturesClick = false;
                capturesRelease = false;

                label = new Label(aText, RelativeScreenPosition.Zero, RelativeScreenPosition.One, Label.TextAllignment.CentreLeft, aTextColor, aTextSize: ChatTextSize);
                AddChild(label);
            }

            public override void Resize(RelativeScreenPosition aSize)
            {
                base.Resize(aSize);
                RelativeScreenPosition padding = RelativeScreenPosition.GetSquareFromX(0.01f, Size);
                label.Move(padding);
                label.Resize(RelativeScreenPosition.One - padding.OnlyX * 2);
            }
        }
    }
}
