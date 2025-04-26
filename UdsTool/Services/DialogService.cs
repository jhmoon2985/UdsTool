using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdsTool.Core.Interfaces;

namespace UdsTool.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowOpenFileDialog(string title, string filter, out string selectedFilePath)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = title,
                Filter = filter
            };

            var result = dialog.ShowDialog() == true;
            selectedFilePath = result ? dialog.FileName : string.Empty;
            return result;
        }

        public bool ShowSaveFileDialog(string title, string filter, out string selectedFilePath)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = title,
                Filter = filter
            };

            var result = dialog.ShowDialog() == true;
            selectedFilePath = result ? dialog.FileName : string.Empty;
            return result;
        }

        public void ShowMessage(string message, string title = "Information")
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK);
        }

        public bool ShowConfirmation(string message, string title = "Confirmation")
        {
            return System.Windows.MessageBox.Show(message, title,
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;
        }
    }
}
