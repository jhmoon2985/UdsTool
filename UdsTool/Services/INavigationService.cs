using System;

namespace UdsTool.Services
{
    public class ViewChangedEventArgs : EventArgs
    {
        public object View { get; }
        public string ViewName { get; }

        public ViewChangedEventArgs(object view, string viewName)
        {
            View = view;
            ViewName = viewName;
        }
    }

    public interface INavigationService
    {
        void NavigateTo(string viewName);
        object CurrentView { get; }
        string CurrentViewName { get; }
        event EventHandler<ViewChangedEventArgs> ViewChanged;
    }
}