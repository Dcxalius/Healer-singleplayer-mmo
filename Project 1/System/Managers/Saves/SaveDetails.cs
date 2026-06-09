using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Managers.Saves
{
    internal class SaveDetails
    {
        [JsonProperty]
        public string Name => name;
        string name;
        [JsonProperty]
        public string ClassName => className;
        string className;

        [JsonProperty]
        public int Level => level;
        int level;

        [JsonProperty]
        public DateTime TimeInfo => timeInfo;
        DateTime timeInfo;

        [JsonProperty]
        public TimeSpan TimeInSave => timeInSave;
        TimeSpan timeInSave;

        public SaveDetails(string aName, string aClassName, int aLevel, TimeSpan aTimeInSave)
        {
            name = aName;
            className = aClassName;
            level = aLevel;
            timeInSave = aTimeInSave;
            timeInfo = DateTime.Now;
        }

        [JsonConstructor]
        public SaveDetails(string name, string className, int level, DateTime timeInfo, TimeSpan timeInSave) : this(name, className, level, timeInSave)
        {
            this.timeInfo = timeInfo;
            this.timeInSave = timeInSave;
        }

        public override string ToString()
        {
            string s = string.Empty;
            s += "Name: " + name + "\n";
            s += "Class: " + className + "\n";
            s += "Level: " + level.ToString() + "\n";
            s += "Time Played: " + timeInSave.ToString(@"dd\.hh\:mm\:ss") + "\n"; //67.22:10:10 Q: Is this the format we want? hh:mm:ss is fine, but dd.hh feels wonk
            s += "Last Time Saved: " + timeInfo.ToString() + "\n";
            return s;
        }
    }
}
