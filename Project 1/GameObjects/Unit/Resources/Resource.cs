﻿using Microsoft.Xna.Framework;
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


        public bool isCastable(float aValue)
        {
            return Value - aValue >= 0;
        }

        public virtual void Update()
        {
            Value += RegenValue;
        }

        public virtual void CastSpell(float aCost)
        {
            Value -= aCost;
        }

        public void LevelUp()
        {
            BaseMaxValue += PerLevel;
            MaxValue += PerLevel;
            Value = MaxValue;
        }

        public void Refresh(PrimaryStats aStats)
        {
            MaxValue = CalculateMaxValue(aStats);
        }

        public abstract float CalculateMaxValue(PrimaryStats aStats);
    }
}
