using System;
using System.Collections.Generic;
using System.Text;

namespace Project_1.World.GameObjects.Entities
{
    internal interface IReportable
    {
        public string Name { get; }
        public string Description { get; }
        public string Data { get; }
    }
}
