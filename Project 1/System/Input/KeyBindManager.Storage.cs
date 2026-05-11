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

        static void LoadBindings(string aKeyBindSetting)
        {
            string dataAsString = File.ReadAllText(aKeyBindSetting);
            KeySet[] importedBinds = SaveManager.ImportData<KeySet[]>(dataAsString);
            if (importedBinds == null || importedBinds.Length == 0 || importedBinds.Length % 2 != 0)
            {
                throw new MissingDataException("Invalid keybind data.");
            }

            int listenerCount = (int)KeyListner.Count;
            int importedListenerCount = importedBinds.Length / 2;
            bool loadingDefaultFile = string.Equals(aKeyBindSetting, SaveManager.DefaultKeyBindSettings, StringComparison.OrdinalIgnoreCase);
            if (loadingDefaultFile && importedListenerCount != listenerCount)
            {
                throw new MissingDataException($"Invalid number of default keybinds. Expected {listenerCount * 2} but got {importedBinds.Length}");
            }

            KeySet[] defaultBinds = loadingDefaultFile
                ? importedBinds
                : SaveManager.ImportData<KeySet[]>(File.ReadAllText(SaveManager.DefaultKeyBindSettings));
            if (defaultBinds == null || defaultBinds.Length != listenerCount * 2)
            {
                throw new MissingDataException("Invalid default keybind data.");
            }

            for (int i = 0; i < listenerCount; i++)
            {
                firstButtons[i] = ResolveBinding(importedBinds, importedListenerCount, defaultBinds, listenerCount, i, false);
                secondButtons[i] = ResolveBinding(importedBinds, importedListenerCount, defaultBinds, listenerCount, i, true);
            }

            if (!loadingDefaultFile && importedListenerCount < listenerCount)
            {
                MigrateLegacyBindings(defaultBinds);
            }
        }

        static KeySet ResolveBinding(KeySet[] aImportedBinds, int aImportedListenerCount, KeySet[] aDefaultBinds, int aListenerCount, int aIndex, bool aSecondary)
        {
            int importedIndex = aSecondary ? aIndex + aImportedListenerCount : aIndex;
            if (aIndex < aImportedListenerCount)
            {
                return aImportedBinds[importedIndex];
            }

            int defaultIndex = aSecondary ? aIndex + aListenerCount : aIndex;
            return aDefaultBinds[defaultIndex];
        }

        static void MigrateLegacyBindings(KeySet[] aDefaultBinds)
        {
            int debugTeleportIndex = (int)KeyListner.DebugTeleport;
            if (firstButtons[debugTeleportIndex].Key == Keys.Q && firstButtons[debugTeleportIndex].Modifiers.Length == 0)
            {
                firstButtons[debugTeleportIndex] = aDefaultBinds[debugTeleportIndex];
            }

            int rotateCameraLeftIndex = (int)KeyListner.RotateCameraLeft;
            if (firstButtons[rotateCameraLeftIndex].Key == Keys.Q && firstButtons[rotateCameraLeftIndex].Modifiers.Length == 0)
            {
                firstButtons[rotateCameraLeftIndex] = aDefaultBinds[rotateCameraLeftIndex];
            }

            int rotateCameraRightIndex = (int)KeyListner.RotateCameraRight;
            if ((firstButtons[rotateCameraRightIndex].Key == Keys.R || firstButtons[rotateCameraRightIndex].Key == Keys.E) &&
                firstButtons[rotateCameraRightIndex].Modifiers.Length == 0)
            {
                firstButtons[rotateCameraRightIndex] = aDefaultBinds[rotateCameraRightIndex];
            }

            int togglePlayerFacingIndex = (int)KeyListner.TogglePlayerCameraFacing;
            if (firstButtons[togglePlayerFacingIndex].Key == Keys.F && firstButtons[togglePlayerFacingIndex].Modifiers.Length == 0)
            {
                firstButtons[togglePlayerFacingIndex] = aDefaultBinds[togglePlayerFacingIndex];
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
