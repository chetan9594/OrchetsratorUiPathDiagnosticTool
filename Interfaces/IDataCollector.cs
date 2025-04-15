using System.Collections.Generic;

namespace UiPathDiagnosticToolSimplified.Interfaces
{
    public interface IDataCollector
    {
        string Name { get; }
        void CollectData(string outputPath);

       
    }
}