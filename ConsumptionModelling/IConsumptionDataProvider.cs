using System;

namespace ConsumptionModelling
{
    public interface IConsumptionDataProvider
    {
        /// <summary>
        /// Obtiene los datos de consumo para una fuente y un intervalo de tiempo
        /// </summary>
        /// <param name="sourceId">Fuente de consumo</param>
        /// <param name="since">Inicio del intervalo</param>
        /// <param name="until">Final del intervalo</param>
        /// <returns></returns>
        float?[] GetData(string sourceId, DateTime since, DateTime until);

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
        float?[] GetData(string sourceId, DateTime since, DateTime until, short startPeriodHour, short endPeriodHour);

        /// <summary>
        /// Obtiene la fecha y hora del último consumo registrado
        /// de un origen de consumo(en UTC)
        /// </summary>
        /// <returns></returns>
        DateTime GetLastDataTimeStamp(string sourceId);
    }
}