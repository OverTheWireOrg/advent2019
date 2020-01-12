using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simulator {

    [TestFixture]
    public class GameTest {
        private GameState game;

        [SetUp]
        public void SetUp() {
            game = new GameState(0);
        }

        [Test]
        public void TestFlightResolutionWhenBothTeamsAttackUnownedStar() {
            var starId = -1;
            for (int i = 0; i < game.stars.Count; i++) {
                if (game.stars[i].Data.Owner == -1) {
                    starId = i;
                    break;
                }
            }
            var flights = new List<Game.Flight>() {
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 1,
                ShipCount = 11,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 2,
                ShipCount = 6,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 8,
                ShipCount = 3,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 15,
                ShipCount = 9,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 15,
                ShipCount = 8,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 18,
                ShipCount = 10,
                TurnsToArrival = 0
                },
            };
            game.ResolveArrivingFlights(starId, flights);
            Assert.AreEqual(game.stars[starId].Data.Owner, 15);
            Assert.AreEqual(game.stars[starId].Data.ShipCount, 2);
        }

        [Test]
        public void TestFlightResolutionWhenOneTeamAttacksUnownedStarFailing() {
            var starId = -1;
            for (int i = 0; i < game.stars.Count; i++) {
                if (game.stars[i].Data.Owner == -1) {
                    starId = i;
                    break;
                }
            }
            var flights = new List<Game.Flight>() {
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 3,
                ShipCount = 3,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 5,
                ShipCount = 2,
                TurnsToArrival = 0
                },
            };
            game.ResolveArrivingFlights(starId, flights);
            Assert.AreEqual(-1, game.stars[starId].Data.Owner);
            Assert.AreEqual(0, game.stars[starId].Data.ShipCount);
        }

        [Test]
        public void TestFlightResolutionWhenOneTeamAttacksUnownedStarSucceeding() {
            var starId = -1;
            for (int i = 0; i < game.stars.Count; i++) {
                if (game.stars[i].Data.Owner == -1) {
                    starId = i;
                    break;
                }
            }
            var flights = new List<Game.Flight>() {
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 3,
                ShipCount = 2,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 5,
                ShipCount = 4,
                TurnsToArrival = 0
                },
            };
            game.ResolveArrivingFlights(starId, flights);
            Assert.AreEqual(5, game.stars[starId].Data.Owner);
            Assert.AreEqual(1, game.stars[starId].Data.ShipCount);
        }

        [Test]
        public void TestFlightResolutionWhenOneTeamAttacksEnemyStarSucceeding0() {
            var starId = -1;
            for (int i = 0; i < game.stars.Count; i++) {
                if (game.stars[i].Data.Owner == 10) {
                    starId = i;
                    break;
                }
            }
            var flights = new List<Game.Flight>() {
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 3,
                ShipCount = 2,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 5,
                ShipCount = 22,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 16,
                ShipCount = 3,
                TurnsToArrival = 0
                },
            };
            game.ResolveArrivingFlights(starId, flights);
            Assert.AreEqual(5, game.stars[starId].Data.Owner);
            Assert.AreEqual(1, game.stars[starId].Data.ShipCount);
        }

        [Test]
        public void TestFlightResolutionWhenOneTeamAttacksEnemyStarSucceeding1() {
            var starId = -1;
            for (int i = 0; i < game.stars.Count; i++) {
                if (game.stars[i].Data.Owner == 0) {
                    starId = i;
                    break;
                }
            }
            var flights = new List<Game.Flight>() {
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 13,
                ShipCount = 2,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 15,
                ShipCount = 22,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 6,
                ShipCount = 3,
                TurnsToArrival = 0
                },
            };
            game.ResolveArrivingFlights(starId, flights);
            Assert.AreEqual(15, game.stars[starId].Data.Owner);
            Assert.AreEqual(1, game.stars[starId].Data.ShipCount);
        }

        [Test]
        public void TestFlightResolutionWhenOneTeamAttacksEnemyStarFailing0() {
            var starId = -1;
            for (int i = 0; i < game.stars.Count; i++) {
                if (game.stars[i].Data.Owner == 10) {
                    starId = i;
                    break;
                }
            }
            var flights = new List<Game.Flight>() {
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 3,
                ShipCount = 2,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 5,
                ShipCount = 15,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 16,
                ShipCount = 3,
                TurnsToArrival = 0
                },
            };
            game.ResolveArrivingFlights(starId, flights);
            Assert.AreEqual(10, game.stars[starId].Data.Owner);
            Assert.AreEqual(6, game.stars[starId].Data.ShipCount);
        }

        [Test]
        public void TestFlightResolutionWhenOneTeamAttacksEnemyStarFailing1() {
            var starId = -1;
            for (int i = 0; i < game.stars.Count; i++) {
                if (game.stars[i].Data.Owner == 0) {
                    starId = i;
                    break;
                }
            }
            var flights = new List<Game.Flight>() {
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 13,
                ShipCount = 2,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 15,
                ShipCount = 15,
                TurnsToArrival = 0
                },
                new Game.Flight() {
                FromStarId = 0,
                ToStarId = starId,
                Owner = 6,
                ShipCount = 3,
                TurnsToArrival = 0
                },
            };
            game.ResolveArrivingFlights(starId, flights);
            Assert.AreEqual(0, game.stars[starId].Data.Owner);
            Assert.AreEqual(6, game.stars[starId].Data.ShipCount);
        }
    }
}