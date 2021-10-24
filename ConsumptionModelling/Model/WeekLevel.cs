using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumptionModelling.Model
{
    /// <summary>
    /// Nivel de semana
    /// </summary>
    [Serializable]
    public sealed class WeekLevel:IModel
    {
        public short MonthWeekNum { get; set; }

        public HolidayLevel HolidayModel { get; set; }
        public WorkingLevel WorkingModel { get; set; }
        public List<WeekDayLevel> WeekDayModels { get; set; }
        public DayLevel DefaultDayModel { get; set; }

        public DayLevel FetchDayModel(DateTime dateTime)
        {
            DayLevel dayModel = (ModelSeeker.SeekWeekDay(dateTime, WeekDayModels) ??
                                 ModelSeeker.SeekHolidayOrWorkingDay(dateTime, WorkingModel, HolidayModel)) ??
                                DefaultDayModel;

            return dayModel;
        }

        public override bool Equals(object obj)
        {
            WeekLevel other = obj as WeekLevel;
            return this.Equals(other);
        }

        public bool Equals(WeekLevel other)
        {
            return
                (other != null) &&
                (this.MonthWeekNum.Equals(other.MonthWeekNum)) &&
                (this.HolidayModel.Equals(other.HolidayModel)) &&
                (this.WorkingModel.Equals(other.WorkingModel)) &&
                (this.WeekDayModels.Equals(other.WeekDayModels)) &&
                (this.DefaultDayModel.Equals(other.DefaultDayModel));
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
                int result = MonthWeekNum.GetHashCode();
                result = (result*397) ^ (HolidayModel != null ? HolidayModel.GetHashCode() : 0);
                result = (result*397) ^ (WorkingModel != null ? WorkingModel.GetHashCode() : 0);
                result = (result*397) ^ (WeekDayModels != null ? WeekDayModels.GetHashCode() : 0);
                result = (result*397) ^ (DefaultDayModel != null ? DefaultDayModel.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            StringBuilder weekDayModelsStr = new StringBuilder();
            foreach (WeekDayLevel weekDayModel in WeekDayModels)
            {
                weekDayModelsStr.Append(weekDayModel + "; ");
            }

            return
                this.MonthWeekNum + "; " +
                this.DefaultDayModel + "; " +
                this.HolidayModel + "; " +
                weekDayModelsStr + "; " +
                this.WorkingModel + "; ";
        }
    }
}
