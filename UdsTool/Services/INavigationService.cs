using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdsTool.Services
{
    public interface INavigationService
    {
        event EventHandler<string> NavigationRequested;
        void NavigateTo(string viewName);
    }
}
