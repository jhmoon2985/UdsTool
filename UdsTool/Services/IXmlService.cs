using System.Collections.ObjectModel;
using UdsTool.Models;

namespace UdsTool.Services
{
    public interface IXmlService
    {
        void SaveToFile(string filePath, ObservableCollection<DiagnosticFrame> frames);
        ObservableCollection<DiagnosticFrame> LoadFromFile(string filePath);
        string SerializeToXml(ObservableCollection<DiagnosticFrame> frames);
    }
}