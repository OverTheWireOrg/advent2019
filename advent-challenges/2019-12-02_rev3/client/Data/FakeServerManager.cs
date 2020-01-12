using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

class FakeServerManager
{
    private Dictionary<string, WeakReference<TcpClient>> connections
        = new Dictionary<string, WeakReference<TcpClient>>();
    private TcpListener listener;
    private Random random = new Random();

    public static readonly FakeServerManager Singleton = new FakeServerManager();

    public async void Start()
    {
        listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 50002);
        listener.ExclusiveAddressUse = true;
        listener.Start();
        PeriodicallyCleanupClients();
        Console.WriteLine("TCP server listening on 50002");
        while (true)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                HandleTcpClient(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async void PeriodicallyCleanupClients()
    {
        while (true)
        {
            await Task.Delay(1);
            lock (connections)
            {
                foreach (var key in connections.Where((kv) =>
                {
                    TcpClient client;
                    return !kv.Value.TryGetTarget(out client) || !client.Connected;
                }).Select((kv) => kv.Key))
                {
                    connections.Remove(key);
                }
            }
        }
    }

    private async void HandleTcpClient(TcpClient client)
    {
        byte[] id = new byte[24];
        random.NextBytes(id);
        string idStr = Convert.ToBase64String(id);
        try
        {
            await client.GetStream().WriteAsync(
                Encoding.UTF8.GetBytes(
                    "Server ID: " + idStr +
                    "\nEnter this ID on the game client to connect to this custom server.\n"));
            lock (connections)
            {
                connections[idStr] = new WeakReference<TcpClient>(client);
            }
        }
        catch (Exception e)
        {
            client.Close();
        }
    }

    public TcpClient LookupClientById(string id)
    {
        lock (connections)
        {
            if (connections.ContainsKey(id))
            {
                TcpClient client;
                if (connections[id].TryGetTarget(out client))
                {
                    return client;
                }
            }
        }
        return null;
    }

    public void ClaimClient(string id) {
        connections.Remove(id);
    }
}