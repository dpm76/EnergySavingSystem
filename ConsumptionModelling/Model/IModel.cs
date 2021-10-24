using System;

namespace ConsumptionModelling.Model
{
    public interface IModel
    {
        DayLevel FetchDayModel(DateTime dateTime);
        DayLevel DefaultDayModel { get; set; }
    }
}
