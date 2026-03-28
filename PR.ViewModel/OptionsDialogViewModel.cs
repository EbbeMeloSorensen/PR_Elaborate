using System;
using Craft.ViewModels.Dialogs;

namespace PR.ViewModel
{
    public class OptionsDialogViewModel : DialogViewModelBase
    {
        private DateTime? _historicalTime;
        private DateTime? _databaseTime;

        public DateTime? HistoricalTime
        {
            get { return _historicalTime; }
            set
            {
                _historicalTime = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? DatabaseTime
        {
            get { return _databaseTime; }
            set
            {
                _databaseTime = value;
                RaisePropertyChanged();
            }
        }

        public OptionsDialogViewModel(
            DateTime? historicalTime,
            DateTime? databaseTime)
        {
            HistoricalTime = historicalTime;
            DatabaseTime = databaseTime;
        }
    }
}
