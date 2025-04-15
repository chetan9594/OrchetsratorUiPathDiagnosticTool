using System;
using System.IO;
using System.Management;
using UiPathDiagnosticToolSimplified.Interfaces;

namespace UiPathDiagnosticToolSimplified.DataCollectors
{
    public class EnvironmentVariablesCollector : IDataCollector
    {
        public string Name => "Environment Variables";

        public void CollectData(string outputPath)
        {
            Directory.CreateDirectory(outputPath);
            var destinationPath = Path.Combine(outputPath, "EnvironmentVariables.txt");

            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT Name, VariableValue, UserName FROM Win32_Environment WHERE " +
                    "Name LIKE 'UIPATH_%' OR Name = 'PATH' OR Name = 'HTTP_PROXY' OR Name = 'HTTPS_PROXY'"))
                {
                    using (var writer = new StreamWriter(destinationPath))
                    {
                        foreach (var envVar in searcher.Get())
                        {
                            writer.WriteLine($"Name: {envVar["Name"]}");
                            writer.WriteLine($"Value: {envVar["VariableValue"]}");
                            writer.WriteLine($"User: {envVar["UserName"]}");
                            writer.WriteLine(new string('-', 50));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(outputPath, "error.txt"), ex.ToString());
            }
        }
    }
}
