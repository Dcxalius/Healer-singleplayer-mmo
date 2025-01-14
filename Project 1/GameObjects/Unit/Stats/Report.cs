using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.GameObjects.Unit.Stats
{
    internal class Report
    {
        string value;

        public Report()
        {
            value = "";
        }

        public void AddLine(string aReport)
        {
            if (aReport == "") return;
            if (aReport == null) return;
            if (value != "")
            {
                value += "\n";
            }
            value += aReport;
        }

    }
}
