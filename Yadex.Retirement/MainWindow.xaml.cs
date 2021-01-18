using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Yadex.Retirement.Common;
using Yadex.Retirement.Views;

namespace Yadex.Retirement
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = ViewModel = new MainWindowViewModel();
        }

        public MainWindowViewModel ViewModel { get; init; }

        private void AddAssetButton_Click(object sender, RoutedEventArgs e)
        {
            var assetDialog = new AssetDialog {Owner = this, DataContext = new AssetDialogViewModel(ViewModel)};
            assetDialog.ShowDialog();
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var assetDialog = new AssetDialog
                {Owner = this, DataContext = new AssetDialogViewModel(ViewModel, ViewModel.AssetSelected.Asset)};
            assetDialog.ShowDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsDialog
                { Owner = this, DataContext = new SettingsDialogViewModel(ViewModel) };
            settingsDialog.ShowDialog();
            
            ViewModel.RefreshViewModel();
        }

        private void SaveCsv_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) 
                return;
            
            var path = Path.Combine(dlg.SelectedPath, $"Yadex Forecast {DateTime.Now:yyyy-MM-dd}.csv");

            var x = 0;
            while (File.Exists(path))
                path = $"Yadex Forecast {DateTime.Now:yyyy-MM-dd} ({++x}).csv";

            File.WriteAllText(path, DataGridHelper.ConvertToCsv(ForecastGrid));
        }
    }
}