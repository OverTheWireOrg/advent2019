public class InventoryItem
{
    public int Id;
    public string Name;
    public string Description;
    public Tile Icon;
}

public static class InventoryItems
{
    private static InventoryItem Sword0 = new InventoryItem
    {
        Id = 0,
        Name = "Club",
        Description = "A basic thing to swing at monsters.",
        Icon = new Tile("item-grid-cell", 5, 13, true, 32)
    };
    private static InventoryItem Sword1 = new InventoryItem
    {
        Id = 1,
        Name = "Rusty Sword",
        Description = "Not the greatest weapon, but it's better than a club.",
        Icon = new Tile("item-grid-cell", 5, 0, true, 32)
    };
    private static InventoryItem Sword2 = new InventoryItem
    {
        Id = 2,
        Name = "Iron Sword",
        Description = "A proper sword that's actually sharp.",
        Icon = new Tile("item-grid-cell", 5, 3, true, 32)
    };
    private static InventoryItem Sword3 = new InventoryItem
    {
        Id = 3,
        Name = "Steel Sword",
        Description = "Slay greater foes with stainless steel technology.",
        Icon = new Tile("item-grid-cell", 5, 4, true, 32)
    };
    private static InventoryItem Sword4 = new InventoryItem
    {
        Id = 4,
        Name = "Titanium Sword",
        Description = "Military-grade strength, against the greatest enemies.",
        Icon = new Tile("item-grid-cell", 5, 1, true, 32)
    };
    private static InventoryItem Sword5 = new InventoryItem
    {
        Id = 5,
        Name = "Diamond Sword",
        Description = "Made with the hardest material found on earth, for the ultra rich.",
        Icon = new Tile("item-grid-cell", 5, 2, true, 32)
    };

    private static InventoryItem HpPotion = new InventoryItem
    {
        Id = 6,
        Name = "Magical Potion",
        Description = "Restores your health. Unlimited uses.",
        Icon = new Tile("item-grid-cell", 9, 12, true, 32)
    };

    private static InventoryItem ClientFlag = new InventoryItem
    {
        Id = 7,
        Name = "Mystery Letter",
        Description = "A letter which reads: 'Here is the client side flag: AOTW{B14ckbOx_N3tw0rK_r3v",
        Icon = new Tile("item-grid-cell", 13, 11, true, 32)
    };
    private static InventoryItem ServerFlag = new InventoryItem
    {
        Id = 8,
        Name = "Scroll of Secrets",
        Description = "It is said that whoever reads the secrets written here would possess unlimited power.",
        Icon = new Tile("item-grid-cell", 13, 10, true, 32)
    };

    public static InventoryItem[] AllItems = {
        Sword0,
        Sword1,
        Sword2,
        Sword3,
        Sword4,
        Sword5,
        HpPotion,
        ClientFlag,
        ServerFlag
    };
}
