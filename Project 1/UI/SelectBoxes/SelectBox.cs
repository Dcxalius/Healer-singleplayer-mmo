using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Content.Input;
using Project_1.Managers;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.SelectBoxes
{
    internal abstract class SelectBox : Box
    {
        bool isOpen;
        float openMaxSize;
        SelectBoxValue[] values;
        int selectedValue;
        SelectBoxValueDisplay displayValue;

        Vector2 defaultPos;
        Vector2 defaultSize;

        public SelectBox(UITexture aGfx, SelectBoxValue[] aSetOfValues, int aStartDisplayValue, Vector2 aPos, Vector2 aSize) : base(aGfx, aPos, aSize)
        {
            defaultPos = aPos;
            defaultSize = aSize;
            values = aSetOfValues;
            displayValue = new SelectBoxValueDisplay(aSetOfValues[aStartDisplayValue], aGfx);
        }

        public override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            if (isOpen == true)
            {
                Close(aClick);
            }
            else 
            { 
                Open();
            }
        }

        void SetNewValue(Point aP)
        {
            int tempSelectedValue = aP.Y / TransformFromRelativeToPoint(defaultSize).Y;
            DebugManager.Print(GetType(),"New selectedValue is: " + tempSelectedValue);

            if (tempSelectedValue == 0 || tempSelectedValue == selectedValue)
            {
                return;
            }
            
            ActionWhenSelected(tempSelectedValue);
            selectedValue = tempSelectedValue;
        }

        protected abstract void ActionWhenSelected(int aSelectedValue);

        void Close(ClickEvent aClick)
        {
            isOpen = false;

            pos.Size = TransformFromRelativeToPoint(defaultSize);

            Point target = aClick.ClickPos - pos.Location;

            SetNewValue(target);
        }

        void Open()
        {
            isOpen = true;

            pos.Size = new Point(pos.Size.X, TransformFromRelativeToPoint(defaultSize * values.Length).Y);

        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);


        }
    }
}
