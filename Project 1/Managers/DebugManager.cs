using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    enum DebugMode
    {
        DebugShapes,
        DebugOverlay,
        Print,
        FalseRandom,
        TileCoords,
        InvCheats,
        Teleport,
        InstantlyContinue,
        Console,
        TeleportStuckThings,
        LearnKill,
        Count
    }

    internal static class DebugManager
    {
        static List<DebugShape> debugShapes = new List<DebugShape>();
        static bool initialized;

        static Text fpsText;
        static Text frameTimeText;
        static Text totalTimeText;
        static Text mailboxText;
        static Text dispatchText;
        static Text workerText;
        static Text screenshotText;
        static AbsoluteScreenPosition debugTextOrigin;


        public static bool Mode(DebugMode aMode) => modes[(int)aMode];
        static readonly bool[] modes = new bool[(int)DebugMode.Count];


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            modes[(int)DebugMode.DebugShapes] = true;
            modes[(int)DebugMode.DebugOverlay] = true;
            modes[(int)DebugMode.FalseRandom] = false;
            modes[(int)DebugMode.Print] = true;
            modes[(int)DebugMode.TileCoords] = false;
            modes[(int)DebugMode.InvCheats] = true;
            modes[(int)DebugMode.Teleport] = true;
            modes[(int)DebugMode.InstantlyContinue] = true;
            modes[(int)DebugMode.Console] = true;
            modes[(int)DebugMode.TeleportStuckThings] = true;
            modes[(int)DebugMode.LearnKill] = true;


            if (modes[(int)DebugMode.Console])
            {
                AllocConsole();
            }

        }

        public static void LoadContent()
        {
            fpsText = new Text("Gloryse", Color.Chartreuse);
            frameTimeText = new Text("Gloryse", Color.Chartreuse);
            totalTimeText = new Text("Gloryse", Color.Chartreuse);
            mailboxText = new Text("Gloryse", Color.Chartreuse);
            dispatchText = new Text("Gloryse", Color.Chartreuse);
            workerText = new Text("Gloryse", Color.Chartreuse);
            screenshotText = new Text("Gloryse", Color.Chartreuse);
            debugTextOrigin = new AbsoluteScreenPosition(12, 12);
        }

        public static void Update()
        {
            InventoryCheats();
            TeleportPlayer();

            ClearDebugShapes();

            UpdateOverlayText();
        }

        static void UpdateOverlayText()
        {
            if (!modes[(int)DebugMode.DebugOverlay]) return;

            double deltaSeconds = TimeManager.SecondsSinceLastFrame;
            double fps = deltaSeconds > 0 ? 1.0 / deltaSeconds : 0;
            double frameTimeMs = deltaSeconds * 1000.0;
            TimeSpan totalTime = TimeManager.InstanceTotalFrameTimeAsTimeSpan;

            fpsText.Value = $"FPS: {fps:0.0}";
            frameTimeText.Value = $"Frame: {frameTimeMs:0.00} ms";
            totalTimeText.Value = $"Total: {totalTime:hh\\:mm\\:ss}";

            var mainStats = Mailboxes.MainStats;
            var uiStats = Mailboxes.UiStats;
            var simStats = Mailboxes.SimStats;
            mailboxText.Value = $"Q M:{mainStats.Pending} U:{uiStats.Pending} S:{simStats.Pending}";
            dispatchText.Value = $"D M:{mainStats.LastDispatchCount}/{mainStats.LastDispatchMs:0.0}ms U:{uiStats.LastDispatchCount}/{uiStats.LastDispatchMs:0.0}ms S:{simStats.LastDispatchCount}/{simStats.LastDispatchMs:0.0}ms";
            var workerStats = WorkerPool.Stats;
            workerText.Value = $"W {workerStats.Pending}/{workerStats.Peak} last:{workerStats.LastWorkMs:0.0}ms";
            var screenshotStats = SaveManager.ScreenshotQueueStats;
            screenshotText.Value = $"S {screenshotStats.Pending}/{screenshotStats.Peak} last:{screenshotStats.LastScreenshotMs:0.0}ms";
        }

        public static void AddDebugShape(DebugShape aShape)
        {
            if (!modes[(int)DebugMode.DebugShapes]) return;
            debugShapes.Add(aShape);
        }


        public static void Print(object aObject,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            Print(aObject?.ToString() ?? "null", filePath, lineNumber, memberName);
        }

        public static void Print(string aMsg,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            if (!modes[(int)DebugMode.Print]) return;

            string fileName = string.IsNullOrWhiteSpace(filePath) ? "unknown" : Path.GetFileName(filePath);
            Console.WriteLine($"{fileName}:{lineNumber} {memberName}: {aMsg}");
        }


        static void ClearDebugShapes()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugDeleteShapes)) return;
            
            debugShapes.Clear();
        }

        static void InventoryCheats()
        {
            if (!modes[(int)DebugMode.InvCheats]) return;
            SpawnTestGear();
            SpawnHealthPotion();
            SpawnManaPotion();
        }
        static void SpawnTestGear()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugTestGear)) return;
            RelativeScreenPosition dialogueBoxSize = new RelativeScreenPosition(0.2f);
            Mailboxes.Ui.Publish(new Messaging.Events.DialogueOpened(
                "Hello Cheater!\n\nxdd",
                Color.White,
                DialogueBox.LocationOfPopUp.HUDManager,
                DialogueBox.PausesGame.Pauses,
                null,
                new GfxPath(GfxType.UI, "GrayBackground"),
                new RelativeScreenPosition(0.5f) - dialogueBoxSize / 2,
                dialogueBoxSize,
                "Close"));
            
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("ZweiHander"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Axe"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Dagger"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Bow"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Shield"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Helmet"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Amulet of spoons"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Big Shoulders"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Backoff"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Chesty"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Bracers"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Glovy"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Belty"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Panties"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Booti"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("FIRST"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Seocnd"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("thrd"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("FORSTA"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("Andra"), 1));
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("tredg"), 1));

        }

        static void SpawnHealthPotion()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugHealthPotion)) return;

            Item hpPot = ItemFactory.CreateItem(ItemFactory.GetItemData("Health Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(hpPot);
            
        }

        static void SpawnManaPotion()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugManaPotion)) return;

            Item mpPot = ItemFactory.CreateItem(ItemFactory.GetItemData("Mana Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(mpPot);
        }

        static void TeleportPlayer()
        {
            if (!modes[(int)DebugMode.Teleport]) return;
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugTeleport)) return;
            

            ObjectManager.Player.Teleport(WorldSpace.FromRelativeScreenSpace(MouseStateCache.Relative));
        }
        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            bool drawShapes = modes[(int)DebugMode.DebugShapes];
            bool drawOverlay = modes[(int)DebugMode.DebugOverlay];

            if (!drawShapes && !drawOverlay) return;

            if (drawShapes)
            {
                for (int i = 0; i < debugShapes.Count; i++)
                {
                    debugShapes[i].Draw(aBatch);
                }
            }

            if (drawOverlay)
            {
                fpsText.TopLeftDraw(aBatch, debugTextOrigin);
                AbsoluteScreenPosition frameTimePos = debugTextOrigin + new AbsoluteScreenPosition(0, (int)Math.Ceiling(fpsText.Offset.Y) + 2);
                frameTimeText.TopLeftDraw(aBatch, frameTimePos);
                AbsoluteScreenPosition totalTimePos = frameTimePos + new AbsoluteScreenPosition(0, (int)Math.Ceiling(frameTimeText.Offset.Y) + 2);
                totalTimeText.TopLeftDraw(aBatch, totalTimePos);
                AbsoluteScreenPosition mailboxPos = totalTimePos + new AbsoluteScreenPosition(0, (int)Math.Ceiling(totalTimeText.Offset.Y) + 2);
                mailboxText.TopLeftDraw(aBatch, mailboxPos);
                AbsoluteScreenPosition dispatchPos = mailboxPos + new AbsoluteScreenPosition(0, (int)Math.Ceiling(mailboxText.Offset.Y) + 2);
                dispatchText.TopLeftDraw(aBatch, dispatchPos);
                AbsoluteScreenPosition workerPos = dispatchPos + new AbsoluteScreenPosition(0, (int)Math.Ceiling(dispatchText.Offset.Y) + 2);
                workerText.TopLeftDraw(aBatch, workerPos);
                AbsoluteScreenPosition screenshotPos = workerPos + new AbsoluteScreenPosition(0, (int)Math.Ceiling(workerText.Offset.Y) + 2);
                screenshotText.TopLeftDraw(aBatch, screenshotPos);
            }
        }
    }
}
