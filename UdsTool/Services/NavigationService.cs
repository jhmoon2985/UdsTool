using System;
using System.Collections.Generic;
using UdsTool.ViewModels;

namespace UdsTool.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Func<object>> _viewModelFactory;
        private object _currentView;

        public NavigationService(Dictionary<string, Func<object>> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public object CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                ViewChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<object> ViewChanged;

        public void NavigateTo(string viewName)
        {
            if (_viewModelFactory.TryGetValue(viewName, out var factory))
            {
                CurrentView = factory();
            }
        }
    }
}