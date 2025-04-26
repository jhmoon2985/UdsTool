using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UdsTool.Core.Models;
using UdsTool.ViewModels;

namespace UdsTool.Core.Interfaces
{
    public interface IXmlService
    {
        Task<UdsDatabase> LoadDatabaseAsync(string filePath);
        Task SaveDatabaseAsync(string filePath, UdsDatabase database);
        Task<IsoTpConfig> LoadIsoTpConfigAsync(string filePath);
        Task SaveIsoTpConfigAsync(string filePath, IsoTpConfig config);
        UdsDatabase CreateDefaultDatabase();
    }
}