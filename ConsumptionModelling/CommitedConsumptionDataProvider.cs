using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using NLog;

namespace ConsumptionModelling
{
    /// <summary>
    /// Proporciona los datos del consumo realizado
    /// </summary>
    public sealed class CommitedConsumptionDataProvider : IConsumptionDataProvider
    {
        private static readonly CultureInfo _dataFormatter = CultureInfo.GetCultureInfo("EN-us");
        private readonly DbConnection _dbConnection;

        public Logger Log { get; set; }

        public CommitedConsumptionDataProvider(DbConnection dbConnection)
        {
            this._dbConnection = dbConnection;
        }

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
            string sqlSentence =
               string.Format("SELECT Energy, ReadTime FROM Consumption WHERE Source = '{0}' AND ReadTime > '{1}' AND ReadTime <= '{2}' ORDER BY ReadTime",
                             sourceId, since.ToUniversalTime().ToString(_dataFormatter), until.ToUniversalTime().ToString(_dataFormatter));
            LogManager.GetCurrentClassLogger().Trace("SQL: " + sqlSentence); 

            List<float?> data = new List<float?>();
            try
            {
                using (DbCommand command = this._dbConnection.CreateCommand())
                {
                    command.CommandText = sqlSentence;
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        DateTime qHour = since.AddMinutes(15d);
                        qHour = new DateTime(qHour.Year, qHour.Month, qHour.Day, qHour.Hour, (qHour.Minute / 15) * 15, 0);
                        bool hasMoreData = reader.Read();
                        while (qHour <= until)
                        {
                            LogManager.GetCurrentClassLogger().Trace(hasMoreData?string.Format(" QHour = {2}; (ReadTime = {0}; Energy = {1};)", reader["ReadTime"], reader["Energy"], qHour):string.Format("No hay datos para la hora {0}.", qHour));

                            if (hasMoreData &&
                                (qHour.Hour > startPeriodHour) && (qHour.Hour <= endPeriodHour) &&
                                (((DateTime)reader["ReadTime"]).Year == qHour.Year) &&
                                (((DateTime)reader["ReadTime"]).Day == qHour.Day) &&
                                (((DateTime)reader["ReadTime"]).Hour == qHour.Hour) &&
                                (((DateTime)reader["ReadTime"]).Minute == qHour.Minute))
                            {
                                double value = (double)reader["Energy"];
                                if (value >= 0)
                                {
                                    data.Add((float?)value);

                                }
                                else
                                {
                                    data.Add(null);
                                    LogManager.GetCurrentClassLogger().Warn(
                                        string.Format(
                                            "Se ha leído un valor erróneo de consumo en la BD: sourceId = {0}; timeStamp = {1}; valor = {2}. Se cambia por 'null'",
                                            sourceId, (DateTime)reader["ReadTime"], value));
                                }

                                hasMoreData = reader.Read();
                            }
                            else
                            {
                                LogManager.GetCurrentClassLogger().Trace("Se sustituye por null");
                                data.Add(null);
                            }

                            qHour = qHour.Add(new TimeSpan(0, 15, 0));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    Log.ErrorException(ex.ToString(), ex);
                }
            }

            return data.ToArray();
        }

        /// <summary>
        /// Obtiene la fecha y hora del último consumo registrado
        /// de un origen de consumo(en UTC)
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastDataTimeStamp(string sourceId)
        {
            DateTime timeStamp;
            string sqlSentence = string.Format("SELECT Max(ReadTime) FROM Consumption WHERE Source='{0}'", sourceId);
            
            using (DbCommand command = this._dbConnection.CreateCommand())
            {
                command.CommandText = sqlSentence;
                timeStamp = (DateTime)command.ExecuteScalar();
            }

            return timeStamp;
        }
    }
}
