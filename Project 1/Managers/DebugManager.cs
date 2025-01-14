using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers
{
    enum DebugMode
    {
        DebugShapes,
        Print,
        TileCoords,
        InvCheats,
        Teleport,
        Count
    }

    [DebuggerStepThrough]
    internal static class DebugManager
    {
        static List<DebugShape> debugShapes = new List<DebugShape>();

        public static bool Mode(DebugMode aMode) => modes[(int)aMode];
        static readonly bool[] modes = new bool[(int)DebugMode.Count];


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();


        public static void Init()
        {
            modes[(int)DebugMode.DebugShapes] = true;
            modes[(int)DebugMode.Print] = true;
            modes[(int)DebugMode.TileCoords] = false;
            modes[(int)DebugMode.InvCheats] = true;
            modes[(int)DebugMode.Teleport] = true;
            AllocConsole();
        }

        public static void Update()
        {
            InventoryCheats();
            TeleportPlayer();

            ClearDebugShapes();
        }

        public static void AddDebugShape(DebugShape aShape)
        {
            if (!modes[(int)DebugMode.DebugShapes]) return;
            debugShapes.Add(aShape);
        }

        public static void Print(Type test, string aMsg)
        {
            if (!modes[(int)DebugMode.Print]) return;

            Console.WriteLine(test.ToString() + ": " + aMsg);
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
            

            ObjectManager.Player.Teleport(WorldSpace.FromRelaticeScreenSpace(InputManager.GetMousePosRelative()));
        }
        public static void Draw(SpriteBatch aBatch)
        {
            if (!modes[(int)DebugMode.DebugShapes]) return;

            for (int i = 0; i < debugShapes.Count; i++)
            {
                debugShapes[i].Draw(aBatch);
            }
        }
    }
}
