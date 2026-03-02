using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;

namespace Project_1.UI.LoadingMenu
{
    internal class LoadingBox : MenuBox
    {
        readonly ScrollableBox savesToLoadFrom;
        readonly SaveDetails saveDetails;
        ExistingSave[] existingSaves = Array.Empty<ExistingSave>();

        public LoadingBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.Orange), aPos, aSize)
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f, Size);
            RelativeScreenPosition size = new RelativeScreenPosition(0.5f - spacing.X - spacing.X, 1 - spacing.Y - spacing.Y);
            savesToLoadFrom = new ScrollableBox(3f, new UITexture("WhiteBackground", Color.AntiqueWhite), Color.AliceBlue, spacing, size);
            AddChild(savesToLoadFrom);
            ExistingSave.callAtClick = SetSave;

            saveDetails = new SaveDetails(new RelativeScreenPosition(size.X + spacing.X * 3, spacing.Y), size);
            AddChild(saveDetails);

            RelativeScreenPosition square = RelativeScreenPosition.GetSquareFromY(0.07f, Size);
            AddChild(new ExitButton(RelativeScreenPosition.One - square - spacing, square));
        }

        public void Setup(SaveUiSnapshot[] saves)
        {
            existingSaves = saves == null ? Array.Empty<ExistingSave>() : new ExistingSave[saves.Length];
            for (int i = 0; i < existingSaves.Length; i++)
            {
                existingSaves[i] = new ExistingSave(saves[i]);
                savesToLoadFrom.AddScrollableElement(existingSaves[i]);
            }
        }

        public void Reset()
        {
            saveDetails.Reset();
            savesToLoadFrom.RemoveAllScrollableElements();
            existingSaves = Array.Empty<ExistingSave>();
        }

        public void SetSave(ExistingSave aExistingSave)
        {
            for (int i = 0; i < existingSaves.Length; i++)
            {
                existingSaves[i].Color = Color.White;
            }

            aExistingSave.Color = Color.Gray;
            saveDetails.SetSave(aExistingSave.Save);
        }
    }
}
