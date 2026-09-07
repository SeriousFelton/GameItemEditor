using System.Windows;
using GameItemEditor.Wpf.ViewModels;

namespace GameItemEditor.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            this.Loaded += async (s, e) =>
            {
                await viewModel.LoadItemsCommand.ExecuteAsync(null);
            };
        }
    }
}