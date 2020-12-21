﻿using System.Windows;
using System.Windows.Input;
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
                {Owner = this, DataContext = new AssetDialogViewModel(ViewModel, ViewModel.AssetSelected)};
            assetDialog.ShowDialog();
        }
    }
}