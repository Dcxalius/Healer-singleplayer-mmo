using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities
{
    internal class VisualEffect
    {
        public bool IsFinished { get => startTime + duration < TimeManager.TotalFrameTime; }
        public Point Size { get => texture.ScaledSize; set => texture.size = value; }

        double startTime;
        double duration;
        Textures.Texture texture;

        public VisualEffect(GfxPath aPath, double aDuration)
        {
            startTime = TimeManager.TotalFrameTime;
            duration = aDuration;
            texture = new Textures.Texture(aPath);
        }


        public void Draw(SpriteBatch aBatch, WorldSpace aPos, float aFeetPos)
        {
            texture.Draw(aBatch, aPos.ToAbsoltueScreenPosition().ToVector2(), aFeetPos);
        }
    }
}
