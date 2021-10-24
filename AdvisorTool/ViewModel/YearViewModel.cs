using System.Collections.Generic;
using System.Linq;
using ConsumptionModelling.Model;

namespace AdvisorTool.ViewModel
{
    public class YearViewModel: BaseElementViewModel
    {
        private readonly YearLevel _yearLevel;

        public YearViewModel(YearLevel yearLevel)
        {
            this._yearLevel = yearLevel;
        }

        /// <summary>
        /// Descripción textual del modelo representado
        /// </summary>
        public override string ModelName
        {
            get { return "Modelo anual"; }
        }

        /// <summary>
        /// Elemento del modelo representado
        /// </summary>
        public override IModel ModelElement
        {
            get { return this._yearLevel; }
        }

        /// <summary>
        /// Modelos de rango inferior
        /// </summary>
        public override IList<BaseElementViewModel> SubModels
        {
            get
            {
                return this._yearLevel.MonthModels.Select(monthModel => new MonthViewModel(monthModel)).Cast<BaseElementViewModel>().ToList();
            }
        }
    }
}
