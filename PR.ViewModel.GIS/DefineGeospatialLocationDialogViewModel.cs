using Craft.UI.Utils;
using Craft.ViewModels.Dialogs;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace PR.ViewModel.GIS
{
    public enum DefineGeospatialLocationMode
    {
        Create,
        Update
    }

    public class DefineGeospatialLocationDialogViewModel : DialogViewModelBase, IDataErrorInfo
    {
        private StateOfView _state;
        private ObservableCollection<ValidationError> _validationMessages;
        private string _error = string.Empty;

        private DefineGeospatialLocationMode _mode;

        private string _latitude;
        private string _longitude;

        private DateTime _from;
        private DateTime? _to;

        // These are for limiting options for the DatePicker controls
        private DateTime _displayDateStart_DateFrom;
        private DateTime _displayDateEnd_DateFrom;
        private DateTime _displayDateStart_DateTo;
        private DateTime _displayDateEnd_DateTo;
        private bool _datePickerForToDateEnabled;

        private string _originalLatitude;
        private string _originalLongitude;
        private DateTime _originalDateFrom;
        private DateTime? _originalDateTo;

        private RelayCommand<object> _okCommand;
        private RelayCommand<object> _cancelCommand;

        public string Latitude
        {
            get { return _latitude; }
            set
            {
                _latitude = value;
                RaisePropertyChanged();
                OKCommand.RaiseCanExecuteChanged();
            }
        }

        public string Longitude
        {
            get { return _longitude; }
            set
            {
                _longitude = value;
                RaisePropertyChanged();
                OKCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime From
        {
            get { return _from; }
            set
            {
                _from = value;
                RaisePropertyChanged();
                OKCommand.RaiseCanExecuteChanged();
                DatePickerForToDateEnabled = _from < DateTime.UtcNow.Date;
                UpdateDatePickerRanges();
            }
        }

        public DateTime? To
        {
            get { return _to; }
            set
            {
                _to = value;
                RaisePropertyChanged();
                OKCommand.RaiseCanExecuteChanged();
                UpdateDatePickerRanges();
            }
        }

        public DateTime DisplayDateStart_DateFrom
        {
            get => _displayDateStart_DateFrom;
            set
            {
                _displayDateStart_DateFrom = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DisplayDateEnd_DateFrom
        {
            get => _displayDateEnd_DateFrom;
            set
            {
                _displayDateEnd_DateFrom = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DisplayDateStart_DateTo
        {
            get => _displayDateStart_DateTo;
            set
            {
                _displayDateStart_DateTo = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DisplayDateEnd_DateTo
        {
            get => _displayDateEnd_DateTo;
            set
            {
                _displayDateEnd_DateTo = value;
                RaisePropertyChanged();
            }
        }

        public bool DatePickerForToDateEnabled
        {
            get => _datePickerForToDateEnabled;
            set
            {
                _datePickerForToDateEnabled = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand<object> OKCommand
        {
            get { return _okCommand ?? (_okCommand = new RelayCommand<object>(OK, CanOK)); }
        }

        public RelayCommand<object> CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand<object>(Cancel, CanCancel)); }
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;

                if (_state == StateOfView.Updated)
                {
                    switch (columnName)
                    {
                        case "Latitude":
                            {
                                break;
                            }
                        case "Longitude":
                            {
                                break;
                            }
                    }
                }

                ValidationMessages
                    .First(e => e.PropertyName == columnName).ErrorMessage = errorMessage;

                return errorMessage;
            }
        }

        public ObservableCollection<ValidationError> ValidationMessages
        {
            get
            {
                if (_validationMessages == null)
                {
                    _validationMessages = new ObservableCollection<ValidationError>
                    {
                        new ValidationError {PropertyName = "Latitude"},
                        new ValidationError {PropertyName = "Longitude"},
                        new ValidationError {PropertyName = "From"},
                        new ValidationError {PropertyName = "To"},
                    };
                }

                return _validationMessages;
            }
        }

        public string Error
        {
            get { return _error; }
            set
            {
                _error = value;
                RaisePropertyChanged();
            }
        }

        public DefineGeospatialLocationDialogViewModel(
            DefineGeospatialLocationMode mode,
            double latitude,
            double longitude,
            DateTime from,
            DateTime? to)
        {
            _mode = mode;
            Latitude = latitude.ToString(CultureInfo.InvariantCulture);
            Longitude = longitude.ToString(CultureInfo.InvariantCulture);
            From = from;
            To = to;

            _originalLatitude = Latitude;
            _originalLongitude = Longitude;
            _originalDateFrom = From;
            _originalDateTo = To;

            UpdateDatePickerRanges();
        }

        private void UpdateDatePickerRanges()
        {
            var currentDate = DateTime.UtcNow.Date;
            DisplayDateEnd_DateFrom = To.HasValue ? To.Value - TimeSpan.FromDays(1) : currentDate;
            DisplayDateStart_DateTo = From.Date + TimeSpan.FromDays(1);
            DisplayDateEnd_DateTo = currentDate;
        }

        private void UpdateState(
            StateOfView state)
        {
            _state = state;
            RaisePropertyChanges();
        }

        private void RaisePropertyChanges()
        {
            RaisePropertyChanged("Latitude");
            RaisePropertyChanged("Longitude");
            RaisePropertyChanged("From");
            RaisePropertyChanged("To");
        }

        private void OK(
            object parameter)
        {
            UpdateState(StateOfView.Updated);

            Error = string.Join("",
                ValidationMessages.Select(e => e.ErrorMessage).ToArray());

            if (!string.IsNullOrEmpty(Error))
            {
                return;
            }

            CloseDialogWithResult(parameter as Window, DialogResult.OK);
        }

        private bool CanOK(
            object parameter)
        {
            return _mode == DefineGeospatialLocationMode.Create ||
                   Latitude != _originalLatitude ||
                   Longitude != _originalLongitude ||
                   From != _originalDateFrom ||
                   To != _originalDateTo;
        }

        private void Cancel(
            object parameter)
        {
            CloseDialogWithResult(parameter as Window, DialogResult.Cancel);
        }

        private bool CanCancel(
            object parameter)
        {
            return true;
        }
    }
}
