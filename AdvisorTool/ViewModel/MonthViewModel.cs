using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConsumptionModelling.Model;

namespace AdvisorTool.ViewModel
{
    public class MonthViewModel:BaseElementViewModel
    {
        private readonly MonthLevel _monthModel;

        public MonthViewModel(MonthLevel monthModel)
        {
            this._monthModel = monthModel;
        }

        /// <summary>
        /// Descripción textual del modelo representado
        /// </summary>
        public override string ModelName
        {
            get { return CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(this._monthModel.Month); }
        }

        /// <summary>
        /// Elemento del modelo representado
        /// </summary>
        public override IModel ModelElement
        {
            get { return this._monthModel; }
        }

        /// <summary>
        /// Modelos de rango inferior
        /// </summary>
        public override IList<BaseElementViewModel> SubModels
        {
            get
            {
                return this._monthModel.WeekModels.Select(weekModel => new WeekViewModel(weekModel)).Cast<BaseElementViewModel>().ToList();
            }
        }
    }
}
