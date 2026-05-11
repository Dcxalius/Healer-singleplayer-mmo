using System;

namespace Project_1.System.Models.BaseModels
{
    internal sealed class EntityModelDefinition
    {
        public string Key { get; }
        public UnitModel.Type ShellType { get; }
        public int PresentationFrameSizePixels { get; }

        public EntityModelDefinition(string aKey, UnitModel.Type aShellType, int aPresentationFrameSizePixels = 32)
        {
            if (string.IsNullOrWhiteSpace(aKey)) throw new ArgumentException("Model definition key is required.", nameof(aKey));

            Key = aKey;
            ShellType = aShellType;
            PresentationFrameSizePixels = Math.Max(1, aPresentationFrameSizePixels);
        }
    }
}
