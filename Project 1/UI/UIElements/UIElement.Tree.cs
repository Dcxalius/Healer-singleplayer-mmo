using Project_1.GameObjects.Entities.Friendlies.Players;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        protected virtual void KillAllChildren()
        {
            children.Clear();
            lastChildren.Clear();
            firstChildren.Clear();
        }

        protected virtual void KillChild(int aIndex)
        {
            UIElement child = children[aIndex];
            children.RemoveAt(aIndex);
            lastChildren.Remove(child);
            firstChildren.Remove(child);
        }

        protected virtual void KillChild(UIElement aChild)
        {
            Debug.Assert(children.Contains(aChild));
            children.Remove(aChild);
            lastChildren.Remove(aChild);
            firstChildren.Remove(aChild);
        }

        protected UIElement GetChild(int aIndex) => children[aIndex];
        protected int GetChildID(UIElement aChild) => children.IndexOf(aChild);

        protected void ForAllChildren(Action<UIElement> aAction)
        {
            for (int i = 0; i < children.Count; i++)
            {
                aAction(children[i]);
            }
        }

        //protected void PlaceChildLast(UIElement aUIElement)
        //{
        //    Debug.Assert(aUIElement != null, "Cannot add null child to UIElement.");
        //    Debug.Assert(children.Contains(aUIElement), "Duplicate child added to UIElement. This can cause issues with removing children. Make sure to only add a child once.");
        //    if (aUIElement.parent != this)
        //    {
        //        throw new InvalidOperationException($"{aUIElement.GetType().Name} must be constructed with parent {GetType().Name} before PlaceChildLast.");
        //    }
        //    children.Remove(aUIElement);
        //    children.Add(aUIElement);
        //}

        public enum ChildSort
        {
            First,
            Last
        }

        public void AddChildToSort(UIElement aChild, ChildSort aSort)
        {
            Debug.Assert(aChild != null, "Cannot add null child to UIElement.");
            Debug.Assert(children.Contains(aChild));
            if (aChild.parent != this)
            {
                throw new InvalidOperationException($"{aChild.GetType().Name} must be constructed with parent {GetType().Name} before AddToSort.");
            }
            if (aSort == ChildSort.First)
            {
                Debug.Assert(!firstChildren.Contains(aChild), "A child cannot be in the same list twice.");
                Debug.Assert(!lastChildren.Contains(aChild), "A child cannot be in both first and last sort lists.");

                firstChildren.Add(aChild);
                Sort();
                
            }
            else
            {
                Debug.Assert(!lastChildren.Contains(aChild), "A child cannot be in the same list twice.");
                Debug.Assert(!firstChildren.Contains(aChild), "A child cannot be in both first and last sort lists.");

                lastChildren.Add(aChild);
                Sort();
                
            }
        }

        void AddChild(UIElement aUIElement)
        {
            Debug.Assert(aUIElement != null, "Cannot add null child to UIElement.");
            Debug.Assert(!children.Contains(aUIElement), "Duplicate child added to UIElement. This can cause issues with removing children. Make sure to only add a child once.");
            if (aUIElement.parent != this)
            {
                throw new InvalidOperationException($"{aUIElement.GetType().Name} must be constructed with parent {GetType().Name} before AddChild.");
            }
            children.Add(aUIElement);
            Sort();
        }

        void Sort()
        {
            if (children.Count == 0) return;
            List<UIElement> sortedChildren = new List<UIElement>();
            sortedChildren.AddRange(firstChildren);
            for (int i = 0; i < children.Count; i++)
            {
                UIElement child = children[i];
                if (lastChildren.Contains(child)) continue;
                if (firstChildren.Contains(child)) continue;
                sortedChildren.Add(child);
            }
            sortedChildren.AddRange(lastChildren);

            children = sortedChildren;
        }
    }
}
