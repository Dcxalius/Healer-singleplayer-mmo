using Project_1.Camera;
using Project_1.Textures;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.UI.HUD
{
    internal class NamePlate : Box
    {
        Bar healthBar;
        public string Name => name.Text;
        Label name;
        static Color backgroundColor = new Color(120, 50, 50, 80);
        RelativeScreenPosition barSize = new RelativeScreenPosition(1f, 0.2f);
        //RelativeScreenPosition barSize = new RelativeScreenPosition(0.02f, 0.006f);

        static Color ColorTransform(Color aRelationColor)
        {
            float transp = 120f / 255f;

            byte r = (byte)((transp * aRelationColor.R / 255f) * 255);
            byte g = (byte)((transp * aRelationColor.G / 255f) * 255);
            byte b = (byte)((transp * aRelationColor.B / 255f) * 255);
            byte a = (byte)((transp * aRelationColor.A / 255f) * 255);

            return new Color(r, g, b, a);
        }

        public NamePlate(in EntityUiSnapshot snapshot) : base(new UITexture("GrayBackground", ColorTransform(snapshot.RelationColor)), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            name = new Label(snapshot.Name, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopCentre);
            healthBar = new Bar(new BarTexture(BarTexture.FillingDirection.Right, Color.Red), new UITexture("WhiteBackground", backgroundColor), RelativeScreenPosition.Zero, barSize);
            

            AddChild(name);
            AddChild(healthBar);

            SetTarget(snapshot);
        }

        void SetTarget(in EntityUiSnapshot snapshot) //TODO: Add a minimum size for bar and a maximum size of name.
        {
            name.Text = snapshot.Name;
            healthBar.Value = (float)(snapshot.CurrentHealth / snapshot.MaxHealth);
            gfx.Color = ColorTransform(snapshot.RelationColor);

            AbsoluteScreenPosition textOffset = new AbsoluteScreenPosition((int)name.UnderlyingTextOffset.X, (int)name.UnderlyingTextOffset.Y);


            healthBar.Resize(barSize);

            Resize((textOffset.OnlyX + textOffset.OnlyY * (1 + barSize.Y)).ToRelativeScreenPosition());

            name.Resize(textOffset.ToRelativeScreenPosition(Size));     

            Reposition(snapshot);

            healthBar.Move(RelativeScreenPosition.One.OnlyY - healthBar.RelativeSize.OnlyY);
        }

        public override void Rescale()
        {
            healthBar.Resize(barSize);
            AbsoluteScreenPosition textOffset = new AbsoluteScreenPosition((int)name.UnderlyingTextOffset.X, (int)name.UnderlyingTextOffset.Y);

            Resize((textOffset.OnlyX + textOffset.OnlyY * (1 + barSize.Y)).ToRelativeScreenPosition());

            name.Resize(textOffset.ToRelativeScreenPosition(Size));

            healthBar.Move(RelativeScreenPosition.One.OnlyY - healthBar.RelativeSize.OnlyY);
        }

        public void Refresh(in EntityUiSnapshot snapshot)
        {
            healthBar.Value = (float)(snapshot.CurrentHealth / snapshot.MaxHealth);
            if (name.Text != snapshot.Name)
            {
                SetTarget(snapshot);
                return;
            }
            Reposition(snapshot);
        }

        public void Reposition(in EntityUiSnapshot snapshot)
        {
            Move((snapshot.FeetPosition.ToAbsoltueScreenPosition() - new AbsoluteScreenPosition(Size.X / 2, snapshot.WorldHeight * 2)).ToRelativeScreenPosition() - RelativeSize.OnlyY);

        }
    }
}
