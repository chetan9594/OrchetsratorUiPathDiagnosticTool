using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System;
using System.Linq;

using UiPathDiagnosticToolSimplified.DataCollectors;
using UiPathDiagnosticToolSimplified.Interfaces;


namespace UiPathDiagnosticToolSimplified.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<IDataCollector> Collectors { get; } = new();

    private bool _isIISLogsSelected;
    public bool IsIISLogsSelected
    {
        get => _isIISLogsSelected;
        set
        {
            if (_isIISLogsSelected != value)
            {
                _isIISLogsSelected = value;
                OnPropertyChanged(nameof(IsIISLogsSelected));
            }
        }
    }

    private int _numberOfDays = 1;
    public int NumberOfDays
    {
        get => _numberOfDays;
        set
        {
            if (_numberOfDays != value)
            {
                _numberOfDays = value;
                OnPropertyChanged(nameof(NumberOfDays));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public MainWindowViewModel()
    {
        LoadCollectors();
    }

    private void LoadCollectors()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(IDataCollector).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is IDataCollector collector)
            {
                Collectors.Add(collector);
            }
        }
    }

    public void RunSelectedCollectors(IEnumerable<IDataCollector> selectedCollectors)
    {
        string basePath = "C:\\Orchestrator_Logs";

        if (Directory.Exists(basePath))
            Directory.Delete(basePath, recursive: true);
        Directory.CreateDirectory(basePath);

        foreach (var collector in selectedCollectors)
        {
            string collectorPath = Path.Combine(basePath, collector.Name.Replace(" ", ""));
            Directory.CreateDirectory(collectorPath);

            if (collector.Name == "IIS Logs" && collector is IISlogs iisLogs)
            {
                iisLogs.CollectData(collectorPath, NumberOfDays);
            }
            else
            {
                collector.CollectData(collectorPath);
            }
        }

        MessageBox.Show("Data Exported");
    }
}
