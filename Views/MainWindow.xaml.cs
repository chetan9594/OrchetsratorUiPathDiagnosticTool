using System.Linq;
using System.Windows;
using System.Windows.Input;
using UiPathDiagnosticToolSimplified.Interfaces;
using UiPathDiagnosticToolSimplified.ViewModels;

namespace UiPathDiagnosticToolSimplified.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RunSelectedCollectors_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                var selectedCollectors = CollectorsList.SelectedItems
                    .OfType<IDataCollector>()
                    .ToList();

                if (selectedCollectors.Any())
                {
                    vm.RunSelectedCollectors(selectedCollectors);
                }
                else
                {
                    MessageBox.Show("Please select at least one collector.");
                }
            }
        }

        private void CollectorsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                var selectedCollectors = CollectorsList.SelectedItems
                    .OfType<IDataCollector>()
                    .ToList();

                vm.IsIISLogsSelected = selectedCollectors.Any(c => c.Name == "IIS Logs");
            }
        }


        private void NumericOnly(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
