using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UdsTool.Core.Base;
using UdsTool.Core.Models;
using UdsTool.ViewModels;

namespace UdsTool.Core.Interfaces
{

    public interface INavigationService
    {
        ViewModelBase CurrentViewModel { get; }
        void NavigateTo<T>() where T : ViewModelBase;
        void RegisterViewModel<T>(T viewModel) where T : ViewModelBase;
    }
}