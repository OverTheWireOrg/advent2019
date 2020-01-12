using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blazor.FileReader;
using Microsoft.AspNetCore.Components;
using Npgsql;

namespace Web {
    partial class Upload {
        [Inject]
        private IFileReaderService fileReaderService { get; set; }

        [Inject]
        private WebConfigs webConfigs { get; set; }

        private string token = "";
        private ElementReference inputFileElement;
        private string error = "";
        private string message = "";
        private Random random = new Random();
        private bool uploading = false;

        private List<BinaryInfo> binaries = new List<BinaryInfo>();

        const int MAX_FILE_SIZE = 10 * 1024 * 1024;

        private async Task DoUpload() {
            if (uploading) return;
            uploading = true;
            try {
                await RealUpload();
            } finally {
                uploading = false;
            }
        }

        private async Task RealUpload() {
            await using var conn = await DbService.NewConnection();
            error = "";
            message = "";
            int teamId = await GetTeamId(token, conn);
            if (teamId == -1) {
                error = "Invalid team token.";
                return;
            }

            foreach (var file in await fileReaderService.CreateReference(inputFileElement).EnumerateFilesAsync()) {
                var fileInfo = await file.ReadFileInfoAsync();
                var totalSize = fileInfo.Size;

                string fileName = webConfigs.BinariesDir + "/" + RandomFileName();
                FileStream output = new FileStream(fileName, FileMode.CreateNew);
                try {
                    // Do not enlarge this buffer. See https://github.com/Tewr/BlazorFileReader.
                    byte[] buf = new byte[16384];
                    await using(Stream stream = await file.OpenReadAsync()) {
                        while (true) {
                            int len = await stream.ReadAsync(buf, 0, 16384);
                            if (len == 0) {
                                await output.DisposeAsync();
                                output = null;
                                await CommitFile(fileName, teamId, conn);
                                message = "Upload succeeded.";
                                return;
                            }
                            await output.WriteAsync(buf, 0, len);
                            if (output.Length >= MAX_FILE_SIZE) {
                                error = "File is too large. The maximum size limit is " + MAX_FILE_SIZE + " bytes.";
                                return;
                            }
                            message = $"Uploading... {Math.Round((double) output.Length / totalSize * 100)}%";
                            StateHasChanged();
                        }
                    }
                } finally {
                    if (output != null) {
                        await output.DisposeAsync();
                        File.Delete(fileName);
                    }
                }
            }
            error = "Please choose a file to upload";
            return;
        }

        private async Task<int> GetTeamId(string token, NpgsqlConnection conn) {
            await using var cmd = new NpgsqlCommand("select id from Team where token = @token", conn);
            cmd.Parameters.AddWithValue("token", token);
            var result = await cmd.ExecuteScalarAsync();
            if (result is int) {
                return (int) result;
            }
            return -1;
        }

        private async Task CommitFile(string fileName, int teamId, NpgsqlConnection conn) {
            SHA256 sha = SHA256.Create();
            string hash;

            await using(var contents = new FileStream(fileName, FileMode.Open)) {
                hash = ByteArrayToString(sha.ComputeHash(contents));
            }
            var targetPath = webConfigs.BinariesDir + "/" + hash;
            if (File.Exists(targetPath)) {
                File.Delete(fileName);
            } else {
                File.Move(fileName, targetPath);
                new Mono.Unix.UnixFileInfo(targetPath).FileAccessPermissions =
                    Mono.Unix.FileAccessPermissions.UserRead |
                    Mono.Unix.FileAccessPermissions.UserExecute;
            }

            Int64 unixTimestamp = (Int64) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            await using var cmd = new NpgsqlCommand("insert into Binaries (team, binary_path, upload_time) values (@id, @binary, @time)", conn);
            cmd.Parameters.AddWithValue("id", teamId);
            cmd.Parameters.AddWithValue("binary", hash);
            cmd.Parameters.AddWithValue("time", unixTimestamp);
            await cmd.ExecuteNonQueryAsync();

            await DoCheckCurrent();
        }

        private async Task CheckCurrent() {
            error = "";
            message = "";
            await DoCheckCurrent();
        }

        private async Task DoCheckCurrent() {
            await using var conn = await DbService.NewConnection();
            binaries.Clear();
            int teamId = await GetTeamId(token, conn);
            if (teamId == -1) {
                error = "Invalid team token.";
                return;
            }

            await using var cmd = new NpgsqlCommand("select binary_path, validation_result_path, validation_passed, upload_time from Binaries where team = @id order by upload_time desc", conn);
            cmd.Parameters.AddWithValue("id", teamId);
            await using var reader = await cmd.ExecuteReaderAsync();
            bool first = true;
            message = "You have not uploaded a binary.";
            while (await reader.ReadAsync()) {
                binaries.Add(new BinaryInfo() {
                    Sha256 = reader.GetString(0),
                        ResultPath = reader.GetValue(1) is string ? reader.GetString(1) : "",
                        ValidationPassed = reader.GetBoolean(2),
                        UploadTime = new DateTime(1970, 1, 1).Add(TimeSpan.FromMilliseconds(reader.GetInt64(3)))
                });
                if (first) {
                    if (reader.GetBoolean(2)) {
                        first = false;
                        message = $"The latest validated binary you have uploaded is {reader.GetString(0)}. It will be used in the next round.";
                    } else {
                        message = $"You have not uploaded a binary that passed basic validation.";
                    }
                }
            }
        }

        private string RandomFileName() {
            byte[] buf = new byte[16];
            random.NextBytes(buf);
            return ByteArrayToString(buf);
        }

        private static string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }

    class BinaryInfo {
        public string Sha256 { get; set; } = "";
        public string ResultPath { get; set; } = "";
        public Boolean ValidationPassed { get; set; }
        public DateTime UploadTime { get; set; }
    }
}