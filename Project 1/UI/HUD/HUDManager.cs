using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.UI.UIElements;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.HUD
{
    internal static class HUDManager
    {
        static PlayerPlateBox playerPlateBox = new PlayerPlateBox(new Vector2(0.1f, 0.1f), new Vector2(0.3f, 0.1f));
        static TargetPlateBox targetPlateBox = new TargetPlateBox(new Vector2(0.1f, 0.3f), new Vector2(0.3f, 0.1f));

        static List<UIElement> hudElements = new List<UIElement>();


        public static void SetNewTarget(Entity aEntity)
        {
            targetPlateBox.SetEntity(aEntity);
        }


        public static void Init()
        {
            hudElements.Add(playerPlateBox);
            hudElements.Add(targetPlateBox);
        }



        public static void Update()
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Update(null);
            }
        }

        public static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Draw(aBatch);
            }

        }

    }
}
