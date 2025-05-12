using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Superorganism.Core.Inventory
{
    /// <summary>
    /// Represents an item in the inventory.
    /// </summary>
    public class InventoryItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }

        public InventoryItem(string name, int quantity, string description)
        {
            Name = name;
            Quantity = quantity;
            Description = description;
        }
    }
}
