using System;
using System.Timers;
using Microsoft.AspNetCore.Components;

namespace Web {
    partial class MainLayout {
        private TournamentCache cache = new TournamentCache();
        private string statusText = "";
        private Timer timer;

        [Inject]
        public ScoreboardCache Cache { get; set; }

        protected override void OnInitialized() {
            Update();
            timer = new Timer(60 * 1000);
            timer.Elapsed += (_, __) => Update();
            timer.Start();
        }

        void Update() {
            InvokeAsync(() => {
                cache = Cache.Cache;
                foreach (var round in cache.Cache.Rounds) {
                    if (!round.Started) {
                        var diff = round.ScheduledTime.UnixMillisToDateTime() - DateTime.UtcNow;
                        statusText = $"Round {round.Round} starts in ";
                        if ((int) diff.TotalHours > 0) {
                            statusText += $"{(int)diff.TotalHours} hours, ";
                        }
                        statusText += $"{diff.Minutes} minutes";
                    } else {
                        statusText = $"Round {round.Round} currently running";
                    }
                    return;
                }
                statusText = "Competition has finished";
                StateHasChanged();
            });
        }

        public void Dispose() {
            timer.Stop();
            timer.Dispose();
            timer = null;
        }
    }
}