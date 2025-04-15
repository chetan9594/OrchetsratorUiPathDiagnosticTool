using System;
using System.IO;
using Microsoft.Web.Administration;
using UiPathDiagnosticToolSimplified.Interfaces;

namespace UiPathDiagnosticToolSimplified.DataCollectors
{
    public class IISlogs : IDataCollector
    {
        public string Name => "IIS Logs";

        public void CollectData(string outputPath)
        {
       
        }

        public void CollectData(string outputPath, int daysBack)
        {
            Directory.CreateDirectory(outputPath);

            try
            {
                using (var serverManager = new ServerManager())
                {
                    foreach (var site in serverManager.Sites)
                    {
                        if (site.Name != "UiPath Orchestrator1")
                            continue;

                        var logDirectory = Environment.ExpandEnvironmentVariables(site.LogFile.Directory);
                        if (!Directory.Exists(logDirectory))
                            continue;

                        var siteOutputPath = Path.Combine(outputPath, site.Name);
                        Directory.CreateDirectory(siteOutputPath);

                        var cutoffDate = DateTime.Now.AddDays(-daysBack);

                        foreach (var file in Directory.GetFiles(logDirectory, "*.log", SearchOption.AllDirectories))
                        {
                            if (File.GetLastWriteTime(file) >= cutoffDate)
                            {
                                var fileName = Path.GetFileName(file);
                                var destPath = Path.Combine(siteOutputPath, fileName);
                                File.Copy(file, destPath, overwrite: true);
                            }
                        }

                        Console.WriteLine($"Collected logs for {site.Name} (last {daysBack} days)");
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
