using System;
using ConsumptionModelling.Model;

namespace AdvisorTool
{
    public class ModelEventArgs:EventArgs
    {
        private readonly IModel _model;

        public ModelEventArgs(IModel model)
        {
            this._model = model;
        }

        public IModel Model
        {
            get { return _model; }
        }
    }
}
