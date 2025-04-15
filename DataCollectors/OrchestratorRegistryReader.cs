using System.Collections.Generic;
using Microsoft.Win32;

namespace UiPath.DiagnosticTool.Helpers
{
    public static class OrchestratorRegistryReader
    {
        private static readonly Dictionary<string, string> _values = new();

        public static void Load(string registryPath)
        {
            _values.Clear();

            using var key = Registry.LocalMachine.OpenSubKey(registryPath);
            if (key == null) return;

            foreach (var name in key.GetValueNames())
            {
                var value = key.GetValue(name)?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _values[name] = value;
                }
            }
        }

        public static string GetValue(string key) =>
            _values.TryGetValue(key, out var value) ? value : null;

        public static IReadOnlyDictionary<string, string> All => _values;
    }
}
