using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.Managers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace Project_1.Input
{
    internal static partial class KeyBindManager
    {
        public static void Init()
        {
            ThreadAffinity.AssertMainThread();
            if (initialized) return;
            initialized = true;
            ImportBindings();
        }

        static void ImportBindings()
        {
            try
            {
                LoadBindings(SaveManager.KeyBindSettings);
            }
            catch (Exception exception)
            {
                if (exception is not IndexOutOfRangeException &&
                    exception is not JsonSerializationException &&
                    exception is not FileNotFoundException &&
                    exception is not MissingDataException)
                {
                    throw;
                }

                DebugManager.Print("Error Importing Bindings from file.");
                LoadBindings(SaveManager.DefaultKeyBindSettings);
            }
        }

        private class MissingDataException : Exception
        {
            public MissingDataException(string message) : base(message) { }
        }

        static void LoadBindings(string aKeyBindSettingPath)
        {
            KeySet[] importedBinds = ImportBinds(aKeyBindSettingPath);

            int listenerCount = (int)KeyListner.Count;
            int importedListenerCount = importedBinds.Length / 2;
            if (importedListenerCount != listenerCount)
            {
                throw new MissingDataException($"Invalid number of default keybinds. Expected {listenerCount * 2} but got {importedBinds.Length}");
            }

            firstButtons = importedBinds.Take(listenerCount).ToArray();
            secondButtons = importedBinds.Skip(listenerCount).Take(listenerCount).ToArray();
        }

        static KeySet[] ImportBinds(string aKeyBindSetting)
        {
            string json = File.ReadAllText(aKeyBindSetting);
            KeySet[] importedBinds = SaveManager.ImportData<KeySet[]>(json);
            if (importedBinds == null || importedBinds.Length == 0 || importedBinds.Length % 2 != 0)
            {
                throw new MissingDataException("Invalid keybind data.");
            }
            return importedBinds;
        }

        public static void SaveBindings()
        {
            ThreadAffinity.AssertUiThread();
            ExportData(SaveManager.KeyBindSettings, firstButtons.Concat(secondButtons));
        }

        static void ExportData(string aDestination, object aObjectToExport)
        {
            string json = JsonConvert.SerializeObject(
                aObjectToExport,
                Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });

            File.WriteAllText(aDestination, json);
        }
    }
}
