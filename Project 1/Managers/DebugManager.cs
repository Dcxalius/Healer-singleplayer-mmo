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
        Off,
        Hitboxes,
        On
    }

    internal static class DebugManager
    {
        public static readonly DebugMode mode = DebugMode.On;


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();


        public static void Init()
        {

            AllocConsole();
        }

        public static void Update()
        {
            if (KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugPoop))
            {
                SpawnPoopInInventory();
            }
            if (KeyBindManager.GetPress(KeyBindManager.KeyListner.DebugBanana))
            {
                SpawnBananaInInventory();
            }
        }

        public static void Print(Type test, string aMsg)
        {
            Print(test, aMsg, DebugMode.On);
        }

        public static void Print(Type test, string aMsg, DebugMode aMode)
        {
            if (mode == aMode || mode == DebugMode.On)
            {
                Console.WriteLine(test.ToString() + ": " + aMsg);
            }
        }

        public static void SpawnPoopInInventory()
        {
            Item poop = ItemFactory.CreateItem(ItemFactory.GetItemData("Poop"), 1);
            ObjectManager.Player.Inventory.AddItem(poop);
            
        }

        static void SpawnBananaInInventory()
        {
            if (mode == DebugMode.Off) return;

            Item banana = ItemFactory.CreateItem(ItemFactory.GetItemData("If this is only a banana why is the name so long"), 1);
            ObjectManager.Player.Inventory.AddItem(banana);
        }
    }
}
