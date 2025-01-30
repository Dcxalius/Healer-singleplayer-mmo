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

namespace Project_1.GameObjects
{
    internal class FloatingText
    {
        Text text;
        Color color;
        WorldSpace position;
        float speed;
        WorldSpace momentum;
        WorldSpace velocity;
        double spawnTime;

        double duration;

        RenderTarget2D renderTarget;
        SpriteBatch spriteBatch;

        public FloatingText(string aTextToDisplay, Color aColor, WorldSpace aStartPos, WorldSpace aHeadingVector, WorldSpace? aVelocity = null, float aSpeed = 75, double aDuration = 600d)
        {
            text = new Text("Gloryse", aTextToDisplay, aColor);
            Point textSize = text.Offset.ToPoint();
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

            //renderTarget = GraphicsManager.CreateRenderTarget(new Point(30, 10));
            renderTarget = GraphicsManager.CreateRenderTarget(textSize); //Once effect is properly implement test to see if its quicker to create rendertargets for every one or have a render target for all
            spriteBatch = GraphicsManager.CreateSpriteBatch();

            GraphicsManager.SetRenderTarget(renderTarget);
            GraphicsManager.ClearScreen(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred);
            //spriteBatch.Begin(samplerState : SamplerState.PointClamp, effect : TextureManager.textOutline);
            text.CentreLeftDraw(spriteBatch, new AbsoluteScreenPosition(0, textSize.Y / 2));
            
            spriteBatch.End();
            GraphicsManager.SetRenderTarget(null);
            //Stream stream = File.OpenWrite("xdd.png");
            //renderTarget.SaveAsPng(stream, textSize.X, textSize.Y);
            //stream.Close();
        }

        public void Update()
        {
            if (spawnTime + duration <= TimeManager.TotalFrameTime)
            {
                ObjectManager.DoWhatLeaguePlayersTellMe(this);
            }

            momentum *= 0.99f;
            momentum += velocity;
            position += momentum * (float)TimeManager.SecondsSinceLastFrame;

        }

        public void Draw(SpriteBatch aBatch)
        {
            aBatch.Draw(renderTarget, position.ToAbsoltueScreenPosition().ToVector2(), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            //aBatch.Draw(renderTarget, Camera.Camera.WorldPosToCameraSpace(position), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }
    }
}
