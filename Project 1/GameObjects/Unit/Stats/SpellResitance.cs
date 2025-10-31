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
        Dictionary<SpellSchools, int> resitanceBySchool;
        public int BaseResitance => baseResitance;
        public int[] ResitanceBySchool
        {
            get
            {
                int maxEnumValue = Enum.GetValues(typeof(SpellSchools)).Cast<int>().Max();
                int[] result = new int[maxEnumValue + 1];

                foreach (var kvp in resitanceBySchool)
                {
                    result[(int)kvp.Key] = kvp.Value;
                }

                return result;
            }
        }

        public int GetResitance(SpellSchools aSchool) => baseResitance + (resitanceBySchool.TryGetValue(aSchool, out int schoolResitance) ? schoolResitance : 0);
        public int GetResitance(SpellSchools[] aSchools, double[] aRatio) => (int)(baseResitance + (aSchools.Select((school, index) => (resitanceBySchool.TryGetValue(school, out int schoolResitance) ? schoolResitance : 0) * aRatio[index]).Sum()));
        public SpellResitance(int aBaseResitance, Dictionary<SpellSchools, int> aResitanceBySchool)
        {
            baseResitance = aBaseResitance;
            resitanceBySchool = aResitanceBySchool;
        }
        public SpellResitance(int aBaseResitance) : this(aBaseResitance, new Dictionary<SpellSchools, int>())
        {

        }
        public SpellResitance() : this(0, new Dictionary<SpellSchools, int>())
        {

        }

        [JsonConstructor]
        public SpellResitance(int baseResitance, int[] resitanceBySchool) : this(baseResitance, new Dictionary<SpellSchools, int>())
        {
            for (int i = 0; i < resitanceBySchool.Length; i++)
            {
                this.resitanceBySchool[(SpellSchools)i] = resitanceBySchool[i];
            }
        }
    }
}
