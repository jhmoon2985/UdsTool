using System.Windows;
using UdsTool.Services;
using UdsTool.ViewModels;
using UdsTool.Views;

public class DialogService : IDialogService
{
    public bool? ShowDialog(object viewModel)
    {
        Window dialog = null;

        if (viewModel is FrameEditDialogViewModel frameEditViewModel)
        {
            dialog = new FrameEditDialog();
            frameEditViewModel.SetWindow(dialog as ICloseable);
        }

        if (dialog != null)
        {
            dialog.DataContext = viewModel;

            // 현재 활성화된 윈도우를 owner로 설정
            if (Application.Current != null)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.IsActive)
                    {
                        dialog.Owner = window;
                        break;
                    }
                }
            }

            return dialog.ShowDialog();
        }

        return false;
    }
}