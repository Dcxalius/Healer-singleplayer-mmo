using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal abstract class State
    {
        public abstract void Update();
        public abstract void OnEnter();
        public abstract void OnLeave();

        public abstract void Rescale();

        public abstract void PopUp(DialogueBox aBox);
        public abstract void RemovePopUp(DialogueBox aBox);

        public abstract bool Click(ClickEvent aClickEvent);
        public abstract bool Release(ReleaseEvent aReleaseEvent);
        public abstract bool Scroll(ScrollEvent aScrollEvent);
        public abstract RenderTarget2D Draw();

    }
}
