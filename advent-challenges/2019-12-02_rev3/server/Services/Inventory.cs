using System.Collections.Generic;


namespace server
{
    public class ItemRegistry
    {
        private static Hpmv.InventoryItem MakeItem(int id, int attack, int hpRestore, long buyPrice, long sellPrice)
        {
            var item = new Hpmv.InventoryItem();
            item.Id = id;
            item.Attack = attack;
            item.HpRestore = hpRestore;
            item.MoveCosts.Add(new Hpmv.InventoryItem.Types.MoveCost { Inventory = 2, Out = true, Cost = -buyPrice });
            item.MoveCosts.Add(new Hpmv.InventoryItem.Types.MoveCost { Inventory = 2, Out = false, Cost = sellPrice });
            return item;
        }

        public static readonly List<Hpmv.InventoryItem> Items = new List<Hpmv.InventoryItem>{
            MakeItem(0, 10, 0, 10, 8),
            MakeItem(1, 20, 0, 100, 80),
            MakeItem(2, 40, 0, 1000, 800),
            MakeItem(3, 80, 0, 10000, 8000),
            MakeItem(4, 160, 0, 100000, 80000),
            MakeItem(5, 320, 0, 1000000, 800000),
            MakeItem(6, 0, 100000, 10, 10),  // HP potion
            MakeItem(7, 0, 0, 0, 0),  // client flag
            MakeItem(8, 0, 0, 10000000, 0),  // server flag
        };
    }
}