using System;
using System.Collections.Generic;
using System.IO;
using ConsumptionModelling.Model;
using ModelSerializer = ConsumptionModelling.GenericSerializer<ConsumptionModelling.Model.YearLevel>;

namespace ConsumptionModelling
{
    /// <summary>
    /// Proporciona datos de los modelos
    /// </summary>
    public class ModelDataProvider
    {
        private readonly Dictionary<string,YearLevel> _models = new Dictionary<string, YearLevel>();

        /// <summary>
        /// Modelos
        /// </summary>
        public Dictionary<string, YearLevel> Models
        {
            get { return _models; }
        }

        /// <summary>
        /// Obtiene los datos de un modelo para un intervalo de fechas
        /// </summary>
        /// <param name="sourceId">Identificador del modelo</param>
        /// <param name="since">Inicio del intervalo</param>
        /// <param name="until">Final del intervalo</param>
        /// <returns></returns>
        public float[] GetData(string sourceId, DateTime since, DateTime until)
        {
            return this.GetData(sourceId, since, until, 0, 24);
        }

        /// <summary>
        /// Obtiene los datos de un modelo para un intervalo de fechas dentro de un periodo horario
        /// </summary>
        /// <param name="sourceId">Identificador del modelo</param>
        /// <param name="since">Inicio del intervalo</param>
        /// <param name="until">Final del intervalo</param>
        /// <param name="startPeriodHour">Hora de inicio del periodo</param>
        /// <param name="endPeriodHour">Hora final del periodo</param>
        /// <returns></returns>
        public float[] GetData(string sourceId, DateTime since, DateTime until, short startPeriodHour, short endPeriodHour)
        {
            if (until.Subtract(since).TotalHours < 1d)
            {
                DateTime time = until;
                if (time.Minute == 0)
                {
                    since = time.Subtract(new TimeSpan(1, 0, 0));
                    until = time;
                }
                else
                {
                    since = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, DateTimeKind.Utc);
                    until = since.AddHours(1d);
                }
            }

            return this._models.ContainsKey(sourceId) ? ModelSeeker.GetHourlyValuesInPeriodFromModel(this._models[sourceId], since, until, startPeriodHour, endPeriodHour) : null;
        }

        /// <summary>
        /// Carga los modelos desde un directorio
        /// </summary>
        /// <param name="directoryPath">Ruta del directorio donde se almacenan los modelos</param>
        /// <returns></returns>
        public bool Load(string directoryPath)
        {
            bool success = false;
            if(Directory.Exists(directoryPath))
            {
                this.Models.Clear();
                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    this.Models.Add(
                        new FileInfo(filePath).Name.Split('.')[0],
                        ModelSerializer.Deserialize(filePath));
                }
                success = true;
            }

            return success;
        }

        /// <summary>
        /// Almacena los modelos en archivos dentro de un directorio
        /// </summary>
        /// <param name="directoryPath">Ruta del directorio</param>
        public void Save(string directoryPath)
        {
            if(Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
            Directory.CreateDirectory(directoryPath);

            foreach (string sourceId in Models.Keys)
            {
                ModelSerializer.Serialize(directoryPath + '\\' + sourceId + ".xml", this.Models[sourceId]);
            }
        }
    }
}
