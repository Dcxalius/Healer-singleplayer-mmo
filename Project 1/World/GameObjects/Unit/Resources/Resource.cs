using Microsoft.Xna.Framework;
using Project_1.GameObjects.Entities;
using Project_1.GameObjects.Unit.Stats;
using Project_1.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Resources
{
    internal abstract class Resource
    {
        static void AssertSimThread() => ThreadAffinity.AssertSimThread();

        public enum ResourceType
        {
            None,
            Mana,
            Energy,
            Rage
        }

        public string Name { get => resourceType.ToString(); }

        public virtual float MaxValue { get; protected set; }
        public virtual float Value { get; set; }

        public virtual float RegenValue { get; protected set; }

        public ResourceType Type { get => resourceType; }
        ResourceType resourceType;

        public Color ResourceColor => resourceColor;
        Color resourceColor;

        protected virtual float BaseMaxValue { get; set; }
        protected virtual float PerLevel { get; set; }

        public Resource(ResourceType aResource, Color aColor)
        {
            resourceType = aResource;
            resourceColor = aColor;
        }

        public bool isCastable(double aValue)
        {
            AssertSimThread();
            return Value - aValue >= 0;
        }

        public abstract void Update();

        public abstract void TickRegen(bool aInCombat);

        public virtual void SetOwner(Entity aOwner) { }

        public virtual void CastSpell(double aCost)
        {
            AssertSimThread();
            //TODO: oh no, this is a double and the resource is a float, should we be doing something about that?
            Value -= (float)aCost;
        }

        public void LevelUp()
        {
            AssertSimThread();
            BaseMaxValue += PerLevel;
            MaxValue += PerLevel;
            Value = MaxValue;
        }

        public abstract void Refresh(TotalPrimaryStats aStats);
    }
}
