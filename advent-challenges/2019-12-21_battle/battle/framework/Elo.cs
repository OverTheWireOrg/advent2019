using System;
using System.Diagnostics.Contracts;

namespace Framework {
    class Elo {
        public static double[] ComputeEloDelta(double[] currentRatings, double[] scores, int kFactor) {
            // only a team of two is supported right now.
            Contract.Requires(currentRatings.Length == 2);

            double[] expected_scores = new double[2];
            double rating_0 = currentRatings[0];
            double rating_1 = currentRatings[1];
            expected_scores[0] = 1.0 / (1.0 + Math.Pow(10, (rating_1 - rating_0) / 400));
            expected_scores[1] = 1.0 / (1.0 + Math.Pow(10, (rating_0 - rating_1) / 400));
            double[] ratingDelta = new double[2];
            for (int i = 0; i < 2; i++) {
                ratingDelta[i] += kFactor * (scores[i] - expected_scores[i]);
            }
            return ratingDelta;
        }
    }
}