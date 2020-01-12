using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Simulator {
    class Program {
        static async Task Main(string[] args) {
            if (args.Length != 3 && args.Length != 4) {
                Console.WriteLine("Usage: simulator <binary1> <binary2> <output> [random_seed]");
                return;
            }
            var runner = new GameRunner(true, null);
            // Seed can be provided to help debug issues by deterministically reproducing a game.
            int seed = args.Length == 4 ? int.Parse(args[3]) : new Random().Next();
            var result = await runner.Run(args.Take(2).ToList(), seed);
            var memoryStream = new MemoryStream();

            using(ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) {
                var entry = archive.CreateEntry("record");
                using(var entryStream = entry.Open()) {
                    var bytes = result.ToByteArray();
                    entryStream.Write(bytes, 0, bytes.Length);
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            await using var fs = new FileStream(args[2], FileMode.Create);
            await memoryStream.CopyToAsync(fs);
        }
    }
}