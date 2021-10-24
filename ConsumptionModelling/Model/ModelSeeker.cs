using System;
using System.Collections.Generic;
using System.Globalization;

namespace ConsumptionModelling.Model
{
    /// <summary>
    /// Métodos para buscar en los modelos
    /// </summary>
    public static class ModelSeeker
    {
        private struct ModelValitationParams<T>
        {
            public T Model;
            public DateTime DateTimeSought;
        }

        private static bool IsConcreteDaySought(ModelValitationParams<ConcreteDay> valitationParams)
        {
            DateTime dateTime = valitationParams.DateTimeSought;
            ConcreteDay concreteDay = valitationParams.Model;

            return
                ((concreteDay.Year == 0) &&
                 (concreteDay.Day == dateTime.Day) &&
                 (concreteDay.Month == dateTime.Month)) ||
                 ((concreteDay.Year != 0) &&
                dateTime.Equals(new DateTime(concreteDay.Year, concreteDay.Month, concreteDay.Day)));
        }

        /// <summary>
        /// Busca un día concreto.
        /// Si el año es 0, se ignorará el año y sólo se considerará el mes y el día
        /// </summary>
        /// <param name="dateTime">fecha (si año es 0, sólo se considera el mes y el día)</param>
        /// <param name="list">Lista de días concretos</param>
        /// <returns></returns>
        public static ConcreteDay SeekConcreteDay(DateTime dateTime, IList<ConcreteDay> list)
        {
            return SeekModel(dateTime, list, IsConcreteDaySought);
        }

        private static bool IsMonthSought(ModelValitationParams<MonthLevel> valitationParams)
        {
            return valitationParams.Model.Month == valitationParams.DateTimeSought.Month;
        }

        public static MonthLevel SeekMonth(DateTime dateTime, IList<MonthLevel> list)
        {
            return SeekModel(dateTime, list, IsMonthSought);
        }

        public static int GetMonthWeekNumber(DateTime dateTime)
        {
            DateTime monthFirstDay = new DateTime(dateTime.Year, dateTime.Month, 1);
            DateTime firstWeekfirstDay = monthFirstDay.Subtract(
                new TimeSpan(
                    ((monthFirstDay.DayOfWeek >= CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek) ?
                    0 : 7) +
                    (monthFirstDay.DayOfWeek - CultureInfo.CurrentUICulture.DateTimeFormat.FirstDayOfWeek)
                    , 0, 0, 0));

            return 1 + (dateTime.Subtract(firstWeekfirstDay).Days / 7);
        }

        private static bool IsWeekSought(ModelValitationParams<WeekLevel> valitationParams)
        {
            DateTime dateTime = valitationParams.DateTimeSought;
            WeekLevel week = valitationParams.Model;

            
            return week.MonthWeekNum == GetMonthWeekNumber(dateTime);
        }

        public static WeekLevel SeekWeek(DateTime dateTime, IList<WeekLevel>list)
        {
            return SeekModel(dateTime, list, IsWeekSought);
        }

        private static bool IsWeekDaySought(ModelValitationParams<WeekDayLevel> valitationParams)
        {
            return valitationParams.Model.DayOfWeek == valitationParams.DateTimeSought.DayOfWeek;
        }

        public static WeekDayLevel SeekWeekDay(DateTime dateTime, IList<WeekDayLevel>list)
        {
            return SeekModel(dateTime, list, IsWeekDaySought);
        }

        public static DayLevel SeekHolidayOrWorkingDay(DateTime dateTime, WorkingLevel workingLevel, HolidayLevel holidayLevel)
        {
// ReSharper disable RedundantCast
            return (dateTime.DayOfWeek < DayOfWeek.Saturday) ? (DayLevel)workingLevel : (DayLevel)holidayLevel;
// ReSharper restore RedundantCast
        }

        private static T SeekModel<T>(DateTime dateTime, IList<T> list, Predicate<ModelValitationParams<T>> isModelSought)
        {
            T model = default(T);
            if ((list != null) && (list.Count > 0))
            {
                ModelValitationParams<T> validationParams = new ModelValitationParams<T> {DateTimeSought = dateTime};
                int i = 0;
                do
                {
                    validationParams.Model = list[i];
                } while (!isModelSought(validationParams) && (++i < list.Count));
                
                if (i < list.Count)
                {
                    model = list[i];
                }
            }
            return model;
        }

        [Obsolete]
        public static float[] GetHourlyValuesFromModel(YearLevel yearModel, DateTime since, DateTime until)
        {
            return GetHourlyValuesInPeriodFromModel(yearModel, since, until, 0, 24);
        }

        public static float[] GetHourlyValuesInPeriodFromModel(YearLevel yearModel, DateTime since, DateTime until, short startPeriodHour, short endPeriodHour)
        {
            //Hay que reajustar el límite final para que recoja también esa hora.
            //Por ejemplo, since = 8:19 y until = 12:19, tiene que devolver los valores del modelo
            //correspondientes a las 8, 9, 10, 11 y 12 horas.
            if(until.Minute != 0)
            {
                until = new DateTime(until.Year, until.Month, until.Day, until.Hour + 1, 0,0, until.Kind);
            }

            int daysCount =
                new DateTime(until.Year, until.Month, until.Day).Subtract(new DateTime(since.Year, since.Month, since.Day)).Days;
            int hourlyValuesCount = (int)Math.Ceiling(until.Subtract(since).TotalHours);
            List<float> hourlyValues = new List<float>(hourlyValuesCount);

            if (daysCount == 0)
            {
                hourlyValues.AddRange(
                    yearModel.FetchDayModel(since).GetHourlyValues(
                        (((short) since.Hour < startPeriodHour) ? startPeriodHour : (short) since.Hour),
                        (((short) until.Hour > endPeriodHour) ? endPeriodHour : (short) until.Hour)));
            }
            else
            {
                hourlyValues.AddRange(
                    GetHourlyValuesInPeriod(yearModel, since, (short)since.Hour, 24, startPeriodHour, endPeriodHour));
                DateTime dateTime = since.AddDays(1d);
                for (int i = 1; i < daysCount; i++)
                {
                    hourlyValues.AddRange(
                        yearModel.FetchDayModel(dateTime).GetHourlyValues(startPeriodHour, endPeriodHour));
                    dateTime.AddDays(1d);
                }
                if (until.Hour > 0)
                {
                    hourlyValues.AddRange(
                        GetHourlyValuesInPeriod(yearModel, until, 0, (short) until.Hour,startPeriodHour, endPeriodHour));
                }
            }

            return hourlyValues.ToArray();
        }

        private static IEnumerable<float> GetHourlyValuesInPeriod(IModel model, DateTime date, short startQueryHour, short endQueryHour, short startPeriodHour, short endPeriodHour)
        {
            return model.FetchDayModel(date).GetHourlyValues(
                            (startQueryHour < startPeriodHour? startPeriodHour: startQueryHour),
                            (endQueryHour > endPeriodHour ? endPeriodHour: endQueryHour));
        }
    }
}
