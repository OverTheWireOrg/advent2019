using System;
using System.Threading.Tasks;
using Npgsql;

namespace Framework {
    class Program {
        static async Task Main(string[] args) {
            string binaryDir = args[0];
            string resultsDir = args[1];
            string sandboxPath = args[2];
            int numThreads = int.Parse(args[3]);
            var simulator = new RealGameSimulator(resultsDir, binaryDir, sandboxPath);
            Console.WriteLine($"Tournament engine running, using binaries from {binaryDir}, persisting results to {resultsDir}");

            var tournament = RunTournament(simulator, numThreads);
            var sanity = RunSanityCheck(simulator);
            await Task.WhenAll(tournament, sanity);
        }

        private static async Task RunTournament(RealGameSimulator simulator, int numThreads) {
            while (true) {
                try {
                    await using var conn = await Connect();
                    Console.WriteLine("Tournament: Connected to database!");
                    // var simulator = new FakeGameSimulator();
                    // await simulator.PopulateDb(conn);
                    // var runner = new TournamentRunner(simulator, conn);

                    var runner = new TournamentRunner(simulator, conn, numThreads);
                    while (true) {
                        if (await runner.StartRound()) {
                            Console.WriteLine("Starting round...");
                            await runner.RunCurrentRoundLoop();
                        } else {
                            Console.WriteLine("Waiting for current round to start...");
                            await Task.Delay(10000);
                        }
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.Message + "\n" + e.StackTrace + "; retrying after 10 seconds...");
                    await Task.Delay(10000);
                }
            }
        }

        private static async Task RunSanityCheck(RealGameSimulator simulator) {
            while (true) {
                try {
                    // Note: must use a separate connection to avoid race conditions!
                    var conn = await Connect();
                    Console.WriteLine("Practice: Connected to database!");
                    var runner = new SanityCheckRunner(conn, simulator);
                    await runner.Run();
                } catch (Exception e) {
                    Console.WriteLine(e.Message + "\n" + e.StackTrace + "; retrying after 10 seconds...");
                    await Task.Delay(10000);
                }
            }
        }

        private static async Task<NpgsqlConnection> Connect() {
            var connString = "Host=db;Username=postgres;Password=Fs2Y2P2udVHZb6Xk;Database=postgres";

            var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            return conn;
        }
    }
}