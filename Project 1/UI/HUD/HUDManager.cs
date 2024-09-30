using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.UI.UIElements;
using SharpDX.MediaFoundation.DirectX;
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
        static PlayerPlateBox playerPlateBox;
        static TargetPlateBox targetPlateBox;
        static PartyPlateBox[] partyPlateBoxes = new PartyPlateBox[4];

        static List<UIElement> hudElements = new List<UIElement>();


        public static void SetNewTarget(Entity aEntity)
        {
            targetPlateBox.SetEntity(aEntity);
        }


        public static void Init()
        {
            playerPlateBox = new PlayerPlateBox(new Vector2(0.1f, 0.1f), new Vector2(0.2f, 0.1f));
            targetPlateBox = new TargetPlateBox(new Vector2(0.33f, 0.1f), new Vector2(0.2f, 0.1f));
            hudElements.Add(playerPlateBox);
            hudElements.Add(targetPlateBox);
        }

        public static void AddWalkerToParty(Walker aWalker)
        {
            int openIndex = -1;
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                if (partyPlateBoxes[i] == null)
                {
                    openIndex = i;
                    break;
                }
            }

            if (openIndex == -1)
            {
                DebugManager.Print(typeof(HUDManager), "Tried to add to full party.");
                return;
            }

            partyPlateBoxes[openIndex] = new PartyPlateBox(aWalker, new Vector2(0.1f, 0.24f), new Vector2(0.2f, 0.1f));
            hudElements.Add(partyPlateBoxes[openIndex]);
        }

        public static void AddWalkerToControl(Walker aWalker)
        {
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                if (partyPlateBoxes[i].BelongsTo(aWalker))
                {
                    partyPlateBoxes[i].VisibleBorder = true;
                    break;
                }
            
            }
        }

        public static void RemoveWalkersFromControl(Walker[] aWalkers)
        {
            for (int i = 0; i < aWalkers.Length; i++)
            {
                for (int j = 0; j < partyPlateBoxes.Length; j++)
                {

                    if (partyPlateBoxes[j].BelongsTo(aWalkers[i]))
                    {
                        partyPlateBoxes[j].VisibleBorder = false;
                        break;
                    }
                }
            }
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
