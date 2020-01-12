using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Simulator {
    class Star {
        public Game.StarPosition Position { get; set; }
        public Game.Star Data { get; set; }
        public int PendingOutgoingFlights { get; set; }

        public Vector2 PositionAsVector {
            get {
                return new Vector2(Position.X, Position.Y);
            }
        }
    }

    class Commander {
        public int Affiliation { get; set; }
        public List<int> AnonymizedCommanderIds { get; set; }
    }

    public class GameState {
        internal List<Star> stars = new List<Star>();
        private List<Commander> commanders = new List<Commander>();
        private List<Game.Flight> flights = new List<Game.Flight>();
        private List<HashSet<int>> links = new List<HashSet<int>>();
        private Random random;
        private Game.GameRecord record = new Game.GameRecord();

        public const int NUM_COMMANDERS_PER_PLAYER = 10;
        public const int NUM_STARS = 90;
        private const int MAP_SIZE = 300;
        private const int TRAVEL_SPEED = 10;
        private const int SIGHT_RANGE = 40;
        private const int FLIGHT_RANGE = 60;
        private const int MAX_OUTGOING_FLIGHTS_IN_FLIGHT = 3;
        private const int TURNS_PER_PRODUCTION = 5;

        public GameState(int seed) {
            random = new Random(seed);

            // Create commanders.
            for (int i = 0; i < 2 * NUM_COMMANDERS_PER_PLAYER; i++) {
                var commander = new Commander() {
                    Affiliation = i / NUM_COMMANDERS_PER_PLAYER
                };
                commander.AnonymizedCommanderIds = new List<int>();
                for (int j = 0; j < 2 * NUM_COMMANDERS_PER_PLAYER; j++) {
                    int anonymizedId = i == j ? 0 : (i / 10 == j / 10 ? 1 : 2);
                    commander.AnonymizedCommanderIds.Add(anonymizedId);
                }
                commanders.Add(commander);
            }

            // Initialize links adjacency map.
            for (int i = 0; i < NUM_STARS; i++) {
                links.Add(new HashSet<int>());
            }

            var generator = new MapGenerator(seed);
            while (true) {
                stars.Clear();
                generator.NumPoints = NUM_STARS;
                generator.MapSize = new System.Numerics.Vector2(MAP_SIZE, MAP_SIZE);
                generator.DesiredDistance = SIGHT_RANGE;
                var map = generator.GenerateMap();
                for (int i = 0; i < NUM_STARS; i++) {
                    stars.Add(new Star {
                        Position = new Game.StarPosition {
                                X = (int) map[i].X,
                                    Y = (int) map[i].Y
                            },
                            Data = new Game.Star {
                                Id = i,
                                    Owner = -1,
                                    ShipCount = 0,
                                    TurnsToNextProduction = 0
                            }
                    });
                }

                bool distanceCheckPasses = true;
                // Make sure no star is too distant from others.
                for (int i = 0; i < NUM_STARS; i++) {
                    int visibleStars = 0;
                    int reachableStars = 0;
                    for (int j = 0; j < NUM_STARS; j++) {
                        if (j == i) continue;
                        if (CalculateDistanceCeiling(i, j) <= SIGHT_RANGE) {
                            visibleStars++;
                        }
                        if (CalculateDistanceCeiling(i, j) <= FLIGHT_RANGE) {
                            reachableStars++;
                        }
                    }
                    if (visibleStars < 1 || reachableStars < 2) {
                        distanceCheckPasses = false;
                    }
                }
                if (distanceCheckPasses) {
                    break;
                }
            }

            // Pick commanders in a way that commanders sit far away from other commanders.
            {
                for (int affiliation = 0; affiliation < 2; affiliation++) {
                    var ownedStars = new List<Vector2>();
                    for (int i = 0; i < NUM_COMMANDERS_PER_PLAYER; i++) {
                        var eligible = new List<int>();
                        for (int m = 0; m < 5; m++) {
                            int star;
                            do {
                                star = random.Next(NUM_STARS);
                            } while (stars[star].Data.Owner != -1 || eligible.Contains(star));
                            eligible.Add(star);
                        }
                        int choice;
                        if (ownedStars.Count == 0) {
                            choice = eligible[0];
                        } else {
                            choice = eligible.OrderByDescending(j => ownedStars.Select(s => (s - stars[j].PositionAsVector).Length()).Min()).First();
                        }
                        stars[choice].Data.Owner = affiliation * NUM_COMMANDERS_PER_PLAYER + i;
                        stars[choice].Data.ShipCount = 10;
                        stars[choice].Data.TurnsToNextProduction = TURNS_PER_PRODUCTION;
                        ownedStars.Add(stars[choice].PositionAsVector);
                    }
                }
            }

            while (true) {
                // Randomize the richness of the stars.
                int richnessDiff = 0;
                for (int i = 0; i < NUM_STARS; i++) {
                    stars[i].Data.Richness = random.Next(5) + 1;
                    if (stars[i].Data.Owner != -1) {
                        if (stars[i].Data.Owner < NUM_COMMANDERS_PER_PLAYER) {
                            richnessDiff += stars[i].Data.Richness;
                        } else {
                            richnessDiff -= stars[i].Data.Richness;
                        }
                    }
                }
                if (richnessDiff == 0) {
                    break;
                }
            }

            record.InitialConfiguration = GetInitialInput();
            record.Turns.Add(GetTurnInputForRecordKeeping());
        }

        public Game.InitialInput GetInitialInput() {
            var result = new Game.InitialInput();
            result.StarPositions.AddRange(stars.Select(s => s.Position));
            return result;
        }

        public Game.GameRecord GetGameRecord() {
            return record;
        }

        class CommanderMap {
            public readonly List<int> CommanderToComponent = new List<int>();
            public readonly List<List<int>> ComponentToCommanders = new List<List<int>>();

            public int NumComponents { get { return ComponentToCommanders.Count; } }
        }

        CommanderMap GetCommanderConnectedComponents() {
            // Like star links, but collapsing all stars of a commander into one node.
            var commanderLinks = new List<HashSet<int>>();
            for (int i = 0; i < 2 * NUM_COMMANDERS_PER_PLAYER; i++) {
                commanderLinks.Add(new HashSet<int>());
            }
            for (int from = 0; from < NUM_STARS; from++) {
                int fromOwner = stars[from].Data.Owner;
                foreach (int to in links[from]) {
                    int toOwner = stars[to].Data.Owner;
                    // Don't need two directions since star links already provide both directions.
                    commanderLinks[fromOwner].Add(toOwner);
                }
            }

            bool[] visited = new bool[NUM_COMMANDERS_PER_PLAYER * 2];

            Action<int, List<int>> floodfill = null;
            floodfill = (i, component) => {
                visited[i] = true;
                component.Add(i);
                foreach (int neighbor in commanderLinks[i]) {
                    if (!visited[neighbor]) {
                        floodfill(neighbor, component);
                    }
                }
            };

            var result = new CommanderMap();
            for (int i = 0; i < 2 * NUM_COMMANDERS_PER_PLAYER; i++) {
                result.CommanderToComponent.Add(-1);
            }
            for (int i = 0; i < 2 * NUM_COMMANDERS_PER_PLAYER; i++) {
                if (!visited[i]) {
                    var component = new List<int>();
                    floodfill(i, component);
                    foreach (int commander in component) {
                        result.CommanderToComponent[commander] = result.ComponentToCommanders.Count;
                    }
                    result.ComponentToCommanders.Add(component);
                }
            }

            return result;
        }

        private int CalculateDistanceCeiling(int fromStar, int toStar) {
            var fromPos = stars[fromStar].Position;
            var toPos = stars[toStar].Position;
            long squareDist = (long) (fromPos.X - toPos.X) * (fromPos.X - toPos.X) +
                (long) (fromPos.Y - toPos.Y) * (fromPos.Y - toPos.Y);

            int lb = 1;
            int ub = MAP_SIZE * 2;
            while (lb < ub) {
                int mb = (lb + ub) / 2;
                if (mb * mb >= squareDist) {
                    ub = mb;
                } else {
                    lb = mb + 1;
                }
            }
            return lb;
        }

        private Game.TurnInput AnonymizeTurnInput(Game.TurnInput input, int commanderId) {
            Commander commander = commanders[commanderId];
            var result = new Game.TurnInput();
            foreach (var star in input.Stars) {
                var copy = star.Clone();
                if (copy.Owner != -1) {
                    copy.Owner = commander.AnonymizedCommanderIds[copy.Owner];
                }
                result.Stars.Add(copy);
            }
            result.Link.AddRange(input.Link);
            foreach (var flight in input.Flight) {
                var copy = flight.Clone();
                copy.Owner = commander.AnonymizedCommanderIds[copy.Owner];
                result.Flight.Add(copy);
            }
            return result;
        }

        public List<Game.TurnInput> GetInputsForTurn() {
            var inputs = new List<Game.TurnInput>();
            for (int i = 0; i < 2 * NUM_COMMANDERS_PER_PLAYER; i++) {
                inputs.Add(null);
            }

            // For each star, the list of commander IDs who can see this star and its surroundings.
            var visibility = new List<List<int>>();

            var components = GetCommanderConnectedComponents();
            List<List<int>> componentOwnedStars = new List<List<int>>();
            for (int i = 0; i < components.NumComponents; i++) {
                componentOwnedStars.Add(new List<int>());
            }
            for (int i = 0; i < NUM_STARS; i++) {
                if (stars[i].Data.Owner != -1) {
                    componentOwnedStars[components.CommanderToComponent[stars[i].Data.Owner]].Add(i);
                }
            }
            for (int i = 0; i < components.NumComponents; i++) {
                var componentInput = new Game.TurnInput();
                for (int starId = 0; starId < NUM_STARS; starId++) {
                    bool canSee = false;
                    foreach (int intelStar in componentOwnedStars[i]) {
                        if (CalculateDistanceCeiling(intelStar, starId) <= SIGHT_RANGE) {
                            canSee = true;
                            break;
                        }
                    }
                    if (canSee) {
                        componentInput.Stars.Add(stars[starId].Data);
                    }
                }
                for (int from = 0; from < NUM_STARS; from++) {
                    foreach (int to in links[from]) {
                        if (to > from) {
                            bool canSee = false;
                            foreach (int intelStar in componentOwnedStars[i]) {
                                if (CalculateDistanceCeiling(intelStar, to) <= SIGHT_RANGE ||
                                    CalculateDistanceCeiling(intelStar, from) <= SIGHT_RANGE) {
                                    canSee = true;
                                    break;
                                }
                            }
                            if (canSee) {
                                componentInput.Link.Add(new Game.Link {
                                    StarIdA = from,
                                        StarIdB = to,
                                });
                            }
                        }
                    }
                }
                foreach (var flight in flights) {
                    int totalTurns = (CalculateDistanceCeiling(flight.FromStarId, flight.ToStarId) + TRAVEL_SPEED - 1) / TRAVEL_SPEED;
                    double posRatio = 1 - (double) flight.TurnsToArrival / totalTurns;
                    var fromPos = stars[flight.FromStarId].Position;
                    var toPos = stars[flight.ToStarId].Position;
                    double posX = fromPos.X + (toPos.X - fromPos.X) * posRatio;
                    double posY = fromPos.Y + (toPos.Y - fromPos.Y) * posRatio;

                    bool canSee = false;
                    foreach (int intelStar in componentOwnedStars[i]) {
                        double intelX = stars[intelStar].Position.X;
                        double intelY = stars[intelStar].Position.Y;
                        if (Math.Pow(intelX - posX, 2) + Math.Pow(intelY - posY, 2) + 1e-5 <= Math.Pow(SIGHT_RANGE, 2)) {
                            canSee = true;
                            break;
                        }
                    }
                    // Commander's own ships are always visible.
                    if (components.ComponentToCommanders[i].Contains(flight.Owner)) {
                        canSee = true;
                    }
                    if (canSee) {
                        componentInput.Flight.Add(flight);
                    }
                }
                foreach (int commander in components.ComponentToCommanders[i]) {
                    inputs[commander] = AnonymizeTurnInput(componentInput, commander);
                }
            }
            return inputs;
        }

        private Game.TurnInput GetTurnInputForRecordKeeping() {
            var result = new Game.TurnInput();
            result.Stars.AddRange(stars.Select(s => s.Data.Clone()));
            for (int from = 0; from < NUM_STARS; from++) {
                foreach (int to in links[from]) {
                    if (to > from) {
                        result.Link.Add(new Game.Link {
                            StarIdA = from,
                                StarIdB = to,
                        });
                    }
                }
            }
            result.Flight.AddRange(flights.Select(f => f.Clone()));
            return result;
        }

        private void AddFlight(Game.Flight flight) {
            stars[flight.FromStarId].PendingOutgoingFlights++;
            stars[flight.FromStarId].Data.ShipCount -= flight.ShipCount;
            flights.Add(flight);
        }

        private void AddLink(int starA, int starB) {
            links[starA].Add(starB);
            links[starB].Add(starA);
        }

        private void RemoveAllLinks(int star) {
            foreach (var other in links[star]) {
                links[other].Remove(star);
            }
            links[star].Clear();
        }

        internal void ResolveArrivingFlights(int star, List<Game.Flight> flights) {
            // First update the in-flight count from the source star.
            foreach (var flight in flights) {
                stars[flight.FromStarId].PendingOutgoingFlights--;
            }

            // Process friendly units.
            var starData = stars[star].Data;
            var starOwner = starData.Owner;
            if (starOwner != -1) {
                foreach (var flight in flights) {
                    if (commanders[flight.Owner].Affiliation == commanders[starOwner].Affiliation) {
                        starData.ShipCount += flight.ShipCount;
                        var fromOwner = stars[flight.FromStarId].Data.Owner;
                        if (fromOwner != starOwner && fromOwner == flight.Owner) {
                            AddLink(flight.FromStarId, flight.ToStarId);
                        }
                    }
                }
            }

            // Process enemy units.
            int enemyShipsTotal = 0;
            int capturingAffiliation = 0;
            var enemyShipsByCommander = new Dictionary<int, int>();
            foreach (var flight in flights) {
                if (starOwner == -1 || commanders[flight.Owner].Affiliation != commanders[starOwner].Affiliation) {
                    enemyShipsTotal += commanders[flight.Owner].Affiliation == capturingAffiliation ? flight.ShipCount : -flight.ShipCount;
                    if (!enemyShipsByCommander.ContainsKey(flight.Owner)) {
                        enemyShipsByCommander[flight.Owner] = 0;
                    }
                    enemyShipsByCommander[flight.Owner] += flight.ShipCount;
                }
            }
            if (enemyShipsTotal < 0) {
                capturingAffiliation = 1 - capturingAffiliation;
                enemyShipsTotal = -enemyShipsTotal;
            }
            int attackOverhead = starData.Owner == -1 ? 5 : 10;
            if (enemyShipsTotal <= attackOverhead + starData.ShipCount) {
                // Attack failed.
                starData.ShipCount -= Math.Max(enemyShipsTotal - attackOverhead, 0);
            } else {
                // takeover happens.
                var maxShips = enemyShipsByCommander.Values.Max();
                var eligibleEnemies = enemyShipsByCommander
                    .Where(kv => kv.Value == maxShips && commanders[kv.Key].Affiliation == capturingAffiliation)
                    .Select(s => s.Key)
                    .ToList();
                var newOwner = eligibleEnemies[random.Next(eligibleEnemies.Count)];
                starData.Owner = newOwner;
                starData.ShipCount = enemyShipsTotal - attackOverhead - starData.ShipCount;
                starData.TurnsToNextProduction = TURNS_PER_PRODUCTION;
                RemoveAllLinks(star);
            }
        }

        // Returns the number of stars owned by each player.
        public List<int> AdvanceTurn(List<Game.TurnOutput> instructions) {
            // First create flights as instructed by the AIs.
            var alreadyIssuedFlights = new HashSet < (int from, int to) > ();
            for (int i = 0; i < 2 * NUM_COMMANDERS_PER_PLAYER; i++) {
                var inst = instructions[i];
                foreach (var fly in inst.Fly) {
                    if (fly.FromStarId < 0 || fly.FromStarId >= NUM_STARS || fly.ToStarId < 0 || fly.ToStarId >= NUM_STARS) {
                        Trace.TraceInformation($"[{i}] Invalid flight, stars out of bound: {fly}");
                        continue;
                    }
                    if (fly.FromStarId == fly.ToStarId) {
                        Trace.TraceInformation($"[{i}] Invalid flight, flying to same star: {fly}");
                        continue;
                    }
                    if (stars[fly.FromStarId].Data.Owner != i) {
                        Trace.TraceInformation($"[{i}] Invalid flight, source star not owned: {fly}");
                        continue;
                    }
                    if (fly.ShipCount <= 0 || stars[fly.FromStarId].Data.ShipCount < fly.ShipCount) {
                        Trace.TraceInformation($"[{i}] Invalid flight, invalid ship count: {fly}");
                        continue;
                    }
                    if (CalculateDistanceCeiling(fly.FromStarId, fly.ToStarId) > FLIGHT_RANGE) {
                        Trace.TraceInformation($"[{i}] Invalid flight, star too far: {fly}, {stars[fly.FromStarId].Position}, {stars[fly.ToStarId].Position}");
                        continue;
                    }
                    if (stars[fly.FromStarId].PendingOutgoingFlights >= MAX_OUTGOING_FLIGHTS_IN_FLIGHT) {
                        Trace.TraceInformation($"[{i}] Invalid flight, too many outgoing flights: {fly}");
                        continue;
                    }
                    if (alreadyIssuedFlights.Contains((fly.FromStarId, fly.ToStarId))) {
                        Trace.TraceInformation($"[{i}] Invalid flight, duplicate flight in same turn: {fly}");
                        continue;
                    }
                    alreadyIssuedFlights.Add((fly.FromStarId, fly.ToStarId));
                    var flight = new Game.Flight {
                        FromStarId = fly.FromStarId,
                        ToStarId = fly.ToStarId,
                        Owner = i,
                        ShipCount = fly.ShipCount,
                        TurnsToArrival = (CalculateDistanceCeiling(fly.FromStarId, fly.ToStarId) + TRAVEL_SPEED - 1) / TRAVEL_SPEED
                    };
                    AddFlight(flight);
                }
            }

            // Now advance 1 turn, granting production of new ships, and resolving arrivals.
            foreach (var star in stars) {
                if (star.Data.Owner != -1) {
                    star.Data.TurnsToNextProduction--;
                    if (star.Data.TurnsToNextProduction == 0) {
                        star.Data.TurnsToNextProduction = TURNS_PER_PRODUCTION;
                        star.Data.ShipCount += star.Data.Richness;
                    }
                }
            }

            var arrivingFlights = new List<List<Game.Flight>>();
            for (int i = 0; i < NUM_STARS; i++) {
                arrivingFlights.Add(new List<Game.Flight>());
            }

            // Erase-remove idiom.
            int j = 0;
            for (int i = 0; i < flights.Count; i++) {
                var flight = flights[i];
                flight.TurnsToArrival -= 1;
                if (flight.TurnsToArrival > 0) {
                    flights[j] = flights[i];
                    j++;
                } else {
                    arrivingFlights[flight.ToStarId].Add(flight);
                }
            }
            flights.RemoveRange(j, flights.Count - j);

            for (int i = 0; i < NUM_STARS; i++) {
                if (arrivingFlights[i].Count > 0) {
                    ResolveArrivingFlights(i, arrivingFlights[i]);
                }
            }

            record.Turns.Add(GetTurnInputForRecordKeeping());

            List<int> numStarsOwned = new List<int>() { 0, 0 };
            foreach (var star in stars) {
                if (star.Data.Owner != -1) {
                    numStarsOwned[commanders[star.Data.Owner].Affiliation]++;
                }
            }
            return numStarsOwned;
        }
    }

}