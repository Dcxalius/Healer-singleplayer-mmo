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
            Inventory, SpellBook, Character, 
            SpellBar1Spell1, SpellBar1Spell2, SpellBar1Spell3, SpellBar1Spell4, SpellBar1Spell5, SpellBar1Spell6, SpellBar1Spell7, SpellBar1Spell8, SpellBar1Spell9, SpellBar1Spell10,

            Count
        }

        static Keys[] firstButtons = new Keys[(int)KeyListner.Count];
        static Keys[] secondButtons = new Keys[(int)KeyListner.Count];

        readonly static Keys[] defaultFirstKeys = new Keys[(int)KeyListner.Count] //TODO: Make this have a variable per entry for modkeys
        {Keys.W, Keys.A, Keys.S, Keys.D, 
         Keys.Q, Keys.NumPad0, Keys.NumPad1, Keys.Delete, Keys.NumPad2,
         Keys.I, Keys.T, Keys.C,
         Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0};
        readonly static Keys[] defaultSecondKeys = new Keys[(int)KeyListner.Count] 
        {Keys.Up, Keys.Left, Keys.Down, Keys.Right, 
         Keys.None, Keys.None, Keys.None, Keys.None, Keys.None,
         Keys.None, Keys.None,Keys.None,
         Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None, Keys.None};

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
                string dataAsString = System.IO.File.ReadAllText(rootDir + "\\Data\\Keybinds.json");

                Keys[] importedBinds = JsonConvert.DeserializeObject<Keys[]>(dataAsString);
                Debug.Assert(importedBinds.Length != (int)KeyListner.Count);
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
            System.IO.File.WriteAllText(aDestination, json);
        }

        static void DefaultKeys()
        {
            for (int i = 0; i < firstButtons.Length; i++)
            {
                firstButtons[i] = defaultFirstKeys[i];
                secondButtons[i] = defaultSecondKeys[i];
            }
        }

        public static Keys? GetKey(bool aFirstButton, KeyListner aListner)
        {
            if (aFirstButton)
            {
                return firstButtons[(int)aListner];
            }
            return secondButtons[(int)aListner];
        }

        public static bool GetPress(KeyListner aListner)
        {
            return InputManager.GetPress(firstButtons[(int)aListner]) || InputManager.GetPress(secondButtons[(int)aListner]);
        }

        public static bool GetHold(KeyListner aListner)
        {
            return InputManager.GetHold(firstButtons[(int)aListner]) || InputManager.GetHold(secondButtons[(int)aListner]);
        }

        public static bool SetKey(bool aFirstButton, KeyListner aListner, Keys aKey)
        {
            if (aFirstButton)
            {
                firstButtons[(int)aListner] = aKey;
                return true;
            }
            else
            {
                secondButtons[(int)aListner] = aKey;
                return true;
            }
        }

        public static bool CheckForNoDupeKeys(Keys aKey)
        {
            for (int i = 0; i < firstButtons.Length; i++)
            {
                if (firstButtons[i] == aKey)
                {
                    return false;
                }
            }
            for (int j = 0; j < secondButtons.Length; j++)
            {
                if (secondButtons[j] == aKey)
                {
                    return false;
                }
            }

            return true;
        }

        public static void Update()
        {

        }
    }
}
