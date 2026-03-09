using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.GameObjects.Entities.Friendlies;
using Project_1.GameObjects.Entities.Friendlies.Players;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Managers;
using Project_1.Messaging;
using Project_1.Messaging.Events;
using Project_1.Tiles;

namespace Project_1.Managers.States
{
    internal static partial class StateManager
    {
        static void HandleChatCommandRequested(ChatCommandRequested e)
        {
            ThreadAffinity.AssertSimThread();
            string raw = (e.CommandText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw)) return;

            if (raw[0] == '/')
            {
                raw = raw.Length == 1 ? string.Empty : raw.Substring(1);
            }

            if (string.IsNullOrWhiteSpace(raw)) return;

            if (!TryTokenizeCommand(raw, out string[] tokens, out string tokenError))
            {
                PublishChatSystemMessage(tokenError);
                return;
            }

            if (tokens.Length == 0) return;

            string command = tokens[0].ToLowerInvariant();
            string[] args = new string[tokens.Length - 1];
            Array.Copy(tokens, 1, args, 0, args.Length);
            ChatCommandSpec? spec = TryGetChatCommandSpec(command);
            if (!spec.HasValue)
            {
                PublishChatSystemMessage($"Unknown command: /{command}. Use /help.");
                return;
            }

            if (spec.Value.Access == ChatCommandAccess.Debug && !DebugManager.Mode(DebugMode.ChatCheats))
            {
                return;
            }

            switch (command)
            {
                case "help":
                    HandleChatHelp();
                    break;
                case "clear":
                    Mailboxes.PublishUiEvent(new ChatCleared());
                    break;
                case "where":
                    HandleChatWhere(args);
                    break;
                case "chunklevels":
                    HandleChatChunkLevels(args);
                    break;
                case "tp":
                    HandleChatTeleport(args);
                    break;
                case "createitem":
                    HandleChatCreateItem(args);
                    break;
            }
        }

