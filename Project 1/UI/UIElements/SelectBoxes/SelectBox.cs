using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.UI.UIElements.SelectBoxes
{
    internal abstract class SelectBox : Box
    {
        bool isOpen;
        float openMaxSize;
        protected SelectBoxValue[] values;
        int selectedValue;
        SelectBoxValueDisplay displayValue;

        Vector2 defaultPos;
        Vector2 defaultSize;

        public SelectBox(UITexture aGfx, SelectBoxValue[] aSetOfValues, int aStartDisplayValue, Vector2 aPos, Vector2 aCollapsedSize) : base(aGfx, aPos, aCollapsedSize)
        {
            defaultPos = aPos;
            defaultSize = aCollapsedSize;
            values = aSetOfValues;
            displayValue = new SelectBoxValueDisplay(aSetOfValues[aStartDisplayValue], aGfx, aPos, aCollapsedSize);
            selectedValue = aStartDisplayValue;
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
            DebugManager.Print(GetType(), "New selectedValue is: " + tempSelectedValue);

            if (tempSelectedValue == 0 || tempSelectedValue == selectedValue + 1)
            {
                return;
            }

            ActionWhenSelected(tempSelectedValue - 1);
        }

        protected virtual void ActionWhenSelected(int aSelectedValue)
        {

            selectedValue = aSelectedValue;
            displayValue.SetToNewValue(values[aSelectedValue]);
        }

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

            pos.Size = new Point(pos.Size.X, TransformFromRelativeToPoint(defaultSize * (values.Length + 1)).Y);

        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);

            displayValue.Draw(aBatch);

            if (isOpen == false)
            {
                return;
            }
            for (int i = 0; i < values.Length; i++)
            {
                values[i].Draw(aBatch);
            }
        }

        public override void Rescale()
        {
            base.Rescale();

            displayValue.Rescale();

            for (int i = 0; i < values.Length; i++)
            {
                values[i].Rescale();
            }
        }
    }
}
