using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Simulator {
    class MapGenerator {
        public float DesiredDistance { get; set; } = 40;
        public float K { get; set; } = 0.3f;

        public int NumPoints { get; set; } = 90;

        public Vector2 MapSize { get; set; } = new Vector2(300, 300);

        private Random random;

        public MapGenerator(int seed) {
            random = new Random(seed);
        }

        public List<Vector2> GenerateMap() {
            var result = new List<Vector2>();
            while (result.Count < NumPoints) {
                Vector2? best = null;
                float bestDist = 0;
                for (int i = 0; i < 10; i++) {
                    var candidate = new Vector2((float) random.NextDouble() * MapSize.X, (float) random.NextDouble() * MapSize.Y);
                    float minDist = 1000000;
                    foreach (var point in result) {
                        minDist = Math.Min(minDist, (point - candidate).Length());
                    }
                    minDist = Math.Min(minDist, candidate.X);
                    minDist = Math.Min(minDist, candidate.Y);
                    minDist = Math.Min(minDist, MapSize.X - candidate.X);
                    minDist = Math.Min(minDist, MapSize.Y - candidate.Y);
                    if (best == null || minDist > bestDist) {
                        best = candidate;
                        bestDist = minDist;
                    }
                }
                result.Add(best.Value);
            }
            return result;
        }

        public List<Vector2> GenerateMap2() {
            var result = new List<Vector2>();
            for (int i = 0; i < NumPoints; i++) {
                result.Add(new Vector2((float) random.NextDouble() * MapSize.X, (float) random.NextDouble() * MapSize.Y));
            }

            for (int rounds = 0; rounds < 100; rounds++) {
                var links = new List < (int a, int b, double force) > ();
                for (int i = 0; i < NumPoints; i++) {
                    Vector2 my = result[i];
                    for (int j = i + 1; j < NumPoints; j++) {
                        Vector2 other = result[j];
                        if ((my - other).Length() > DesiredDistance * 3) {
                            continue;
                        }
                        links.Add((i, j, (DesiredDistance - (my - other).Length()) * K));
                    }
                    var(closestIndex, dist) = Enumerable.Range(0, NumPoints).Where(p => p != i).Select(p => (p, (result[p] - my).Length())).OrderBy(x => x.Item2).First();
                    if (dist > DesiredDistance * 3) {
                        links.Add((i, closestIndex, (DesiredDistance - dist) * K));
                    }
                }

                Vector2[] forces = new Vector2[NumPoints];
                foreach (var(i, j, force) in links) {
                    forces[i] += (result[j] - result[i]) / (result[j] - result[i]).Length() * (float) - force;
                    forces[j] += (result[i] - result[j]) / (result[i] - result[j]).Length() * (float) - force;
                }
                for (int i = 0; i < NumPoints; i++) {
                    if (result[i].X < DesiredDistance * 2) {
                        forces[i] += new Vector2((DesiredDistance - result[i].X) * K, 0);
                    }
                    if (result[i].Y < DesiredDistance * 2) {
                        forces[i] += new Vector2(0, (DesiredDistance - result[i].Y) * K);
                    }
                    if (MapSize.X - result[i].X < DesiredDistance * 2) {
                        forces[i] -= new Vector2((DesiredDistance - (MapSize.X - result[i].X)) * K, 0);
                    }
                    if (MapSize.Y - result[i].Y < DesiredDistance * 2) {
                        forces[i] -= new Vector2(0, (DesiredDistance - (MapSize.Y - result[i].Y)) * K);
                    }
                }
                for (int i = 0; i < NumPoints; i++) {
                    result[i] += forces[i];
                }
            }
            return result;
        }
    }
}