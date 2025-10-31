using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Textures;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        LearnKill,
        Count
    }

    internal static class DebugManager
    {
        static List<DebugShape> debugShapes = new List<DebugShape>();
        static List<string> printLog = new List<string>();
        static int printLogCount = 0;
        static StreamWriter logger;

        static Text fpsText;
        static Text frameTimeText;
        static Text totalTimeText;
        static AbsoluteScreenPosition debugTextOrigin;


        public static bool Mode(DebugMode aMode) => modes[(int)aMode];
        static readonly bool[] modes = new bool[(int)DebugMode.Count];


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        static DebugManager()
        {
            modes[(int)DebugMode.DebugShapes] = true;
            modes[(int)DebugMode.DebugOverlay] = true;
            modes[(int)DebugMode.FalseRandom] = false;
            modes[(int)DebugMode.Print] = true;
            modes[(int)DebugMode.TileCoords] = false;
            modes[(int)DebugMode.InvCheats] = true;
            modes[(int)DebugMode.Teleport] = true;
            modes[(int)DebugMode.InstantlyContinue] = true;
            modes[(int)DebugMode.Console] = true;
            modes[(int)DebugMode.LearnKill] = true;


            if (modes[(int)DebugMode.Console])
            {
                AllocConsole();
                //logger = new StreamWriter(Console.OpenStandardOutput());
            }

            fpsText = new Text("Gloryser", Color.Chartreuse);
            frameTimeText = new Text("Gloryser", Color.Chartreuse);
            totalTimeText = new Text("Gloryser", Color.Chartreuse);
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
        }

        public static void AddDebugShape(DebugShape aShape)
        {
            if (!modes[(int)DebugMode.DebugShapes]) return;
            debugShapes.Add(aShape);
        }


        public static void Print(Type aType, object aObject) => Print(aType, aObject.ToString());

        public static void Print(Type aType, string aMsg)
        {
            if (!modes[(int)DebugMode.Print]) return;

            //logger.WriteLine(aType.ToString() + ": " + aMsg);
            //if (printLog.Count <= printLogCount)
            //{
            //    printLog.Add(aType.ToString() + ": " + aMsg);
            //}
            //else
            //{
            //    printLog[printLogCount] = aType.ToString() + ": " + aMsg;
            //}
            //printLogCount++;
            Console.WriteLine(aType.ToString() + ": " + aMsg);
            //Console.Out.WriteLineAsync(aType.ToString() + ": " + aMsg);
            
        }


        static void ClearDebugShapes()
        {
            if (!KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugDeleteShapes)) return;
            
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
            if (!KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugTestGear)) return;
            RelativeScreenPosition dialogueBoxSize = new RelativeScreenPosition(0.2f);
            DialogueBox testDialogueBox = new DialogueBox("Hello Cheater!\n\nxdd", Color.White, DialogueBox.LocationOfPopUp.HUDManager,DialogueBox.PausesGame.Pauses, null, new UITexture(new GfxPath(GfxType.UI, "GrayBackground"), Color.White), new RelativeScreenPosition(0.5f) - dialogueBoxSize / 2, dialogueBoxSize, "Close");

            HUDManager.AddDialogueBox(testDialogueBox);
            
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
            if (!KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugHealthPotion)) return;

            Item hpPot = ItemFactory.CreateItem(ItemFactory.GetItemData("Health Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(hpPot);
            
        }

        static void SpawnManaPotion()
        {
            if (!KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugManaPotion)) return;

            Item mpPot = ItemFactory.CreateItem(ItemFactory.GetItemData("Mana Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(mpPot);
        }

        static void TeleportPlayer()
        {
            if (!modes[(int)DebugMode.Teleport]) return;
            if (!KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugTeleport)) return;
            

            ObjectManager.Player.Teleport(WorldSpace.FromRelativeScreenSpace(InputManager.GetMousePosRelative()));
        }
        public static void Draw(SpriteBatch aBatch)
        {
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
            }
        }
    }
}
