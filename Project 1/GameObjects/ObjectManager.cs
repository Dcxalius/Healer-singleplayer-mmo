using Microsoft.Xna.Framework.Graphics;
using Project_1.Input;
using Project_1.UI.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.VisualBasic.ApplicationServices;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Content;
using Project_1.Managers;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Entities.Players;
using Project_1.Camera;
using Project_1.GameObjects.Spawners;
using System.Diagnostics;
using Project_1.Tiles;
using Project_1.Managers.Saves;
using Project_1.GameObjects.FloatingTexts;
using Project_1.GameObjects.Entities.Corspes;
using Project_1.GameObjects.Entities.Projectiles;

namespace Project_1.GameObjects
{
    internal static class ObjectManager
    {
        public const float DistanceOfCircleAroundPlayer = 700;
        public static Player Player { get => player; }

        public static List<Entity> entities = new List<Entity>();
        public static List<GuildMember> guild = new List<GuildMember>();

        static Player player = null;

        static List<Entity> All
        {
            get
            {
                var r = entities.Union(guild).ToList();
                r.Add(player);
                return r;
            }
        }
        static List<GuildMember> GuildMembersInWorld
        {
            get
            {
                return guild.Where(x => entities.Contains(x)).ToList();
            }
         }

        public static void Init()
        {
            //guild.AddRange(ObjectFactory.GetGuildMemebers());
            //player = new Player();
            //player.GetPartyMembersFromGuild();
            //Camera.Camera.BindCamera(player);


            
            //entities.Add(new Walker(new WorldSpace(11*32)));//Debug
            //player.Party.AddToParty(entities[entities.Count - 1] as GuildMember); //Debug


        }

        public static GuildMember FriendlyTargetCycle()
        {
            
            if (player.Target == null) return GetClosestGuildMember();
            if (player.Target.GetType() != typeof(GuildMember)) return GetClosestGuildMember();
            return entities.Where(x => x.DistanceTo(player.FeetPosition) > player.Target.DistanceTo(player.FeetPosition)).MinBy(x => x.DistanceTo(player.FeetPosition)) as GuildMember;

        }

        static GuildMember GetClosestGuildMember()
        {
            return entities.MinBy(x => x.DistanceTo(player.FeetPosition)) as GuildMember;
        }

        public static void CreateNewPlayer(string aName, string aClass)
        {
            Reset();
            player = new Player(aName, aClass);
            ObjectFactory.PlayerData = player.PlayerData;
            Camera.Camera.BindCamera(player);
        }

        

        public static void Load(Save aSave)
        {
            ObjectFactory.Load(aSave);
            Reset();
            guild.AddRange(ObjectFactory.GetGuildMemebers());
            
            player = new Player(ObjectFactory.PlayerData);
            player.GetPartyMembersFromGuild();
            Camera.Camera.BindCamera(player);

            
        }

        public static void Reset()
        {
            for (int i = guild.Count - 1; i >= 0; i--)
            {
                guild[i].Delete();
            }
            HUDManager.ClearParty();
            entities.Clear();
            guild.Clear();
            CorpseManager.Reset();
            FloatingTextManager.Reset();
            if (player != null) player.Delete();
        }

        public static List<GuildMember> GetGuildMembers() => guild;

        public static bool SpawnGuildMemberToParty(GuildMember aMember, WorldSpace? aPosition)
        {
            Debug.Assert(guild.Contains(aMember));
            Debug.Assert(!player.Party.IsInParty(aMember));

            WorldSpace position = ( aPosition ??= FindTileAroundPlayer());

            aMember.Teleport(position);
            entities.Add(guild.Find(member => member == aMember));
            return player.Party.AddToParty(entities[entities.Count - 1] as GuildMember);
        }

        public static bool RemoveGuildMemberFromParty(GuildMember aMember)
        {
            Debug.Assert(guild.Contains(aMember));
            Debug.Assert(player.Party.IsInParty(aMember));

            WorldSpace position = FindTileAroundPlayer();
            aMember.RecieveDirectWalkingOrder(position);
            return player.Party.RemoveFromParty(aMember);
        }

        static WorldSpace FindTileAroundPlayer()
        {
            Tile[] tiles = new Tile[0];
            float start = DistanceOfCircleAroundPlayer; //TODO: Find better way to get these values relating to max camera distance
            float step = 50;
            while (tiles.Length == 0)
            {

                tiles = TileManager.GetTilesAroundPosition(Player.FeetPosition, start);
                start -= step;
                Debug.Assert(start > 0);
            }
            Tile tile = tiles[RandomManager.RollInt(tiles.Length)];
            //tile = tiles[tiles.Length - 1];
            return tile.Position;
        }

        public static void RemoveEntity(Entity aObject) => entities.Remove(aObject);

        public static void Update()
        {
            for (int i = All.Count - 1; i >= 0; i--)
            {
                All[i].Update();
            }

            if (TimeManager.TotalFrameTime % 2000 < 1)
            {
                for (int i = 0; i < All.Count; i++)
                {
                    All[i].ServerTick();
                }
            }

            
        }

        public static void RefreshPlates()
        {
            for (int i = 0; i < All.Count; i++)
            {
                All[i].RefreshPlates();
            }
        }


        public static bool Click(ClickEvent aClickEvent)
        {
            if (player.Click(aClickEvent)) return true;

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].Click(aClickEvent)) return true;
            }

            if (SpawnerManager.Click(aClickEvent)) return true; 

            return false;
        }

        public static bool ClickGround(ClickEvent aClickEvent)
        {
            if (LeftClickedGround(aClickEvent)) return true;
            if (RightClickGround(aClickEvent)) return true;
            //if (MiddleClickGround(aClickEvent)) return true;
            return false;
        }

        static bool LeftClickedGround(ClickEvent aClickEvent)
        {

            if (aClickEvent.ButtonPressed != InputManager.ClickType.Left) return false;

            if (aClickEvent.ModifiersOr(new InputManager.HoldModifier[] { InputManager.HoldModifier.Shift, InputManager.HoldModifier.Ctrl }))
            {
                player.Party.ClearCommand();
            }
            else
            {
                player.RemoveTarget();
            }
            return true;
        }

        static bool RightClickGround(ClickEvent aClickEvent)
        {
            if (aClickEvent.ButtonPressed != InputManager.ClickType.Right) return false;

            player.Party.IssueMoveOrder(aClickEvent);
            return true;
        }

        public static void Draw(SpriteBatch aSpriteBatch)
        {
            for (int i = 0; i < All.Count; i++)
            {
                All[i].Draw(aSpriteBatch);
            }
        }
    }
}
