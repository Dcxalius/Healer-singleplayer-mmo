using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Spells
{
    internal struct AreaOfEffectData
    {
        public enum HitBoxType
        {
            Circle,
            Ellipse,
            Rectangle
        }

        public enum Target
        {
            Enemy,
            Friendly,
            Self
        }

        string name;
        Point hitBoxSize;
        HitBoxType hitBoxType;
        SpellEffect[] effects;
        //double duration;
        //double timeBetweenTriggers;
        //bool aGoodNameForABoolToIndicateAnEntityCanTakeDamageFromTheAoEAgainIfTheyKeepStandingInIt;
        Target[] targets;


        [JsonConstructor]
        public AreaOfEffectData(string name, int hitBoxWidth, int hitBoxHeight, HitBoxType? hitBoxType, string[] effects, Target[] targets)
        {
            this.name = name;
            hitBoxSize = new Point(hitBoxWidth, hitBoxHeight);
            Debug.Assert(hitBoxType.HasValue);
            this.hitBoxType = hitBoxType.Value;
            List<SpellEffect> spellEffects = new List<SpellEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                spellEffects.Add(new SpellEffect(effects[i]));
            }
            this.effects = spellEffects.ToArray();
            this.targets = targets;

            Assert();
        }


        void Assert()
        {
            bool b = hitBoxSize != Point.Zero;

            b = b && effects.Length > 0;

            b = b && targets.Length > 0;


            Debug.Assert(b);
        }
    }
}
