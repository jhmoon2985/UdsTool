using System;
using System.Collections.Generic;
using UdsTool.Core.Base;
using UdsTool.Core.Interfaces;
using UdsTool.ViewModels;

namespace UdsTool.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<Type, ViewModelBase> _viewModels;
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CurrentViewModelChanged;

        public NavigationService()
        {
            _viewModels = new Dictionary<Type, ViewModelBase>();
        }

        public void NavigateTo<T>() where T : ViewModelBase
        {
            var type = typeof(T);
            if (_viewModels.ContainsKey(type))
            {
                CurrentViewModel = _viewModels[type];
            }
            else
            {
                throw new InvalidOperationException($"ViewModel of type {type.Name} is not registered.");
            }
        }

        public void RegisterViewModel<T>(T viewModel) where T : ViewModelBase
        {
            var type = typeof(T);
            if (!_viewModels.ContainsKey(type))
            {
                _viewModels[type] = viewModel;
            }
        }
    }
}