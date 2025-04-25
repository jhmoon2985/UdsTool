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
        Task<UdsConfiguration> LoadConfigurationAsync(string filePath);
        Task SaveConfigurationAsync(UdsConfiguration configuration, string filePath);
        string SerializeToString(UdsConfiguration configuration);
        UdsConfiguration DeserializeFromString(string xmlContent);
    }
}
