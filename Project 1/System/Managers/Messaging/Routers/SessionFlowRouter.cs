using Project_1.GameObjects;
using Project_1.Managers.Saves;
using Project_1.Managers.States;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Tiles;

namespace Project_1.Managers
{
    internal static class SessionFlowRouter
    {
        static bool initialized;

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            SubscribeSimCommand<ResetToMainMenuRequested>(_ => HandleResetToMainMenuRequested());
            SubscribeSimCommand<CreateNewPlayerRequested>(HandleCreateNewPlayerRequested);
            SubscribeSimCommand<SaveDataRequested>(_ => HandleSaveDataRequested());
            SubscribeSimCommand<LoadSaveRequested>(HandleLoadSaveRequested);
            SubscribeSimCommand<ContinueLastSaveRequested>(_ => HandleContinueLastSaveRequested());
            SubscribeSimCommand<NewGameRequested>(_ => HandleNewGameRequested());
            SubscribeSimCommand<SaveLoadParsed>(HandleSaveLoadParsed);
        }

        static void SubscribeSimCommand<T>(System.Action<T> handler)
        {
            Mailboxes.RegisterSimCommandType<T>();
            Mailboxes.Sim.Subscribe(handler);
        }

        static void HandleResetToMainMenuRequested()
        {
            ThreadAffinity.AssertSimThread();
            StateManager.SetState(StateManager.States.StartScreen);
            ObjectManager.Reset();
        }

        static void HandleCreateNewPlayerRequested(CreateNewPlayerRequested e)
        {
            ThreadAffinity.AssertSimThread();
            if (string.IsNullOrWhiteSpace(e.Name) || string.IsNullOrWhiteSpace(e.ClassName)) return;

            ObjectManager.CreateNewPlayer(e.Name, e.ClassName);
            SaveManager.CreateNewSave(e.Name);
            StateManager.SetState(StateManager.States.Game);
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
                StateManager.SetState(StateManager.States.LoadingMenu);
                return;
            }

            if (!SaveManager.TryGetSaveByName(e.SaveName, out Save save)) return;
            if (!SaveManager.RequestLoadData(save)) return;
            StateManager.SetState(StateManager.States.LoadingMenu);
        }

        static void HandleContinueLastSaveRequested()
        {
            ThreadAffinity.AssertSimThread();
            if (!SaveManager.RequestContinueLastSave()) return;
            StateManager.SetState(StateManager.States.LoadingMenu);
        }

        static void HandleNewGameRequested()
        {
            ThreadAffinity.AssertSimThread();
            TileManager.New();
            StateManager.SetState(StateManager.States.NewGame);
        }

        static void HandleSaveLoadParsed(SaveLoadParsed e)
        {
            ThreadAffinity.AssertSimThread();
            if (e.Payload == null) return;
            if (!SaveManager.ApplyLoadPayload(e.Payload, e.RequestId)) return;

            StateManager.SetState(StateManager.States.Game);
            StateManager.RedrawGame();
        }
    }
}
