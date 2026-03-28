using Craft.UI.Utils;
using Craft.Utils;
using Craft.ViewModels.Dialogs;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PR.ViewModel.GIS
{
    public class CreateObservingFacilityDialogViewModel : DialogViewModelBase, IDataErrorInfo
    {
        private StateOfView _state;
        private ObservableCollection<ValidationError> _validationMessages;
        private string _error = string.Empty;

        private string _name;
        private double _latitude;
        private double _longitude;
        private DateTime _from;
        private DateTime? _to;

        // These are for limiting options for the DatePicker controls
        private DateTime _displayDateStart_DateFrom;
        private DateTime _displayDateEnd_DateFrom;
        private DateTime _displayDateStart_DateTo;
        private DateTime _displayDateEnd_DateTo;
        private bool _datePickerForToDateEnabled;

        private RelayCommand<object> _okCommand;
        private RelayCommand<object> _cancelCommand;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public DateTime From
        {
            get { return _from; }
            set
            {
                _from = value;
                RaisePropertyChanged();
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

        public double Latitude
        {
            get { return _latitude; }
            set
            {
                _latitude = value;
                RaisePropertyChanged();
            }
        }

        public double Longitude
        {
            get { return _longitude; }
            set
            {
                _longitude = value;
                RaisePropertyChanged();
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

        public RelayCommand<object> OKCommand
        {
            get { return _okCommand ?? (_okCommand = new RelayCommand<object>(OK, CanOK)); }
        }

        public RelayCommand<object> CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand<object>(Cancel, CanCancel)); }
        }

        public CreateObservingFacilityDialogViewModel(
            Point mousePositionWorld)
        {
            var currentDate = DateTime.Now.Date;
            From = currentDate;
            Latitude = Math.Round(mousePositionWorld.X, 4);
            Longitude = -Math.Round(mousePositionWorld.Y, 4);

            UpdateDatePickerRanges();
        }

        private void OK(object parameter)
        {
            UpdateState(StateOfView.Updated);

            Error = string.Join("", ValidationMessages.Select(e => e.ErrorMessage).ToArray());

            if (!string.IsNullOrEmpty(Error))
            {
                return;
            }

            Name = Name.NullifyIfEmpty();

            // In the database, we represent a missing to date with the maxDate value
            if (!To.HasValue)
            {
                To = DateTime.MaxValue;
            }

            CloseDialogWithResult(parameter as Window, DialogResult.OK);
        }

        private bool CanOK(object parameter)
        {
            return true;
        }

        private void Cancel(object parameter)
        {
            CloseDialogWithResult(parameter as Window, DialogResult.Cancel);
        }

        private bool CanCancel(object parameter)
        {
            return true;
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
                        case "Name":
                            {
                                if (string.IsNullOrEmpty(Name))
                                {
                                    errorMessage = "Name is required";
                                }
                                else if (Name.Length > 127)
                                {
                                    errorMessage = "Name cannot exceed 127 characters";
                                }

                                break;
                            }
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
                    new ValidationError {PropertyName = "Name"},
                    new ValidationError {PropertyName = "Latitude"},
                    new ValidationError {PropertyName = "Longitude"},
                    new ValidationError {PropertyName = "From"},
                    new ValidationError {PropertyName = "To"},
                };
                }

                return _validationMessages;
            }
        }

        private void RaisePropertyChanges()
        {
            RaisePropertyChanged("Name");
            RaisePropertyChanged("Latitude");
            RaisePropertyChanged("Longitude");
            RaisePropertyChanged("From");
            RaisePropertyChanged("To");
        }

        private void UpdateState(StateOfView state)
        {
            _state = state;
            RaisePropertyChanges();
        }

        private void UpdateDatePickerRanges()
        {
            var currentDate = DateTime.UtcNow.Date;
            DisplayDateEnd_DateFrom = To.HasValue ? To.Value - TimeSpan.FromDays(1) : currentDate;
            DisplayDateStart_DateTo = From.Date + TimeSpan.FromDays(1);
            DisplayDateEnd_DateTo = currentDate;
        }
    }
}