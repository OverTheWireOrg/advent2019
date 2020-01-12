using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

namespace Web {
    public class ScoreboardCache {
        private TournamentCache _cache = new TournamentCache();
        public event Action CacheUpdated;

        public ScoreboardCache() {
            BeginRefresh();
        }

        public async void BeginRefresh() {
            while (true) {
                try {
                    await using var conn = await DbService.NewConnection();
                    await Refresh(conn);
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        public TournamentCache Cache {
            get {
                lock(this) {
                    return _cache;
                }
            }
        }

        private async Task Refresh(NpgsqlConnection conn) {
            var cache = new TournamentCache();

            var roundIdToRound = new Dictionary<int, RoundData>();
            var maxRoundId = 0;

            {
                await using var cmd = new NpgsqlCommand("select round, scheduled_start_time, actual_start_time, actual_end_time from Tournament order by round asc", conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    var roundId = reader.GetInt32(0);
                    var round = new RoundData {
                        Round = roundId,
                        Started = reader.GetValue(2) is long,
                        ScheduledTime = reader.GetInt64(1)
                    };
                    roundIdToRound[roundId] = round;
                    maxRoundId = roundId;
                    if (!(reader.GetValue(3) is long)) break;
                }
                for (var round = maxRoundId; round >= 1; round--) {
                    cache.Cache.Rounds.Add(roundIdToRound[round]);
                }
            }

            var teamLatestBinary = new Dictionary<int, string>();

            {

                await using var cmd = new NpgsqlCommand("select team, binary_path from Binaries where validation_passed = true order by upload_time asc", conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    teamLatestBinary[reader.GetInt32(0)] = reader.GetString(1);
                }
            }

            var teamIdToLastTeamRound = new Dictionary<int, TeamData>();

            {
                await using var cmd = new NpgsqlCommand("select id, name from Team", conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    cache.TeamNames[id] = name;
                    cache.TeamNameToId[name] = id;
                    if (teamLatestBinary.ContainsKey(id)) {
                        teamIdToLastTeamRound[id] = new TeamData {
                            Name = name,
                            Binary = teamLatestBinary[id],
                            StartingElo = -1,
                            EndingElo = -1
                        };
                        if (!cache.Cache.Rounds[0].Started) {
                            cache.Cache.Rounds[0].Teams.Add(teamIdToLastTeamRound[id]);
                        }
                    }
                }
            }

            {
                await using var cmd = new NpgsqlCommand("select round, team, elo, elo_gain, binary_path from Round order by round desc, elo desc", conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    int round = reader.GetInt32(0);
                    var roundData = roundIdToRound[round];
                    var teamData = new TeamData {
                        Binary = reader.GetString(4),
                        Id = reader.GetInt32(1),
                        Name = cache.TeamNames[reader.GetInt32(1)]
                    };
                    if (reader.GetValue(2) is double) {
                        teamData.StartingElo = (int) (reader.GetDouble(2) - reader.GetDouble(3));
                        teamData.EndingElo = (int) reader.GetDouble(2);
                        if (round + 1 == cache.Cache.Rounds[0].Round) {
                            if (teamIdToLastTeamRound.ContainsKey(teamData.Id)) {
                                teamIdToLastTeamRound[teamData.Id].StartingElo = teamData.EndingElo;
                            }
                        }
                    } else {
                        teamData.StartingElo = teamData.EndingElo = -1;
                        if (round == cache.Cache.Rounds[0].Round) {
                            teamIdToLastTeamRound[teamData.Id] = teamData;
                        }
                    }
                    roundData.Teams.Add(teamData);
                }
            }

            {
                await using var cmd = new NpgsqlCommand(
                    "select round, teams, scores, start_time, end_time, result_path, elo_deltas, id from Game order by start_time desc", conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) {
                    int round = reader.GetInt32(0);
                    var roundData = roundIdToRound[round];
                    var gameData = new GameData {
                        Id = reader.GetInt32(7),
                        StartTime = reader.GetInt64(3),
                        EndTime = reader.GetValue(4) is int ? reader.GetInt64(4) : -1,
                        ResultsLink = reader.GetValue(5) is string ? reader.GetString(5) : ""
                    };
                    foreach (var team in (int[]) reader.GetValue(1)) {
                        gameData.Teams.Add(team);
                    }
                    if (!(reader.GetValue(2) is DBNull)) {
                        foreach (var score in (double[]) reader.GetValue(2)) {
                            gameData.Scores.Add(score);
                        }
                    } else {
                        gameData.Scores.Add(double.NaN);
                        gameData.Scores.Add(double.NaN);
                    }
                    if (!(reader.GetValue(6) is DBNull)) {
                        foreach (var eloDelta in (double[]) reader.GetValue(6)) {
                            gameData.EloDeltas.Add(eloDelta);
                        }
                    } else {
                        gameData.EloDeltas.Add(double.NaN);
                        gameData.EloDeltas.Add(double.NaN);
                    }
                    roundData.Games.Add(gameData);
                }
            }

            lock(this) {
                _cache = cache;
            }

            Action temp = CacheUpdated;
            if (temp != null) {
                temp();
            }
        }

    }

    public class TournamentCache {
        public TournamentData Cache = new TournamentData();
        public Dictionary<int, string> TeamNames = new Dictionary<int, string>();
        public Dictionary<string, int> TeamNameToId = new Dictionary<string, int>();
    }
}