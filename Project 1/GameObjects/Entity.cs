using Microsoft.Xna.Framework;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Project_1.GameObjects
{
    internal class Entity : MovingObject
    {
        public string Name { get => name; }
        public bool HasDestination { get => destinations.Count > 0; }

        Vector2 FeetPos { get => pos + new Vector2(size.X / 2, size.Y); }

        static Texture ShadowTexture = new Texture(new GfxPath(GfxType.Object, "Shadow"));
        Rectangle shadowPos;


        List<Vector2> destinations = new List<Vector2>();

        int speed = 50;

        string name;



        public Entity(Texture aTexture, Vector2 aStartingPos, float aMaxSpeed) : base(aTexture, aStartingPos, aMaxSpeed)
        {
            shadowPos = new Rectangle((pos + new Vector2(size.X/2, size.Y)).ToPoint(), size);

            name = "xdd";
        }

        public override bool Click(ClickEvent aClickEvent)
        {
            if (Camera.WorldPosToCameraSpace(ScreenRectangle).Contains(aClickEvent.ClickPoint))
            {
                ClickedOn(aClickEvent);
                return true;
            }
            return false;
        }

        protected virtual void ClickedOn(ClickEvent aClickEvent) { }

        Vector2 GetDirVectorToNextDestination(Vector2 aDestination, out float aLenghtTillDestination)
        {
            Vector2 dirV = aDestination - FeetPos;
            aLenghtTillDestination = dirV.Length();
            dirV.Normalize();
            return dirV;
        }

        void Walk()
        { 
            if (destinations.Count == 0)
            {
                return;
            }
            float length = 0;
            Vector2 directionToWalk = GetDirVectorToNextDestination(destinations[0], out length);

            if (length < speed * 0.9f) //TODO: Find a good factor
            {
                destinations.RemoveAt(0);

                return;
            }


            velocity += directionToWalk * speed * (float)TimeManager.SecondsSinceLastFrame;
        }

        protected void OverwriteDestination(Vector2 aDestination)
        {
            destinations.Clear();
            destinations.Add(aDestination);
        }

        protected void AddDestination(Vector2 aDestination) { destinations.Add(aDestination); }

        public override void Update()
        {
            Walk();
            
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
