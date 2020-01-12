using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Simulator {
    public class GameRunner {
        private readonly bool _isPracticeGame;
        private readonly string _sandboxPath;

        public GameRunner(bool isPracticeGame, string sandboxPath) {
            _isPracticeGame = isPracticeGame;
            _sandboxPath = sandboxPath;
        }

        private GameState state;

        private List<IProcess> processes = new List<IProcess>();

        private Regex flyRegex = new Regex(@"^fly ([0-9]+) ([0-9]+) ([0-9]+)$",
            RegexOptions.Compiled);

        public async Task<Game.GameRecord> Run(List<string> binaryPaths, int seed = 0) {
            state = new GameState(seed);
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < GameState.NUM_COMMANDERS_PER_PLAYER; j++) {
                    var startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.StandardInputEncoding = Encoding.ASCII;
                    startInfo.StandardOutputEncoding = Encoding.ASCII;
                    startInfo.RedirectStandardError = true;
                    if (_sandboxPath != null) {
                        // Sandbox is used in production server.
                        startInfo.FileName = _sandboxPath;
                        startInfo.ArgumentList.Add(binaryPaths[i]);
                    } else {
                        // For offline simulation we bypass the sandbox.
                        startInfo.FileName = binaryPaths[i];
                    }
                    processes.Add(new StreamProcess(() => Process.Start(startInfo), i * GameState.NUM_COMMANDERS_PER_PLAYER + j));
                }
            }

            var initialInput = state.GetInitialInput();
            string initialInputStr; {
                var builder = new StringBuilder("stars");
                foreach (var star in initialInput.StarPositions) {
                    builder.Append(" ").Append(star.X).Append(" ").Append(star.Y);
                }
                builder.Append("\n");
                initialInputStr = builder.ToString();
            }

            {
                var writeTasks = new List<Task>();
                for (int i = 0; i < 2 * GameState.NUM_COMMANDERS_PER_PLAYER; i++) {
                    writeTasks.Add(processes[i].WriteMessageAsync(new List<string> { initialInputStr }, false));
                }
                await Task.WhenAll(writeTasks);
            }

            var currentScores = new List<int> { GameState.NUM_COMMANDERS_PER_PLAYER, GameState.NUM_COMMANDERS_PER_PLAYER };
            var result = new Game.GameRecord();
            for (int i = 0; i < 1000; i++) {
                // If all processes failed, terminate early.
                if (processes.All(p => p.Failed)) {
                    break;
                }

                // Communicate with each AI in succession. Don't parallelize them because we would
                // just be waiting for the straggler. So game simulation is "single threaded", and
                // we do parallelization by simulating multiple games at the same time instead.
                var inputs = state.GetInputsForTurn();
                var outputs = new List<Game.TurnOutput>();
                for (int j = 0; j < 2 * GameState.NUM_COMMANDERS_PER_PLAYER; j++) {
                    var input = inputs[j];
                    List<string> lines = new List<string>();
                    foreach (var star in input.Stars) {
                        lines.Add($"star {star.Id} {star.Richness} {star.Owner} {star.ShipCount} {star.TurnsToNextProduction}");
                    }
                    foreach (var link in input.Link) {
                        lines.Add($"link {link.StarIdA} {link.StarIdB}");
                    }
                    foreach (var flight in input.Flight) {
                        lines.Add($"flight {flight.FromStarId} {flight.ToStarId} {flight.ShipCount} {flight.Owner} {flight.TurnsToArrival}");
                    }
                    await processes[j].WriteMessageAsync(lines, true);

                    var output = new Game.TurnOutput();
                    var(success, outputLines) = await processes[j].ReadMessageAsync();
                    if (success) {
                        try {
                            foreach (var line in outputLines) {
                                var match = flyRegex.Match(line);
                                if (match.Success) {
                                    output.Fly.Add(new Game.Fly() {
                                        FromStarId = int.Parse(match.Groups[1].Value),
                                            ToStarId = int.Parse(match.Groups[2].Value),
                                            ShipCount = int.Parse(match.Groups[3].Value)
                                    });
                                }
                            }
                        } catch {}
                    }
                    outputs.Add(output);
                }

                currentScores = state.AdvanceTurn(outputs);

                if (currentScores[0] >= 3 * GameState.NUM_STARS / 4) {
                    result.Scores.Add(1);
                    result.Scores.Add(0);
                    break;
                } else if (currentScores[1] >= 3 * GameState.NUM_STARS / 4) {
                    result.Scores.Add(0);
                    result.Scores.Add(1);
                    break;
                }
            }

            for (int i = 0; i < 2 * GameState.NUM_COMMANDERS_PER_PLAYER; i++) {
                if (processes[i].Failed) {
                    if (_isPracticeGame) {
                        result.FailureMessage.Add(new Game.FailureMessage() { ProcessId = i, Msg = processes[i].FailureMessage });
                    }
                }
                processes[i].Stop();
            }

            if (result.Scores.Count == 0) {
                if (currentScores[0] >= 3 * GameState.NUM_STARS / 4) {
                    result.Scores.Add(1);
                    result.Scores.Add(0);
                } else if (currentScores[1] >= 3 * GameState.NUM_STARS / 4) {
                    result.Scores.Add(0);
                    result.Scores.Add(1);
                } else {
                    int s0 = Math.Max(0, currentScores[0] - GameState.NUM_STARS / 4);
                    int s1 = Math.Max(0, currentScores[1] - GameState.NUM_STARS / 4);
                    int totalScore = s0 + s1;
                    if (totalScore == 0) {
                        result.Scores.Add(0.5);
                        result.Scores.Add(0.5);
                    } else {
                        result.Scores.Add((double) s0 / totalScore);
                        result.Scores.Add((double) s1 / totalScore);
                    }
                }
            }
            result.MergeFrom(state.GetGameRecord());
            result.NumStars.AddRange(currentScores);
            return result;
        }
    }
}