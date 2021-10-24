using System.Collections.Generic;
using ConsumptionModelling.Model;

namespace AdvisorTool.ViewModel
{
    public class WeekViewModel:BaseElementViewModel
    {
        private readonly WeekLevel _weekModel;

        public WeekViewModel(WeekLevel weekModel)
        {
            this._weekModel = weekModel;
        }

        /// <summary>
        /// Descripción textual del modelo representado
        /// </summary>
        public override string ModelName
        {
            get { return "Semana " + this._weekModel.MonthWeekNum; }
        }

        /// <summary>
        /// Elemento del modelo representado
        /// </summary>
        public override IModel ModelElement
        {
            get { return this._weekModel; }
        }

        /// <summary>
        /// Modelos de rango inferior
        /// </summary>
        public override IList<BaseElementViewModel> SubModels
        {
            get { return new List<BaseElementViewModel>();}
        }
    }
}
