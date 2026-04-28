using Newtonsoft.Json;
using System.Collections.Generic;

namespace Project_1.Tiles
{
    // Legacy save bridge for the short-lived column-list format.
    // New runtime chunk storage uses Block[,,] directly.
    internal sealed class BlockColumn
    {
        public IReadOnlyList<Block> Blocks => blocks;
        readonly List<Block> blocks;

        [JsonConstructor]
        public BlockColumn(IEnumerable<Block> blocks = null)
        {
            this.blocks = blocks != null ? new List<Block>(blocks) : new List<Block>();
        }
    }
}
