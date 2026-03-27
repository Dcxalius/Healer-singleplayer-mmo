using System;
using System.Collections.Generic;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        protected virtual void KillAllChildren() => children.Clear();
        protected virtual void KillChild(int aIndex) => children.RemoveAt(aIndex);
        protected virtual void KillChild(UIElement aChild) => children.Remove(aChild);
        protected UIElement GetChild(int aIndex) => children[aIndex];
        protected int GetChildID(UIElement aChild) => children.IndexOf(aChild);

        protected void ForAllChildren(Action<UIElement> aAction)
        {
            for (int i = 0; i < children.Count; i++)
            {
                aAction(children[i]);
            }
        }

        protected virtual void AddChild(UIElement aUIElement)
        {
            aUIElement.parent = this;
            children.Add(aUIElement);
        }

        protected void AddChildren(UIElement[] aUIElement)
        {
            for (int i = 0; i < aUIElement.Length; i++)
            {
                aUIElement[i].parent = this;
                AddChild(aUIElement[i]);
            }
        }

        protected void AddChildren<T>(List<T> aUIElement) where T : UIElement
        {
            for (int i = 0; i < aUIElement.Count; i++)
            {
                aUIElement[i].parent = this;
                AddChild(aUIElement[i]);
            }
        }
    }
}
