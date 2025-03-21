using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Input
{
    internal static class KeyBindManager
    {
        public enum KeyListner
        {
            MoveCharacterUp, MoveCharacterLeft, MoveCharacterDown, MoveCharacterRight,
            DebugTeleport, DebugHealthPotion, DebugManaPotion, DebugDeleteShapes, DebugTestGear,
            Inventory, SpellBook, Character, GuildRoster, 
            SpellBar1Spell1, SpellBar1Spell2, SpellBar1Spell3, SpellBar1Spell4, SpellBar1Spell5, SpellBar1Spell6, SpellBar1Spell7, SpellBar1Spell8, SpellBar1Spell9, SpellBar1Spell10,

            Count
        }

        static KeySet[] firstButtons = new KeySet[(int)KeyListner.Count];
        static KeySet[] secondButtons = new KeySet[(int)KeyListner.Count];

        readonly static KeySet[] defaultSecondKeys = new KeySet[(int)KeyListner.Count] 
        {Keys.Up, Keys.Left, Keys.Down, Keys.Right, 
         Keys.None, Keys.None, Keys.None, Keys.None, Keys.None,
         Keys.None, Keys.None,Keys.None, Keys.None,
         Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None};

        readonly static KeySet[] defaultFirstKeys = new KeySet[(int)KeyListner.Count]
        {
         Keys.W, Keys.A, Keys.S, Keys.D,
         Keys.Q, Keys.NumPad0, Keys.NumPad1, Keys.Delete, new KeySet(Keys.C, Keys.LeftShift),
         Keys.I, Keys.T, Keys.C, Keys.G,
         Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0};

        static string rootDir;

        public static void Init(ContentManager aContentManager)
        {
            
            rootDir = aContentManager.RootDirectory;
            ImportBindings();
        }

        static void ImportBindings()
        {
            try
            {
                string dataAsString = File.ReadAllText(rootDir + "\\Data\\Keybinds.json");

                KeySet[] importedBinds = JsonConvert.DeserializeObject<KeySet[]>(dataAsString);
                //Debug.Assert(importedBinds.Length == (int)KeyListner.Count * 2);
                //throw new Exception("")
                for (int i = 0; i < (int)KeyListner.Count; i++)
                {
                    firstButtons[i] = importedBinds[i];
                    secondButtons[i] = importedBinds[i + (int)KeyListner.Count];
                }

            }
            catch (Exception exception)
            {
                if (!(exception is IndexOutOfRangeException || exception is JsonSerializationException || exception is FileNotFoundException))
                {
                    throw;
                }

                DebugManager.Print(typeof(KeyBindManager), "Error Importing Bindings from file.");
                Debug.Assert(defaultFirstKeys.Length == (int)KeyListner.Count && defaultSecondKeys.Length == (int)KeyListner.Count, "Default Key has wrong count");

                
                DefaultKeys();
            }

        }

        public static void SaveBindings()
        {
            ExportData(rootDir + "\\Data\\Keybinds.json", firstButtons.Concat(secondButtons));
        }

        static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
            File.WriteAllText(aDestination, json);
        }

        static void DefaultKeys()
        {
            firstButtons = defaultFirstKeys;
            secondButtons = defaultSecondKeys;
        }

        public static KeySet GetKey(bool aFirstButton, KeyListner aListner)
        {
            if (aFirstButton) return firstButtons[(int)aListner];
            return secondButtons[(int)aListner];
        }

        public static bool GetPress(KeyListner aListner) => firstButtons[(int)aListner].GetPress || secondButtons[(int)aListner].GetPress;

        public static bool GetHold(KeyListner aListner) => firstButtons[(int)aListner].GetHold || secondButtons[(int)aListner].GetHold;

        public static void SetKey(bool aFirstButton, KeyListner aListner, Keys aKey)
        {
            if (aFirstButton)
            {
                firstButtons[(int)aListner] = new KeySet(aKey, InputManager.CheckHoldModifiers());
            }
            else
            {
                secondButtons[(int)aListner] = new KeySet(aKey, InputManager.CheckHoldModifiers());
            }
        }

        public static bool CheckForNoDupeKeys(Keys aKey, bool[] aModifier)
        {
            KeySet keySet = new KeySet(aKey, aModifier);
            for (int i = 0; i < firstButtons.Length; i++)
            {
                if (firstButtons[i].Equals(keySet))
                {
                    return false;
                }
            }
            for (int j = 0; j < secondButtons.Length; j++)
            {
                if (secondButtons[j].Equals(keySet))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
