using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers.Saves;
using Project_1.Managers.States;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.LoadingMenu
{
    internal class SaveDetails : Box
    {
        Save save;
        Label textDetails;
        //TODO: Image from last gameframe
        Box imagePlaceHolder;

        Button loadButton;


        public SaveDetails(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("WhiteBackground", Color.Beige), aPos, aSize)
        {
            RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);
            RelativeScreenPosition imgSize = new RelativeScreenPosition(aSize.X - spacing.X - spacing.X, aSize.Y / 3 - spacing.Y - spacing.Y);

            imagePlaceHolder = new Box(new UITexture("GrayBackground", Color.Gray), spacing, imgSize);
            AddChild(imagePlaceHolder);
            textDetails = new Label(null, imgSize.OnlyY + spacing, imgSize, Label.TextAllignment.TopLeft, Color.Black);
            AddChild(textDetails);
            capturesClick = false;
            RelativeScreenPosition buttonSize = new RelativeScreenPosition(0.15f, 0.05f);
            loadButton = new Button(new List<Action>() { LoadSave }, spacing.OnlyX + aSize.OnlyY - buttonSize.OnlyY - spacing.OnlyY, buttonSize, Color.White, "Load Save", Color.Black);
            //AddChild(loadButton);
        }

        void LoadSave()
        {
            save.LoadData();
            StateManager.SetState(StateManager.States.Game);
        }

        public void SetSave(Save aSave)
        {
            save = aSave;
            textDetails.Text = save.SaveDetails.Stringify;
            AddChild(loadButton);
        }

        public void Reset()
        {
            save = null;
            textDetails.Text = null;
            KillChild(loadButton);
        }
    }
}
