create table Tournament (
	id serial,
	round integer,
	scheduled_start_time bigint,
	actual_start_time bigint,
	actual_end_time bigint,
	desired_games_per_team integer,
	teams_limit integer,
	elo_k_factor integer,
	primary key (id)
);

CREATE TABLE Team (
	id serial,
	name text,
	token text default '',
	PRIMARY KEY( id )
);

create table Binaries (
	id serial,
	team integer references Team(id),
	upload_time bigint,
	binary_path text,
	validation_result_path text,
	validation_passed boolean default false,
	primary key(id)
);

create table Round (
    id serial,
    team integer references Team(id),
	round integer,
	binary_path text,
	wins integer default 0,
	losses integer default 0,
	draws integer default 0,
	elo_gain double precision,
	elo double precision,
	games integer array,
    primary key (id)
);

create table Game (
	id serial,
	round integer,
	teams integer array,
	scores double precision array,
	elo_deltas double precision array,
	start_time bigint default 0,
	end_time bigint default 0,
	result_path text,
	primary key (id)
);