        static void HandleChatSayRequested(ChatSayRequested e)
        {
            ThreadAffinity.AssertSimThread();
            string text = (e.MessageText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            string senderName = ObjectManager.Player?.Name;
            if (string.IsNullOrWhiteSpace(senderName))
            {
                senderName = "Player";
            }

            Mailboxes.PublishUiEvent(new ChatMessagePosted(
                new ChatMessage(ChatMessageType.Say, text, senderName, ChatSpeakerType.Player)));
        }

        static void HandleChatHelp()
        {
            string systemCommands = string.Join(" | ", chatCommandSpecs
                .Where(x => x.Access == ChatCommandAccess.System)
                .OrderBy(x => x.Name)
                .Select(x => $"{x.Usage}: {x.Description}"));
            string debugCommands = string.Join(" | ", chatCommandSpecs
                .Where(x => x.Access == ChatCommandAccess.Debug)
                .OrderBy(x => x.Name)
                .Select(x => $"{x.Usage}: {x.Description}"));
            PublishChatSystemMessage($"System: {systemCommands}");
            PublishChatSystemMessage($"Debug (requires ChatCheats): {debugCommands}");
        }

        static void HandleChatWhere(string[] args)
        {
            if (args.Length > 1)
            {
                PublishChatSystemMessage("Usage: /where <friendly name>");
                return;
            }

            Friendly friendly;
            if (args.Length == 0)
            {
                friendly = ObjectManager.Player;
                if (friendly == null) return;
            }
            else
            {
                string name = args[0];
                if (!ObjectManager.TryGetFriendlyByName(name, out friendly))
                {
                    PublishChatSystemMessage($"Could not find non-mob named '{name}'.");
                    return;
                }
            }

            PublishChatSystemMessage($"{friendly.Name} is at {FormatCoordinate(friendly.FeetPosition.X)} {FormatCoordinate(friendly.FeetPosition.Y)}");
        }

        static void HandleChatTeleport(string[] args)
        {
            if (args.Length != 3)
            {
                PublishChatSystemMessage("Usage: /tp <friendly name> <x> <y>");
                return;
            }

            string name = args[0];
            if (!ObjectManager.TryGetFriendlyByName(name, out Friendly friendly))
            {
                PublishChatSystemMessage($"Could not find non-mob named '{name}'.");
                return;
            }

            if (!TryParseFloat(args[1], out float x) || !TryParseFloat(args[2], out float y))
            {
                PublishChatSystemMessage("Coordinates must be numbers. Usage: /tp <friendly name> <x> <y>");
                return;
            }

            friendly.Teleport(new WorldSpace(x, y));
            PublishChatSystemMessage($"{friendly.Name} teleported to {FormatCoordinate(x)} {FormatCoordinate(y)}");
        }

        static void HandleChatChunkLevels(string[] args)
        {
            if (args.Length != 0)
            {
                PublishChatSystemMessage("Usage: /chunklevels");
                return;
            }

            Chunk[] chunks = TileManager.GetChunksSnapshot();
            if (chunks.Length == 0)
            {
                PublishChatSystemMessage("No chunks are currently generated.");
                return;
            }

            Point center = ResolveChunkLevelsCenter();
            if (!HasCompleteChunkLevelsWindow(chunks, center))
            {
                PublishChunkLevelsAllGenerated(chunks);
                return;
            }

            PublishChunkLevelsWindow(chunks, center);
        }

        static void PublishChunkLevelsAllGenerated(Chunk[] chunks)
        {
            Chunk[] ordered = chunks
                .OrderBy(c => c.ChunkPosition.Y)
                .ThenBy(c => c.ChunkPosition.X)
                .ToArray();

            PublishChatSystemMessage($"Chunk levels for all generated chunks ({ordered.Length}):");

            const int entriesPerMessage = 6;
            for (int i = 0; i < ordered.Length; i += entriesPerMessage)
            {
                int take = Math.Min(entriesPerMessage, ordered.Length - i);
                string[] entries = new string[take];
                for (int j = 0; j < take; j++)
                {
                    Chunk chunk = ordered[i + j];
                    Point p = chunk.ChunkPosition;
                    entries[j] = $"({p.X},{p.Y})={chunk.AverageLevel:00}";
                }
                PublishChatSystemMessage(string.Join("  ", entries));
            }
        }

        static void PublishChunkLevelsWindow(Chunk[] chunks, Point center)
        {
            Dictionary<Point, int> levelsByChunk = new Dictionary<Point, int>(chunks.Length);
            for (int i = 0; i < chunks.Length; i++)
            {
                Chunk chunk = chunks[i];
                levelsByChunk[chunk.ChunkPosition] = chunk.AverageLevel;
            }

            int halfLow = ChunkLevelsWindowSize / 2;
            int halfHigh = ChunkLevelsWindowSize - halfLow - 1;
            int minX = center.X - halfLow;
            int maxX = center.X + halfHigh;
            int minY = center.Y - halfLow;
            int maxY = center.Y + halfHigh;

            PublishChatSystemMessage($"Chunk levels 10x10 near ({center.X},{center.Y}) [-- = not generated]:");

            string[] xLabels = new string[ChunkLevelsWindowSize];
            for (int x = minX; x <= maxX; x++)
            {
                xLabels[x - minX] = x.ToString("00;-00;00", CultureInfo.InvariantCulture);
            }
            PublishChatSystemMessage($"x: {string.Join(" ", xLabels)}");

            for (int y = maxY; y >= minY; y--)
            {
                string[] row = new string[ChunkLevelsWindowSize];
                for (int x = minX; x <= maxX; x++)
                {
                    Point key = new Point(x, y);
                    if (levelsByChunk.TryGetValue(key, out int level))
                    {
                        row[x - minX] = level.ToString("00", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        row[x - minX] = "--";
                    }
                }

                PublishChatSystemMessage($"y {y.ToString("00;-00;00", CultureInfo.InvariantCulture)}: {string.Join(" ", row)}");
            }
        }

        static Point ResolveChunkLevelsCenter()
        {
            Chunk playerChunk = ObjectManager.Player != null
                ? TileManager.GetChunkUnder(ObjectManager.Player.FeetPosition)
                : null;
            return playerChunk != null ? playerChunk.ChunkPosition : Point.Zero;
        }

        static bool HasCompleteChunkLevelsWindow(Chunk[] chunks, Point center)
        {
            HashSet<Point> generated = new HashSet<Point>();
            for (int i = 0; i < chunks.Length; i++)
            {
                generated.Add(chunks[i].ChunkPosition);
            }

            int halfLow = ChunkLevelsWindowSize / 2;
            int halfHigh = ChunkLevelsWindowSize - halfLow - 1;
            int minX = center.X - halfLow;
            int maxX = center.X + halfHigh;
            int minY = center.Y - halfLow;
            int maxY = center.Y + halfHigh;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!generated.Contains(new Point(x, y))) return false;
                }
            }

