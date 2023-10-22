using System.Windows;

namespace Yadex.Retirement.Views
{
    public partial class AssetDialog : Window
    {
        public AssetDialog()
        {
            InitializeComponent();
        }

        private AssetDialogViewModel ViewModel => DataContext as AssetDialogViewModel;

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

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            ViewModel.IsNew = true;
            ViewModel.AssetId = Guid.NewGuid();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var errors = ViewModel.DeleteViewModel();
            if (errors.Count > 0)
            {
                MessageBox.Show(this, string.Join("\n", errors), "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // succeeded
            MessageBox.Show("Delete successfully!");
            Close();
            ViewModel.Parent.RefreshViewModel();
        }

    }
}