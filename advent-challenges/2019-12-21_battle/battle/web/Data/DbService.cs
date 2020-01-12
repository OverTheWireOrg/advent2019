using System.Threading.Tasks;
using Npgsql;

namespace Web
{
    class DbService
    {
        public static async Task<NpgsqlConnection> NewConnection()
        {
            var connString = "Host=db;Username=postgres;Password=Fs2Y2P2udVHZb6Xk;Database=postgres";
            var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