            return true;
        }

        static void HandleChatCreateItem(string[] args)
        {
            if (args.Length != 3)
            {
                PublishChatSystemMessage("Usage: /createitem <friendly name> <item id> <count>");
                return;
            }

            string name = args[0];
            if (!ObjectManager.TryGetFriendlyByName(name, out Friendly friendly))
            {
                PublishChatSystemMessage($"Could not find non-mob named '{name}'.");
                return;
            }

            if (friendly is not Player player)
            {
                PublishChatSystemMessage($"{friendly.Name} cannot receive items (no inventory).");
                return;
            }

            if (!int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int itemId) || itemId < 0)
            {
                PublishChatSystemMessage("Item id must be a non-negative integer.");
                return;
            }

            if (!int.TryParse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count <= 0)
            {
                PublishChatSystemMessage("Item count must be greater than 0.");
                return;
            }

            if (!TryGetItemData(itemId, out ItemData itemData))
            {
                PublishChatSystemMessage($"Item id {itemId} does not exist.");
                return;
            }

            Items.Item item = ItemFactory.CreateItem(itemData, count);
            bool addedAll = player.Inventory.AddItem(item);
            if (addedAll)
            {
                PublishChatSystemMessage($"Gave {player.Name} {count}x {item.Name}");
                return;
            }

            int leftover = item.Count;
            int added = count - leftover;
            if (added <= 0)
            {
                PublishChatSystemMessage($"{player.Name} has no room for {item.Name}.");
                return;
            }

            PublishChatSystemMessage($"Gave {player.Name} {added}x {item.Name} ({leftover}x did not fit)");
        }

        static bool TryGetItemData(int itemId, out ItemData itemData)
        {
            itemData = null;
            ItemData[] all = ItemFactory.GetAllItemDataSnapshot();
            if (all.Length == 0) return false;

            if (itemId >= 0 && itemId < all.Length)
            {
                ItemData indexed = all[itemId];
                if (indexed != null && indexed.ID == itemId)
                {
                    itemData = indexed;
                    return true;
                }
            }

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null || all[i].ID != itemId) continue;
                itemData = all[i];
                return true;
            }

            return false;
        }

        static bool TryTokenizeCommand(string text, out string[] tokens, out string error)
        {
            error = null;
            List<string> parsed = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\\' && i + 1 < text.Length && (text[i + 1] == '"' || text[i + 1] == '\\'))
                {
                    current.Append(text[i + 1]);
                    i++;
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        parsed.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }

                current.Append(c);
            }

            if (inQuotes)
            {
                tokens = Array.Empty<string>();
                error = "Unclosed quote in command.";
                return false;
            }

            if (current.Length > 0)
            {
                parsed.Add(current.ToString());
            }

            tokens = parsed.ToArray();
            return true;
        }

        static bool TryParseFloat(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
        }

        static ChatCommandSpec? TryGetChatCommandSpec(string commandName)
        {
            for (int i = 0; i < chatCommandSpecs.Length; i++)
            {
                if (!string.Equals(chatCommandSpecs[i].Name, commandName, StringComparison.OrdinalIgnoreCase)) continue;
                return chatCommandSpecs[i];
            }

            return null;
        }

        static string FormatCoordinate(float value)
        {
            float rounded = MathF.Round(value);
            if (MathF.Abs(value - rounded) < 0.0001f)
            {
                return ((int)rounded).ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        static void PublishChatSystemMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            Mailboxes.PublishUiEvent(new ChatMessagePosted(
                new ChatMessage(ChatMessageType.System, message, "System", ChatSpeakerType.System)));
        }

        readonly struct ChatCommandSpec
        {
            public ChatCommandSpec(string name, string usage, string description, ChatCommandAccess access)
            {
                Name = name;
                Usage = usage;
                Description = description;
                Access = access;
            }

            public string Name { get; }
            public string Usage { get; }
            public string Description { get; }
            public ChatCommandAccess Access { get; }
        }

        enum ChatCommandAccess
        {
            System,
            Debug
        }
    }
}
