using GameItemEditor.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && viewModel.SelectedItem != null)
            {
                viewModel.EditItemCommand.Execute(viewModel.SelectedItem);
            }
        }
        private void DataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Если кликнули не по строке — снимаем выделение
            var grid = sender as DataGrid;
            if (grid != null)
            {
                var hit = VisualTreeHelper.HitTest(grid, e.GetPosition(grid));
                if (hit == null || hit.VisualHit == null)
                {
                    if (DataContext is MainViewModel viewModel)
                    {
                        viewModel.SelectedItem = null;
                    }
                }
                else
                {
                    // Проверяем, что клик был не по строке
                    var row = FindVisualParent<DataGridRow>(hit.VisualHit);
                    if (row == null)
                    {
                        if (DataContext is MainViewModel viewModel)
                        {
                            viewModel.SelectedItem = null;
                        }
                    }
                }
            }
        }

        // Вспомогательный метод для поиска родительского элемента
        private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}