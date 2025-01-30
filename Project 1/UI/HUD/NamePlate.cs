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

namespace Project_1.UI.HUD
{
    internal class NamePlate : Box
    {
        Bar healthBar;
        Label name;
        static Color backgroundColor = new Color(120, 50, 50, 80);
        RelativeScreenPosition barSize = new RelativeScreenPosition(0.02f, 0.006f);
        WorldSpace offset;

        public NamePlate(Entity aEntity) : base(null, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero)
        {
            name = new Label(aEntity.Name, RelativeScreenPosition.Zero, RelativeScreenPosition.Zero, Label.TextAllignment.TopCentre);
            healthBar = new Bar(new BarTexture(BarTexture.FillingDirection.Right, Color.Red), new UITexture("WhiteBackground", backgroundColor), new AbsoluteScreenPosition(0, (int)name.UnderlyingTextOffset.Y).ToRelativeScreenPosition(), barSize);
            
            AddChild(name);
            AddChild(healthBar);

            SetTarget(aEntity);
        }

        public void SetTarget(Entity aEntity)
        {
            name.Text = aEntity.Name;
            healthBar.Value = aEntity.CurrentHealth / aEntity.MaxHealth;
            AbsoluteScreenPosition barOffset = AbsoluteScreenPosition.FromRelativeScreenPosition(barSize) / 2;
            Vector2 textOffset = name.UnderlyingTextOffset / 2;
            name.Resize(new AbsoluteScreenPosition(name.UnderlyingTextOffset.ToPoint()).ToRelativeScreenPosition());
            //healthBar.Move(name.RelativeSize.OnlyY);
            healthBar.Move(RelativeScreenPosition.Zero);
            float sizeY = name.RelativeSize.Y + barSize.Y;
            if (textOffset.X < barOffset.X)
            {
                offset.X = -barOffset.X;
                name.Move(new RelativeScreenPosition(barSize.X / 2 - textOffset.X / 2, 0));
                Resize(new RelativeScreenPosition(barSize.X , sizeY));
            }
            else
            {
                Resize(new RelativeScreenPosition(name.RelativeSize.X, sizeY));
                offset.X = -textOffset.X;
                healthBar.Move(new AbsoluteScreenPosition((int)(textOffset.X - barOffset.X), healthBar.AbsolutePos.Y).ToRelativeScreenPosition());
            }

            offset.Y = - aEntity.Size.Y;
            Move((aEntity.FeetPosition + offset).ToAbsoltueScreenPosition().ToRelativeScreenPosition() - RelativeSize.OnlyY);
        }

        public void Refresh(Entity aEntity)
        {
            healthBar.Value = aEntity.CurrentHealth / aEntity.MaxHealth;
        }

        public void Reposition(Entity aEntity)
        {
            Move((aEntity.FeetPosition + offset).ToAbsoltueScreenPosition().ToRelativeScreenPosition() - RelativeSize.OnlyY);
        }
    }
}
