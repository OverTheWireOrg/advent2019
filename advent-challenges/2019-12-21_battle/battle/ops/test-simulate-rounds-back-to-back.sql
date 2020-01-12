insert into Tournament
(round, scheduled_start_time, desired_games_per_team, teams_limit) values
(1, round(extract(epoch from now()) * 1000), 20, null),
(2, round(extract(epoch from now()) * 1000 + 120000), 20, null),
(3, round(extract(epoch from now()) * 1000 + 120000 * 2), 20, null),
(4, round(extract(epoch from now()) * 1000 + 120000 * 3), 20, null),
(5, round(extract(epoch from now()) * 1000 + 120000 * 4), 20, null),
(6, round(extract(epoch from now()) * 1000 + 120000 * 5), 20, null),
(7, round(extract(epoch from now()) * 1000 + 120000 * 6), 20, null),
(8, round(extract(epoch from now()) * 1000 + 120000 * 7), 20, null);