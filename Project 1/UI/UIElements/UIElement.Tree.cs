using Project_1.GameObjects.Entities.Friendlies.Players;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        protected void KillAllChildren(UIElement child) => KillAllChildren(new List<UIElement>() { child });

        protected void KillAllChildren(List<UIElement> sparedChildren)
        {
            List<UIElement> remainingLast = new List<UIElement>();
            List<UIElement> remainingFirst = new List<UIElement>();
            foreach (UIElement child in sparedChildren)
            {
                if (lastChildren.Contains(child)) remainingLast.Add(child);
                if (firstChildren.Contains(child)) remainingFirst.Add(child);
            }
            KillAllChildren();
            children.AddRange(sparedChildren);
            firstChildren.AddRange(remainingFirst);
            lastChildren.AddRange(remainingLast);
            MarkRenderStale();
        }

        protected virtual void KillAllChildren()
        {
            children.Clear();
            lastChildren.Clear();
            firstChildren.Clear();
            clipChildren.Clear();
            MarkRenderStale();
        }

        protected virtual void KillChild(int aIndex)
        {
            UIElement child = children[aIndex];
            children.RemoveAt(aIndex);
            lastChildren.Remove(child);
            firstChildren.Remove(child);
            clipChildren.Remove(child);
            child.clipAttachment = default;
            MarkRenderStale();
        }

        protected virtual void KillChild(UIElement aChild)
        {
            Debug.Assert(children.Contains(aChild));
            children.Remove(aChild);
            lastChildren.Remove(aChild);
            firstChildren.Remove(aChild);
            clipChildren.Remove(aChild);
            aChild.clipAttachment = default;
            MarkRenderStale();
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
            MarkRenderStale();
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
            MarkRenderStale();
        }

        public T ClipLeft<T>(T child, float startingHeight) where T : UIElement, IClipChild => Clip(child, ClipSide.Left, startingHeight);
        public T ClipRight<T>(T child, float startingHeight) where T : UIElement, IClipChild => Clip(child, ClipSide.Right, startingHeight);
        public T ClipTop<T>(T child, float startingWidth) where T : UIElement, IClipChild => Clip(child, ClipSide.Top, startingWidth);
        public T ClipBottom<T>(T child, float startingWidth) where T : UIElement, IClipChild => Clip(child, ClipSide.Bottom, startingWidth);

        T Clip<T>(T child, ClipSide side, float start) where T : UIElement, IClipChild
        {
            Debug.Assert(child != null, "Cannot add null ClipChild.");
            Debug.Assert(child.parent == this, "ClipChild must be constructed with this UIElement as parent.");
            Debug.Assert(start >= 0f, "ClipChild start cannot be negative.");

            children.Remove(child);
            firstChildren.Remove(child);
            lastChildren.Remove(child);
            if (!clipChildren.Contains(child))
            {
                clipChildren.Add(child);
            }

            child.clipAttachment = new ClipAttachment(this, side, start);
            AssertClipChildFits(child, side, start);
            child.MarkRenderStale();
            MarkRenderStale();
            return child;
        }

        void AssertClipChildFits(UIElement child, ClipSide side, float start)
        {
            if (side == ClipSide.Left || side == ClipSide.Right)
            {
                Debug.Assert(start + child.Size.Y <= Size.Y, "ClipChild height exceeds the vertical clip strip.");
                return;
            }

            Debug.Assert(start + child.Size.X <= Size.X, "ClipChild width exceeds the horizontal clip strip.");
        }
    }
}
