﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Entities.Resources
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

        public virtual float MaxValue { get; }
        public virtual float Value { get; protected set; }

        public virtual float RegenValue { get; protected set; }

        public ResourceType Type { get => resourceType; }
        ResourceType resourceType;

        public Color ResourceColor => resourceColor;
        Color resourceColor;


        public Resource(ResourceType aResource, Color aColor) 
        {
            resourceType = aResource;
            resourceColor = aColor;

        }


        public bool isCastable(float aValue)
        {
            return Value - aValue > 0;
        }

        public virtual void Update() 
        {
            Value += RegenValue;
        }

        public virtual void CastSpell(float aCost) 
        {
            Value -= aCost;
        }
    }
}