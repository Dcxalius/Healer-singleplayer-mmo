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
        protected List<GroundEffect> groundEffects; //TODO: Consider adding a rectangle for only these effects rather than using modded screenrect

        public WorldObject(Texture aTexture, WorldSpace aStartingPos) : base(aTexture, aStartingPos)
        {
            groundEffects = new List<GroundEffect>();
            
        }

        //public override float MaxSpeed => throw new NotImplementedException();

        public bool HitTest(WorldSpace worldPos) => WorldRectangle.Contains(worldPos.ToPoint());

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {
            Project_1.Managers.ThreadAffinity.AssertMainThread();
            for (int i = 0; i < groundEffects.Count; i++)
            {
                groundEffects[i].Draw(aBatch, this);
            }
            base.Draw(aBatch);
        }
    }
}
