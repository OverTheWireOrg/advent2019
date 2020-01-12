using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Web {
    partial class Scoreboard {
        private TournamentCache data = new TournamentCache();
        private int roundId = 0;
        private RoundData currentRound = new RoundData();

        private int page = 0;

        private int teamId = -1;

        private string _searchText = "";
        private string searchText {
            get {
                return _searchText;
            }
            set {
                _searchText = value;
                if (data.TeamNameToId.ContainsKey(value)) {
                    teamId = data.TeamNameToId[value];
                } else {
                    teamId = -1;
                }
            }
        }

        private IEnumerable<GameData> SearchResult {
            get {
                return teamId == -1 ?
                    currentRound.Games :
                    currentRound.Games.Where(g => g.Teams.Contains(teamId));
            }
        }

        [Inject]
        public ScoreboardCache Cache { get; set; }

        private Action subscription;

        protected override async Task OnInitializedAsync() {
            subscription = () => {
                InvokeAsync(() => {
                    data = Cache.Cache;
                    if (roundId >= data.Cache.Rounds.Count) {
                        roundId = 0;
                    }
                    if (roundId < data.Cache.Rounds.Count) {
                        currentRound = data.Cache.Rounds[roundId];
                    } else {
                        currentRound = new RoundData();
                    }
                    if (page >= (SearchResult.Count() + 100 - 1) / 100) {
                        page = 0;
                    }
                    StateHasChanged();
                });
            };
            Cache.CacheUpdated += subscription;
            subscription();
        }

        public void Dispose() {
            Cache.CacheUpdated -= subscription;
        }

        private string DeltaDisplay(double delta) {
            if (double.IsNaN(delta)) return "";
            double eloRounded = Math.Round(delta, 1);
            if (eloRounded == 0) { return "±0"; }
            if (eloRounded > 0) { return "+" + eloRounded; }
            return eloRounded.ToString();
        }

        private string ScoreDisplay(double score) {
            if (double.IsNaN(score)) return "?";
            return Math.Round(score, 1).ToString();
        }

        private string DeltaClass(double delta) {
            if (double.IsNaN(delta)) return "";
            int eloRounded = (int) Math.Round(delta);
            if (eloRounded == 0) { return "delta-neutral"; }
            if (eloRounded > 0) { return "delta-positive"; }
            return "delta-negative";
        }

        private string ScoreClass(double score) {
            if (double.IsNaN(score)) return "score-pending";
            if (score == 1) {
                return "score-win";
            }
            if (score == 0) {
                return "score-lose";
            }
            return "score-tie";
        }

        private string DownloadLink(string resultsPath) {
            return "/results/" + resultsPath;
        }

        private string ScheduledTime {
            get {
                return currentRound.ScheduledTime.UnixMillisToDateTime().ToString();
            }
        }
    }
}