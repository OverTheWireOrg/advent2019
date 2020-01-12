using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Simulator {
    public interface IProcess {
        Task < (bool, List<string>) > ReadMessageAsync();
        Task<bool> WriteMessageAsync(List<string> data, bool sendDone);
        void Stop();
        bool Failed { get; }
        string FailureMessage { get; }
    }

    public class StreamProcess : IProcess {
        private Process _process;
        private StreamReader _out;
        private StreamWriter _in;
        private bool failed = false;
        private String failureMessage = "";
        private readonly int _id;

        public StreamProcess(Func<Process> process, int id) {
            try {
                _process = process();
                _out = _process.StandardOutput;
                _in = _process.StandardInput;
                _process.StandardError.Close();
                _id = id;
            } catch (Exception e) {
                MarkAsFailed(e.Message);
            }
        }

        private void MarkAsFailed(string message) {
            if (!failed) {
                failed = true;
                failureMessage = message;
                if (_process != null) {
                    _process.Kill(true);
                    _process.Dispose();
                }
            }
        }

        public async Task < (bool, List<string>) > ReadMessageAsync() {
            if (failed) {
                return (false, new List<string>());
            }
            var result = new List<string>();
            try {
                while (true) {
                    var readTask = _out.ReadLineAsync();
                    var timeoutTask = Task.Delay(2000);
                    var completedTask = await Task.WhenAny(readTask, timeoutTask);
                    if (completedTask == timeoutTask) {
                        MarkAsFailed("Your AI failed to provide an answer within 2 seconds of walltime. A likely cause is your AI is trying to read more inputs from stdin.");
                        return (false, new List<string>());
                    }
                    var line = await readTask;
                    if (line == null) {
                        MarkAsFailed("Process terminated before turn decision is emitted; this could be due to exceeding resource limits, crashing, or issuing a disallowed syscall, or simply exiting.");
                        return (false, new List<string>());
                    }
                    if (line != "done") {
                        if (!line.StartsWith("fly ")) {
                            MarkAsFailed("Invalid line written to stdout: " + line);
                            return (false, new List<string>());
                        }
                        result.Add(line);
                    } else {
                        return (true, result);
                    }
                }
            } catch (IOException) {
                MarkAsFailed("Process terminated before turn decision is emitted; this could be due to exceeding resource limits, crashing, or issuing a disallowed syscall, or simply exiting.");
                return (false, new List<string>());
            }
        }

        public void Stop() {
            MarkAsFailed("");
        }

        public bool Failed {
            get {
                return failed;
            }
        }
        public string FailureMessage {
            get {
                return failureMessage;
            }
        }

        public async Task<bool> WriteMessageAsync(List<string> data, bool sendDone) {
            if (failed) {
                return false;
            }
            try {
                foreach (string line in data) {
                    await _in.WriteLineAsync(line);
                }
                if (sendDone) {
                    await _in.WriteLineAsync("done");
                }
                await _in.FlushAsync();
                return true;
            } catch (IOException) {
                MarkAsFailed("Process terminated before it could read turn input; this could be due to exceeding resource limits, crashing, or issuing a disallowed syscall, or simply exiting.");
                return false;
            }
        }
    }
}