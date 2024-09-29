using LayoutDesigner.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace LayoutDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var dataContext = new LayoutDesignerViewModel();
            this.DataContext = dataContext;
            dataContext.LoadJsonProperties(@"c:\LayoutDesigner\properties.json");
            ((LayoutDesignerViewModel)DataContext).PropertyChanged += ViewModel_PropertyChanged;

        }
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var viewModel = (LayoutDesignerViewModel)DataContext;
            if (e.PropertyName == nameof(viewModel.PrintPreviewText))
            {
                RightTextField.Document = viewModel.PrintPreviewText;
            }
        }

        private void PrintButton(object sender, RoutedEventArgs e)
        {
            LeftTextField.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            if (DataContext is LayoutDesignerViewModel viewModel && viewModel.PrintButton.CanExecute(null))
            {
                viewModel.PrintButton.Execute(null);
            }
        }
    }
}