using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;

namespace Project_1.UI.LoadingMenu
{
    internal class SaveDetails : Box
    {
        SaveUiSnapshot? save;
        readonly Label textDetails;
        readonly RuntimeImage image;
        readonly Button loadButton;

        public SaveDetails(UIElement aParent, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(aParent, new UITexture("WhiteBackground", Color.Beige), aPos, aSize)
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);
            RelativeScreenPosition imgSize = new RelativeScreenPosition(1 - spacing.X - spacing.X, 1f / 3f - spacing.Y - spacing.Y);

            image = new RuntimeImage(this, spacing, imgSize);
            AddChild(image);

            textDetails = new Label(this, imgSize.OnlyY + spacing, imgSize, Label.TextAllignment.TopLeft, Color.Black);
            AddChild(textDetails);

            capturesClick = false;
            RelativeScreenPosition buttonSize = new RelativeScreenPosition(0.15f, 0.05f);
            loadButton = new Button(this, new List<Action> { LoadSave }, spacing.OnlyX + aSize.OnlyY - buttonSize.OnlyY - spacing.OnlyY, buttonSize, Color.White, "Load Save", Color.Black);
            //AddChild(loadButton);
        }

        void LoadSave()
        {
            if (!save.HasValue) return;
            MailboxManager.PublishSimCommand(new LoadSaveRequested(save.Value.SaveName));
        }

        public void SetSave(SaveUiSnapshot aSave)
        {
            save = aSave;
            textDetails.Text = aSave.DetailsText;
            image.SetImage(aSave.ImagePath);
            AddChild(loadButton);
        }

        public void Reset()
        {
            save = null;
            textDetails.Text = null;
            image.Clear();
            KillChild(loadButton);
        }
    }
}
