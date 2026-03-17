using Microsoft.Xna.Framework.Input;
using Project_1.Managers;

namespace Project_1.Input
{
    internal static partial class KeyBindManager
    {
        // Note: Any new key added here must have BOTH primary and secondary entries in the default keybind file.
        public enum KeyListner
        {
            MoveCharacterUp, MoveCharacterLeft, MoveCharacterDown, MoveCharacterRight,
            DebugTeleport, DebugHealthPotion, DebugManaPotion, DebugDeleteShapes, DebugTestGear,
            Inventory, SpellBook, Character, GuildRoster, LogicWindow,
            CenterCamera,
            SpellBar1Spell1, SpellBar1Spell2, SpellBar1Spell3, SpellBar1Spell4, SpellBar1Spell5, SpellBar1Spell6, SpellBar1Spell7, SpellBar1Spell8, SpellBar1Spell9, SpellBar1Spell10,

            Count
        }

        static KeySet[] firstButtons = new KeySet[(int)KeyListner.Count];
        static KeySet[] secondButtons = new KeySet[(int)KeyListner.Count];
        static bool initialized;

        public static KeySet GetKey(bool aFirstButton, KeyListner aListner)
        {
            if (aFirstButton) return firstButtons[(int)aListner];
            return secondButtons[(int)aListner];
        }
    }
}
