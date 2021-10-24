using System;
using ConsumptionModelling;

namespace TestFixtures.ConsumptionModelling
{
    public class FakeConsumptionDataProvider:IConsumptionDataProvider
    {

        public float Ac;

        /// <summary>
        /// Obtiene los datos de consumo para una fuente y un intervalo de tiempo
        /// </summary>
        /// <param name="sourceId">Fuente de consumo</param>
        /// <param name="since">Inicio del intervalo</param>
        /// <param name="until">Final del intervalo</param>
        /// <returns></returns>
        public float?[] GetData(string sourceId, DateTime since, DateTime until)
        {
            return this.GetData(sourceId, since, until, 0, 24);
        }

        /// <summary>
        /// Obtiene los datos de consumo para una fuente y un intervalo de 
        /// tiempo, dentro de un periodo horario
        /// </summary>
        /// <param name="sourceId">Fuente de consumo</param>
        /// <param name="since">Inicio del intervalo</param>
        /// <param name="until">Final del intervalo</param>
        /// <param name="startPeriodHour">Hora de inicio del periodo</param>
        /// <param name="endPeriodHour">Hora de fin del periodo</param>
        /// <returns></returns>
        public float?[] GetData(string sourceId, DateTime since, DateTime until, short startPeriodHour, short endPeriodHour)
        {
            this.Ac = 0;
            float?[] data = new float?[100];
            for (int i = 0; i < 100; i++)
            {
                data[i] = ((i != 10) ? (float?)i : null);
                Ac += (i != 10) ? i : 0f;
            }

            return data;
        }

        public DateTime LastDataTimeStamp;

        /// <summary>
        /// Obtiene la fecha y hora del último consumo registrado
        /// de un origen de consumo(en UTC)
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastDataTimeStamp(string sourceId)
        {
            return LastDataTimeStamp;
        }
    }
}
