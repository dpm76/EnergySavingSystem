using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ConsumptionModelling.Model;

namespace AdvisorTool
{
    /// <summary>
    /// Lógica de interacción para WeekModelEditor.xaml
    /// </summary>
    public partial class WeekModelEditor : UserControl
    {
        private readonly Dictionary<DayOfWeek, CheckBox> _dayOfWeek2CheckBoxDictionary;
        private readonly Dictionary<DayOfWeek, Button> _dayOfWeek2ButtonDictionary;
        private readonly Dictionary<DayOfWeek, WeekDayLevel> _dayOfWeek2WeekDayLevelDictionary = new Dictionary<DayOfWeek, WeekDayLevel>(7);

        private HolidayLevel _holidayModel;
        private WorkingLevel _workingModel;

        private WeekLevel _weekModel;

        public event EventHandler<DayModelEventArgs> DaySelected;

        public WeekLevel WeekModel
        {
            get { return this._weekModel; }
            set
            {
                this._weekModel = value;
                this.UpdateData();
            }
        }

        public WeekModelEditor()
        {
            InitializeComponent();

            this._dayOfWeek2CheckBoxDictionary =
                new Dictionary<DayOfWeek, CheckBox>(7)
                    {
                        {DayOfWeek.Monday, this.MondayCheckBox},
                        {DayOfWeek.Tuesday, this.TuesdayCheckBox},
                        {DayOfWeek.Wednesday, this.WednesdayCheckBox},
                        {DayOfWeek.Thursday, this.ThursdayCheckBox},
                        {DayOfWeek.Friday, this.FridayCheckBox},
                        {DayOfWeek.Saturday, this.SaturdayCheckBox},
                        {DayOfWeek.Sunday, this.SundayCheckBox}
                    };

            foreach (KeyValuePair<DayOfWeek, CheckBox> keyValuePair in _dayOfWeek2CheckBoxDictionary)
            {
                keyValuePair.Value.Tag = keyValuePair.Key;
            }

            this._dayOfWeek2ButtonDictionary =
                new Dictionary<DayOfWeek, Button>(7)
                    {
                        {DayOfWeek.Monday, this.MondayButton},
                        {DayOfWeek.Tuesday, this.TuesdayButton},
                        {DayOfWeek.Wednesday, this.WednesdayButton},
                        {DayOfWeek.Thursday, this.ThursdayButton},
                        {DayOfWeek.Friday, this.FridayButton},
                        {DayOfWeek.Saturday, this.SaturdayButton},
                        {DayOfWeek.Sunday, this.SundayButton}
                    };

            foreach (KeyValuePair<DayOfWeek, Button> keyValuePair in _dayOfWeek2ButtonDictionary)
            {
                keyValuePair.Value.Tag = keyValuePair.Key;
            }

            
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                this._dayOfWeek2WeekDayLevelDictionary.Add(dayOfWeek, null);
            }

        }

        protected override void OnVisualParentChanged(System.Windows.DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (this.Parent != null)
            {
                this.Width = double.NaN;
                this.Height = double.NaN;
            }
        }

        private void UpdateData()
        {
            this.Clear();

            //Actualizar modelos de día de semana
            foreach (WeekDayLevel weekDayModel in this.WeekModel.WeekDayModels)
            {
                this._dayOfWeek2WeekDayLevelDictionary[weekDayModel.DayOfWeek] = weekDayModel;
                this._dayOfWeek2CheckBoxDictionary[weekDayModel.DayOfWeek].IsChecked = true;
            }

            //Actualizar laborables y festivo
            this._workingModel = this.WeekModel.WorkingModel;
            this._holidayModel = this.WeekModel.HolidayModel;
            this.WorkingDayModelCheckBox.IsChecked = (this._weekModel != null);
            this.HolidayDayModelCheckBox.IsChecked = (this._holidayModel != null);
        }

        public void Clear()
        {
            foreach (CheckBox checkBox in _dayOfWeek2CheckBoxDictionary.Values)
            {
                checkBox.IsChecked = false;
            }
            foreach (Button button in _dayOfWeek2ButtonDictionary.Values)
            {
                button.IsEnabled = false;
            }

            DayOfWeek[] daysOfWeek = new DayOfWeek[7];
            this._dayOfWeek2WeekDayLevelDictionary.Keys.CopyTo(daysOfWeek, 0);
            foreach (DayOfWeek dayOfWeek in daysOfWeek)
            {
                this._dayOfWeek2WeekDayLevelDictionary[dayOfWeek] = null;
            }

            this._workingModel = null;
            this._holidayModel = null;
        }

        private void OnModelSelected(DayLevel dayModel)
        {
            EventHandler<DayModelEventArgs> temp = this.DaySelected;
            if(temp != null)
            {
                temp(this, new DayModelEventArgs(dayModel));
            }
        }

        private void OnDayOfWeekButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnModelSelected(this._dayOfWeek2WeekDayLevelDictionary[(DayOfWeek)((Button)sender).Tag]);
        }

        private void OnDayOfWeekCheckBoxChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            DayOfWeek dayOfWeek = (DayOfWeek)((CheckBox) sender).Tag;
            this._dayOfWeek2ButtonDictionary[dayOfWeek].IsEnabled = true;
            if (this._dayOfWeek2WeekDayLevelDictionary[dayOfWeek] == null)
            {
                this._dayOfWeek2WeekDayLevelDictionary[dayOfWeek] = new WeekDayLevel { DayOfWeek = dayOfWeek };
            }
            if (!this.WeekModel.WeekDayModels.Contains(this._dayOfWeek2WeekDayLevelDictionary[dayOfWeek]))
            {
                this.WeekModel.WeekDayModels.Add(this._dayOfWeek2WeekDayLevelDictionary[dayOfWeek]);
            }

        }

        private void OnDayOfWeekCheckBoxUnchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            DayOfWeek dayOfWeek = (DayOfWeek)((CheckBox)sender).Tag;
            this._dayOfWeek2ButtonDictionary[dayOfWeek].IsEnabled = false;
            if (this.WeekModel.WeekDayModels.Contains(this._dayOfWeek2WeekDayLevelDictionary[dayOfWeek]))
            {
                this.WeekModel.WeekDayModels.Remove(this._dayOfWeek2WeekDayLevelDictionary[dayOfWeek]);
            }
        }

        private void OnDefaultDayModelButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnModelSelected(this.WeekModel.DefaultDayModel);
        }

        private void OnWorkingDayModelCheckBoxChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WorkingDayModelButton.IsEnabled = true;
            if(this._workingModel == null)
            {
                this._workingModel = new WorkingLevel();
            }
            this.WeekModel.WorkingModel = this._workingModel;
        }

        private void OnWorkingDayModelCheckBoxUnchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WorkingDayModelButton.IsEnabled = false;
            this.WeekModel.WorkingModel = null;
        }

        private void OnHolidayDayModelCheckBoxChecked(object sender, System.Windows.RoutedEventArgs e)
        {

            this.HolidayModelButton.IsEnabled = true;
            if (this._holidayModel == null)
            {
                this._holidayModel = new HolidayLevel();
            }
            this.WeekModel.HolidayModel = this._holidayModel;
        }

        private void OnHolidayDayModelCheckBoxUnchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.HolidayModelButton.IsEnabled = false;
            this.WeekModel.HolidayModel = null;
        }

        private void OnWorkingDayModelButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnModelSelected(this._workingModel);
        }

        private void OnHolidayModelButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnModelSelected(this._holidayModel);
        }

       
    }
}
