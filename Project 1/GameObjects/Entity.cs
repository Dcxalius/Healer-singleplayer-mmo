using Microsoft.Xna.Framework;
using Project_1.Textures;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects
{
    internal class Entity : MovingObject
    {
        static Texture ShadowTexture = new Texture(new GfxPath(GfxType.Object, "Shadow"));
        Rectangle shadowPos;

        public Entity(Texture aTexture, Vector2 aStartingPos, float aMaxSpeed) : base(aTexture, aStartingPos, aMaxSpeed)
        {
            shadowPos = new Rectangle((pos + new Vector2(size.X/2, size.Y)).ToPoint()  , size);

        }

        public override void Update()
        {
            base.Update();

            Vector2 offset = new Vector2(0, size.Y / 2.5f);
            shadowPos.Location = (pos + offset ).ToPoint() ;
            shadowPos.Size = (size.ToVector2()  * Camera.Scale).ToPoint();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch aBatch)
        {


            ShadowTexture.Draw(aBatch, Camera.WorldPosToCameraSpace(shadowPos));
            
            base.Draw(aBatch);

            
        }
    }
}
