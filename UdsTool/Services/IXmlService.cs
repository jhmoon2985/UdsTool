using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdsTool.Models;

namespace UdsTool.Services
{
    public interface IXmlService
    {
        Task<UdsXmlConfig> LoadConfigurationAsync(string filePath);
        Task SaveConfigurationAsync(UdsXmlConfig configuration, string filePath);
        UdsXmlConfig CreateDefaultConfiguration();
    }
}
