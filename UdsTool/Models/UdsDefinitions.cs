using System.Collections.Generic;

namespace UdsTool.Models
{
    public static class UdsDefinitions
    {
        public static Dictionary<byte, string> ServiceIdentifiers = new Dictionary<byte, string>
        {
            { 0x10, "Diagnostic Session Control" },
            { 0x11, "ECU Reset" },
            { 0x14, "Clear Diagnostic Information" },
            { 0x19, "Read DTC Information" },
            { 0x22, "Read Data By Identifier" },
            { 0x23, "Read Memory By Address" },
            { 0x24, "Read Scaling Data By Identifier" },
            { 0x27, "Security Access" },
            { 0x28, "Communication Control" },
            { 0x2E, "Write Data By Identifier" },
            { 0x2F, "Input Output Control By Identifier" },
            { 0x31, "Routine Control" },
            { 0x34, "Request Download" },
            { 0x35, "Request Upload" },
            { 0x36, "Transfer Data" },
            { 0x37, "Request Transfer Exit" },
            { 0x3D, "Write Memory By Address" },
            { 0x3E, "Tester Present" },
            { 0x85, "Control DTC Setting" },
        };

        public static Dictionary<byte, Dictionary<byte, string>> SubFunctions = new Dictionary<byte, Dictionary<byte, string>>
        {
            { 0x10, new Dictionary<byte, string> // Diagnostic Session Control
                {
                    { 0x01, "Default Session" },
                    { 0x02, "Programming Session" },
                    { 0x03, "Extended Session" },
                }
            },
            { 0x11, new Dictionary<byte, string> // ECU Reset
                {
                    { 0x01, "Hard Reset" },
                    { 0x02, "Key Off On Reset" },
                    { 0x03, "Soft Reset" },
                }
            },
            { 0x19, new Dictionary<byte, string> // Read DTC Information
                {
                    { 0x01, "Report Number Of DTC By Status Mask" },
                    { 0x02, "Report DTC By Status Mask" },
                    { 0x04, "Report DTC Snapshot Record By DTC Number" },
                    { 0x06, "Report DTC Extended Data Record By DTC Number" },
                    { 0x0A, "Report Supported DTC" },
                }
            },
            { 0x27, new Dictionary<byte, string> // Security Access
                {
                    { 0x01, "Request Seed Level 1" },
                    { 0x02, "Send Key Level 1" },
                    { 0x03, "Request Seed Level 2" },
                    { 0x04, "Send Key Level 2" },
                }
            },
            { 0x28, new Dictionary<byte, string> // Communication Control
                {
                    { 0x00, "Enable Rx And Tx" },
                    { 0x01, "Enable Rx And Disable Tx" },
                    { 0x02, "Disable Rx And Enable Tx" },
                    { 0x03, "Disable Rx And Tx" },
                }
            },
            { 0x31, new Dictionary<byte, string> // Routine Control
                {
                    { 0x01, "Start Routine" },
                    { 0x02, "Stop Routine" },
                    { 0x03, "Request Routine Results" },
                }
            },
            { 0x3E, new Dictionary<byte, string> // Tester Present
                {
                    { 0x00, "Zero Sub Function" },
                }
            },
            { 0x85, new Dictionary<byte, string> // Control DTC Setting
                {
                    { 0x01, "On" },
                    { 0x02, "Off" },
                }
            },
        };

        public static Dictionary<ushort, string> DataIdentifiers = new Dictionary<ushort, string>
        {
            { 0xF180, "Boot Software Identification" },
            { 0xF181, "Application Software Identification" },
            { 0xF182, "Application Data Identification" },
            { 0xF183, "Boot Software Fingerprint" },
            { 0xF184, "Application Software Fingerprint" },
            { 0xF185, "Application Data Fingerprint" },
            { 0xF186, "Active Diagnostic Session" },
            { 0xF187, "Vehicle Manufacturer Spare Part Number" },
            { 0xF188, "Vehicle Manufacturer ECU Software Number" },
            { 0xF189, "Vehicle Manufacturer ECU Software Version Number" },
            { 0xF18A, "System Supplier Identifier" },
            { 0xF18B, "ECU Manufacturing Date" },
            { 0xF18C, "ECU Serial Number" },
            { 0xF190, "Vehicle Identification Number" },
            { 0xF191, "Vehicle Manufacturer ECU Hardware Number" },
            { 0xF192, "System Supplier ECU Hardware Number" },
            { 0xF193, "System Supplier ECU Hardware Version Number" },
            { 0xF195, "System Name or Engine Type" },
            { 0xF197, "System Supplier Diagnostic Specification Version Number" },
            { 0xF198, "Repair Shop Code or Tester Serial Number" },
            { 0xF199, "Programming Date" },
        };
    }
}