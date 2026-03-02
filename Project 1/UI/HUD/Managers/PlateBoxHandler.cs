using Project_1.GameObjects.Unit;
using Project_1.Camera;
using Project_1.Input;
using Project_1.UI.HUD.PlateBoxes;
using Project_1.UI.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Project_1.Managers;
using System.Diagnostics;
using Project_1.Messaging.Events;
using Project_1.GameObjects.Entities.Friendlies.Players;

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
            AssertUiOrMainThread();
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                if (!plateBoxes[i].HudMoveable) continue;
                saveables.Add(plateBoxes[i].Save);
            }
        }

        public void Update()
        {
            ThreadAffinity.AssertUiThread();
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Update();
            }
        }

        public void HudMovableUpdate()
        {
            ThreadAffinity.AssertUiThread();

            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Update();
            }
        }

        public bool Click(ClickEvent aClickEvent)
        { 
            ThreadAffinity.AssertUiThread();
            for (int i = plateBoxes.Count - 1; i >= 0; i--)
            {
                if (plateBoxes[i].ClickedOn(aClickEvent)) return true;
            }
            return false;
        }

        public void Rescale()
        {
            AssertUiOrMainThread();
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Rescale();
            }
        }

        public void SetHudMovable(bool aSet)
        {
            AssertUiOrMainThread();

            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].SetHudMoveable(aSet);
            }

        }

        public void ResetHudMovable()
        {
            AssertUiOrMainThread();

            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].ResetHudMoveable();
            }
        }

        public void InitPlateBoxes(List<(string, RelativeScreenPosition, RelativeScreenPosition)> aLoadedSettings)
        {
            ThreadAffinity.AssertMainThread();
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
            playerBuffBox = new BuffBox(BuffBox.FillDirection.TopRightToDown, loadedPos[0].Item2, loadedPos[0].Item3);
            plateBoxes.Add(playerBuffBox);
            targetBuffBox = new BuffBox(BuffBox.FillDirection.TopRightToDown, loadedPos[1].Item2, loadedPos[1].Item3);
            plateBoxes.Add(targetBuffBox);
            partyBuffBoxes = new BuffBox[4];
            for (int i = 0; i < partyBuffBoxes.Length; i++)
            {
                partyBuffBoxes[i] = new BuffBox(BuffBox.FillDirection.TopRightToDown, loadedPos[2 + i].Item2, loadedPos[2 + i].Item3);
            }
            plateBoxes.AddRange(partyBuffBoxes);
        }

        public void AddBuff(in BuffUiSnapshot aBuff, int ownerRenderId)
        {
            if (targetBuffBox.IsThisMine(ownerRenderId))
            {
                targetBuffBox.AddBuff(aBuff);
                HUDManager.InvalidatePlates();
            }
            if (playerBuffBox.IsThisMine(ownerRenderId))
            {
                playerBuffBox.AddBuff(aBuff);
                HUDManager.InvalidatePlates();
                return;
            }
            for (int i = 0; i < partyBuffBoxes.Length; i++)
            {
                if (partyBuffBoxes[i] == null) continue;

                if (partyBuffBoxes[i].IsThisMine(ownerRenderId))
                {
                    partyBuffBoxes[i].AddBuff(aBuff);
                    HUDManager.InvalidatePlates();
                    return;
                }
            }
        }

        public void SetPlayerPlateBox(in EntityUiSnapshot playerSnapshot)
        {
            playerPlateBox.SetData(playerSnapshot);
            playerBuffBox.AssignBox(playerSnapshot.RenderId);
        }

        public void RefreshPlates(in EntityUiSnapshot snapshot)
        {
            switch (snapshot.RelationToPlayer)
            {
                case RelationToPlayerKind.Self:
                    playerPlateBox.Refresh(snapshot);
                    if (!targetPlateBox.BelongsTo(snapshot.RenderId)) break;
                    targetPlateBox.Refresh(snapshot);
                    HUDManager.InvalidatePlates();
                    break;
                case RelationToPlayerKind.Friendly:
                    if (targetPlateBox.BelongsTo(snapshot.RenderId)) targetPlateBox.Refresh(snapshot);
                    for (int i = 0; i < partyPlateBoxes.Length; i++)
                    {
                        if (!partyPlateBoxes[i].BelongsTo(snapshot.RenderId)) continue;
                        partyPlateBoxes[i].Refresh(snapshot);
                        HUDManager.InvalidatePlates();
                        break;
                    }
                    break;
                case RelationToPlayerKind.Neutral:
                case RelationToPlayerKind.Hostile:
                    if (!targetPlateBox.BelongsTo(snapshot.RenderId)) break;
                    targetPlateBox.Refresh(snapshot);
                    HUDManager.InvalidatePlates();
                    break;
                default:
                    break;
            }
        }
        public void SetNewTarget(RelationToPlayerKind aTargeterRelation, EntityUiSnapshot? aTarget)
        {
            switch (aTargeterRelation)
            {
                case RelationToPlayerKind.Self:
                    targetPlateBox.SetTarget(aTarget);
                    targetBuffBox.AssignBox(aTarget?.RenderId);
                    HUDManager.InvalidatePlates();
                    break;
                case RelationToPlayerKind.Friendly:
                case RelationToPlayerKind.Neutral:
                case RelationToPlayerKind.Hostile:
                    break;
                default:
                    throw new NotImplementedException();
            }


        }

        public void AddGuildMemberToParty(in EntityUiSnapshot aGuildMember)
        {
            if (PartyPlateBox.PartyBoxesActive >= Party.maxPartySize)
            {
                DebugManager.Print("Tried to add to full party.");
                return;
            }

            partyPlateBoxes[PartyPlateBox.PartyBoxesActive].SetTarget(aGuildMember);
            partyBuffBoxes[PartyPlateBox.PartyBoxesActive - 1].AssignBox(aGuildMember.RenderId);
            HUDManager.InvalidatePlates();
        }

        public void RemoveGuildMemberFromParty(int memberRenderId)
        {
            int index = FindGuildMemberPartyIndex(memberRenderId);

            Debug.Assert(index >= 0);
            if (index < 0) return;

            if (PartyPlateBox.PartyBoxesActive - 1 == index)
            {
                partyPlateBoxes[index].RemoveTarget();
                return;
            }

            for (int i = index; i < PartyPlateBox.PartyBoxesActive - 1; i++)
            {
                if (partyPlateBoxes[i + 1].Snapshot.HasValue)
                {
                    partyPlateBoxes[i].SetTarget(partyPlateBoxes[i + 1].Snapshot.Value);
                }
                else
                {
                    partyPlateBoxes[i].RemoveTarget();
                }
            }

            partyPlateBoxes[PartyPlateBox.PartyBoxesActive - 1].RemoveTarget();
            HUDManager.InvalidatePlates();
        }

        public void ClearParty()
        {
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                partyPlateBoxes[i].RemoveTarget();
            }
            PartyPlateBox.ClearPartyBoxes();
            HUDManager.InvalidatePlates();
        }

        int FindGuildMemberPartyIndex(int aGuildMemberRenderId)
        {
            for (int i = 0; i < partyPlateBoxes.Length; i++)
            {
                if (partyPlateBoxes[i].BelongsTo(aGuildMemberRenderId))
                {
                    return i;
                }

            }

            return -1;
        }

        public void AddGuildMemberToControl(int memberRenderId)
        {
            int index = FindGuildMemberPartyIndex(memberRenderId);

            if (index == -1) return;

            partyPlateBoxes[index].VisibleBorder = false;
            HUDManager.InvalidatePlates();
        }
        public void RemoveWalkerFromControl(int[] memberRenderIds)
        {
            if (memberRenderIds == null) return;
            for (int i = 0; i < memberRenderIds.Length; i++)
            {
                RemoveWalkerFromControl(memberRenderIds[i]);
            }
        }

        public void RemoveWalkerFromControl(int memberRenderId)
        {
            int index = FindGuildMemberPartyIndex(memberRenderId);

            if (index == -1) return;

            partyPlateBoxes[index].VisibleBorder = false;
            HUDManager.InvalidatePlates();
        }

        public void HudMovableDraw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].HudMovableDraw(aBatch);
            }
        }

        public void Draw(SpriteBatch aBatch)
        {
            ThreadAffinity.AssertMainThread();
            for (int i = 0; i < plateBoxes.Count; i++)
            {
                plateBoxes[i].Draw(aBatch);
            }
        }

        public int DrawListCount
        {
            get
            {
                AssertUiOrMainThread();
                return plateBoxes?.Count ?? 0;
            }
        }

        public int CopyDrawList(UIElement[] destination)
        {
            AssertUiOrMainThread();
            if (destination == null || destination.Length == 0 || plateBoxes == null || plateBoxes.Count == 0) return 0;

            int count = Math.Min(destination.Length, plateBoxes.Count);
            for (int i = 0; i < count; i++)
            {
                destination[i] = plateBoxes[i];
            }
            return count;
        }

        static void AssertUiOrMainThread()
        {
            if (ThreadAffinity.IsMainThread) return;
            ThreadAffinity.AssertUiThread();
        }
    }
}
