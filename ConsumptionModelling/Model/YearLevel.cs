using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumptionModelling.Model
{
    /// <summary>
    /// Nivel anual
    /// </summary>
    [Serializable]
    public sealed class YearLevel:IModel
    {
        private DayLevel _defaultDayModel = DayLevel.Default;
        /// <summary>
        /// Días concretos.
        /// Si el año de una fecha es 0, sólo se considerará el mes y el día
        /// </summary>
        public List<ConcreteDay> ConcreteDayModels { get; set; }
        public HolidayLevel HolidayModel { get; set; }
        public WorkingLevel WorkingModel { get; set; }
        public List<MonthLevel> MonthModels { get; set; }
        public List<WeekDayLevel> WeekDayModels { get; set; }
        public DayLevel DefaultDayModel
        {
            get { return this._defaultDayModel; }
            set { this._defaultDayModel = value; }
        }

        public DayLevel FetchDayModel(DateTime dateTime)
        {
            //Buscar día concreto
            DayLevel dayModel = ModelSeeker.SeekConcreteDay(dateTime, ConcreteDayModels);

            if (dayModel == null)
            {
                //Buscar mes
                MonthLevel monthModel = ModelSeeker.SeekMonth(dateTime, MonthModels);

                //Si no hay mes, buscar por día de la semana y laborable/festivo
                //Si no, día por defecto
                dayModel = ((monthModel != null)
                                ? monthModel.FetchDayModel(dateTime)
                                : (ModelSeeker.SeekWeekDay(dateTime, WeekDayModels) ??
                                   ModelSeeker.SeekHolidayOrWorkingDay(dateTime, WorkingModel, HolidayModel))) ??
                           DefaultDayModel;
            }

            return dayModel;
        }

        public override bool Equals(object obj)
        {
            YearLevel other = obj as YearLevel;

            return
                this.Equals(other);
        }

        public bool Equals(YearLevel other)
        {
            return
                (other != null) &&
                ((this.ConcreteDayModels == other.ConcreteDayModels)||this.ConcreteDayModels.Equals(other.ConcreteDayModels)) &&
                ((this.HolidayModel == other.HolidayModel) || this.HolidayModel.Equals(other.HolidayModel)) &&
                ((this.WorkingModel == other.WorkingModel) || this.WorkingModel.Equals(other.WorkingModel)) &&
                ((this.MonthModels == other.MonthModels) || this.MonthModels.Equals(other.MonthModels)) &&
                ((this.WeekDayModels == other.WeekDayModels) || this.WeekDayModels.Equals(other.WeekDayModels));
        }

        public override string ToString()
        {
            StringBuilder concreteDaysModelsStr = new StringBuilder();
            if (this.ConcreteDayModels != null)
            {
                foreach (ConcreteDay concreteDayModel in ConcreteDayModels)
                {
                    concreteDaysModelsStr.Append(concreteDayModel + "; ");
                }
            }

            StringBuilder monthModelsStr = new StringBuilder();
            if (this.MonthModels != null)
            {
                foreach (MonthLevel monthModel in this.MonthModels)
                {
                    monthModelsStr.Append(monthModel + "; ");
                }
            }

            StringBuilder weekDayModelsStr = new StringBuilder();
            if (this.WeekDayModels != null)
            {
                foreach (WeekDayLevel weekDayModel in WeekDayModels)
                {
                    weekDayModelsStr.Append(weekDayModel + "; ");
                }
            }

            return
                this.DefaultDayModel + "; " +
                concreteDaysModelsStr + "; " +
                this.HolidayModel + "; " +
                monthModelsStr + "; " +
                weekDayModelsStr + "; " +
                this.WorkingModel + "; ";
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
            unchecked
            {
                int result = (ConcreteDayModels != null ? ConcreteDayModels.GetHashCode() : 0);
                result = (result*397) ^ (HolidayModel != null ? HolidayModel.GetHashCode() : 0);
                result = (result*397) ^ (WorkingModel != null ? WorkingModel.GetHashCode() : 0);
                result = (result*397) ^ (MonthModels != null ? MonthModels.GetHashCode() : 0);
                result = (result*397) ^ (WeekDayModels != null ? WeekDayModels.GetHashCode() : 0);
                return result;
            }
        }
    }
}
