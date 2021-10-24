//#define DELETE_DB_ON_START

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace EnergyConsumption
{
    public sealed class FakeDataReader:DataReader
    {
        private readonly Random _rand = new Random();
        private DateTime _lastReadTime;

        public FakeDataReader(int currentPowerReadInterval, int qPowerReadInterval, DbConnection dbConnection):
            base(currentPowerReadInterval, qPowerReadInterval, dbConnection)
        {
            this.SourceId = "Fake";
            this.Magnitudes = new[] {"V", "A", "W"};
            DateTime utcNow = DateTime.UtcNow;
            int minute;
            if (utcNow.Minute < 15)
            {
                minute = 0;
            }
            else if (utcNow.Minute < 30)
            {
                minute = 15;
            }
            else if (utcNow.Minute < 45)
            {
                minute = 30;
            }
            else
            {
                minute = 45;
            }
            this._lastReadTime = new DateTime(utcNow.Year, utcNow.Month,utcNow.Day, utcNow.Hour, minute, 0, DateTimeKind.Utc);

#if DELETE_DB_ON_START
            //Eliminar contenido de la tabla de consumo
            dbConnection.Open();
            using(DbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = "DELETE Consumption";
                command.ExecuteNonQuery();
            }
            dbConnection.Close();

            //Eliminar alertas
            string path = Directory.GetCurrentDirectory() + "\\alerts";
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
#endif

        }

        /// <summary>
        /// Lee los datos actuales del consumo energético
        /// </summary>
        protected override Dictionary<string, float> ReadCurrentPowerData()
        {
            return new Dictionary<string, float>(3)
                       {
                           {this.Magnitudes[0], (float) (320d + (this._rand.NextDouble()*10d))},
                           {this.Magnitudes[1], (float) (this._rand.NextDouble()*1500d)},
                           {this.Magnitudes[2], (float) (this._rand.NextDouble()*1000d)}
                       };
        }

        protected override bool IsTimeToRead()
        {
            return true;
        }

        /// <summary>
        /// Lee el dato de consumo cuarto-horario
        /// </summary>
        /// <returns></returns>
        protected override ConsumptionData ReadQPowerData()
        {
            this._lastReadTime = this._lastReadTime.AddMinutes(15d);

            return new ConsumptionData
                       {
                           Data = (float) (this._rand.NextDouble()*10d),
                           TimeStamp = this._lastReadTime
                       };
        }
    }
}
