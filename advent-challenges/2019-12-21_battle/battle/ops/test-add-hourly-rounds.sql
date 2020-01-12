insert into Tournament
(round, scheduled_start_time, desired_games_per_team, teams_limit, elo_k_factor) values
(1, round(extract(epoch from now()) * 1000 + 300 * 1000 * 1), 20, 256, 32),
(2, round(extract(epoch from now()) * 1000 + 300 * 1000 * 2), 20, 128, 24),
(3, round(extract(epoch from now()) * 1000 + 300 * 1000 * 3), 20, 64, 24),
(4, round(extract(epoch from now()) * 1000 + 300 * 1000 * 4), 20, 32, 24),
(5, round(extract(epoch from now()) * 1000 + 300 * 1000 * 5), 20, 16, 24),
(6, round(extract(epoch from now()) * 1000 + 300 * 1000 * 6), 20, 8, 16),
(7, round(extract(epoch from now()) * 1000 + 300 * 1000 * 7), 20, 4, 16),
(8, round(extract(epoch from now()) * 1000 + 300 * 1000 * 8), 20, 2, 16)
;
