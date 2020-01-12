using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using common;
using Microsoft.AspNetCore.Components;
using Google.Protobuf;
using System.Collections.Generic;

namespace client.Pages
{

    [System.Serializable]
    public class SpecificException : System.Exception
    {
        public SpecificException() { }
        public SpecificException(string message) : base(message) { }
        public SpecificException(string message, System.Exception inner) : base(message, inner) { }
        protected SpecificException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public partial class Game
    {
        [Parameter]
        public TcpClient Socket { get; set; }

        [Parameter]
        public bool EnableAudio { get; set; }

        public string username = "";
        public string password = "";

        private bool isLoading = false;
        private string errorMessage = "";
        private bool loginSucceeded = false;

        private Hpmv.PlayerInfo player = new Hpmv.PlayerInfo();
        private Hpmv.Inventory[] inventories = new Hpmv.Inventory[3] {
            new Hpmv.Inventory(),
            new Hpmv.Inventory(),
            new Hpmv.Inventory(),
        };
        private Hpmv.Inventory Backpack { get => inventories[0]; }
        private Hpmv.Inventory Stash { get => inventories[1]; }
        private Hpmv.Inventory Shop { get => inventories[2]; }

        private bool StashDialogVisible = false;
        private bool ShopDialogVisible = false;
        private bool BackpackDialogVisible = false;
        private bool BattleDialogVisible = false;
        private bool MessageDialogVisible = false;
        private (int level, string name) CurrentMonster = (0, "");
        private List<string> BattleDetails = new List<string>();
        private bool BattleDone = false;
        private string MessageDialogMessage = "";
        private bool PreventDismiss = false;

        private bool AllowInput
        {
            get
            {
                return !StashDialogVisible && !ShopDialogVisible && !BackpackDialogVisible
                && !BattleDialogVisible && !MessageDialogVisible && !isLoading;
            }
        }

        private List<(int x, int y, int level, Tile tile, string name)> Monsters = new List<(int x, int y, int level, Tile tile, string name)>();
        private Random random = new Random();

        private Channel<ReqRespPair> channel = Channel.CreateBounded<ReqRespPair>(16);

        protected override void OnInitialized()
        {
            Start();
            RandomizeMonsters();
        }

        public void Dispose()
        {
            channel.Writer.Complete();
        }

        private void RandomizeMonsters()
        {
            Monsters.RemoveAll((m) => !(m.level >= player.Level - 1 && m.level <= player.Level + 2));
            for (int level = player.Level - 1; level <= player.Level + 2; level++)
            {
                if (!Monsters.Any((m) => m.level == level))
                {
                    var coord = Town.WildCoordinates[random.Next(Town.WildCoordinates.Count)];
                    var tile = Tiles.Monsters[random.Next(Tiles.Monsters.Length)];
                    Monsters.Add((coord.x, coord.y, level, tile.Item1, tile.Item2));
                }
            }
        }

        private async void Start()
        {
            byte[] buffer = new byte[65536];
            var stream = Socket.GetStream();
            byte[] key = new byte[16];
            await stream.ReadNBytes(key, 0, 16);
            common.EncryptedStream clientKey = new common.EncryptedStream(key);
            common.EncryptedStream serverKey = new common.EncryptedStream(key);
            try
            {
                while (true)
                {
                    var channelTask = channel.Reader.ReadAsync().AsTask();
                    var timeoutTask = Task.Delay(60 * 1000);
                    if (await Task.WhenAny(channelTask, timeoutTask) == timeoutTask) {
                        throw new SpecificException("Disconnected due to inactivity.");
                    }
                    var pair = await channelTask;
                    isLoading = true;
                    StateHasChanged();
                    byte[] req = pair.request.ToByteArray();
                    if (req.Length > 65535)
                    {
                        throw new SpecificException("Could not communicate with the server: message too long.");
                    }
                    byte[] length = BitConverter.GetBytes((ushort)req.Length);
                    // Console.WriteLine("Client:\n" + common.Utils.HexDump(length, 2));
                    // Console.WriteLine("Client:\n" + common.Utils.HexDump(req, req.Length));
                    clientKey.Transform(length, 2);
                    clientKey.Transform(req, req.Length);
                    await stream.WriteAsync(length);
                    await stream.WriteAsync(req);
                    await stream.ReadNBytes(buffer, 0, 2);
                    serverKey.Transform(buffer, 2);
                    // Console.WriteLine("Server:\n" + common.Utils.HexDump(buffer, 2));
                    ushort respLength = BitConverter.ToUInt16(buffer, 0);
                    await stream.ReadNBytes(buffer, 0, respLength);
                    serverKey.Transform(buffer, respLength);
                    // Console.WriteLine("Server:\n" + common.Utils.HexDump(buffer, respLength));
                    var response = new Hpmv.SessionResponse();
                    response.MergeFrom(buffer, 0, respLength);
                    pair.response.SetResult(response);
                    isLoading = false;
                    StateHasChanged();
                }
            }
            catch (SpecificException e) {
                Socket.Close();
                Console.WriteLine(e.Message);
                ShowMessage(e.Message + " Please reload the page.", true);
            }
            catch (Exception e)
            {
                Socket.Close();
                Console.WriteLine(e.Message);
                ShowMessage("Disconnected. Please reload the page.", true);
            }
        }

        private void ShowMessage(string message, bool fatal)
        {
            PreventDismiss = fatal;
            MessageDialogVisible = true;
            MessageDialogMessage = message;
            StateHasChanged();
        }

        private async Task MoveItem(int inventory, int itemIndex, int targetInventory)
        {
            var response = await Request(new Hpmv.SessionRequest
            {
                InventoryMove = new Hpmv.InventoryMoveRequest
                {
                    FromInventory = inventory,
                    ItemIndex = itemIndex,
                    ToInventory = targetInventory
                }
            });
            player.Money = response.InventoryMove.NewPlayerMoney;
            inventories[inventory] = response.InventoryMove.FromInventory;
            inventories[targetInventory] = response.InventoryMove.ToInventory;
        }

        private async Task UseItem(int itemIndex)
        {
            var response = await Request(new Hpmv.SessionRequest
            {
                ItemUse = new Hpmv.ItemUseRequest
                {
                    ItemIndex = itemIndex
                }
            });
            player.Hp = response.ItemUse.NewHp;
            if (response.ItemUse.Message.Count() != 0)
            {
                ShowMessage(response.ItemUse.Message, false);
            }
        }

        private async Task<bool> Battle(int level, string name)
        {
            CurrentMonster = (level, name);
            BattleDetails.Clear();
            BattleDialogVisible = true;
            BattleDone = false;
            StateHasChanged();
            var response = await Request(new Hpmv.SessionRequest
            {
                Battle = new Hpmv.BattleRequest
                {
                    MonsterLevel = level
                }
            });
            foreach (var round in response.Battle.Rounds)
            {
                await Task.Delay(500);
                BattleDetails.Add("You attack the monster, reducing its HP to " + round.MonsterHp);
                StateHasChanged();
                await Task.Delay(500);
                BattleDetails.Add("The monster attacks you, reducing your HP to " + round.SelfHp);
                player.Hp = round.SelfHp;
                StateHasChanged();
            }
            await Task.Delay(500);
            if (response.Battle.Victory)
            {
                BattleDetails.Add("You have defeated the monster!");
                BattleDetails.Add("You gained " + response.Battle.GainedExperience + " EXP and "
                    + response.Battle.GainedCoins + " coins!");
            }
            else
            {
                BattleDetails.Add("Your HP is too low; you were forced to retreat.");
            }
            BattleDone = true;
            StateHasChanged();

            player = response.Battle.NewInfo;
            return response.Battle.Victory;
        }

        private async Task<Hpmv.SessionResponse> Request(Hpmv.SessionRequest request)
        {
            var pair = new ReqRespPair();
            pair.request = request;
            pair.response = new TaskCompletionSource<Hpmv.SessionResponse>();
            await channel.Writer.WriteAsync(pair);
            var result = await pair.response.Task;
            // Console.WriteLine(result.ToString());  // DEBUG
            if (result.Message.Length > 0)
            {
                ShowMessage(result.Message, false);
            }
            return result;
        }

        private async Task DoLogin()
        {
            if (username.Length > 16)
            {
                ShowMessage("Username must be no more than 16 characters", false);
                return;
            }
            if (username.Length < 4)
            {
                ShowMessage("Username must be at least 4 characters", false);
                return;
            }
            if (password.Length > 256)
            {
                ShowMessage("Password must be no more than 256 characters", false);
                return;
            }
            if (password.Length == 0)
            {
                ShowMessage("Password cannot be empty", false);
                return;
            }

            var response = await Request(new Hpmv.SessionRequest
            {
                Login = new Hpmv.LoginRequest
                {
                    Username = username,
                    Password = password
                }
            });
            if (response.Login.Successful)
            {
                loginSucceeded = true;
                player = response.Login.Player;
                response.Login.Inventory.CopyTo(inventories, 0);
                RandomizeMonsters();
            }
        }

        private async Task HandleMoveItem((int fromInventory, int itemIndex, int toInventory) param)
            => await MoveItem(param.fromInventory, param.itemIndex, param.toInventory);

        private async Task OnMonsterEncounter(int index)
        {
            var result = await Battle(Monsters[index].level, Monsters[index].name);
            if (result)
            {
                Monsters.RemoveAt(index);
                RandomizeMonsters();
            }
        }
    }
}