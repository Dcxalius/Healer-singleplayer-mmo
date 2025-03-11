using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Textures;
using Project_1.UI.OptionMenu;
using Project_1.UI.UIElements.Boxes;
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
        //TODO: float openMaxSize;
        protected SelectBoxValue[] values;
        int selectedValue;
        protected SelectBoxValueDisplay displayValue;

        RelativeScreenPosition defaultPos;
        RelativeScreenPosition defaultSize;

        public SelectBox(UITexture aGfx, int aStartDisplayValue, RelativeScreenPosition aPos, RelativeScreenPosition aCollapsedSize) : base(aGfx, aPos, aCollapsedSize)
        {
            defaultPos = aPos;
            defaultSize = aCollapsedSize;
            selectedValue = aStartDisplayValue;
        }

        public override void Update()
        {
            base.Update();


        }

        protected override void ClickedOnMe(ClickEvent aClick)
        {
            base.ClickedOnMe(aClick);

            //DebugManager.Print(this.GetType(), pos.ToString());

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
            //DebugManager.Print(GetType(), "New selectedValue is: " + tempSelectedValue);

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

        public override void Close()
        {
            isOpen = false;

            Resize(defaultSize);
        }

        void Close(ClickEvent aClick)
        {
            Close();

            Point target = aClick.RelativePos.ToAbsoluteScreenPos() - AbsolutePos.Location;

            SetNewValue(target);
        }

        void Open()
        {
            OptionManager.CloseAllOptionMenuStuff();

            isOpen = true;

            Resize(new RelativeScreenPosition(Size.X, defaultSize.Y * (values.Length + 1)));

        }

        public override void Draw(SpriteBatch aBatch)
        {
            base.Draw(aBatch);
         
            //displayValue.Draw(aBatch); 

            if (isOpen == false)
            {
                return;
            }
            //for (int i = 0; i < values.Length; i++)
            //{
            //    values[i].Draw(aBatch);
            //}
        }

        public override void Rescale()
        {
            base.Rescale();

            //displayValue.Rescale();

            //for (int i = 0; i < values.Length; i++)
            //{
            //    values[i].Rescale();
            //}
        }
    }
}
