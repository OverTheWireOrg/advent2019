using Microsoft.AspNetCore.Components;

namespace client.Pages
{
    public partial class Inventory
    {
        [Parameter]
        public Hpmv.Inventory Contents { get; set; }

        [Parameter]
        public int OtherInventoryId { get; set; }

        [Parameter]
        public EventCallback<(int fromInventory, int itemIndex, int toInventory)> OnMoveItem { get; set; }

        [Parameter]
        public EventCallback<int> OnUseItem { get; set; }

        private string Title { get {
            if (Contents == null) return "";
            switch (Contents.Id) {
                case 0: return "Equipment";
                case 1: return "Stash";
                case 2: return "Shop";
            }
            return "";
        }}

        private long TransferCost(Hpmv.InventoryItem item, int inventory, bool isOut)
        {
            foreach (var cost in item.MoveCosts)
            {
                if (cost.Inventory == inventory && cost.Out == isOut)
                {
                    return cost.Cost;
                }
            }
            return 0;
        }
    }
}