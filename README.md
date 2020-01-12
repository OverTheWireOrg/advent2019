This is the repository with challenges from the OverTheWire Advent Bonanza 2019 CTF

# Directory structure

* ```advent-challenges/``` holds the challenges that were used on the individual advent days
* ```hidden-challenges/``` holds hidden challenges (challenge zero, easter eggs and extra flags for other challenges)

# Set up a server

Online challenges run via docker-compose. On Ubuntu, you can set this up with:

	apt install -y docker.io docker-compose
	adduser <your username> docker

# Start a challenge

Each online challenge has a ```docker-compose.yml``` or ```docker-compose.yaml``` file.
Go into the challenge directory and start the docker container with:

	docker-compose up

