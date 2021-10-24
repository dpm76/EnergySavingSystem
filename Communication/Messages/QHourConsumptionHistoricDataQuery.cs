using System;

namespace Communication.Messages
{
    /// <summary>
    /// Consulta de datos históricos Q-horarios
    /// </summary>
    [Serializable]
    public class QHourConsumptionHistoricDataQuery:IMessage
    {
        public string SourceId { get; set; }
        public DateTime Since { get; set; }
        public DateTime Until { get; set; }

        public override string ToString()
        {
            return
                base.ToString() +
                string.Format(" -> SourceId = {0}; Since = {1}; Until = {2}", SourceId, Since, Until);
        }
    }
}
