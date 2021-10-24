using System;
using ConsumptionModelling.Model;

namespace AdvisorTool
{
    public class DayModelEventArgs:EventArgs
    {
        private readonly DayLevel _dayModel;

        public DayModelEventArgs(DayLevel dayModel)
        {
            this._dayModel = dayModel;
        }

        public DayLevel DayModel
        {
            get { return this._dayModel; }
        }
    }
}
