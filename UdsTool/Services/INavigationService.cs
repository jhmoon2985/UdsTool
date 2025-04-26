namespace UdsTool.Services
{
    public interface INavigationService
    {
        void NavigateTo(string viewName);
        object CurrentView { get; }
        event EventHandler<object> ViewChanged;
    }
}