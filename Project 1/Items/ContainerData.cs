﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1.Items
{
    internal class ContainerData : ItemData
    {
        public int SlotCount { get => slotCount; }
        int slotCount;
        [JsonConstructor]
        public ContainerData(int id, string gfxName, string name, string description, int slotCount) : base(id, gfxName, name, description, 1, ItemType.Container)
        {
            this.slotCount = slotCount;
            Assert();
        }

        void Assert()
        {

            Debug.Assert(slotCount > 0);
        }
    }
}