using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            DebugFire,


            Count
        }

        static Keys?[] firstButtons = new Keys?[(int)KeyListner.Count];
        static Keys?[] secondButtons = new Keys?[(int)KeyListner.Count];

        readonly static Keys[] defaultFirstKeys = new Keys[(int)KeyListner.Count] {Keys.W, Keys.A, Keys.S, Keys.D, Keys.Q};
        readonly static Keys[] defaultSecondKeys = new Keys[(int)KeyListner.Count] {Keys.Up, Keys.Left, Keys.Down, Keys.Right , Keys.None};

        public static void Init(ContentManager aContentManager)
        {
            
            ImportBindings(aContentManager.RootDirectory);
        }

        static void ImportBindings(string aPathToData)
        {
            try
            {
                string dataAsString = System.IO.File.ReadAllText(aPathToData + "\\Data\\Keybinds.json");

                Keys[] importedBinds = JsonConvert.DeserializeObject<Keys[]>(dataAsString);
                Debug.Assert(importedBinds.Length != (int)KeyListner.Count);
                for (int i = 0; i < (int)KeyListner.Count; i++)
                {
                    firstButtons[i] = importedBinds[i];
                    secondButtons[i] = importedBinds[i + (int)KeyListner.Count];
                }

            }
            catch (Exception)
            {
                
                Debug.Assert(defaultFirstKeys.Length - 1 != (int)KeyListner.Count || defaultSecondKeys.Length - 1 != (int)KeyListner.Count, "Default Key has wrong count");

                
                DefaultKeys();
            }

        }

        public static void SaveBindings(string aPathToData)
        {
            ExportData(aPathToData + "\\Data\\Keybinds.json", firstButtons.Union(secondButtons));

        }

        static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(aObjectToExport);
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

        public static bool GetPress(KeyListner aListner)
        {
            return InputManager.GetPress(firstButtons[(int)aListner].Value) || InputManager.GetPress(secondButtons[(int)aListner].Value);
        }

        public static bool GetHold(KeyListner aListner)
        {
            return InputManager.GetHold(firstButtons[(int)aListner].Value) || InputManager.GetHold(secondButtons[(int)aListner].Value);
        }

        public static void Update()
        {

        }
    }
}
