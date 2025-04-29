using System;
using System.Collections.Generic;
using UdsTool.ViewModels;

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
            // 현재 화면이 XmlEditorViewModel이면 저장 확인
            if (_currentView is XmlEditorViewModel xmlEditorViewModel)
            {
                // 저장 확인이 취소되었으면 화면 전환 취소
                if (!xmlEditorViewModel.PromptSaveIfDirty())
                {
                    return;
                }
            }

            if (_viewModelFactory.TryGetValue(viewName, out var factory))
            {
                // 이미 생성된 ViewModel 인스턴스 사용 (싱글톤 패턴)
                _currentView = factory();
                _currentViewName = viewName;
                ViewChanged?.Invoke(this, new ViewChangedEventArgs(_currentView, _currentViewName));
            }
        }
    }
}