using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.Managers;
using Project_1.Managers.States;
using Project_1.UI;
using Project_1.UI.HUD;
using Project_1.UI.HUD.Chat;
using Project_1.UI.HUD.Inventory;
using Project_1.UI.HUD.PlateBoxes;
using Project_1.UI.HUD.SpellBook;
using Project_1.UI.UIElements;
using Project_1.UI.UIElements.Bars;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Project_1.UI.HUD.Managers
{
    internal static partial class HUDManager
    {
        internal static readonly object UiLock = new object();
        static PlateBoxHandler plateBoxHandler;
        static NamePlateHandler namePlateHandler;
        static WindowHandler windowHandler;

        static List<UIElement> hudElements;

        static InventoryBox inventoryBox;
        static LootBox lootBox;
        static DescriptorBox descriptorBox;

        static CastBar playerCastBar;
        static SpellBar firstSpellBar;

        static Minimap minimap;
        static SaveStatusIndicator saveStatusIndicator;
        static ChatPanel chatPanel;

        static HeldItem heldItem;
        static HeldSpell heldSpell;

        static List<DialogueBox> dialogueBoxes;
        static SizeChanger sizeChanger;

        static bool hasSelfExpSnapshot;
        static int lastSelfLevel;
        static int lastSelfExperience;

        static List<(string, RelativeScreenPosition, RelativeScreenPosition)> LoadedSettings;

        public static bool HudMoving => hudMoving;
        public static Action UiInvalidated;
        public static Action PlatesInvalidated;

        static readonly UiDrawList uiDrawListA = new UiDrawList();
        static readonly UiDrawList uiDrawListB = new UiDrawList();
        static volatile UiDrawList uiDrawList = uiDrawListA;
        static readonly PlateDrawList plateDrawListA = new PlateDrawList();
        static readonly PlateDrawList plateDrawListB = new PlateDrawList();
        static volatile PlateDrawList plateDrawList = plateDrawListA;
        static readonly HudMoveDrawList hudMoveDrawListA = new HudMoveDrawList();
        static readonly HudMoveDrawList hudMoveDrawListB = new HudMoveDrawList();
        static volatile HudMoveDrawList hudMoveDrawList = hudMoveDrawListA;
        static UIElement[] plateBoxScratch = Array.Empty<UIElement>();
        static UIElement[] plateElementScratch = Array.Empty<UIElement>();

        static bool uiDrawListDirty = true;
        static bool plateDrawListDirty = true;
        static bool hudMoveDrawListDirty = true;
        static bool hudMoving;
        static bool initialized;

        static void AssertUiThreadOrMainFallback()
        {
            if (UiThread.IsRunning)
            {
                ThreadAffinity.AssertUiThread();
                return;
            }

            ThreadAffinity.AssertMainThread();
        }

        static void AssertUiOrMainThread()
        {
            if (ThreadAffinity.IsMainThread) return;
            ThreadAffinity.AssertUiThread();
        }

        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;

            plateBoxHandler = new PlateBoxHandler();
            namePlateHandler = new NamePlateHandler();
            windowHandler = new WindowHandler();

            hudMoving = false;
            ImportSettings();

            hudElements = new List<UIElement>();
            plateBoxHandler.InitPlateBoxes(LoadedSettings);

            lootBox = new LootBox(new RelativeScreenPosition(0.1f, 0.5f), new RelativeScreenPosition(0.4f, 0.4f));
            hudElements.Add(lootBox);
            inventoryBox = new InventoryBox(new RelativeScreenPosition(0.59f, 0.60f), new RelativeScreenPosition(0.4f), 16);
            hudElements.Add(inventoryBox);

            descriptorBox = new DescriptorBox(null);
            sizeChanger = new SizeChanger(null);
            windowHandler.InitWindows(ref hudElements);

            var loaded = LoadedSettings.Find(x => x.Item1 == typeof(SpellBar).Name);
            firstSpellBar = new SpellBar(Color.White, 10, loaded.Item2, loaded.Item3.X);
            hudElements.Add(firstSpellBar);
            loaded = LoadedSettings.Find(x => x.Item1 == typeof(CastBar).Name);
            playerCastBar = new CastBar(loaded.Item2, loaded.Item3);
            hudElements.Add(playerCastBar);

            heldItem = new HeldItem();
            heldSpell = new HeldSpell();
            dialogueBoxes = new List<DialogueBox>();

            RelativeScreenPosition mmSize = RelativeScreenPosition.GetSquareFromX(0.2f);
            minimap = new Minimap(new RelativeScreenPosition(0.75f, 0.05f), mmSize);
            hudElements.Add(minimap);

            RelativeScreenPosition saveSize = RelativeScreenPosition.GetSquareFromX(0.03f);
            RelativeScreenPosition savePos = RelativeScreenPosition.One - saveSize - new RelativeScreenPosition(0.02f, 0.02f);
            saveStatusIndicator = new SaveStatusIndicator(savePos, saveSize);
            hudElements.Add(saveStatusIndicator);

            RelativeScreenPosition chatDefaultPos = new RelativeScreenPosition(0.02f, 0.70f);
            RelativeScreenPosition chatDefaultSize = new RelativeScreenPosition(0.36f, 0.26f);
            var chatLoaded = LoadedSettings.Find(x => x.Item1 == typeof(ChatPanel).Name);
            if (!string.IsNullOrWhiteSpace(chatLoaded.Item1))
            {
                chatDefaultPos = chatLoaded.Item2;
                chatDefaultSize = chatLoaded.Item3;
            }

            chatPanel = new ChatPanel(chatDefaultPos, chatDefaultSize);
            hudElements.Add(chatPanel);

            RegisterUiSubscriptions();

            uiDrawListDirty = true;
            plateDrawListDirty = true;
            BuildDrawLists();
            InvalidateUi();
            InvalidatePlates();
        }

        static void ImportSettings()
        {
            if (File.Exists(SaveManager.HudSettings))
            {
                string json = File.ReadAllText(SaveManager.HudSettings);
                LoadedSettings = SaveManager.ImportData<List<(string, RelativeScreenPosition, RelativeScreenPosition)>>(json);
            }
            else
            {
                string json = File.ReadAllText(SaveManager.DefaultHudSettings);
                LoadedSettings = SaveManager.ImportData<List<(string, RelativeScreenPosition, RelativeScreenPosition)>>(json);
            }
        }

        public static void Update()
        {
            AssertUiThreadOrMainFallback();
            long interactionVersionBefore = UIElement.InteractionVersion;
            namePlateHandler.Update();
            plateBoxHandler.Update();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Update();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Update();
            }

            if ((StateManager.CurrentState == StateManager.States.Game || StateManager.CurrentState == StateManager.States.MoveHUD)
                && UIElement.InteractionVersion != interactionVersionBefore)
            {
                InvalidateUi();
            }

            BuildDrawLists();
        }

        public static void Rescale()
        {
            AssertUiOrMainThread();
            namePlateHandler.Rescale();
            plateBoxHandler.Rescale();

            for (int i = 0; i < hudElements.Count; i++)
            {
                hudElements[i].Rescale();
            }

            for (int i = 0; i < dialogueBoxes.Count; i++)
            {
                dialogueBoxes[i].Rescale();
            }
        }

        public static void InvalidateUi()
        {
            AssertUiThreadOrMainFallback();
            uiDrawListDirty = true;
            hudMoveDrawListDirty = true;
            UiInvalidated?.Invoke();
        }

        public static void InvalidatePlates()
        {
            AssertUiThreadOrMainFallback();
            plateDrawListDirty = true;
            hudMoveDrawListDirty = true;
            PlatesInvalidated?.Invoke();
        }

        internal static UiDrawList UiDrawListSnapshot
        {
            get
            {
                ThreadAffinity.AssertMainThread();
                return uiDrawList;
            }
        }

        internal static PlateDrawList PlateDrawListSnapshot
        {
            get
            {
                ThreadAffinity.AssertMainThread();
                return plateDrawList;
            }
        }

        internal static HudMoveDrawList HudMoveDrawListSnapshot
        {
            get
            {
                ThreadAffinity.AssertMainThread();
                return hudMoveDrawList;
            }
        }

        internal static void BuildDrawLists()
        {
            AssertUiOrMainThread();
            int namePlateCount = 0;
            int plateBoxCount = 0;
            int plateElementCount = 0;
            if (plateDrawListDirty || hudMoveDrawListDirty)
            {
                namePlateCount = namePlateHandler.DrawListCount;
                plateBoxCount = plateBoxHandler.DrawListCount;
                EnsurePlateElementScratchCapacity(namePlateCount + plateBoxCount);
                namePlateCount = namePlateHandler.CopyDrawList(plateElementScratch);

                EnsurePlateBoxScratchCapacity(plateBoxCount);
                plateBoxCount = plateBoxHandler.CopyDrawList(plateBoxScratch);
                for (int i = 0; i < plateBoxCount; i++)
                {
                    plateElementScratch[namePlateCount + i] = plateBoxScratch[i];
                }
                plateElementCount = namePlateCount + plateBoxCount;
            }

            if (uiDrawListDirty)
            {
                UiDrawList buildTarget = ReferenceEquals(uiDrawList, uiDrawListA) ? uiDrawListB : uiDrawListA;
                buildTarget.Set(hudElements, dialogueBoxes, descriptorBox, heldItem, heldSpell);
                uiDrawList = buildTarget;
                uiDrawListDirty = false;
            }

            if (plateDrawListDirty)
            {
                PlateDrawList buildTarget = ReferenceEquals(plateDrawList, plateDrawListA) ? plateDrawListB : plateDrawListA;
                buildTarget.Set(plateElementScratch, plateElementCount);
                plateDrawList = buildTarget;
                plateDrawListDirty = false;
            }

            if (hudMoveDrawListDirty)
            {
                HudMoveDrawList buildTarget = ReferenceEquals(hudMoveDrawList, hudMoveDrawListA) ? hudMoveDrawListB : hudMoveDrawListA;
                buildTarget.Set(plateBoxScratch, plateBoxCount, hudElements, dialogueBoxes, sizeChanger);
                hudMoveDrawList = buildTarget;
                hudMoveDrawListDirty = false;
            }
        }

        static void EnsurePlateBoxScratchCapacity(int count)
        {
            if (count <= plateBoxScratch.Length) return;
            int capacity = Math.Max(count, Math.Max(8, plateBoxScratch.Length * 2));
            plateBoxScratch = new UIElement[capacity];
        }

        static void EnsurePlateElementScratchCapacity(int count)
        {
            if (count <= plateElementScratch.Length) return;
            int capacity = Math.Max(count, Math.Max(8, plateElementScratch.Length * 2));
            plateElementScratch = new UIElement[capacity];
        }
    }
}
