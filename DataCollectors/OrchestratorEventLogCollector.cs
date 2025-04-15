using System;
using System.Diagnostics;
using System.IO;
using UiPathDiagnosticToolSimplified.Interfaces;

namespace UiPathDiagnosticToolSimplified.DataCollectors
{
    public class OrchestratorEventLogCollector : IDataCollector
    {
        public string Name => "Orchestrator Event Viewer";


        public enum LogsCollected
        {
            Application,
            System,
            Bits
        }

        [Flags]
        public enum EventLevel
        {
            None = 0,
            Verbose = 1,
            Information = 2,
            Warning = 4,
            Error = 8,
            Critical = 16,
            All = 31,
        }

        // Example usage of enums (can later be bound to UI)
        public LogsCollected LogType { get; set; } = LogsCollected.Application;
        public EventLevel LogLevel { get; set; } = EventLevel.All;

        public void CollectData(string outputPath)
        {
            Directory.CreateDirectory(outputPath);

            string logName = GetLogName(LogType);
            string exportFilePath = Path.Combine(outputPath, $"{logName}.evtx");

            var wevtutilPath = Environment.ExpandEnvironmentVariables(@"%WINDIR%\\System32\\wevtutil.exe");
            string arguments = $"epl \"{logName}\" \"{exportFilePath}\"";

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = wevtutilPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        string error = process.StandardError.ReadToEnd();
                        File.WriteAllText(Path.Combine(outputPath, "error.txt"), $"Failed to export logs:\n{error}");
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(outputPath, "exception.txt"), ex.ToString());
            }
        }

        private string GetLogName(LogsCollected type)
        {
            return type switch
            {
                LogsCollected.Application => "Application",
                LogsCollected.System => "System",
                LogsCollected.Bits => "Microsoft-Windows-Bits-Client/Operational",
                _ => "Application"
            };
        }
    }
}
