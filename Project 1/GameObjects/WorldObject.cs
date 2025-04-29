using Project_1.Camera;
using Project_1.Input;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal abstract class WorldObject : MovingObject
    {
        public bool CapturesClick { get => capturesClick; protected set => capturesClick = value; }
        bool capturesClick;


        public WorldObject(Texture aTexture, WorldSpace aStartingPos) : base(aTexture, aStartingPos)
        {
            capturesClick = false;
        }

        //public override float MaxSpeed => throw new NotImplementedException();

        public virtual bool Click(ClickEvent aClickEvent)
        {
            if (Camera.Camera.WorldRectToScreenRect(WorldRectangle).Contains(aClickEvent.AbsolutePos.ToPoint()))
            {
                ClickedOn(aClickEvent);

                return true;
            }
            return capturesClick;
        }

        public virtual void ClickedOn(ClickEvent aEvent) { }
    }
}
