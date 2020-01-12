using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using common;
using System.Threading;

namespace server
{
    public class GameService
    {
        private Dictionary<string, GameData> _games = new Dictionary<string, GameData>();
        private static int CLEANUP_THRESHOLD = 60 * 60 * 1000;
        private TcpListener _tcp;
        private Random _random = new Random();

        public GameService(int port)
        {
            _tcp = new TcpListener(IPAddress.Any, port);
            BeginCleanup();
        }

        private async void BeginCleanup()
        {
            while (true)
            {
                await Task.Delay(60 * 1000);
                lock (_games)
                {
                    var keys = _games.Where((item) => item.Value.mutex.CurrentCount == 1 && item.Value.lastLogoff.AddMilliseconds(CLEANUP_THRESHOLD) < DateTime.Now)
                            .Select((item) => item.Key).ToList();
                    Console.WriteLine("Cleaning up " + keys.Count + " keys; " + (_games.Count - keys.Count) + " remaining");
                    foreach (var key in keys)
                    {
                        _games.Remove(key);
                    }
                }
            }
        }

        public async Task BeginHandling()
        {
            _tcp.Start();
            while (true)
            {
                var conn = await _tcp.AcceptTcpClientAsync();
                HandleConnection(conn);
            }
        }

        private void CreateGame(String user, String pass)
        {
            _games[user] = new GameData()
            {
                password = pass,
                player = new Hpmv.PlayerInfo
                {
                    Level = 1,
                    Experience = 0,
                    Hp = 220,
                    Money = 0
                },
                inventory = CreateInitialInventory()
            };
        }

        private List<Hpmv.Inventory> CreateInitialInventory()
        {
            var result = new List<Hpmv.Inventory>();
            var player = new Hpmv.Inventory();
            player.Id = 0;
            player.Items.Add(ItemRegistry.Items[6]);
            player.Items.Add(ItemRegistry.Items[0]);
            player.TotalCapacity = 6;
            result.Add(player);

            var stash = new Hpmv.Inventory();
            stash.Id = 1;
            stash.TotalCapacity = 32;
            result.Add(stash);

            var shop = new Hpmv.Inventory();
            shop.Id = 2;
            shop.TotalCapacity = 1000;
            for (int i = 0; i < 6; i++)
            {
                shop.Items.Add(ItemRegistry.Items[0]);
                shop.Items.Add(ItemRegistry.Items[1]);
                shop.Items.Add(ItemRegistry.Items[2]);
                shop.Items.Add(ItemRegistry.Items[3]);
                shop.Items.Add(ItemRegistry.Items[4]);
                shop.Items.Add(ItemRegistry.Items[5]);
            }
            shop.Items.Add(ItemRegistry.Items[8]);
            result.Add(shop);
            return result;
        }

        private Hpmv.PlayerInfo CalculatePlayerInfo(Hpmv.PlayerInfo player)
        {
            var result = new Hpmv.PlayerInfo();
            result.MergeFrom(player);
            result.MaxHp = 200 + player.Level * 20;
            result.Attack = player.Level * 1;
            result.ExperienceTillNextLevel = player.Level * 100;
            return result;
        }

