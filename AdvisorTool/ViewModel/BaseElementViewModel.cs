using System.Collections.Generic;
using System.ComponentModel;
using ConsumptionModelling.Model;

namespace AdvisorTool.ViewModel
{
    public abstract class BaseElementViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private bool _isSelected;
        private bool _isExpanded;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Descripción textual del modelo representado
        /// </summary>
        public abstract string ModelName { get; }

        /// <summary>
        /// Elemento del modelo representado
        /// </summary>
        public abstract IModel ModelElement { get; }

        /// <summary>
        /// Modelos de rango inferior
        /// </summary>
        public abstract IList<BaseElementViewModel> SubModels { get; }

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected
    }
}
