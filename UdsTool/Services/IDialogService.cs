using UdsTool.Models;

namespace UdsTool.Services
{
    public interface IDialogService
    {
        bool? ShowDialog(object viewModel);
    }
}