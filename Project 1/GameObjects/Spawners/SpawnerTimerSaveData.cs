using Newtonsoft.Json;

namespace Project_1.GameObjects.Spawners
{
    internal sealed class SpawnerTimerSaveData
    {
        [JsonProperty("NextSpawnTime")]
        public double NextSpawnTime { get; set; }

        [JsonProperty("TimeSinceLastDeath")]
        public double TimeSinceLastDeath { get; set; }
    }
}
