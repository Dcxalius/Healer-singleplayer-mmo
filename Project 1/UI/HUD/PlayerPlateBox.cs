using Microsoft.Xna.Framework;
using Project_1.GameObjects;
using Project_1.Textures;
using Project_1.UI.UIElements.PlateBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Project_1.UI.HUD
{
    internal class PlayerPlateBox : PlateBox
    {
        static Player p = ObjectManager.Player;

        //static PlateBoxSegment[] a = new PlateBoxSegment[] { new PlateBoxNameSegment(p.Name, aPos, new Vector2(aSize.X, aSize.Y)) };
        
        public PlayerPlateBox(ref Rectangle aParentPos, Vector2 aPos, Vector2 aSize) : base(ref aParentPos, null, null, aPos, aSize)
        {
            

        }

        
    }
}
