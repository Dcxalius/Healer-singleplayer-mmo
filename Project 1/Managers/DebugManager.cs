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
        Hitboxes,
        Print,
        TileCoords,
        InvCheats,
        Teleport,
        Count
    }

    internal static class DebugManager
    {
        public static List<DebugShape> debugShapes = new List<DebugShape>();

        public static bool Mode(DebugMode aMode) => modes[(int)aMode];
        static readonly bool[] modes = new bool[(int)DebugMode.Count];


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();


        public static void Init()
        {
            modes[(int)DebugMode.Print] = true;
            modes[(int)DebugMode.InvCheats] = true;
            modes[(int)DebugMode.Teleport] = true;
            AllocConsole();
        }

        public static void Update()
        {
            if (KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugPoop))
            {
                SpawnHealthPotion();
            }
            if (KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugBanana))
            {
                SpawnManaPotion();
            }
            if (KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugTeleport))
            {
                TeleportPlayer();
            }
        }

        public static void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < debugShapes.Count; i++)
            {
                debugShapes[i].Draw(aBatch);
            }
        }

        [DebuggerStepThrough]
        public static void Print(Type test, string aMsg)
        {
            Print(test, aMsg, DebugMode.Print);
        }

        [DebuggerStepThrough]
        public static void Print(Type test, string aMsg, DebugMode aMode)
        {
            if (!modes[(int)DebugMode.Print]) return;
            
            Console.WriteLine(test.ToString() + ": " + aMsg);
            
        }

        public static void SpawnHealthPotion()
        {
            if (!modes[(int)DebugMode.InvCheats]) return;
            Item poop = ItemFactory.CreateItem(ItemFactory.GetItemData("Health Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(poop);
            
        }

        static void SpawnManaPotion()
        {
            if (!modes[(int)DebugMode.InvCheats]) return;

            Item banana = ItemFactory.CreateItem(ItemFactory.GetItemData("Mana Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(banana);
        }

        static void TeleportPlayer()
        {
            if (!modes[(int)DebugMode.Teleport]) return;

            ObjectManager.Player.Teleport(WorldSpace.FromRelaticeScreenSpace(InputManager.GetMousePosRelative()));
        }
    }
}
