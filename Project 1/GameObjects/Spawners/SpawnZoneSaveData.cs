using Newtonsoft.Json;
using Project_1.GameObjects.Spawners.Pathing;

namespace Project_1.GameObjects.Spawners
{
    internal sealed class SpawnZoneSaveData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("MobNames")]
        public string[] MobNames { get; set; }

        [JsonProperty("Pathing", TypeNameHandling = TypeNameHandling.Auto)]
        public MobPathing[] Pathing { get; set; }
    }
}
