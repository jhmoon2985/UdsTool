using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using UdsTool.Models;
using System.Linq;

namespace UdsTool.Services
{
    public class XmlService : IXmlService
    {
        public void SaveToFile(string filePath, ObservableCollection<DiagnosticFrame> frames)
        {
            try
            {
                // 저장하기 전에 순서대로 정렬
                var orderedFrames = new ObservableCollection<DiagnosticFrame>(
                    frames.OrderBy(frame => frame.Idx).ToList());

                var serializer = new XmlSerializer(typeof(ObservableCollection<DiagnosticFrame>));
                using (var writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, orderedFrames);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving XML file: {ex.Message}");
            }
        }

        public ObservableCollection<DiagnosticFrame> LoadFromFile(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<DiagnosticFrame>));
                using (var reader = new StreamReader(filePath))
                {
                    var loadedFrames = (ObservableCollection<DiagnosticFrame>)serializer.Deserialize(reader);

                    // 각 프레임마다 필요한 초기화 작업 수행
                    foreach (var frame in loadedFrames)
                    {
                        // Children 컬렉션이 없으면 초기화
                        if (frame.Children == null)
                        {
                            frame.Children = new ObservableCollection<DiagnosticFrame>();
                        }
                    }

                    // ResponseIdx 기반으로 Request-Response 관계 복원은 ViewModel에서 처리
                    return loadedFrames;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading XML file: {ex.Message}");
            }
        }

        public string SerializeToXml(ObservableCollection<DiagnosticFrame> frames)
        {
            try
            {
                // 문자열 변환 전에 순서대로 정렬
                var orderedFrames = new ObservableCollection<DiagnosticFrame>(
                    frames.OrderBy(frame => frame.Idx).ToList());

                var serializer = new XmlSerializer(typeof(ObservableCollection<DiagnosticFrame>));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, orderedFrames);
                    return writer.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error serializing to XML: {ex.Message}");
            }
        }
    }
}