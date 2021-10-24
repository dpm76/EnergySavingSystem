using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumptionModelling.Model
{
    /// <summary>
    /// Nivel mensual
    /// </summary>
    [Serializable]
    public sealed class MonthLevel:IModel
    {
        public int Month { get; set; }

        public WorkingLevel WorkingModel { get; set; }
        public HolidayLevel HolidayModel { get; set; }
        public List<WeekLevel> WeekModels { get; set; }
        public List<WeekDayLevel> WeekDayModels { get; set; }
        public DayLevel DefaultDayModel { get; set; }

        public DayLevel FetchDayModel(DateTime dateTime)
        {
            WeekLevel weekModel = ModelSeeker.SeekWeek(dateTime, WeekModels);
            DayLevel dayModel = ((weekModel != null)
                                     ? weekModel.FetchDayModel(dateTime)
                                     : ModelSeeker.SeekWeekDay(dateTime, WeekDayModels)) ??
                                ModelSeeker.SeekHolidayOrWorkingDay(dateTime, WorkingModel, HolidayModel) ??
                                DefaultDayModel;

            return dayModel;
        }

        public override bool Equals(object obj)
        {
            MonthLevel other = obj as MonthLevel;
            return this.Equals(other);
        }

        public bool Equals(MonthLevel other)
        {
            return
                (other != null) &&
                (this.Month == other.Month) &&
                ((this.WorkingModel == other.WorkingModel) || this.WorkingModel.Equals(other.WorkingModel)) &&
                ((this.HolidayModel == other.HolidayModel)||this.HolidayModel.Equals(other.HolidayModel)) &&
                ((this.WeekModels == other.WeekModels)||this.WeekModels.Equals(other.WeekModels)) &&
                ((this.WeekDayModels == other.WeekDayModels)||this.WeekDayModels.Equals(other.WeekDayModels)) &&
                ((this.DefaultDayModel == other.DefaultDayModel) || this.DefaultDayModel.Equals(other.DefaultDayModel));
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
                int result = Month;
                result = (result*397) ^ (WorkingModel != null ? WorkingModel.GetHashCode() : 0);
                result = (result*397) ^ (HolidayModel != null ? HolidayModel.GetHashCode() : 0);
                result = (result*397) ^ (WeekModels != null ? WeekModels.GetHashCode() : 0);
                result = (result*397) ^ (WeekDayModels != null ? WeekDayModels.GetHashCode() : 0);
                result = (result*397) ^ (DefaultDayModel != null ? DefaultDayModel.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            StringBuilder weekDayModelsStr = new StringBuilder();
            if (this.WeekDayModels != null)
            {
                foreach (WeekDayLevel weekDayModel in WeekDayModels)
                {
                    weekDayModelsStr.Append(weekDayModel + "; ");
                }
            }

            StringBuilder weekModelsStr = new StringBuilder();
            if (this.WeekModels != null)
            {
                foreach (WeekLevel weekModel in WeekModels)
                {
                    weekModelsStr.Append(weekModel + "; ");
                }
            }

            return
                this.DefaultDayModel + "; " +
                this.HolidayModel + "; " +
                this.Month + "; " +
                weekDayModelsStr + "; " +
                weekModelsStr + "; " +
                this.WorkingModel + "; ";
        }
    }
}
