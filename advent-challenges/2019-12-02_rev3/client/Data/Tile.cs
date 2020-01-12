public class Tile
{
    public readonly string CssClass;
    public readonly int Row;
    public readonly int Col;
    public readonly string CssBackgroundPosition;
    public readonly bool CanWalk;

    public Tile(string clazz, int row, int col, bool canWalk = true, int size = 16)
    {
        CssClass = clazz;
        Row = row;
        Col = col;
        CssBackgroundPosition = "-" + Col * size + "px -" + Row * size + "px";
        CanWalk = canWalk;
    }
}

public static class Tiles
{
    private static Tile Main(int row, int col, bool canWalk = true)
        => new Tile("main-grid-cell", row, col, canWalk);

    public static Tile[] Player = {
        Main(0, 1), Main(1, 1), Main(2, 1), Main(3, 1)
    };

    public static Tile[][] Grass = {
        new Tile[]{Main(1, 4), Main(1, 5), Main(1, 6)},
        new Tile[]{Main(2, 4), Main(2, 5), Main(2, 6)},
        new Tile[]{Main(3, 4, false), Main(3, 5, false), Main(3, 6, false)},
    };

    public static Tile GrassIsland = Main(0, 4);
    public static Tile Dirt = Main(0, 5, false);
    public static Tile[] GrassVariant = {
        Main(2, 5), Main(2, 5), Main(2, 5), Main(2, 5), Main(0, 7), Main(0, 8), Main(1, 7), Main(1, 8)
    };


    public static Tile[][] Road = {
        new Tile[]{Main(4, 0), Main(4, 1), Main(4, 2)},
        new Tile[]{Main(5, 0), Main(5, 1), Main(5, 2)},
        new Tile[]{Main(6, 0), Main(6, 1), Main(6, 2)},
    };

    public static Tile[] Flower = {
        Main(6, 9), Main(6, 10), Main(6, 12)
    };

    public static Tile FenceV = Main(5, 3, false);
    public static Tile FenceH = Main(5, 4, false);
    public static Tile FenceL = Main(5, 5, false);
    public static Tile FenceR = Main(6, 3, false);
    public static Tile FenceRV = Main(6, 4, false);
    public static Tile FenceLV = Main(6, 5, false);

    public static Tile None = Main(7, 0);

    public static Tile Stash = new Tile("stash-grid-cell", 0, 0);
    public static Tile Shop = new Tile("shop-grid-cell", 0, 0);

    public static Tile Heart = new Tile("heart-grid-cell", 0, 0);
    public static Tile Coin = new Tile("coin-grid-cell", 0, 0);
    public static Tile Attack = new Tile("attack-grid-cell", 4, 7);

    public static (Tile, string)[] Monsters = {
        (new Tile("monster-tile-1", 0, 0), "Bat"),
        (new Tile("monster-tile-2", 0, 0), "Cobra"),
        (new Tile("monster-tile-3", 0, 0), "Imp"),
        (new Tile("monster-tile-4", 0, 0), "Spider"),
    };

}
