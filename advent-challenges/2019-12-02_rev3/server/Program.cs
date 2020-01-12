using System.Threading.Tasks;

namespace server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var service = new GameService(50001);
            TcpMonitor.StartWritingToLog();
            await service.BeginHandling();
        }

    }
}
