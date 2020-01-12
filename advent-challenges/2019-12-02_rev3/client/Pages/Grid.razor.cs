using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace client.Pages
{
    public partial class Grid
    {

        [Parameter]
        public Tile[][] GridTiles { get; set; } = { };

        [Parameter]
        public List<(Tile tile, int x, int y)> MonsterTiles { get; set; } = new List<(Tile tile, int x, int y)>();

        [Parameter]
        public (int, int) Position { get; set; } = (0, 0);

        [Parameter]
        public int Orientation { get; set; } = 0;

        [Parameter]
        public EventCallback<(int, int, int)> PositionChanged { get; set; }

        [Parameter]
        public bool AllowInput {get; set;} = true;

        private bool Focused {get; set; }

        private Tile Player
        {
            get
            {
                return Tiles.Player[Orientation];
            }
        }

        private int PlayerTop
        {
            get
            {
                return Position.Item1 * 16;
            }
        }
        private int PlayerLeft
        {
            get
            {
                return Position.Item2 * 16;
            }
        }

        private void OnKeyPress(KeyboardEventArgs args)
        {
            if (!AllowInput) return;
            int x = Position.Item1;
            int y = Position.Item2;
            if (args.Key == "w")
            {
                Orientation = 3;
                if (x > 0 && GridTiles[x - 1][y].CanWalk)
                {
                    Position = (x - 1, y);
                }
            }
            else if (args.Key == "s")
            {
                Orientation = 0;
                if (x < GridTiles.Length - 1 && GridTiles[x + 1][y].CanWalk)
                {
                    Position = (x + 1, y);
                }
            }
            else if (args.Key == "a")
            {
                Orientation = 1;
                if (y > 0 && GridTiles[x][y - 1].CanWalk)
                {
                    Position = (x, y - 1);
                }
            }
            else if (args.Key == "d")
            {
                Orientation = 2;
                if (y < GridTiles[x].Length - 1 && GridTiles[x][y + 1].CanWalk)
                {
                    Position = (x, y + 1);
                }
            }
            PositionChanged.InvokeAsync((Position.Item1, Position.Item2, Orientation));
        }
    }
}