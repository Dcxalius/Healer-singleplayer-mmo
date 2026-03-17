using Project_1.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Input
{
    internal class ScrollEvent : Modifiable
    {
        public enum Direction
        {
            Up,
            Down
        }
        public AbsoluteScreenPosition AbsolutePos { get => AbsoluteScreenPosition.FromRelativeScreenPosition(pos); }
        public RelativeScreenPosition RelativePos { get => pos; }
        RelativeScreenPosition pos;

        public int DirectionAndSteps
        {
            get
            {
                if (direction == Direction.Down)
                {
                    return -steps;
                }
                return steps;
            }
        }
        public int Steps => steps;
        int steps;

        public bool Up => direction == Direction.Up;
        public bool Down => direction == Direction.Down;
        Direction direction;

        protected override byte ModifiersMask => modifierMask;
        byte modifierMask;


        public ScrollEvent(AbsoluteScreenPosition aPos, int aSteps, Direction aDirection, bool[] aModifiers) : this(aPos.ToRelativeScreenPosition(), aSteps, aDirection, aModifiers) { }
        public ScrollEvent(AbsoluteScreenPosition aPos, int aSteps, Direction aDirection, byte modifiersMask) : this(aPos.ToRelativeScreenPosition(), aSteps, aDirection, modifiersMask) { }

        public ScrollEvent(RelativeScreenPosition aPos, int aSteps, Direction aDirection, bool[] aModifiers)
            : this(aPos, aSteps, aDirection, BuildModifiersMask(aModifiers))
        {
        }

        public ScrollEvent(RelativeScreenPosition aPos, int aSteps, Direction aDirection, byte modifiersMask)
        {
            pos = aPos;
            steps = aSteps;
            direction = aDirection;
            modifierMask = modifiersMask;
        }



    }
}
