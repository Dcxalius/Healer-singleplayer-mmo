using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements
{
    internal class BuffBox : Box
    {
        public enum FillDirection
        {
            TopRightToLeftDown
        }

        FillDirection fillDirection;
        List<Buff> buffs;

        Vector2 StartPosition
        {
            get
            {
                switch (fillDirection)
                {
                    case FillDirection.TopRightToLeftDown:
                        return new Vector2(RelativeSize.X - spacing.X, spacing.Y);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        Vector2 StepPosition(int i)
        {
            int maxInX = (int)Math.Floor((buffSize.X + spacing.X) / Size.X + spacing.X);
            int maxInY = (int)Math.Floor((buffSize.Y + spacing.Y) / Size.Y + spacing.Y);
            switch (fillDirection)
            {
                case FillDirection.TopRightToLeftDown:
                    Vector2 xdd = Vector2.Zero;
                    xdd.X = (buffSize.X + spacing.X) * i % maxInX ;
                    xdd.Y = (float)((buffSize.Y + spacing.Y) * Math.Floor((double)i / maxInY));
                    return xdd;
                default:
                    throw new NotImplementedException();
            }
        }

        Vector2 buffSize = Camera.Camera.GetRelativeXSquare(0.15f) + new Vector2(0, Camera.Camera.GetRelativeXSquare(0.15f).Y / 2);
        Vector2 spacing = Camera.Camera.GetRelativeXSquare(0.05f);

        public BuffBox(FillDirection aDir, Vector2 aPos, Vector2 aSize) : base(UITexture.Null, aPos, aSize)
        {
            buffs = new List<Buff>();
            children.AddRange(buffs);
        } 

        public void AddBuff(GameObjects.Spells.Buff aBuff)
        {
            buffs.Add(new Buff(aBuff, Vector2.Zero, buffSize));
            SortBuffs();
        }

        public override void Update(in UIElement aParent)
        {
            base.Update(aParent);

            CheckLast();
        }

        void CheckLast()
        {
            if (buffs.Count == 0) return;
            if (buffs.Last().duration <= 0)
            {
                buffs.RemoveAt(buffs.Count - 1);
                CheckLast();
            }
        }

        void SortBuffs()
        {
            buffs.Sort();
            Vector2 pos = StartPosition;
            for (int i = 0; i < buffs.Count; i++)
            {
                buffs[i].Move(pos + StepPosition(i));
            }
        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
        }
    }
}
