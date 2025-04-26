using System.Windows;
using UdsTool.Services;
using UdsTool.ViewModels;

namespace UdsTool.Views
{
    public partial class FrameEditDialog : Window, ICloseable
    {
        public FrameEditDialog()
        {
            InitializeComponent();
        }

        public void Close(bool? dialogResult)
        {
            this.DialogResult = dialogResult;
            this.Close();
        }
    }
}