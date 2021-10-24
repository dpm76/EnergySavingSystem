using System.Collections.Generic;
using ConsumptionModelling.Model;

namespace AdvisorTool.ViewModel
{
    public class ViewModelRoot
    {
        private readonly IList<YearViewModel> _yearModels = new List<YearViewModel>();

        public ViewModelRoot(YearLevel yearModel)
        {
            this._yearModels.Add(new YearViewModel(yearModel));
        }

        public IList<YearViewModel> SubModels
        {
            get { return this._yearModels; }
        }
    }
}
