using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project_1.GameObjects.Unit.Relation;

namespace Project_1.GameObjects.Unit
{
    internal class Relation
    {
        public enum RelationToPlayer
        {
            Self,
            Friendly,
            Neutral,
            Hostile
        }

        static Color[] RelationColors = new Color[] { Color.AliceBlue, Color.LightSeaGreen, Color.Yellow, Color.IndianRed };
        public Color RelationColor() { return RelationColors[(int)relationToPlayer]; }

        public RelationToPlayer ToPlayer { get => relationToPlayer; set => relationToPlayer = value; }
        RelationToPlayer relationToPlayer;

        public Relation(RelationToPlayer? aRelation)
        {

            Debug.Assert(aRelation.HasValue);
            relationToPlayer = aRelation.Value;
        }
    }
}
