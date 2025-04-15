using System;
using System.IO;
using System.Text.RegularExpressions;
using UiPathDiagnosticToolSimplified.Interfaces;
using UiPath.DiagnosticTool.Helpers;

namespace UiPath.DiagnosticTool.DataCollectors
{
    public class ConfigurationAndWebConfigCollector : IDataCollector
    {
        public string Name => "Configuration & Web Config Collector";
        public string RegistryKeyValue { get; set; } = "InstallDir";
        public string Orchestrator { get; set; } = "WebsiteHost";
        public string IdentityWebAppName { get; set; } = "IdentityWebAppName";
        public string WebhooksWebAppName { get; set; } = "WebhooksWebAppName";

        public void CollectData(string outputFolder)
        {
            const string registryPath = @"SOFTWARE\WOW6432Node\UiPath\UiPath Orchestrator";

            try
            {
                OrchestratorRegistryReader.Load(registryPath);
                var installDir = OrchestratorRegistryReader.GetValue(RegistryKeyValue);
                var OrchestratorWebName = OrchestratorRegistryReader.GetValue(Orchestrator);
                var IdentityWebName = OrchestratorRegistryReader.GetValue(IdentityWebAppName);
                var WebhooksWebName = OrchestratorRegistryReader.GetValue(WebhooksWebAppName);
                if (string.IsNullOrEmpty(installDir))
                    return;

                // === Orchestrator Files ===
                var orchestratorOutput = Path.Combine(outputFolder, OrchestratorWebName);
                Directory.CreateDirectory(orchestratorOutput);

                CopyFile(Path.Combine(installDir, "web.config"), Path.Combine(orchestratorOutput, "web.config"));
                CopyFile(Path.Combine(installDir, "UiPath.Orchestrator.dll.config"), Path.Combine(orchestratorOutput, "UiPath.Orchestrator.dll.config"));

                // === Identity Files ===
                var identityOutput = Path.Combine(outputFolder, IdentityWebName);
                Directory.CreateDirectory(identityOutput);

                var identityJson = Path.Combine(identityOutput, "appsettings.Production.json");
                CopyFile(Path.Combine(installDir, IdentityWebName, "web.config"), Path.Combine(identityOutput, "web.config"));
                CopyFile(Path.Combine(installDir, IdentityWebName, "appsettings.Production.json"), identityJson);
                MaskPasswordInConnectionString(identityJson);

                // === Webhooks Files ===
                var webhooksOutput = Path.Combine(outputFolder, WebhooksWebName);
                Directory.CreateDirectory(webhooksOutput);

                var webhooksJson = Path.Combine(webhooksOutput, "appsettings.Production.json");
                CopyFile(Path.Combine(installDir, WebhooksWebName, "web.config"), Path.Combine(webhooksOutput, "web.config"));
                CopyFile(Path.Combine(installDir, WebhooksWebName, "appsettings.Production.json"), webhooksJson);
                MaskPasswordInConnectionString(webhooksJson);

                // === IIS applicationhost.config File ===
                var appHostOutput = Path.Combine(outputFolder, "ApplicationHost");
                Directory.CreateDirectory(appHostOutput);
                CopyApplicationHostConfig(appHostOutput);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(outputFolder, "collector_exception.txt"), ex.ToString());
            }
        }

        private void CopyFile(string sourcePath, string destinationPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destinationPath, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(destinationPath + "_error.txt", ex.ToString());
            }
        }

        private void CopyApplicationHostConfig(string outputFolder)
        {
            try
            {
                var system32Path = Environment.SystemDirectory;
                var configPath = Path.Combine(system32Path, "inetsrv", "config", "applicationhost.config");

                if (File.Exists(configPath))
                {
                    var destinationPath = Path.Combine(outputFolder, "applicationhost.config");
                    File.Copy(configPath, destinationPath, overwrite: true);
                }
                else
                {
                    File.WriteAllText(Path.Combine(outputFolder, "applicationhost_missing.txt"), "applicationhost.config not found.");
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(outputFolder, "applicationhost_exception.txt"), ex.ToString());
            }
        }
        private void MaskPasswordInConnectionString(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;

                string json = File.ReadAllText(filePath);

                // Match and mask password in any connection string with "Password=..."
                string masked = Regex.Replace(json, @"(?<=User ID=)([^;]*)(?=;)", "******");
                masked = Regex.Replace(masked, @"(?<=Password=)([^;]*)(?=;)", "******");

                File.WriteAllText(filePath, masked);
            }
            catch (Exception ex)
            {
                File.WriteAllText(filePath + "_mask_error.txt", ex.ToString());
            }
        }

    }
}
