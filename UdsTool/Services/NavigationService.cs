using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdsTool.Services
{
    public class NavigationService : INavigationService
    {
        public event EventHandler<string> NavigationRequested;

        public void NavigateTo(string viewName)
        {
            NavigationRequested?.Invoke(this, viewName);
        }
    }
}
