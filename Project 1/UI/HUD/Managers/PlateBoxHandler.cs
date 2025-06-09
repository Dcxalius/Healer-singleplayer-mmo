using Project_1.GameObjects.Entities.Players;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit;
using Project_1.UI.HUD.PlateBoxes;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Spells.AoE.AreaOfEffectData;
using Project_1.Camera;
using Project_1.GameObjects;
using Project_1.Input;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System.Diagnostics;

namespace Project_1.UI.HUD.Managers
{
    internal class PlateBoxHandler
    {
        PlayerPlateBox playerPlateBox;
        TargetPlateBox targetPlateBox;
        PartyPlateBox[] partyPlateBoxes;
        BuffBox playerBuffBox;
        BuffBox targetBuffBox;
        BuffBox[] partyBuffBoxes;

        List<UIElement> plateBoxes;

        public void Save(ref List<(string, RelativeScreenPosition, RelativeScreenPosition)> saveables)
        {
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                if (!plateBoxes[i].HudMoveable) continue;
                saveables.Add(plateBoxes[i].Save);
            }
        }

        public void Update()
        {
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Update();
            }
        }

        public void HudMovableUpdate()
        {

            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Update();
            }
        }

        public bool Click(ClickEvent aClickEvent)
        { 
            for (int i = plateBoxes.Count - 1; i >= 0; i--)
            {
                if (plateBoxes[i].ClickedOn(aClickEvent)) return true;
            }
            return false;
        }

        public void Rescale()
        {
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Rescale();
            }
        }

        public void SetHudMovable(bool aSet)
        {

            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].SetHudMoveable(aSet);
            }

        }

        public void ResetHudMovable()
        {

            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].ResetHudMoveable();
            }
        }

        public void InitPlateBoxes(List<(string, RelativeScreenPosition, RelativeScreenPosition)> aLoadedSettings)
        {
            plateBoxes = new List<UIElement>();

            var loaded = aLoadedSettings.Find(x => x.Item1 == typeof(PlayerPlateBox).Name);
            playerPlateBox = new PlayerPlateBox(loaded.Item2, loaded.Item3);
            plateBoxes.Add(playerPlateBox);
            loaded = aLoadedSettings.Find(x => x.Item1 == typeof(TargetPlateBox).Name);
            targetPlateBox = new TargetPlateBox(loaded.Item2, loaded.Item3);
            plateBoxes.Add(targetPlateBox);
            partyPlateBoxes = new PartyPlateBox[4];
            var loadedPos = aLoadedSettings.Where(x => x.Item1 == typeof(PartyPlateBox).Name).ToArray();
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {

                partyPlateBoxes[i] = new PartyPlateBox(loadedPos[i].Item2, loadedPos[i].Item3, i);
            }
            plateBoxes.AddRange(partyPlateBoxes);

            loadedPos = aLoadedSettings.Where(x => x.Item1 == typeof(BuffBox).Name).ToArray();
            playerBuffBox = new BuffBox(ObjectManager.Player, BuffBox.FillDirection.TopRightToDown, loadedPos[0].Item2, loadedPos[0].Item3);
            plateBoxes.Add(playerBuffBox);
            targetBuffBox = new BuffBox(null, BuffBox.FillDirection.TopRightToDown, loadedPos[1].Item2, loadedPos[1].Item3);
            plateBoxes.Add(targetBuffBox);
            partyBuffBoxes = new BuffBox[4];
            for (int i = 0; i < partyBuffBoxes.Length; i++)
            {

                partyBuffBoxes[i] = new BuffBox(null, BuffBox.FillDirection.TopRightToDown, loadedPos[2 + i].Item2, loadedPos[2 + i].Item3);
            }
            plateBoxes.AddRange(partyBuffBoxes);
        }

        public void AddBuff(GameObjects.Spells.Buff.Buff aBuff, Entity aEntity)
        {
            if (targetBuffBox.IsThisMine(aEntity))
            {
                targetBuffBox.AddBuff(aBuff);
            }
            if (playerBuffBox.IsThisMine(aEntity))
            {
                playerBuffBox.AddBuff(aBuff);
                return;
            }
            for (int i = 0; i < partyBuffBoxes.Length; i++)
            {
                if (partyBuffBoxes[i] == null) continue;

                if (partyBuffBoxes[i].IsThisMine(aEntity))
                {
                    partyBuffBoxes[i].AddBuff(aBuff);
                    return;
                }
            }
        }

        public void SetPlayerPlateBox(Player aPlayer) => playerPlateBox.SetData(aPlayer);

        public void RefreshPlates(Entity aEntity)
        {
            switch (aEntity.RelationToPlayer)
            {
                case Relation.RelationToPlayer.Self:
                    playerPlateBox.Refresh(aEntity);
                    if (!targetPlateBox.BelongsTo(aEntity)) break;
                    targetPlateBox.Refresh(aEntity);
                    break;
                case Relation.RelationToPlayer.Friendly:
                    if (targetPlateBox.BelongsTo(aEntity)) targetPlateBox.Refresh(aEntity);
                    for (int i = 0; i < partyPlateBoxes.Length; i++)
                    {
                        if (partyPlateBoxes[i].BelongsTo(null)) break;
                        if (!partyPlateBoxes[i].BelongsTo(aEntity as GuildMember)) continue;
                        partyPlateBoxes[i].Refresh(aEntity);
                        break;
                    }
                    break;
                case Relation.RelationToPlayer.Neutral:
                case Relation.RelationToPlayer.Hostile:
                    if (!targetPlateBox.BelongsTo(aEntity)) break;
                    targetPlateBox.Refresh(aEntity);
                    break;
                default:
                    break;
            }
        }
        public void SetNewTarget(Entity aTargeter, Entity aTarget)
        {
            switch (aTargeter.RelationToPlayer)
            {
                case Relation.RelationToPlayer.Self:
                    targetPlateBox.SetTarget(aTarget);
                    targetBuffBox.AssignBox(aTarget);
                    break;
                case Relation.RelationToPlayer.Friendly:
                case Relation.RelationToPlayer.Neutral:
                case Relation.RelationToPlayer.Hostile:
                    break;
                default:
                    throw new NotImplementedException();
            }


        }

        public void AddGuildMemberToParty(GuildMember aGuildMember)
        {
            if (PartyPlateBox.PartyBoxesActive >= Party.maxPartySize)
            {
                DebugManager.Print(typeof(HUDManager), "Tried to add to full party.");
                return;
            }

            partyPlateBoxes[PartyPlateBox.PartyBoxesActive].SetTarget(aGuildMember);
            partyBuffBoxes[PartyPlateBox.PartyBoxesActive - 1].AssignBox(aGuildMember);
        }

        public void RemoveGuildMemberFromParty(GuildMember aGuildMember)
        {
            int index = FindGuildMemberPartyIndex(aGuildMember);

            Debug.Assert(index >= 0);

            if (PartyPlateBox.PartyBoxesActive - 1 == index)
            {
                partyPlateBoxes[index].RemoveTarget();
                return;
            }

            for (int i = index; i < PartyPlateBox.PartyBoxesActive - index - 1; i++)
            {
                partyPlateBoxes[i].SetTarget(partyPlateBoxes[i + 1].GuildMember);
            }

            partyPlateBoxes[PartyPlateBox.PartyBoxesActive - 1].RemoveTarget();
        }

        public void ClearParty()
        {
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                partyPlateBoxes[i].RemoveTarget();
            }
            PartyPlateBox.ClearPartyBoxes();
        }

        public int FindGuildMemberPartyIndex(GuildMember aGuildMember)
        {
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                if (partyPlateBoxes[i].BelongsTo(aGuildMember))
                {
                    return i;
                }

            }

            return -1;
        }

        public void AddGuildMemberToControl(GuildMember[] aGuildMembers)
        {
            for (int i = 0; i < aGuildMembers.Length; i++)
            {
                AddGuildMemberToControl(aGuildMembers[i]);
            }
        }

        public void AddGuildMemberToControl(GuildMember aGuildMember)
        {
            int index = FindGuildMemberPartyIndex(aGuildMember);

            if (index == -1) return;

            partyPlateBoxes[index].VisibleBorder = false;
        }
        public void RemoveWalkerFromControl(GuildMember[] aGuildMembers)
        {
            for (int i = 0; i < aGuildMembers.Length; i++)
            {
                RemoveWalkerFromControl(aGuildMembers[i]);
            }
        }

        public void RemoveWalkerFromControl(GuildMember aGuildmember)
        {
            int index = FindGuildMemberPartyIndex(aGuildmember);

            if (index == -1) return;

            partyPlateBoxes[index].VisibleBorder = false;
        }

        public void HudMovableDraw(SpriteBatch aBatch)
        {
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].HudMovableDraw(aBatch);
            }
        }

        public void Draw(SpriteBatch aBatch)
        {
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Draw(aBatch);
            }
        }
    }
}
