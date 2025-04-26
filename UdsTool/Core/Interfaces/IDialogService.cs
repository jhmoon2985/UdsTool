using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdsTool.Core.Interfaces
{
    public interface IDialogService
    {
        bool ShowOpenFileDialog(string title, string filter, out string selectedFilePath);
        bool ShowSaveFileDialog(string title, string filter, out string selectedFilePath);
        void ShowMessage(string message, string title = "Information");
        bool ShowConfirmation(string message, string title = "Confirmation");
    }
}
