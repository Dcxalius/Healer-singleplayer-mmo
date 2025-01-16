using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit
{
    internal class PairReport 
    {
        public string Value
        {
            get
            {
                string s = "";
                if (pairs.Count == 0) return s;
                s += pairs[0].Item2 + " " + pairs[0].Item1;
                for (int i = 1; i < pairs.Count; i++)
                {
                    s += "\n" + pairs[i].Item2 + " " + pairs[i].Item1;
                }
                return s;
            }
        }

        public string StringsOnly
        {
            get
            {
                string s = "";
                if (pairs.Count == 0) return s;

                s += pairs[0].Item1;
                for (int i = 1; i < pairs.Count; i++)
                {
                    s += "\n" + pairs[i].Item1;
                }
                return s;
            }
        }

        public string NumbersOnly
        {
            get
            {
                string s = "";
                if (pairs.Count == 0) return s;

                s += pairs[0].Item2;
                for (int i = 1; i < pairs.Count; i++)
                {
                    s += "\n" + pairs[i].Item2;
                }
                return s;
            }
        }

        List<(string, double)> pairs;


        public PairReport()
        {
            pairs = new List<(string, double)>();
        }

        public void AddLine(string aReport, double aValue)
        {
             
            pairs.Add((aReport, aValue));
        }
    }
}
