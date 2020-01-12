using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace client.Pages
{
    public partial class Town
    {
        [Parameter]
        public bool EnableAudio { get; set; }

        private static Tile O = Tiles.None;
        private static Tile I = Tiles.FenceV;
        private static Tile H = Tiles.FenceH;
        private static Tile L = Tiles.FenceLV;
        private static Tile R = Tiles.FenceRV;
        private static Tile C = Tiles.FenceL;
        private static Tile M = Tiles.FenceR;
        private static Tile S = Tiles.Stash;
        private static Tile A = Tiles.Shop;

        private (int, int) Position { get; set; } = (8, 5);
        private int Orientation { get; set; } = 0;

        private static Tile[][] GroundTiles = {
            new Tile[]{O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O},
            new Tile[]{O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O},
            new Tile[]{O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O},
            new Tile[]{O, O, O, C, H, H, H, M, O, O, O, C, H, H, H, M, O, O, O},
            new Tile[]{O, O, O, I, O, O, O, I, O, O, O, I, O, O, O, I, O, O, O},
            new Tile[]{O, O, O, I, O, S, O, I, O, O, O, I, O, O, O, I, O, O, O},
            new Tile[]{O, O, O, I, O, O, O, L, H, H, H, R, O, O, O, I, O, O, O},
            new Tile[]{O, O, O, I, O, O, O, O, O, O, O, O, O, O, O, I, O, O, O},
            new Tile[]{O, O, O, I, O, O, O, O, O, O, O, O, O, O, O, I, O, O, O},
            new Tile[]{O, O, O, I, O, O, O, O, O, O, O, O, O, O, O, I, O, O, O},
            new Tile[]{O, O, O, I, O, O, O, C, R, O, L, M, O, O, O, I, O, O, O},
            new Tile[]{O, O, O, I, O, O, O, I, O, O, O, I, O, A, O, I, O, O, O},
            new Tile[]{O, O, O, I, O, O, O, I, O, O, O, I, O, O, O, I, O, O, O},
            new Tile[]{O, O, O, L, H, H, H, R, O, O, O, L, H, H, H, R, O, O, O},
            new Tile[]{O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O},
            new Tile[]{O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O},
            new Tile[]{O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O, O},
        };

        private static int[][] TownMask = {
            new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0},
            new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        };

        public static List<(int x, int y)> WildCoordinates;

        static Town()
        {
            WildCoordinates = new List<(int x, int y)>();
            for (int i = 0; i < TownMask.Length; i++)
            {
                for (int j = 0; j < TownMask[i].Length; j++)
                {
                    if (TownMask[i][j] == 0)
                    {
                        WildCoordinates.Add((i, j));
                    }
                }
            }
        }

        [Parameter]
        public bool AllowInput { get; set; } = true;

        [Parameter]
        public List<(int x, int y, int level, Tile tile, string name)> Monsters { get; set; }
             = new List<(int x, int y, int level, Tile tile, string name)>();

        private async Task OnPositionChanged((int x, int y, int orientation) input)
        {
            Position = (input.x, input.y);
            Orientation = input.orientation;
            if (GroundTiles[input.x][input.y] == S)
            {
                await OnEnterStash.InvokeAsync(true);
            }
            else if (GroundTiles[input.x][input.y] == A)
            {
                await OnEnterShop.InvokeAsync(true);
            }
            else
            {
                for (var i = 0; i < Monsters.Count; i++)
                {
                    var monster = Monsters[i];
                    if (monster.x == input.x && monster.y == input.y)
                    {
                        await OnMonsterEncounter.InvokeAsync(i);
                        break;
                    }
                }
            }
        }

        [Parameter]
        public EventCallback<bool> OnEnterStash { get; set; }

        [Parameter]
        public EventCallback<bool> OnEnterShop { get; set; }

        [Parameter]
        public EventCallback<int> OnMonsterEncounter { get; set; }

        private bool IsInTown
        {
            get
            {
                return TownMask[Position.Item1][Position.Item2] == 1;
            }
        }
    }
}
