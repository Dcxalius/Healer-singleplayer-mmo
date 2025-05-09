using Project_1.Camera;
using Project_1.GameObjects.Entities.GroundEffect;
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

        protected List<GroundEffect> groundEffects; //TODO: Consider adding a rectangle for only these effects rather than using modded screenrect

        public WorldObject(Texture aTexture, WorldSpace aStartingPos) : base(aTexture, aStartingPos)
        {
            groundEffects = new List<GroundEffect>();
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

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            for (int i = 0; i < groundEffects.Count; i++)
            {
                groundEffects[i].Draw(aBatch, this);
            }
            base.Draw(aBatch);
        }
    }
}
