using Newtonsoft.Json;
using Project_1.GameObjects;
using Project_1.Managers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

        static void LoadBindings(string aKeyBindSetting)
        {
            string dataAsString = File.ReadAllText(aKeyBindSetting);
            KeySet[] importedBinds = SaveManager.ImportData<KeySet[]>(dataAsString);
            if (importedBinds.Length != (int)KeyListner.Count * 2)
            {
                throw new MissingDataException($"Invalid number of keybinds in both files. Expected {(int)KeyListner.Count * 2} but got {importedBinds.Length}");
            }

            for (int i = 0; i < (int)KeyListner.Count; i++)
            {
                firstButtons[i] = importedBinds[i];
                secondButtons[i] = importedBinds[i + (int)KeyListner.Count];
            }
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
