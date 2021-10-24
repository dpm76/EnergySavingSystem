using System;

namespace ConsumptionDaemon
{
    [Serializable]
    public class DataReaderConfigItem
    {
        public string SourceId { get; set; }
        public string ClassName { get; set; }
        public string ComPort { get; set; }
        public byte DeviceId { get; set; }
    }
}
