using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Framework {
    class RealGameSimulator : IGameSimulator {
        private readonly string _recordsDirectory;
        private readonly string _binaryDirectory;
        private readonly string _sandboxPath;
        private Random random = new Random();

        public RealGameSimulator(string recordsDirectory, string binaryDirectory, string sandboxPath) {
            _recordsDirectory = recordsDirectory;
            _binaryDirectory = binaryDirectory;
            _sandboxPath = sandboxPath;
        }

        public int NumPlayers => 2;

        public async Task<GameResults> Launch(GameLaunchParams p) {
            Simulator.GameRunner runner = new Simulator.GameRunner(p.Practice, _sandboxPath);
            var result = await runner.Run(p.Teams.Select(t => _binaryDirectory + "/" + t.BinaryPath).ToList(), random.Next());
            var ret = new GameResults();
            ret.Points.AddRange(result.Scores);

            var memoryStream = new MemoryStream();

            using(ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) {
                var entry = archive.CreateEntry("record");
                using(var entryStream = entry.Open()) {
                    var bytes = result.ToByteArray();
                    entryStream.Write(bytes, 0, bytes.Length);
                }
            }

            var serialized = memoryStream.ToArray();

            SHA256 sha = SHA256.Create();
            var filename = ByteArrayToString(sha.ComputeHash(serialized));
            var resultsFile = _recordsDirectory + "/" + filename;
            await File.WriteAllBytesAsync(resultsFile, serialized);
            new Mono.Unix.UnixFileInfo(resultsFile).FileAccessPermissions =
                Mono.Unix.FileAccessPermissions.UserRead |
                Mono.Unix.FileAccessPermissions.UserExecute;
            ret.ResultPath = filename;
            ret.NumTurns = result.Turns.Count;
            ret.NumStars.AddRange(result.NumStars);
            ret.CpuTime.AddRange(result.CpuTime);
            return ret;
        }

        // from stackoverflow.
        private static string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}