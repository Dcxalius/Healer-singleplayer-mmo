using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.FloatingTexts
{
    internal class FloatingText
    {
        Color color;
        Color borderColor;
        float borderWidth;
        readonly string textValue;
        WorldSpace position;
        float speed;
        WorldSpace momentum;
        WorldSpace velocity;
        double spawnTime;

        double duration;
        Text builtText;

        public FloatingText(string aTextToDisplay, Color aColor, WorldSpace aStartPos, WorldSpace aHeadingVector, Color aBorderColor, float aBorderWidth, WorldSpace? aVelocity = null, float aSpeed = 75, double aDuration = 600d)
        {
            textValue = aTextToDisplay;
            speed = aSpeed;
            color = aColor;
            position = aStartPos;
            aHeadingVector.Y = -3;
            momentum = aHeadingVector * speed;
            spawnTime = TimeManager.TotalFrameTime;
            if (aVelocity.HasValue)
            {
                velocity = aVelocity.Value;
            }
            else
            {
                velocity = new WorldSpace(0, 9.8f);
            }
            duration = aDuration;
            borderColor = aBorderColor;
            borderWidth = aBorderWidth;
            builtText = new Text("Comfortaa-msdf", textValue, color, aTextSize: 30, aBorderColor: borderColor, aBorderWidth: borderWidth);
        }

        public void Update()
        {
            if (spawnTime + duration <= TimeManager.TotalFrameTime)
            {
                FloatingTextManager.DoWhatLeaguePlayersTellMe(this);
            }

            momentum *= 0.99f;
            momentum += velocity;
            position += momentum * (float)TimeManager.SecondsSinceLastFrame;

        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();

            builtText.TopLeftDraw(aBatch, position.ToAbsoltueScreenPosition());
        }
    }
}
