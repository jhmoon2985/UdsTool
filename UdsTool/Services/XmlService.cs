using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UdsTool.Core.Interfaces;
using UdsTool.Core.Models;

namespace UdsTool.Services
{
    public class XmlService : IXmlService
    {
        public async Task<UdsDatabase> LoadDatabaseAsync(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(UdsDatabase));
                    return await Task.Run(() => (UdsDatabase)serializer.Deserialize(fileStream));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load UDS database: {ex.Message}", ex);
            }
        }

        public async Task SaveDatabaseAsync(string filePath, UdsDatabase database)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(UdsDatabase));
                    await Task.Run(() => serializer.Serialize(fileStream, database));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save UDS database: {ex.Message}", ex);
            }
        }

        public async Task<IsoTpConfig> LoadIsoTpConfigAsync(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(IsoTpConfig));
                    return await Task.Run(() => (IsoTpConfig)serializer.Deserialize(fileStream));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load ISO-TP configuration: {ex.Message}", ex);
            }
        }

        public async Task SaveIsoTpConfigAsync(string filePath, IsoTpConfig config)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(IsoTpConfig));
                    await Task.Run(() => serializer.Serialize(fileStream, config));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save ISO-TP configuration: {ex.Message}", ex);
            }
        }

        public UdsDatabase CreateDefaultDatabase()
        {
            var database = new UdsDatabase();
            var serviceIds = database.ServiceIds;

            // Add some default services
            var diagnosticsSessionControl = new ServiceId
            {
                Name = "DiagnosticSessionControl",
                Description = "Diagnostic Session Control",
                Value = 0x10
            };

            // Add subfunctions to DiagnosticSessionControl
            diagnosticsSessionControl.Subfunctions.Add(new Subfunction
            {
                Name = "DefaultSession",
                Description = "Default Session",
                Value = 0x01
            });
            diagnosticsSessionControl.Subfunctions.Add(new Subfunction
            {
                Name = "ProgrammingSession",
                Description = "Programming Session",
                Value = 0x02
            });
            diagnosticsSessionControl.Subfunctions.Add(new Subfunction
            {
                Name = "ExtendedSession",
                Description = "Extended Diagnostic Session",
                Value = 0x03
            });

            serviceIds.Add(diagnosticsSessionControl);

            // Add ReadDataByIdentifier
            var readDataById = new ServiceId
            {
                Name = "ReadDataByIdentifier",
                Description = "Read Data By Identifier",
                Value = 0x22
            };

            // Add DIDs to ReadDataByIdentifier
            var vinDid = new DataId
            {
                Name = "VIN",
                Description = "Vehicle Identification Number",
                Value = 0xF1
            };
            readDataById.DataIds.Add(vinDid);

            var ecnDid = new DataId
            {
                Name = "ECUName",
                Description = "ECU Name",
                Value = 0xF2
            };
            readDataById.DataIds.Add(ecnDid);

            serviceIds.Add(readDataById);

            // Add WriteDataByIdentifier
            var writeDataById = new ServiceId
            {
                Name = "WriteDataByIdentifier",
                Description = "Write Data By Identifier",
                Value = 0x2E
            };
            writeDataById.DataIds.Add(new DataId
            {
                Name = "TestIdentifier",
                Description = "Test Identifier",
                Value = 0x01
            });

            serviceIds.Add(writeDataById);

            return database;
        }
    }
}