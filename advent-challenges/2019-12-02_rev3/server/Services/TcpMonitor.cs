using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace server
{
    public class TcpMonitor
    {
        private static int ActiveConnections = 0;
        private static object Sync = new object();

        public static void OnTcpClientConnect(TcpClient tcp)
        {
            lock (Sync)
            {
                ActiveConnections++;
            }
        }

        public static void OnTcpClientDisconnect(TcpClient tcp)
        {
            lock (Sync)
            {
                ActiveConnections--;
            }
        }

        public static async void StartWritingToLog()
        {
            while (true)
            {
                await Task.Delay(5000);
                File.AppendAllLines("/tmp/server.log", new string[] {
                    "Active connections: " + ActiveConnections
                });
            }
        }
    }
}