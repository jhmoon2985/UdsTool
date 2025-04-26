using System.Windows.Controls;
using UdsTool.ViewModels;

namespace UdsTool.Views
{
    public partial class XmlEditorView : UserControl
    {
        public XmlEditorView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is XmlEditorViewModel viewModel)
            {
                viewModel.SelectedFrame = e.NewValue as Models.DiagnosticFrame;
            }
        }
    }
}