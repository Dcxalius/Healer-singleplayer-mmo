using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Camera;
using Project_1.DebugTools;
using Project_1.GameObjects;
using Project_1.Input;
using Project_1.Items;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.System.Models.BaseModels;
using Project_1.Textures;
using Project_1.UI;
using Project_1.UI.HUD.Managers;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.Managers
{
    internal static partial class DebugManager
    {
        static readonly List<DebugShape> debugShapes = new List<DebugShape>();
        static readonly ConcurrentQueue<DebugShape> pendingDebugShapes = new ConcurrentQueue<DebugShape>();
        static volatile bool clearDebugShapesRequested;

        public static void AddDebugShape(DebugShape aShape)
        {
            if (!Mode(DebugMode.DebugShapes)) return;
            if (aShape == null) return;
            pendingDebugShapes.Enqueue(aShape);
        }

        static void CheckForClearingDebugShapes()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugDeleteShapes)) return;
            clearDebugShapesRequested = true;
        }

        static void InventoryCheats()
        {
            if (!Mode(DebugMode.InvCheats)) return;
            SpawnTestGear();
            SpawnHealthPotion();
            SpawnManaPotion();
        }

        static void SpawnTestGear()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugTestGear)) return;

            RelativeScreenPosition dialogueBoxSize = new RelativeScreenPosition(0.2f);
            MailboxManager.PublishUiEvent(new DialogueOpened(
                "Hello Cheater!\n\nxdd",
                Color.White,
                DialogueBox.LocationOfPopUp.HUDManager.ToDialoguePopupLocation(),
                DialogueBox.PausesGame.Pauses.ToDialoguePauseKind(),
                null,
                new GfxPath(GfxType.UI, "GrayBackground"),
                new RelativeScreenPosition(0.5f) - dialogueBoxSize / 2,
                dialogueBoxSize,
                "Close")); //TODO: This should be removed at some point but only place that currently uses DialogueBoxes so it stays for now

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
            ObjectManager.Player.Inventory.AddItem(ItemFactory.CreateItem(ItemFactory.GetItemData("tredg"), 1)); //TODO: Ponder if this should be handled another way, fine for now though
        }

        static void SpawnHealthPotion()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugHealthPotion)) return; //TODO: This should be removed and handled by chat system instead

            Item hpPot = ItemFactory.CreateItem(ItemFactory.GetItemData("Health Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(hpPot);
        }

        static void SpawnManaPotion()
        {
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugManaPotion)) return;//TODO: This should be removed and handled by chat system instead

            Item mpPot = ItemFactory.CreateItem(ItemFactory.GetItemData("Mana Potion"), 1);
            ObjectManager.Player.Inventory.AddItem(mpPot);
        }

        static void TeleportPlayer()
        {
            if (!Mode(DebugMode.Teleport)) return;
            if (!KeyBindStateCache.GetPress(KeyBindManager.KeyListner.DebugTeleport)) return;

            ObjectManager.Player.Teleport(WorldSpace.FromRelativeScreenSpace(MouseStateCache.Relative));
        }

        public static void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            DrawShapes(aBatch);
            DrawOverlay(aBatch);
        }

        static void DrawShapes(SpriteBatch aBatch)
        {
            if (!Mode(DebugMode.DebugShapes)) return; 
            
            if (clearDebugShapesRequested)
            {
                debugShapes.Clear();
                clearDebugShapesRequested = false;
                while (pendingDebugShapes.TryDequeue(out _)) { }
            }

            while (pendingDebugShapes.TryDequeue(out DebugShape pending))
            {
                debugShapes.Add(pending);
            }

            for (int i = 0; i < debugShapes.Count; i++)
            {
                debugShapes[i].Draw(aBatch);
            }
            
        }

        static void DrawOverlay(SpriteBatch aBatch)
        {
            if (!Mode(DebugMode.DebugOverlay)) return;
            
            AbsoluteScreenPosition cursor = debugTextOrigin;
            DrawOverlayInfo(aBatch, fpsText, DebugOverlayInfo.Fps, ref cursor);
            DrawOverlayInfo(aBatch, frameTimeText, DebugOverlayInfo.FrameTime, ref cursor);
            DrawOverlayInfo(aBatch, totalTimeText, DebugOverlayInfo.TotalTime, ref cursor);
            DrawOverlayInfo(aBatch, mailboxText, DebugOverlayInfo.MailboxQueue, ref cursor);
            DrawOverlayInfo(aBatch, dispatchText, DebugOverlayInfo.MailboxDispatch, ref cursor);
            DrawOverlayInfo(aBatch, workerText, DebugOverlayInfo.Worker, ref cursor);
            DrawOverlayInfo(aBatch, screenshotText, DebugOverlayInfo.ScreenshotQueue, ref cursor);
            DrawOverlayInfo(aBatch, simThreadText, DebugOverlayInfo.SimThread, ref cursor);
            DrawOverlayInfo(aBatch, uiThreadText, DebugOverlayInfo.UiThread, ref cursor);
            DrawOverlayInfo(aBatch, renderSyncText, DebugOverlayInfo.RenderSync, ref cursor);
        }
    }
}
