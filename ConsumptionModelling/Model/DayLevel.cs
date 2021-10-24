using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumptionModelling.Model
{
    /// <summary>
    /// Nivel diario
    /// </summary>
    [Serializable]
    public class DayLevel
    {
        private readonly List<ConsumptionRange> _ranges = new List<ConsumptionRange>();

        public DayLevel()
        {
            this._ranges.Add(new ConsumptionRange{Since = 0, Until = 24, Value = 0f});
        }

        /// <summary>
        /// Rangos horarios para cada valor
        /// Necesario para la serialización
        /// </summary>
        public ConsumptionRange[] Ranges
        {
            get { return this._ranges.ToArray(); }
            set
            {
                this._ranges.Clear();
                this._ranges.AddRange(value);
            }
        }

        /// <summary>
        /// Obtiene los valores horarios para un intervalo
        /// </summary>
        /// <param name="since">Hora de inicio del intervalo</param>
        /// <param name="until">Hora final del intervalo</param>
        /// <remarks>
        /// La hora final está fuera del intervalo solicitado.
        /// Por ejemplo, si se quiere saber los valores para las 3 y las 4,
        /// los parámetros de entrada serán: since=3; until=5.
        /// Para saber el valor para una única hora, hay que poner, por ejemplo
        /// para las 7: since=7; until=8.
        /// 
        /// Si until es 0, se entiende que es el día siguiente
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Indica un intervalo no válido, cuando no se cumple since &lt; until
        /// </exception>
        /// <returns></returns>
        public float[] GetHourlyValues(short since, short until)
        {
            if(since >= until)
            {
                throw new ArgumentOutOfRangeException("since", since, "El inicio del intervalo debe ser estrictament menor que el final");
            }

            if (since < 0)
            {
                since = 0;
            }else if(since > 23)
            {
                since = 23;
            }

            if((until < 1) || (until > 24))
            {
                until = 24;
            }
            
            float[] data = new float[until-since];
            
            int rangeIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                int hour = since + i;
                while ((hour < this._ranges[rangeIndex].Since) || (hour >= this._ranges[rangeIndex].Until))
                {
                    rangeIndex++;
                }
                data[i] = this._ranges[rangeIndex].Value;
            }

            return data;
        }

        public float[] GetAllHourlyValues()
        {
            return this.GetHourlyValues(0, 24);
        }

        public void AddConsumptionRange(ConsumptionRange newRange)
        {
            foreach (ConsumptionRange range in _ranges.ToArray())
            {
                if((range.Since >= newRange.Since) && (range.Until <= newRange.Until))
                {
                    this._ranges.Remove(range);
                }else if((newRange.Since > range.Since) && (newRange.Until < range.Until))
                {
                    ConsumptionRange splitRange = (ConsumptionRange)range.Clone();
                    range.Until = newRange.Since;
                    splitRange.Since = newRange.Until;
                    int i = this._ranges.IndexOf(range);
                    this._ranges.Insert(i + 1, newRange);
                    this._ranges.Insert(i + 2, splitRange);
                }
                else if((newRange.Until >= range.Since) && (newRange.Until <= range.Until))
                {
                    range.Since = newRange.Until;
                    this._ranges.Insert(this._ranges.IndexOf(range), newRange);
                }else if((newRange.Since >= range.Since) && (newRange.Since < range.Until))
                {
                    range.Until = newRange.Since;
                }
            }
        }

        /// <summary>
        /// Obtiene el modelo diario por defecto
        /// </summary>
        public static DayLevel Default
        {
            get
            {
                return new DayLevel();
            }
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as DayLevel);
        }

        public bool Equals(DayLevel other)
        {
            return
                (other != null) &&
                (this._ranges.Equals(other._ranges));
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (_ranges != null ? _ranges.GetHashCode() : 0);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (ConsumptionRange range in Ranges)
            {
                sb.Append(range + "; ");
            }

            return sb.ToString();
        }
    }
}
