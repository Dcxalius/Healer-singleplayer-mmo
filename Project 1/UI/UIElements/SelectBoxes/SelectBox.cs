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
        //TODO: Add a check to see if this should open up or down
        bool isOpen;

        protected SelectBoxValue[] values;
        protected int selectedValue;
        protected SelectBoxValueDisplay displayValue;

        protected ScrollableBox allValues;

        RelativeScreenPosition defaultPos;
        RelativeScreenPosition defaultSize;

        const float sizeMulti = 5;

        public SelectBox(UITexture aGfx, int aStartDisplayValue, RelativeScreenPosition aPos, RelativeScreenPosition aCollapsedSize) : base(aGfx, aPos, aCollapsedSize) 
        {
            defaultPos = aPos;
            defaultSize = aCollapsedSize;
            selectedValue = aStartDisplayValue;

            allValues = new ScrollableBox(5, UITexture.Null, Color.AliceBlue, new RelativeScreenPosition(ScrollableBox.WidthOfBar + ScrollableBox.WidthOfSpacing * 2, 1 / sizeMulti), new RelativeScreenPosition(1 - ScrollableBox.WidthOfBar - ScrollableBox.WidthOfSpacing * 3, 1 - 1 / sizeMulti));
            AddChild(allValues);
            allValues.Visible = false;
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
                Close();
            }
            else
            {
                Open();
            }
        }

        void SetNewValue(int aIndex)
        {
            if (aIndex == selectedValue) return;

            ActionWhenSelected(aIndex);
        }

        protected virtual void ActionWhenSelected(int aSelectedValue)
        {
            selectedValue = aSelectedValue;
            displayValue.SetToNewValue(values[aSelectedValue]);
        }

        public override void Close()
        {
            isOpen = false;

            displayValue.Resize(RelativeScreenPosition.One);

            allValues.Visible = false;
            Resize(defaultSize);
        }

        public void ClickedOnSelectBoxValue(SelectBoxValue aVal)
        {
            //displayValue
        }

        public void Close(SelectBoxValue aVal)
        {
            Close();

            SetNewValue(Array.IndexOf(values, aVal));
        }

        void Open()
        {
            OptionManager.CloseAllOptionMenuStuff();

            isOpen = true;

            displayValue.Resize(new RelativeScreenPosition(1, 1 / (sizeMulti)));
            Resize(new RelativeScreenPosition(RelativeSize.X, defaultSize.Y * sizeMulti));
            allValues.Visible = true;
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
