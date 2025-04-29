using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using UdsTool.Models;

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
                    serializer.Serialize(writer, frames);
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
                    return (ObservableCollection<DiagnosticFrame>)serializer.Deserialize(reader);
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
                    serializer.Serialize(writer, frames);
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