using System;
using Project_1.GameObjects;
using Project_1.Managers;
using Project_1.Managers.Saves;
using Project_1.Messaging.Events;
using Project_1.Tiles;

namespace Project_1.Managers.States
{
    internal static partial class StateManager
    {
        static void HandleResetToMainMenuRequested()
        {
            ThreadAffinity.AssertSimThread();
            SetState(States.StartScreen);
            ObjectManager.Reset();
        }

        static void HandleCreateNewPlayerRequested(CreateNewPlayerRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (string.IsNullOrWhiteSpace(e.Name) || string.IsNullOrWhiteSpace(e.ClassName)) return;
            ObjectManager.CreateNewPlayer(e.Name, e.ClassName);
            SaveManager.CreateNewSave(e.Name);
            SetState(States.Game);
        }

        static void HandleSaveDataRequested()
        {
            ThreadAffinity.AssertSimThread();
            SaveManager.SaveData();
        }

        static void HandleLoadSaveRequested(LoadSaveRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (string.IsNullOrWhiteSpace(e.SaveName))
            {
                if (!SaveManager.RequestLoadData(SaveManager.CurrentSave)) return;
                SetState(States.LoadingMenu);
                return;
            }

            if (!SaveManager.TryGetSaveByName(e.SaveName, out Save save)) return;
            if (!SaveManager.RequestLoadData(save)) return;
            SetState(States.LoadingMenu);
        }

        static void HandleContinueLastSaveRequested()
        {
            ThreadAffinity.AssertSimThread();
            if (!SaveManager.RequestContinueLastSave()) return;
            SetState(States.LoadingMenu);
        }

        static void HandleNewGameRequested()
        {
            ThreadAffinity.AssertSimThread();
            TileManager.New();
            SetState(States.NewGame);
        }

        static void HandleSaveLoadParsed(SaveLoadParsed e)
        {
            ThreadAffinity.AssertSimThread();
            if (e.Payload == null) return;
            if (!SaveManager.ApplyLoadPayload(e.Payload, e.RequestId)) return;
            SetState(States.Game);
            RedrawGame();
        }
    }
}
