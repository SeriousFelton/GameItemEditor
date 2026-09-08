using System.Windows;
using GameItemEditor.Wpf.ViewModels;

namespace GameItemEditor.Wpf.Views
{
    public partial class ItemDialog : Window
    {
        private readonly ItemDialogViewModel _viewModel;
        public ItemDialog(ItemDialogViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.CloseDialog = CloseDialog;
        }

        private void CloseDialog(bool result)
        {
            DialogResult = result;
            Close();
        }
    }
}
