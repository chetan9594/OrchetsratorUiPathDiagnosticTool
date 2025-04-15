using System.IO;
using System.Text;
using UiPathDiagnosticToolSimplified.Interfaces;
using UiPath.DiagnosticTool.Helpers;
using System;

namespace UiPath.DiagnosticTool.DataCollectors
{
    public sealed class OrchestratorRegistryCollector : IDataCollector
    {
        public string Name => "Orchestrator Registry Collector";

        public void CollectData(string outputFolder)
        {
            const string registryPath = @"SOFTWARE\WOW6432Node\UiPath\UiPath Orchestrator";
            var sb = new StringBuilder();

            sb.AppendLine("Registry dump for: UiPath Orchestrator");
            sb.AppendLine($"Registry Path: {registryPath}");
            sb.AppendLine(new string('-', 60));

            try
            {
                OrchestratorRegistryReader.Load(registryPath);

                foreach (var pair in OrchestratorRegistryReader.All)
                {
                    sb.AppendLine($"{pair.Key}: {pair.Value}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Failed to read registry:");
                sb.AppendLine(ex.ToString());
            }

            var filePath = Path.Combine(outputFolder, "OrchestratorRegistry.txt");
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
