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
using Project_1.GameObjects.Entities;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
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
        WorldSpace offset;

        static Color ColorTransform(Entity aE)
        {
            float transp = 120f / 255f;

            byte r = (byte)((transp * aE.RelationColor.R / 255f) * 255);
            byte g = (byte)((transp * aE.RelationColor.G / 255f) * 255);
            byte b = (byte)((transp * aE.RelationColor.B / 255f) * 255);
            byte a = (byte)((transp * aE.RelationColor.A / 255f) * 255);

            return new Color(r, g, b, a);
        }

        public NamePlate(Entity aEntity) : base(new UITexture("GrayBackground", ColorTransform(aEntity)), RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            name = new Label(aEntity.Name, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopCentre);
            healthBar = new Bar(new BarTexture(BarTexture.FillingDirection.Right, Color.Red), new UITexture("WhiteBackground", backgroundColor), RelativeScreenPosition.Zero, barSize);
            

            AddChild(name);
            AddChild(healthBar);

            SetTarget(aEntity);
        }

        void SetTarget(Entity aEntity) //TODO: Add a minimum size for bar and a maximum size of name.
        {
            name.Text = aEntity.Name;
            healthBar.Value = aEntity.CurrentHealth / aEntity.MaxHealth;

            AbsoluteScreenPosition textOffset = new AbsoluteScreenPosition((int)name.UnderlyingTextOffset.X, (int)name.UnderlyingTextOffset.Y);


            healthBar.Resize(barSize);

            Resize((textOffset.OnlyX + textOffset.OnlyY * (1 + barSize.Y)).ToRelativeScreenPosition());

            name.Resize(textOffset.ToRelativeScreenPosition(Size));     

            Reposition(aEntity);

            healthBar.Move(RelativeScreenPosition.One.OnlyY - healthBar.RelativeSize.OnlyY);
        }

        public void Refresh(Entity aEntity)
        {
            healthBar.Value = aEntity.CurrentHealth / aEntity.MaxHealth;
        }

        public void Reposition(Entity aEntity)
        {
            //Move((aEntity.FeetPosition + offset).ToAbsoltueScreenPosition().ToRelativeScreenPosition() - RelativeSize.OnlyY);
            Move((aEntity.FeetPosition.ToAbsoltueScreenPosition() - new AbsoluteScreenPosition(Size.X / 2, aEntity.WorldRectangle.Size.Y * 2)).ToRelativeScreenPosition() - RelativeSize.OnlyY);

        }
    }
}
