﻿using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using Project_1.UI.UIElements.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.CharacterCreator
{
    internal class NewGameBox : MenuBox
    {
        InputBox inputBox;
        FinalizeCharacterButton finalizeCharacterButton;
        public NewGameBox(RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(new UITexture("GrayBackground", Color.AliceBlue), aPos, aSize)
        {
            RelativeScreenPosition size = RelativeScreenPosition.GetSquareFromY(0.05f, Size);
            AddChild(new LoadingMenu.ExitButton(RelativeScreenPosition.One - size, size));
            inputBox = new InputBox("Name:", Size, new InputBox.ValidInputs[] { InputBox.ValidInputs.Letters },Color.Black, "Input Character Name", Color.White, true, Color.LightGray, Color.Black, new RelativeScreenPosition(0.05f), new RelativeScreenPosition(0.5f, 0.08f));
            AddChild(inputBox);
            //AddChild(new GFXButton(new List<Action>() { new Action(() => className = "Priest")}, new GfxPath(GfxType.UI, "Priest"), new RelativeScreenPosition(0.4f), new AbsoluteScreenPosition(64).ToRelativeScreenPosition(), Color.AliceBlue));
            //AddChild(new GFXButton(new List<Action>() { new Action(() => className = "Druid")}, new GfxPath(GfxType.UI, "Druid"), new RelativeScreenPosition(0.5f, 0.4f), new AbsoluteScreenPosition(64).ToRelativeScreenPosition(), Color.AliceBlue));
            AddChild(new ClassSelector(new RelativeScreenPosition(0.4f, 0.3f), new RelativeScreenPosition(0.2f)));
            RelativeScreenPosition finalButtonSize = new RelativeScreenPosition(0.3f, 0.05f);
            finalizeCharacterButton = new FinalizeCharacterButton(new List<Action>() { new Action(CreateNewCharacter) }, new RelativeScreenPosition(0.5f - finalButtonSize.X / 2, 0.95f - finalButtonSize.Y), finalButtonSize);
            AddChild(finalizeCharacterButton);
            finalizeCharacterButton.Visible = false;
        }

        public override void Update()
        {
            //DebugManager.Print(GetType(), name);
            base.Update();
            if (SaveManager.NameAlreadyExists(inputBox.Input))
            {
                finalizeCharacterButton.Visible = false;
                return;
            }
            if (finalizeCharacterButton.Visible == true) return;

            if (inputBox.Input == "" || ClassSelectButton.ClassName == null) return;

            finalizeCharacterButton.Visible = true;
        }

        void CreateNewCharacter()
        {
            string name = inputBox.Input;
            ObjectManager.CreateNewPlayer(name, ClassSelectButton.ClassName);
            SaveManager.CreateNewSave(name);
            StateManager.SetState(StateManager.States.Game);
        }
    }
}
