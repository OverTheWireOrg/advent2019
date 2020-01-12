using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

namespace Framework
{
    public interface IGameSimulator
    {
        Task<GameResults> Launch(GameLaunchParams p);
        int NumPlayers { get; }
    }

    public class FakeGameSimulator : IGameSimulator
    {
        public int NumPlayers => 2;

        private Dictionary<int, int> expected_elo = new Dictionary<int, int>();

        public FakeGameSimulator()
        {
            for (int i = 1; i <= 100; i++)
            {
                expected_elo[i] = 500 + 2000 * i / 100;
            }
        }

        private Random random = new Random();

        public async Task<GameResults> Launch(GameLaunchParams p)
        {
            double[] scores = new double[2];

            double rating_i = expected_elo[p.Teams[0].Id];
            double rating_j = expected_elo[p.Teams[1].Id];
            double win_rate = 1.0 / (1.0 + Math.Pow(10, (rating_j - rating_i) / 400));
            if (random.NextDouble() < win_rate)
            {
                scores[0] = 1;
            }
            else
            {
                scores[1] = 1;
            }

            GameResults results = new GameResults();
            results.ResultPath = "test_path";
            results.Points.AddRange(scores);
            return results;
        }

        public async Task PopulateDb(NpgsqlConnection conn)
        {
            await using var cmd = new NpgsqlCommand("INSERT INTO Team (name, binary_path) VALUES (@name, 'fake')", conn);
            cmd.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Text);
            await cmd.PrepareAsync();
            for (int i = 1; i <= 100; i++)
            {
                cmd.Parameters[0].Value = "team" + i;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
