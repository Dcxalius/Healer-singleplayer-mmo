using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class SpellResitance
    {
        int baseResitance;
        Dictionary<SpellSchool, int> resitanceBySchool;
        public int BaseResitance => baseResitance;
        public int[] ResitanceBySchool
        {
            get
            {
                int maxEnumValue = Enum.GetValues(typeof(SpellSchool)).Cast<int>().Max();
                int[] result = new int[maxEnumValue + 1];

                foreach (var kvp in resitanceBySchool)
                {
                    result[(int)kvp.Key] = kvp.Value;
                }

                return result;
            }
        }

        public int GetResitance(SpellSchool aSchool) => baseResitance + (resitanceBySchool.TryGetValue(aSchool, out int schoolResitance) ? schoolResitance : 0);
        public int GetResitance(SpellSchool[] aSchools, double[] aRatio) => (int)(baseResitance + (aSchools.Select((school, index) => (resitanceBySchool.TryGetValue(school, out int schoolResitance) ? schoolResitance : 0) * aRatio[index]).Sum()));
        public SpellResitance(int aBaseResitance, Dictionary<SpellSchool, int> aResitanceBySchool)
        {
            baseResitance = aBaseResitance;
            resitanceBySchool = aResitanceBySchool;
        }
        public SpellResitance(int aBaseResitance) : this(aBaseResitance, new Dictionary<SpellSchool, int>())
        {

        }
        public SpellResitance() : this(0, new Dictionary<SpellSchool, int>())
        {

        }

        [JsonConstructor]
        public SpellResitance(int baseResitance, int[] resitanceBySchool) : this(baseResitance, new Dictionary<SpellSchool, int>())
        {
            for (int i = 0; i < resitanceBySchool.Length; i++)
            {
                this.resitanceBySchool[(SpellSchool)i] = resitanceBySchool[i];
            }
        }
    }
}
