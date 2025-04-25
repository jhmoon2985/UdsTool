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
        public async Task<UdsXmlConfig> LoadConfigurationAsync(string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(UdsXmlConfig));
                    var config = (UdsXmlConfig)serializer.Deserialize(stream);
                    config.FilePath = filePath;
                    return config;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"XML 파일 로드 중 오류 발생: {ex.Message}", ex);
            }
        }

        public async Task SaveConfigurationAsync(UdsXmlConfig configuration, string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(UdsXmlConfig));
                    serializer.Serialize(stream, configuration);
                    configuration.FilePath = filePath;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"XML 파일 저장 중 오류 발생: {ex.Message}", ex);
            }
        }

        public UdsXmlConfig CreateDefaultConfiguration()
        {
            var config = new UdsXmlConfig();

            // 기본 SID 추가 (예시)
            var readDataSid = new UdsSid
            {
                Id = "0x22",
                Name = "ReadDataByIdentifier",
                Description = "데이터 ID를 통해 데이터 읽기"
            };

            var writeDataSid = new UdsSid
            {
                Id = "0x2E",
                Name = "WriteDataByIdentifier",
                Description = "데이터 ID를 통해 데이터 쓰기"
            };

            var diagnosticControlSid = new UdsSid
            {
                Id = "0x10",
                Name = "DiagnosticSessionControl",
                Description = "진단 세션 제어"
            };

            // Subfunction 예시 추가
            var defaultSessionSubfunc = new UdsSubfunction
            {
                Id = "0x01",
                Name = "DefaultSession",
                Description = "기본 진단 세션"
            };

            var extendedSessionSubfunc = new UdsSubfunction
            {
                Id = "0x02",
                Name = "ExtendedSession",
                Description = "확장 진단 세션"
            };

            diagnosticControlSid.Subfunctions.Add(defaultSessionSubfunc);
            diagnosticControlSid.Subfunctions.Add(extendedSessionSubfunc);

            // 샘플 DID 추가
            var vinDid = new UdsDid
            {
                Id = "0xF190",
                Name = "VIN",
                Description = "Vehicle Identification Number"
            };

            readDataSid.Subfunctions.Add(new UdsSubfunction
            {
                Id = "ReadVIN",
                Name = "Vehicle ID",
                Description = "차량 ID 정보 읽기"
            });

            // 기본 구성에 추가
            config.Services.Add(diagnosticControlSid);
            config.Services.Add(readDataSid);
            config.Services.Add(writeDataSid);

            return config;
        }
    }
}
