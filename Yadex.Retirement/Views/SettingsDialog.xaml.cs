using System;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace Yadex.Retirement.Views
{
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private SettingsDialogViewModel ViewModel => DataContext as SettingsDialogViewModel;

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var errors = ViewModel.SaveViewModel();

            if (errors.Count > 0)
            {
                MessageBox.Show(this, string.Join("\n", errors), "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // succeeded
            MessageBox.Show("Save successfully!");
            Close();
            ViewModel.Parent.RefreshViewModel();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnChangePathClick(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.AssetRootFolder = dlg.SelectedPath;
            }
        }
    }
}