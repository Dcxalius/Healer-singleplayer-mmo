using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            Debug.Assert(aUIElement != null, "Cannot add null child to UIElement.");
            Debug.Assert(children.Contains(aUIElement), "Duplicate child added to UIElement. This can cause issues with removing children. Make sure to only add a child once.");
            if (aUIElement.parent != this)
            {
                throw new InvalidOperationException($"{aUIElement.GetType().Name} must be constructed with parent {GetType().Name} before AddChild.");
            }
            children.Add(aUIElement);
        }

        protected void AddChildren(UIElement[] aUIElement)
        {
            for (int i = 0; i < aUIElement.Length; i++)
            {
                AddChild(aUIElement[i]);
            }
        }

        protected void AddChildren<T>(List<T> aUIElement) where T : UIElement
        {
            for (int i = 0; i < aUIElement.Count; i++)
            {
                AddChild(aUIElement[i]);
            }
        }
    }
}
