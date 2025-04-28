using System;
using System.Collections.Generic;

namespace UdsTool.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Func<object>> _viewModelFactory;
        private object _currentView;
        private string _currentViewName;

        public NavigationService(Dictionary<string, Func<object>> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public object CurrentView => _currentView;
        public string CurrentViewName => _currentViewName;

        public event EventHandler<ViewChangedEventArgs> ViewChanged;

        public void NavigateTo(string viewName)
        {
            if (_viewModelFactory.TryGetValue(viewName, out var factory))
            {
                _currentView = factory();
                _currentViewName = viewName;
                ViewChanged?.Invoke(this, new ViewChangedEventArgs(_currentView, _currentViewName));
            }
        }
    }
}