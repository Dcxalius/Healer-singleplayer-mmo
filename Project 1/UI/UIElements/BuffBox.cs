using Project_1.Camera;
using Project_1.Managers;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project_1.UI.UIElements
{
    internal class BuffBox : Box
    {
        public enum FillDirection
        {
            TopRightToDown
        }

        readonly FillDirection fillDirection;
        readonly List<Buff> buffs;
        int? ownerRenderId;

        RelativeScreenPosition StartPosition
        {
            get
            {
                return fillDirection switch
                {
                    FillDirection.TopRightToDown => new RelativeScreenPosition(RelativeSize.X - buffSize.X - spacing.X, spacing.Y),
                    _ => throw new NotImplementedException()
                };
            }
        }

        readonly RelativeScreenPosition buffSize = RelativeScreenPosition.GetSquareFromX(0.015f);
        readonly RelativeScreenPosition spacing = RelativeScreenPosition.GetSquareFromX(0.005f);
        readonly RelativeScreenPosition textSpacing = new RelativeScreenPosition(0, 0.007f);

        public BuffBox(FillDirection aDir, RelativeScreenPosition aPos, RelativeScreenPosition aSize) : base(UITexture.Null, aPos, aSize)
        {
            fillDirection = aDir;
            buffs = new List<Buff>();
            AddChildren(buffs);
            capturesClick = false;
        }

        public bool IsThisMine(int renderId) => ownerRenderId.HasValue && ownerRenderId.Value == renderId;
        public int BuffCount => buffs.Count;

        public void AssignBox(int? aOwnerRenderId)
        {
            ownerRenderId = aOwnerRenderId;
            ClearBuffs();
            Visible = aOwnerRenderId.HasValue;
        }

        void ClearBuffs()
        {
            buffs.Clear();
            KillAllChildren();
        }

        public void AddBuff(in BuffUiSnapshot aBuff)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i].EffectId != aBuff.EffectId) continue;
                buffs[i].Refresh(aBuff);
                SortBuffs();
                return;
            }

            buffs.Add(new Buff(aBuff, RelativeScreenPosition.Zero, buffSize));
            AddChild(buffs.Last());
            SortBuffs();
        }

        public override void Update()
        {
            base.Update();
            CheckLast();
        }

        void CheckLast()
        {
            if (buffs.Count == 0) return;
            if (buffs.Last().Duration <= 0)
            {
                KillChild(buffs.Count - 1);
                buffs.RemoveAt(buffs.Count - 1);
                CheckLast();
            }
        }

        void SortBuffs()
        {
            buffs.Sort();
            RelativeScreenPosition pos = StartPosition;
            int maxInX = (int)Math.Floor(RelativeSize.X / (buffSize.X + spacing.X + textSpacing.X) + spacing.X);
            int maxInY = (int)Math.Floor(RelativeSize.Y / (buffSize.Y + spacing.Y + textSpacing.Y) + spacing.Y);

            if (maxInX * maxInY < buffs.Count) throw new NotImplementedException(); //TODO: You probably should have fixed this by now xdd

            for (int i = 0; i < buffs.Count; i++)
            {
                buffs[i].Move(pos + StepPosition(i, maxInX, maxInY));
            }
        }

        RelativeScreenPosition StepPosition(int i, int aMaxX, int aMaxY)
        {
            return fillDirection switch
            {
                FillDirection.TopRightToDown => new RelativeScreenPosition(
                    (float)((buffSize.X + spacing.X + textSpacing.X) * -Math.Floor((double)i / aMaxY)),
                    (buffSize.Y + spacing.Y + textSpacing.Y) * (i % aMaxY)),
                _ => throw new NotImplementedException()
            };
        }

        static void AssertUiOrMainThread()
        {
            if (ThreadAffinity.IsMainThread) return;
            ThreadAffinity.AssertUiThread();
        }
    }
}
