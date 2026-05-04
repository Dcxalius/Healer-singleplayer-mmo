using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System;
using System.Diagnostics;

namespace Project_1.UI.UIElements
{
    internal abstract partial class UIElement
    {
        public void HudMovableDraw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (!hudMoveable) return;

            if (gfx != null)
            {
                gfx.Draw(aBatch, AbsolutePos);
            }

            MovableGfx.Draw(aBatch, AbsolutePos);
            nameText.CentredDraw(aBatch, Location + Size / 2);
        }

        public virtual void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (!visible) return;

            if (gfx != null)
            {
                gfx.Draw(aBatch, AbsolutePos);
            }

            DrawChildren(aBatch);
        }
        protected void DrawChildren(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            if (children.Count == 0) return;
            //TODO: Lock this or change draw system
            GraphicsManager.CaptureScissor(this, AbsolutePos);
            try
            {
                foreach (UIElement child in children)
                {
                    //TODO: This collection is not thread safe, we should lock it or change the draw system
                    /*System.InvalidOperationException
                    HResult=0x80131509
                    Message=Collection was modified; enumeration operation may not execute.
                    Source=System.Private.CoreLib
                    StackTrace:
                    at System.ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion()
                    at System.Collections.Generic.List`1.Enumerator.MoveNext()
                    at Project_1.UI.UIElements.UIElement.Draw(SpriteBatch aBatch) in C:\Users\Cassandra\source\repos\Project 1\Project 1\UI\UIElements\UIElement.Rendering.cs:line 35
                    at Project_1.UI.UIElements.UIElement.Draw(SpriteBatch aBatch) in C:\Users\Cassandra\source\repos\Project 1\Project 1\UI\UIElements\UIElement.Rendering.cs:line 37
                    at Project_1.UI.UiDrawList.Draw(SpriteBatch batch) in C:\Users\Cassandra\source\repos\Project 1\Project 1\UI\UiDrawList.cs:line 166
                    at Project_1.Managers.States.GameState.UIDraw() in C:\Users\Cassandra\source\repos\Project 1\Project 1\System\Managers\States\GameState.cs:line 104
                    at Project_1.Managers.States.Game.Draw() in C:\Users\Cassandra\source\repos\Project 1\Project 1\System\Managers\States\Game.cs:line 113
                    at Project_1.Managers.States.StateManager.Draw() in C:\Users\Cassandra\source\repos\Project 1\Project 1\System\Managers\States\StateManager.cs:line 238
                    at Project_1.Game1.Draw(GameTime gameTime) in C:\Users\Cassandra\source\repos\Project 1\Project 1\Game1.cs:line 130
                    at Microsoft.Xna.Framework.Game.DoDraw(GameTime gameTime)
                    at Microsoft.Xna.Framework.Game.Tick()
                    at Microsoft.Xna.Framework.SdlGamePlatform.RunLoop()
                    at Microsoft.Xna.Framework.Game.Run(GameRunBehavior runBehavior)
                    at Project_1.Program.Main(String[] args) in C:\Users\Cassandra\source\repos\Project 1\Project 1\Program.cs:line 16*/
                    try
                    {
                        child.Draw(aBatch);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UI child draw failed '{child?.GetType().Name ?? "<null>"}': {ex.GetType().Name}: {ex.Message}");
                        Debug.WriteLine($"UI child draw failed '{child?.GetType().Name ?? "<null>"}' and was skipped: {ex}");
                    }
                }
            }
            finally
            {
                GraphicsManager.ReleaseScissor(this);
            }
        }

    }   
}
