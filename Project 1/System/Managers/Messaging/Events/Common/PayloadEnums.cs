using Project_1.GameObjects.Unit;
using Project_1.Input;
using Project_1.Managers.States;
using Project_1.UI.UIElements.Boxes;

namespace Project_1.Messaging.Events
{
    internal enum StateKind
    {
        StartScreen,
        Game,
        MoveHUD,
        PauseMenu,
        OptionMenu,
        LoadingMenu,
        NewGame
    }

    internal enum ClickKind
    {
        Left,
        Middle,
        Right
    }

    internal enum RelationToPlayerKind
    {
        Self,
        Friendly,
        Neutral,
        Hostile
    }

    internal enum EquipmentSlotKind
    {
        Head,
        Neck,
        Shoulders,
        Back,
        Chest,
        Wrist,
        Hands,
        Belt,
        Legs,
        Feet,
        Finger1,
        Finger2,
        Trinket1,
        Trinket2,
        MainHand,
        OffHand,
        Ranged,
        Count
    }

    internal enum DialoguePopupLocation
    {
        HUDManager,
        StateManager
    }

    internal enum DialoguePauseKind
    {
        Pauses,
        NoPause
    }

    internal static class PayloadEnumConversions
    {
        public static StateKind ToStateKind(this StateManager.States value)
        {
            return (StateKind)value;
        }

        public static StateManager.States ToStateManagerState(this StateKind value)
        {
            return (StateManager.States)value;
        }

        public static ClickKind ToClickKind(this InputManager.ClickType value)
        {
            return (ClickKind)value;
        }

        public static InputManager.ClickType ToInputClickType(this ClickKind value)
        {
            return (InputManager.ClickType)value;
        }

        public static RelationToPlayerKind ToRelationToPlayerKind(this Relation.RelationToPlayer value)
        {
            return (RelationToPlayerKind)value;
        }

        public static Relation.RelationToPlayer ToRelationToPlayer(this RelationToPlayerKind value)
        {
            return (Relation.RelationToPlayer)value;
        }

        public static EquipmentSlotKind ToEquipmentSlotKind(this Equipment.Slot value)
        {
            return (EquipmentSlotKind)value;
        }

        public static Equipment.Slot ToEquipmentSlot(this EquipmentSlotKind value)
        {
            return (Equipment.Slot)value;
        }

        public static DialoguePopupLocation ToDialoguePopupLocation(this DialogueBox.LocationOfPopUp value)
        {
            return (DialoguePopupLocation)value;
        }

        public static DialogueBox.LocationOfPopUp ToDialogueBoxLocation(this DialoguePopupLocation value)
        {
            return (DialogueBox.LocationOfPopUp)value;
        }

        public static DialoguePauseKind ToDialoguePauseKind(this DialogueBox.PausesGame value)
        {
            return (DialoguePauseKind)value;
        }

        public static DialogueBox.PausesGame ToDialogueBoxPause(this DialoguePauseKind value)
        {
            return (DialogueBox.PausesGame)value;
        }
    }
}
