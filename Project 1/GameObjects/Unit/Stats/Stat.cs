﻿using System;
using System.CodeDom;
using System.Diagnostics;

namespace Project_1.GameObjects.Unit.Stats
{
    internal abstract class Stat
    {
        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        int value;

        public Stat(int aValue)
        {
            value = aValue;
        }



        public override string ToString()
        {
            if (value == 0) return "";
            return value + " " + GetType().Name.ToString();
        }

        public static implicit operator int(Stat stat) => stat.Value;

        public static bool operator ==(Stat lhs, int aValue) => lhs.Equals(aValue);
        public static bool operator !=(Stat lhs, int aValue) => !lhs.Equals(aValue);


        public bool Equals(int aInt) => Value == aInt;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            throw new System.NotImplementedException();
        }

        public void Decrease(int aAmount)
        {
            value -= aAmount;
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }

        internal void Increase(int aAmount)
        {
            value += aAmount;
        }
    }
}
