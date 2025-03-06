using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI.LoadingMenu;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.States
{
    internal class LoadingMenu : State
    {
        LoadingBox loadingBox;
        public LoadingMenu() : base()
        {
            loadingBox = new LoadingBox(new Camera.RelativeScreenPosition(0.05f, 0.05f), new Camera.RelativeScreenPosition(0.9f, 0.9f));
        }

        public override StateManager.States GetStateEnum => StateManager.States.LoadingMenu;



        public override bool Click(ClickEvent aClickEvent) => loadingBox.ClickedOn(aClickEvent);

        public override RenderTarget2D Draw()
        {
            PrepRender(Color.Lime);
            loadingBox.Draw(spriteBatch);

            CleanRender();
            return renderTarget;
        }

        public override void OnEnter()
        {
            //TODO: Load files
        }

        public override void OnLeave()
        {
            //TODO: Unload files
        }

        #region NYI
        public override void PopUp(DialogueBox aBox) => throw new NotImplementedException();

        public override bool Release(ReleaseEvent aReleaseEvent) => throw new NotImplementedException();

        public override void RemovePopUp(DialogueBox aBox) => throw new NotImplementedException();
        #endregion
        public override bool Scroll(ScrollEvent aScrollEvent) => loadingBox.ScrolledOn(aScrollEvent);

        public override void Update() => loadingBox.Update();
    }
}
