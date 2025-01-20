using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
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

        public abstract bool Click(ClickEvent aClickEvent);
        public abstract bool Release(ReleaseEvent aReleaseEvent);
        public abstract bool Scroll(ScrollEvent aScrollEvent);
        public abstract void Draw(SpriteBatch aBatch);

    }
}
