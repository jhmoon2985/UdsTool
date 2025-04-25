using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UdsTool.Models;

namespace UdsTool.Services
{
    public class XmlService : IXmlService
    {
        private readonly XmlSerializer _serializer;

        public XmlService()
        {
            _serializer = new XmlSerializer(typeof(UdsConfiguration));
        }

        public async Task<UdsConfiguration> LoadConfigurationAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Configuration file not found", filePath);

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return await Task.Run(() => (UdsConfiguration)_serializer.Deserialize(stream));
            }
        }

        public async Task SaveConfigurationAsync(UdsConfiguration configuration, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await Task.Run(() =>
                {
                    _serializer.Serialize(stream, configuration);
                    return Task.CompletedTask;
                });
            }
        }

        public string SerializeToString(UdsConfiguration configuration)
        {
            using (var writer = new StringWriter())
            {
                _serializer.Serialize(writer, configuration);
                return writer.ToString();
            }
        }

        public UdsConfiguration DeserializeFromString(string xmlContent)
        {
            using (var reader = new StringReader(xmlContent))
            {
                return (UdsConfiguration)_serializer.Deserialize(reader);
            }
        }
    }
}
