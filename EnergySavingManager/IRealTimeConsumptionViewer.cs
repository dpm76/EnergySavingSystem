using System;
using System.Collections.Generic;

namespace EnergySavingManager
{
    /// <summary>
    /// Visor de consumo en tiempo real
    /// </summary>
    public interface IRealTimeConsumptionViewer
    {
        /// <summary>
        /// Identificador de la fuente
        /// </summary>
        string SourceId { get; set; }
        /// <summary>
        /// Añade una colección de datos de un momento concreto
        /// </summary>
        /// <param name="dataColection">Colección de datos</param>
        /// <param name="timeStamp">Momento de lectura</param>
        void PumpData(Dictionary<string, float> dataColection, DateTime timeStamp);
        /// <summary>
        /// Muestra un dato cuarto-horario
        /// </summary>
        /// <param name="data">dato</param>
        /// <param name="savingData">dato de consumo de ahorro</param>
        /// <param name="timeStamp">Marca de hora</param>
        void ShowQData(float? data, float savingData, DateTime timeStamp);
        /// <summary>
        /// Borra todos los datos mostrados
        /// </summary>
        void Clear();

        /// <summary>
        /// Número máximo de datos cuarto-horarios mostrados
        /// </summary>
        int MaxQData { get; set; }
    }
}