// Views/XmlEditorView.xaml.cs
using System.Windows;
using System.Windows.Controls;
using UdsTool.Core.Models;
using UdsTool.ViewModels;

namespace UdsTool.Views
{
    public partial class XmlEditorView : UserControl
    {
        private XmlEditorViewModel ViewModel => DataContext as XmlEditorViewModel;

        public XmlEditorView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is UdsConfiguration message && ViewModel != null)
            {
                // TreeView에서 선택한 메시지를 ViewModel에 설정
                ViewModel.SelectedMessage = message;
            }
        }
    }
}