        private async void HandleConnection(TcpClient socket)
        {
            TcpMonitor.OnTcpClientConnect(socket);
            byte[] key = new byte[16];
            _random.NextBytes(key);
            var stream = socket.GetStream();
            await stream.WriteAsync(key);
            common.EncryptedStream clientKey = new common.EncryptedStream(key);
            common.EncryptedStream serverKey = new common.EncryptedStream(key);

            Func<Hpmv.SessionResponse, Task> SendResponse = async (Hpmv.SessionResponse response) =>
            {
                // Console.WriteLine(response.ToString());
                byte[] outBuf = response.ToByteArray();
                if (outBuf.Length > 65535)
                {
                    throw new Exception("Server message length too long");
                }
                byte[] lenBuf = BitConverter.GetBytes((ushort)outBuf.Length);
                serverKey.Transform(lenBuf, 2);
                serverKey.Transform(outBuf, outBuf.Length);
                await stream.WriteAsync(lenBuf);
                await stream.WriteAsync(outBuf);
            };

            byte[] buffer = new byte[65536];
            GameData game = null;
            try
            {
                while (true)
                {
                    await stream.ReadNBytes(buffer, 0, 2);
                    clientKey.Transform(buffer, 2);
                    ushort length = BitConverter.ToUInt16(buffer, 0);
                    await stream.ReadNBytes(buffer, 0, length);
                    clientKey.Transform(buffer, length);
                    Hpmv.SessionRequest request = Hpmv.SessionRequest.Parser.ParseFrom(buffer, 0, length);
                    // Console.WriteLine("Request: " + request.ToString());
                    switch (request.RequestCase)
                    {
                        case Hpmv.SessionRequest.RequestOneofCase.Login:
                            {
                                var user = request.Login.Username;
                                var pass = request.Login.Password;
                                GameData gameTemp = null;
                                lock (_games)
                                {
                                    if (!_games.ContainsKey(user))
                                    {
                                        CreateGame(user, pass);
                                    }
                                    gameTemp = _games[user];
                                }

                                if (gameTemp.password != pass)
                                {
                                    await SendResponse(new Hpmv.SessionResponse()
                                    {
                                        Login = new Hpmv.LoginResponse() { Successful = false },
                                        Message = "Invalid password."
                                    });
                                    break;
                                }
                                if (!await gameTemp.mutex.WaitAsync(0))
                                {
                                    await SendResponse(new Hpmv.SessionResponse()
                                    {
                                        Login = new Hpmv.LoginResponse() { Successful = false },
                                        Message = "This account is already logged in."
                                    });
                                    break;
                                }
                                if (DateTime.Now < gameTemp.lastLogoff.AddSeconds(20))
                                {
                                    gameTemp.mutex.Release();
                                    await SendResponse(new Hpmv.SessionResponse()
                                    {
                                        Login = new Hpmv.LoginResponse() { Successful = false },
                                        Message = "You may only login to the same account again after 20 seconds of cooldown."
                                    });
                                    break;
                                }
                                game = gameTemp;
                                var response = new Hpmv.SessionResponse()
                                {
                                    Login = new Hpmv.LoginResponse()
                                    {
                                        Successful = true,
                                        Player = CalculatePlayerInfo(game.player)
                                    }
                                };
                                response.Login.Inventory.AddRange(game.inventory);
                                await SendResponse(response);
                                break;
                            }
                        case Hpmv.SessionRequest.RequestOneofCase.Battle:
                            {
                                if (game == null) throw new Exception("Not logged in");
                                var level = request.Battle.MonsterLevel;
                                if (level < game.player.Level - 1 || level > game.player.Level + 2)
                                {
                                    throw new Exception("Invalid level " + level);
                                }
                                var monsterHp = 50 + level * 20;
                                var monsterAttack = 5 + level * 1;
                                var playerAttack = game.player.Level + game.inventory[0].Items.Select((i) => i.Attack).Sum();
                                var resp = new Hpmv.BattleResponse();
                                for (int i = 0; i < 32; i++)
                                {
                                    if (game.player.Hp <= monsterAttack)
                                    {
                                        break;
                                    }
                                    monsterHp -= playerAttack;
                                    if (monsterHp <= 0) break;
                                    game.player.Hp -= monsterAttack;
                                    resp.Rounds.Add(new Hpmv.BattleResponse.Types.OneRound { SelfHp = game.player.Hp, MonsterHp = monsterHp });
                                }
                                resp.Victory = monsterHp <= 0;
                                if (resp.Victory)
                                {
                                    resp.GainedExperience = (level + 1 - game.player.Level) * (level + 20);
                                    resp.GainedCoins = 10 + (level + 1 - game.player.Level) * level / 10;
                                    game.player.Experience += resp.GainedExperience;
                                    var calculated = CalculatePlayerInfo(game.player);
                                    if (game.player.Experience >= calculated.ExperienceTillNextLevel)
                                    {
                                        game.player.Level++;
                                        game.player.Experience -= calculated.ExperienceTillNextLevel;
                                    }
                                    game.player.Money += resp.GainedCoins;
                                }
                                resp.NewInfo = CalculatePlayerInfo(game.player);
                                await SendResponse(new Hpmv.SessionResponse() { Battle = resp });
                                break;
                            }
                        case Hpmv.SessionRequest.RequestOneofCase.ItemUse:
                            {
                                if (game == null) throw new Exception("Not logged in");
                                int index = request.ItemUse.ItemIndex;
                                if (index < 0 || index >= game.inventory[0].Items.Count)
                                {
                                    throw new Exception("No such item");
                                }
                                var item = game.inventory[0].Items[index];
                                var response = new Hpmv.ItemUseResponse();
                                if (item.HpRestore > 0)
                                {
                                    game.player.Hp = Math.Min(CalculatePlayerInfo(game.player).MaxHp, game.player.Hp + item.HpRestore);
                                }
                                response.NewHp = game.player.Hp;
                                if (item.Id == 8)
                                {
                                    response.Message = "Congratulations! Here is the server-side flag: _is_f0R_Th3_13373sT}   The client-side flag is clearly written on a piece of paper that was lost by the shopkeeper some time ago. If only you could find it...";
                                }
                                await SendResponse(new Hpmv.SessionResponse() { ItemUse = response });
                                break;
                            }
                        case Hpmv.SessionRequest.RequestOneofCase.InventoryMove:
                            {
                                if (game == null) throw new Exception("Not logged in");
                                int fromInventory = request.InventoryMove.FromInventory;
                                int toInventory = request.InventoryMove.ToInventory;
                                Hpmv.Inventory from = game.inventory[fromInventory].Clone();
                                Hpmv.Inventory to = game.inventory[toInventory].Clone();
                                var item = from.Items[request.InventoryMove.ItemIndex];
                                from.Items.RemoveAt(request.InventoryMove.ItemIndex);
                                to.Items.Add(item);
                                long costDelta = 0;
                                foreach (var cost in item.MoveCosts)
                                {
                                    if (cost.Inventory == fromInventory && cost.Out)
                                    {
                                        costDelta += cost.Cost;
                                    }
                                    if (cost.Inventory == toInventory && !cost.Out)
                                    {
                                        costDelta += cost.Cost;
                                    }
                                }
                                if (costDelta + game.player.Money >= 0 && to.Items.Count <= to.TotalCapacity)
                                {
                                    game.player.Money += costDelta;
                                    game.inventory[fromInventory] = from;
                                    game.inventory[toInventory] = to;
                                }
                                var response = new Hpmv.InventoryMoveResponse();
                                response.FromInventory = game.inventory[fromInventory];
                                response.ToInventory = game.inventory[toInventory];
                                response.NewPlayerMoney = game.player.Money;
                                await SendResponse(new Hpmv.SessionResponse() { InventoryMove = response });
                                break;
                            }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Session error: " + e.Message);
            }
            finally
            {
                if (game != null)
                {
                    game.lastLogoff = DateTime.Now;
                    game.mutex.Release();
                }
            }
            socket.Close();
            TcpMonitor.OnTcpClientDisconnect(socket);
        }
    }

    class GameData
    {
        public String password;
        public Hpmv.PlayerInfo player;
        public List<Hpmv.Inventory> inventory;
        public readonly SemaphoreSlim mutex = new SemaphoreSlim(1, 1);
        public DateTime lastLogoff = DateTime.MinValue;
    }
}
