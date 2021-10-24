using System;
using EnergySavingManager.Properties;
using NLog;

namespace EnergySavingManager
{
    /// <summary>
    /// Genera el periodo de facturación según la configuración
    /// </summary>
    public static class InvoicePeriodProvider
    {
        /// <summary>
        /// Inicio y fin del periodo
        /// </summary>
        public struct Period
        {
            public DateTime Since;
            public DateTime Until;
        }

        /// <summary>
        /// Obtiene el periodo basándose en una fecha de referencia
        /// y un tipo de periodo.
        /// </summary>
        /// <param name="referenceDate">Fecha de referencia, perteneciente al periodo buscado.</param>
        /// <param name="periodType">Tipo de periodo</param>
        /// <returns></returns>
        public static Period GetPeriod(DateTime referenceDate, PeriodType periodType)
        {
            Period period;
            switch (periodType)
            {
                case PeriodType.Yearly:
                    period = GetYearlyPeriod(referenceDate);
                    break;
                case PeriodType.Monthly:
                    period = GetMonthlyPeriod(referenceDate);
                    break;
                default:
                    LogManager.GetCurrentClassLogger().Warn(string.Format("El tipo de periodo '{0}' no tiene tratamiento. Se considerará un periodo mensual.", periodType));
                    period = GetMonthlyPeriod(referenceDate);
                    break;
            }

            return period;
        }

        /// <summary>
        /// Obtiene el periodo basándose en una fecha de referencia
        /// y según el tipo de periodo de la configuración.
        /// </summary>
        /// <param name="referenceDate">Fecha de referencia, perteneciente al periodo buscado.</param>
        /// <returns></returns>
        public static Period GetPeriod(DateTime referenceDate)
        {
            PeriodType periodType;
            try
            {
                periodType = (PeriodType) Enum.Parse(typeof (PeriodType), Settings.Default.PeriodType, true);
            }catch(ArgumentException)
            {
                LogManager.GetCurrentClassLogger().Warn(string.Format("No se reconoce el tipo de periodo '{0}'. Se utilizará un periodo mensual.", Settings.Default.PeriodType));
                periodType = PeriodType.Monthly;
            }

            return GetPeriod(referenceDate, periodType);
        }

        private static Period GetMonthlyPeriod(DateTime referenceDate)
        {
            return new Period
                       {
                           Since = new DateTime(referenceDate.Year, referenceDate.Month, 1),
                           Until = (referenceDate.Month != 12)
                                       ? new DateTime(referenceDate.Year, referenceDate.Month + 1, 1)
                                       : new DateTime(referenceDate.Year + 1, 1, 1)
                       };
        }

        private static Period GetYearlyPeriod(DateTime referenceDate)
        {
            return new Period
                       {
                           Since = new DateTime(referenceDate.Year, 1, 1),
                           Until = new DateTime(referenceDate.Year+1, 1, 1)
                       };
        }

    }
}
