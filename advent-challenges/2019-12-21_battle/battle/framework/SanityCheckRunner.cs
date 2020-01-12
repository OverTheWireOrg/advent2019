using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace Framework {
    class SanityCheckRunner {
        private NpgsqlConnection conn;
        private RealGameSimulator sim;

        public SanityCheckRunner(NpgsqlConnection conn, RealGameSimulator sim) {
            this.conn = conn;
            this.sim = sim;
        }

        public async Task Run() {
            while (true) {
                var tasks = new List < (int id, int team, string binary) > (); {
                    await using var cmd = new NpgsqlCommand("select id, team, binary_path from Binaries where validation_result_path is null ORDER BY upload_time ASC", conn);
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync()) {
                        tasks.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2)));
                    }
                }

                if (tasks.Count == 0) {
                    await Task.Delay(10000);
                    continue;
                }

                foreach (var(id, team, binary) in tasks) {
                    var launch = new GameLaunchParams();
                    launch.Practice = true;
                    for (int i = 0; i < 2; i++) {
                        launch.Teams.Add(
                            new TeamData() {
                                Id = team,
                                    BinaryPath = binary
                            }
                        );
                    }
                    Console.WriteLine($"Practice mode: simulating game for team {team} binary {binary}");
                    var result = await sim.Launch(launch);
                    bool success = result.NumStars.Sum() >= Simulator.GameState.NUM_STARS * 3 / 4;

                    {
                        await using var cmd = new NpgsqlCommand("update Binaries set validation_result_path = @path, validation_passed = @success where id = @id", conn);
                        cmd.Parameters.AddWithValue("path", result.ResultPath);
                        cmd.Parameters.AddWithValue("id", id);
                        cmd.Parameters.AddWithValue("success", success);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}