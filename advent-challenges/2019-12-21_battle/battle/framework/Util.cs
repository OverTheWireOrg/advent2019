using System;

namespace Framework {
    public static class Util {
        public static DateTime UnixMillisToDateTime(this long timestamp) {
            return new DateTime(1970, 1, 1).Add(TimeSpan.FromMilliseconds(timestamp));
        }

        public static long ToUnixMillis(this DateTime time) {
            return (Int64) (time.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }
    }
}