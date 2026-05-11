using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.Input;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        List<UIElement> lastChildren;
        List<UIElement> firstChildren;

        static int nextUiElementId;
        static readonly object uiElementRegistryLock = new object();
        static readonly Dictionary<int, WeakReference<UIElement>> uiElementRegistry = new Dictionary<int, WeakReference<UIElement>>();
        static long interactionVersion;

        public static long InteractionVersion => Volatile.Read(ref interactionVersion);

        static void TouchInteraction()
        {
            Interlocked.Increment(ref interactionVersion);
        }

        public int UiElementId { get; }

        public static bool TryResolve(int uiElementId, out UIElement element)
        {
            element = null;
            lock (uiElementRegistryLock)
            {
                if (!uiElementRegistry.TryGetValue(uiElementId, out WeakReference<UIElement> reference))
                {
                    return false;
                }

                if (reference.TryGetTarget(out element) && element != null)
                {
                    return true;
                }

                uiElementRegistry.Remove(uiElementId);
                return false;
            }
        }

        public virtual bool Visible
        {
            get => parent != null ? parent.Visible ? visible : false : visible;
            set
            {
                if (visible == value) return;
                visible = value;

                TouchInteraction();
            }
        }

        bool visible;
        protected KeyBindManager.KeyListner? visibleKey;
        bool Hovered => AbsolutePos.Contains(UiMouseStateCache.Absolute.ToPoint());
        protected bool isHovered;

        public bool CapturesClick { get => capturesClick; set => capturesClick = value; }
        protected bool capturesClick;
        public bool CapturesScroll { get => capturesScroll; set => capturesScroll = value; }
        protected bool capturesScroll;
        public bool CapturesRelease { get => capturesRelease; set => capturesRelease = value; }
        protected bool capturesRelease;

        public bool AlwaysOnScreen
        {
            get => alwaysOnScreen;
            set
            {
                alwaysOnScreen = value;
                if (value)
                {
                    alwaysFullyOnScreen = false;
                }
            }
        }

        protected bool alwaysOnScreen;
        readonly Point alwaysOnScreenAmount = new Point(10, 10);

        public bool AlwaysFullyOnScreen
        {
            get => alwaysFullyOnScreen;
            protected set
            {
                alwaysFullyOnScreen = value;
                if (value)
                {
                    alwaysOnScreen = false;
                }
            }
        }

        bool alwaysFullyOnScreen;

        public bool Dragable
        {
            get => dragable;
            protected set => dragable = value;
        }

        bool dragable;
        RelativeScreenPosition oldPosition;
        protected bool hudMoving;
        public bool HudMoveable => hudMoveable;
        protected bool hudMoveable;
        static UITexture movableGfx;
        static bool movableGfxInitialized;

        static UITexture MovableGfx
        {
            get
            {
                if (movableGfxInitialized) return movableGfx;
                ThreadAffinity.AssertMainThread();
                movableGfx = new UITexture("MovableHUD", Color.White);
                movableGfxInitialized = true;
                return movableGfx;
            }
        }

        Text nameText;
        readonly TimeSpan timeBeforeDragRegisters = TimeSpan.FromSeconds(0.2);
        public HoldEvent heldEvents;

        enum ClipSide
        {
            None,
            Left,
            Right,
            Top,
            Bottom
        }

        readonly struct ClipAttachment
        {
            public ClipAttachment(UIElement owner, ClipSide side, float start)
            {
                Owner = owner;
                Side = side;
                Start = start;
            }

            public UIElement Owner { get; }
            public ClipSide Side { get; }
            public float Start { get; }
            public bool IsAttached => Owner != null && Side != ClipSide.None;
        }

        public RelativeScreenPosition RelativePos => relativePos;
        RelativeScreenPosition relativePos;
        public RelativeScreenPosition RelativeSize => relativeSize;
        RelativeScreenPosition relativeSize;

        public RelativeScreenPosition RelativePositionOnScreen => Location.ToRelativeScreenPosition();

        public Rectangle AbsolutePos
        {
            get
            {
                Rectangle tempRec = Rectangle.Empty;
                tempRec.Location = Location;
                tempRec.Size = Size;
                return tempRec;
            }
        }

        public AbsoluteScreenPosition Location => ParentPos + (RelativePos * ParentRelativeSize).ToAbsoluteScreenPos();
        public AbsoluteScreenPosition Size => (RelativeSize * ParentRelativeSize).ToAbsoluteScreenPos();
        AbsoluteScreenPosition NormalLocation => ParentPos + (RelativePos * ParentRelativeSize).ToAbsoluteScreenPos();

        protected UIElement parent;
        ClipAttachment clipAttachment;
        protected AbsoluteScreenPosition ParentPos => clipAttachment.IsAttached ? clipAttachment.Owner.GetClipParentLocation(clipAttachment) : parent == null ? AbsoluteScreenPosition.Zero : parent.NormalLocation;
        protected RelativeScreenPosition ParentRelativePos => parent == null ? RelativeScreenPosition.Zero : parent.RelativePos;
        protected AbsoluteScreenPosition ParentSize => clipAttachment.IsAttached ? clipAttachment.Owner.GetClipParentSize(clipAttachment) : parent == null ? Camera.Camera.WindowSize : parent.Size;
        protected RelativeScreenPosition ParentRelativeSize => ParentSize.ToRelativeScreenPosition();

        List<UIElement> children = new List<UIElement>();
        readonly List<UIElement> clipChildren = new List<UIElement>();
        protected int ChildCount => children.Count;

        public UITexture Gfx => gfx;
        protected UITexture gfx;
        public virtual Color Color
        {
            get => gfx.Color;
            set
            {
                if (gfx.Color == value) return;
                gfx.Color = value;
                MarkRenderStale();
            }
        }

        public (string, RelativeScreenPosition, RelativeScreenPosition) Save => (GetType().Name, RelativePos, RelativeSize);

        protected UIElement(UIElement aParent, UITexture aGfx, RelativeScreenPosition aPos, RelativeScreenPosition aSize)
        {
            UiElementId = Interlocked.Increment(ref nextUiElementId);
            lock (uiElementRegistryLock)
            {
                uiElementRegistry[UiElementId] = new WeakReference<UIElement>(this);
            }

            visible = true;
            if (aParent != null)
            {
                parent = aParent;
                parent.AddChild(this);
            }
            gfx = aGfx;
            relativePos = aPos;
            relativeSize = aSize;
            nameText = new Text("Comfortaa-msdf", GetType().Name);

            capturesClick = true; //TODO: Should the default be false?
            capturesScroll = false;
            capturesRelease = true;
            alwaysOnScreen = false;
            alwaysFullyOnScreen = false;
            hudMoveable = true;

            firstChildren = new List<UIElement>();
            lastChildren = new List<UIElement>();
        }

    }
}
