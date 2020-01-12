using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Framework {

    public class TournamentRunner {
        private IGameSimulator simulator;
        private NpgsqlConnection conn;
        private int numThreads;

        public TournamentRunner(IGameSimulator simulator, NpgsqlConnection conn, int numThreads) {
            this.simulator = simulator;
            this.conn = conn;
            this.numThreads = numThreads;
        }

        private async Task < (int round, int numGamesPerTeam, int maxTeams, int eloKFactor) ? > GetCurrentActiveRound() {
            await using(var cmd = new NpgsqlCommand("SELECT round, desired_games_per_team, teams_limit, elo_k_factor FROM Tournament where actual_start_time is not null and actual_end_time is null", conn)) {
                await using(var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        return (reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3));
                    }
                }
            }
            return null;
        }

        private async Task < (int round, DateTime scheduledStartTime, int maxTeams) ? > GetNextUnstartedRound() {
            await using(var cmd = new NpgsqlCommand("SELECT round, scheduled_start_time, teams_limit FROM Tournament where actual_start_time is null ORDER BY round ASC LIMIT 1", conn)) {
                await using(var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        return (reader.GetInt32(0), reader.GetInt64(1).UnixMillisToDateTime(), reader.GetInt32(2));
                    }
                }
            }
            return null;
        }

        public async Task FinishRound() {
            var activeRound = await GetCurrentActiveRound();
            if (!activeRound.HasValue) {
                throw new Exception("Attempting to finish non-active round.");
            }

            var(round, _, __, ___) = activeRound.Value;

            var prevRound = await RecoverRoundData();

            {
                var tx = await conn.BeginTransactionAsync();

                {
                    await using var cmd = new NpgsqlCommand("UPDATE Round SET elo_gain = @elogain, elo = @elo, games = @games WHERE team = @team AND round = @round", conn);
                    cmd.Parameters.Add("elogain", NpgsqlTypes.NpgsqlDbType.Double);
                    cmd.Parameters.Add("elo", NpgsqlTypes.NpgsqlDbType.Double);
                    cmd.Parameters.Add("games", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer);
                    cmd.Parameters.Add("team", NpgsqlTypes.NpgsqlDbType.Integer);
                    cmd.Parameters.Add("round", NpgsqlTypes.NpgsqlDbType.Integer);
                    foreach (var team in prevRound.Teams.Values) {
                        cmd.Parameters[0].Value = team.PendingElo - team.StartingElo;
                        cmd.Parameters[1].Value = team.PendingElo;
                        cmd.Parameters[2].Value = team.Games.ToArray();
                        cmd.Parameters[3].Value = team.Id;
                        cmd.Parameters[4].Value = round;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                {
                    await using var cmd = new NpgsqlCommand("UPDATE Tournament SET actual_end_time = @time WHERE round = @round", conn);
                    cmd.Parameters.AddWithValue("time", DateTime.Now.ToUnixMillis());
                    cmd.Parameters.AddWithValue("round", round);
                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
        }

        public async Task<bool> StartRound() {
            {
                var activeRound = await GetCurrentActiveRound();
                if (activeRound.HasValue) {
                    return true;
                }
            }
            var unstartedRound = await GetNextUnstartedRound();
            if (!unstartedRound.HasValue) {
                // No more rounds.
                return false;
            }

            var(round, startTime, maxTeams) = unstartedRound.Value;
            if (startTime > DateTime.Now) {
                // Not yet time to start round.
                return false;
            }

            var eligibleTeams = new Dictionary<int, string>();
            await using(var cmd = new NpgsqlCommand("SELECT team, binary_path FROM Binaries WHERE validation_passed = true ORDER BY upload_time ASC", conn)) {
                await using(var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        eligibleTeams[reader.GetInt32(0)] = reader.GetString(1);
                    }
                }
            }

            var previousTeamRatings = new Dictionary<int, double>();
            await using(var cmd = new NpgsqlCommand("SELECT team, elo from Round where round = @round", conn)) {
                cmd.Parameters.AddWithValue("round", round - 1);
                await using(var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        previousTeamRatings[reader.GetInt32(0)] = reader.GetDouble(1);
                    }
                }
            }
            var rankedTeams = new List < (int rating, int team) > ();
            foreach (int team in eligibleTeams.Keys) {
                if (previousTeamRatings.ContainsKey(team)) {
                    rankedTeams.Add(((int) previousTeamRatings[team], team));
                } else {
                    rankedTeams.Add((-10000, team));
                }
            }

            var lastRating = 0;
            var qualifiedTeams = new List<int>();
            foreach (var(rating, team) in rankedTeams.OrderByDescending((x) => x.rating)) {
                if (qualifiedTeams.Count >= maxTeams) {
                    if (lastRating != rating) {
                        break;
                    }
                }
                qualifiedTeams.Add(team);
                lastRating = rating;
            }

            {
                var tx = await conn.BeginTransactionAsync();

                {
                    await using var cmd = new NpgsqlCommand("INSERT INTO Round (team, round, binary_path) VALUES (@team, @round, @binarypath)", conn);
                    cmd.Parameters.Add("team", NpgsqlTypes.NpgsqlDbType.Integer);
                    cmd.Parameters.Add("round", NpgsqlTypes.NpgsqlDbType.Integer);
                    cmd.Parameters.Add("binarypath", NpgsqlTypes.NpgsqlDbType.Text);
                    await cmd.PrepareAsync();
                    foreach (int team in qualifiedTeams) {
                        cmd.Parameters[0].Value = team;
                        cmd.Parameters[1].Value = round;
                        cmd.Parameters[2].Value = eligibleTeams[team];
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                {
                    await using var cmd = new NpgsqlCommand("UPDATE Tournament SET actual_start_time = @time WHERE round = @round", conn);
                    cmd.Parameters.AddWithValue("time", DateTime.Now.ToUnixMillis());
                    cmd.Parameters.AddWithValue("round", round);
                    await cmd.ExecuteNonQueryAsync();
                }
                await tx.CommitAsync();
            }
            return true;
        }

        private async Task<OneRoundTempData> RecoverRoundData() {
            var result = new OneRoundTempData();
            var activeRound = await GetCurrentActiveRound();
            if (!activeRound.HasValue) {
                throw new Exception("Attempting to simulate non-active round.");
            }
            var(round, numGamesPerTeam, maxTeams, eloKFactor) = activeRound.Value;
            result.Round = round;
            result.DesiredNumGamesPerTeam = numGamesPerTeam;
            result.MaxTeams = maxTeams;
            result.EloKFactor = eloKFactor;
            // Read the snapshot binary path and teams of the current round.
            {
                await using var cmd = new NpgsqlCommand("select team, binary_path from Round where round = @round", conn);
                cmd.Parameters.AddWithValue("round", result.Round);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    int team = reader.GetInt32(0);
                    string binaryPath = reader.GetString(1);
                    result.Teams[team] = new OneTeamTempData {
                        Id = team,
                        BinaryPath = binaryPath,
                        StartingElo = 1500,
                        PendingElo = 1500,
                    };
                }
            }

            // Read elo from previous round.
            {
                await using var cmd = new NpgsqlCommand("select team, elo from Round where round = @round", conn);
                cmd.Parameters.AddWithValue("round", result.Round - 1);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    int team = reader.GetInt32(0);
                    double elo = reader.GetDouble(1);
                    if (result.Teams.ContainsKey(team)) {
                        result.Teams[team].StartingElo = elo;
                        result.Teams[team].PendingElo = elo;
                    }
                }
            }

            // Recover previous games
            {
                await using var cmd = new NpgsqlCommand("select teams, id, elo_deltas from Game where round = @round and end_time != 0 ORDER BY id ASC", conn);
                cmd.Parameters.AddWithValue("round", result.Round);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    var teams = (int[]) reader.GetValue(0);
                    var gameId = reader.GetInt32(1);
                    var eloDeltas = (double[]) reader.GetValue(2);
                    result.UpdateRatings(teams, eloDeltas, gameId, true);
                    result.NumGames++;
                }
            }
            return result;
        }

        public async Task RunCurrentRoundLoop() {
            await RunRoundLoop(await RecoverRoundData());
        }

        private async Task RunRoundLoop(OneRoundTempData round) {
            Console.WriteLine($"Simulating tournament round with {round.Teams.Count} teams with {numThreads} threads.");
            Task[] tasks = new Task[numThreads];
            for (int i = 0; i < tasks.Length; i++) {
                tasks[i] = Task.CompletedTask;
            }
            // Parallelization.
            while (round.NumGames < round.Teams.Count * round.DesiredNumGamesPerTeam / 2) {
                var completed = await Task.WhenAny(tasks);
                tasks[Array.IndexOf(tasks, completed)] = RunOneGame(round);
                round.NumGames++;
            }
            await Task.WhenAll(tasks);
            await FinishRound();
        }

        private async Task RunOneGame(OneRoundTempData round) {
            await round.Semaphore.WaitAsync();
            int[] teams;
            int gameId = -1;
            try {
                teams = round.SelectPlayers(simulator.NumPlayers);

                {
                    await using var cmd = new NpgsqlCommand("insert into Game (round, teams, start_time) VALUES (@round, @teams, @starttime) RETURNING id", conn);
                    cmd.Parameters.AddWithValue("round", round.Round);
                    cmd.Parameters.AddWithValue("teams", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer,
                        teams);
                    Int64 unixTimestamp = (Int64) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
                    cmd.Parameters.AddWithValue("starttime", unixTimestamp);
                    gameId = (int) await cmd.ExecuteScalarAsync();
                }
            } finally {
                round.Semaphore.Release();
            }

            Console.WriteLine("Simulating game between " + String.Join(", ", teams));

            GameLaunchParams launch = new GameLaunchParams();
            launch.Practice = false;
            foreach (int team in teams) {
                launch.Teams.Add(new TeamData {
                    Id = team,
                        BinaryPath = round.Teams[team].BinaryPath
                });
            }
            var result = await simulator.Launch(launch);

            double[] eloDeltas = Elo.ComputeEloDelta(teams.Select(t => round.Teams[t].StartingElo).ToArray(), result.Points.ToArray(), round.EloKFactor);

            await round.Semaphore.WaitAsync();
            try {
                round.UpdateRatings(teams, eloDeltas, gameId, false);

                {
                    await using var cmd = new NpgsqlCommand(
                        "update Game set end_time = @endtime, scores = @scores, result_path = @resultpath, " +
                        "elo_deltas = @elodeltas where id = @id", conn);
                    Int64 unixTimestamp = (Int64) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
                    cmd.Parameters.AddWithValue("endtime", unixTimestamp);
                    cmd.Parameters.AddWithValue("scores", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Double, result.Points.ToArray());
                    cmd.Parameters.AddWithValue("elodeltas", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Double, eloDeltas);
                    cmd.Parameters.AddWithValue("id", gameId);
                    cmd.Parameters.AddWithValue("resultpath", result.ResultPath);
                    await cmd.ExecuteNonQueryAsync();
                }
            } finally {
                round.Semaphore.Release();
            }
        }
    }

    class OneTeamTempData {
        public int Id { get; set; }
        public string BinaryPath { get; set; }
        public double StartingElo { get; set; }
        public double PendingElo { get; set; }
        public readonly List<int> Games = new List<int>();
        public int NumGamesPending { get; set; }
    }

    class OneRoundTempData {
        public int Round { get; set; }
        public readonly Dictionary<int, OneTeamTempData> Teams = new Dictionary<int, OneTeamTempData>();
        public int NumGames { get; set; }
        public int DesiredNumGamesPerTeam { get; set; }
        public int MaxTeams { get; set; }
        public int EloKFactor { get; set; }
        public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        public void UpdateRatings(int[] teams, double[] eloDeltas, int game_id, bool fromRecovery) {
            int n = teams.Length;
            Contract.Requires(n != 2);

            for (int i = 0; i < 2; i++) {
                var team = Teams[teams[i]];
                team.PendingElo += eloDeltas[i];
                team.Games.Add(game_id);
                if (!fromRecovery) {
                    team.NumGamesPending--;
                }
            }
        }

        private static Random rng = new Random();

        public static void Shuffle<T>(IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public int[] SelectPlayers(int n) {
            var list = Teams.Where(kv => kv.Value.Games.Count < DesiredNumGamesPerTeam).Select(kv => kv.Key).ToList();
            if (list.Count <= 1) {
                Console.WriteLine("Warning: simulating more games then there should be.");
                list = Teams.Keys.ToList();
            }

            Shuffle(list);
            if (list.Count < n) throw new Exception("Not enough teams :(");
            var result = list.Take(2).ToArray();
            foreach (var team in result) {
                Teams[team].NumGamesPending++;
            }
            return result;
        }
    }
}