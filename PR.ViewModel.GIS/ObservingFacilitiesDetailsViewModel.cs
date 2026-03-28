using Craft.UI.Utils;
using Craft.Utils;
using Craft.ViewModel.Utils;
using Craft.ViewModels.Dialogs;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using PR.Persistence;
using PR.ViewModel.GIS.Domain;
using StateOfView = Craft.UI.Utils.StateOfView;

namespace PR.ViewModel.GIS
{
    public class ObservingFacilitiesDetailsViewModel : ViewModelBase, IDataErrorInfo
    {
        private StateOfView _state;
        private ObservableCollection<ValidationError> _validationMessages;
        private string _error = string.Empty;

        private ObjectCollection<ObservingFacility> _observingFacilities;

        private string _originalSharedName;
        private DateTime? _originalSharedDateEstablished;
        private DateTime? _originalSharedDateClosed;

        private string _sharedName;
        private DateTime? _sharedDateEstablished;
        private DateTime? _sharedDateClosed;
        private string _sharedDateEstablishedAsText;
        private string _sharedDateClosedAsText;

        private DateTime _displayDateStart_DateEstablished;
        private DateTime _displayDateEnd_DateEstablished;
        private DateTime _displayDateStart_DateClosed;
        private DateTime _displayDateEnd_DateClosed;

        private bool _isVisible;
        private bool _isReadOnly;

        private AsyncCommand _applyChangesCommand;

        // Block removed for refactoring
        //public event EventHandler<ObservingFacilitiesEventArgs> ObservingFacilitiesUpdated;

        public string SharedName
        {
            get { return _sharedName; }
            set
            {
                _sharedName = value;
                RaisePropertyChanged();
                ApplyChangesCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? SharedDateEstablished
        {
            get { return _sharedDateEstablished; }
            set
            {
                _sharedDateEstablished = value;
                RaisePropertyChanged();

                SharedDateEstablishedAsText = _sharedDateEstablished.HasValue
                    ? _sharedDateEstablished.Value.AsDateString()
                    : "";

                ApplyChangesCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? SharedDateClosed
        {
            get { return _sharedDateClosed; }
            set
            {
                _sharedDateClosed = value;
                RaisePropertyChanged();

                SharedDateClosedAsText = _sharedDateClosed.HasValue
                    ? _sharedDateClosed.Value.AsDateString()
                    : "";

                ApplyChangesCommand.RaiseCanExecuteChanged();
            }
        }

        public string SharedDateEstablishedAsText
        {
            get { return _sharedDateEstablishedAsText; }
            set
            {
                _sharedDateEstablishedAsText = value;
                RaisePropertyChanged();
            }
        }

        public string SharedDateClosedAsText
        {
            get { return _sharedDateClosedAsText; }
            set
            {
                _sharedDateClosedAsText = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DisplayDateStart_DateEstablished
        {
            get => _displayDateStart_DateEstablished;
            set
            {
                _displayDateStart_DateEstablished = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DisplayDateEnd_DateEstablished
        {
            get => _displayDateEnd_DateEstablished;
            set
            {
                _displayDateEnd_DateEstablished = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DisplayDateStart_DateClosed
        {
            get => _displayDateStart_DateClosed;
            set
            {
                _displayDateStart_DateClosed = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DisplayDateEnd_DateClosed
        {
            get => _displayDateEnd_DateClosed;
            set
            {
                _displayDateEnd_DateClosed = value;
                RaisePropertyChanged();
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                RaisePropertyChanged();
            }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                RaisePropertyChanged();
            }
        }

        private IUnitOfWorkFactory _unitOfWorkFactory;

        public IUnitOfWorkFactory UnitOfWorkFactory
        {
            get => _unitOfWorkFactory;
            set
            {
                _unitOfWorkFactory = value;
                GeospatialLocationsViewModel.UnitOfWorkFactory = value;
            }
        }

        public GeospatialLocationsViewModel GeospatialLocationsViewModel { get; }

        public AsyncCommand ApplyChangesCommand
        {
            get { return _applyChangesCommand ?? (_applyChangesCommand = new AsyncCommand(ApplyChanges, CanApplyChanges)); }
        }

        public ObservingFacilitiesDetailsViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            IDialogService applicationDialogService,
            ObservableObject<DateTime?> databaseTimeOfInterest,
            ObjectCollection<ObservingFacility> observingFacilities)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _observingFacilities = observingFacilities;

            _observingFacilities.PropertyChanged += Initialize;

            GeospatialLocationsViewModel = new GeospatialLocationsViewModel(
                unitOfWorkFactory,
                applicationDialogService,
                databaseTimeOfInterest,
                observingFacilities);
        }

        private void Initialize(
            object sender,
            PropertyChangedEventArgs e)
        {
            _state = StateOfView.Initial;
            var temp = sender as ObjectCollection<ObservingFacility>;

            var firstObservingFacility = temp?.Objects.FirstOrDefault();

            if (firstObservingFacility == null)
            {
                IsVisible = false;
                return;
            }

            IsVisible = true;

            // If the observing facilities have the same name then show the shared name, otherwise leave the field empty
            SharedName = temp.Objects.All(_ => _.Name == firstObservingFacility.Name)
                ? firstObservingFacility.Name
                : null;

            var now = DateTime.Now;

            // Dette er for at sikre, at den foreslår dags dato, hvis man selecter den
            SharedDateEstablished = now;
            SharedDateClosed = now;

            // If the observing facilities were established the same date then show the shared date, otherwise leave the field empty
            SharedDateEstablished = temp.Objects.All(_ => _.DateEstablished == firstObservingFacility.DateEstablished)
                ? firstObservingFacility.DateEstablished
                : null;

            // Determine valid options for changing the Establishing date for the selected observing facilities
            var earliestDateClosed = temp.Objects.Min(_ => _.DateClosed);
            var latestDateEstablished = temp.Objects.Max(_ => _.DateEstablished);

            DisplayDateStart_DateEstablished = earliestDateClosed;
            DisplayDateStart_DateClosed = latestDateEstablished;
            DisplayDateEnd_DateClosed = now;

            var sharedDateClosed = temp.Objects.All(_ => _.DateClosed == firstObservingFacility.DateClosed)
                ? firstObservingFacility.DateClosed
                : DateTime.MaxValue;

            SharedDateClosed = sharedDateClosed.Year != 9999
                ? SharedDateClosed = sharedDateClosed
                : null;

            _originalSharedName = SharedName;
            _originalSharedDateEstablished = SharedDateEstablished;
            _originalSharedDateClosed = SharedDateClosed;

            ApplyChangesCommand.RaiseCanExecuteChanged();
        }

        private async Task ApplyChanges()
        {
            UpdateState(StateOfView.Updated);

            Error = string.Join("",
                ValidationMessages.Select(e => e.ErrorMessage).ToArray());

            if (!string.IsNullOrEmpty(Error))
            {
                return;
            }

            throw new NotImplementedException("Block removed for refactoring");
            //using (var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork())
            //{
            //    var objectIds = _observingFacilities.Objects.Select(_ => _.ObjectId).ToList();

            //    var observingFacilitiesForUpdating = (await unitOfWork.ObservingFacilities
            //        .Find(_ => _.Superseded == DateTime.MaxValue && objectIds.Contains(_.ObjectId)))
            //        .ToList();

            //    var now = DateTime.UtcNow;

            //    observingFacilitiesForUpdating.ForEach(_ => _.Superseded = now);
            //    unitOfWork.ObservingFacilities.UpdateRange(observingFacilitiesForUpdating);

            //    var newObservingFacilityRows = observingFacilitiesForUpdating.Select(_ =>
            //    {
            //        return new ObservingFacility(_.ObjectId, now)
            //        {
            //            Name = SharedName != _originalSharedName ? SharedName : _.Name,
            //            DateEstablished = SharedDateEstablished != _originalSharedDateEstablished
            //                    ? new DateTime(
            //                        SharedDateEstablished.Value.Year,
            //                        SharedDateEstablished.Value.Month,
            //                        SharedDateEstablished.Value.Day,
            //                        0, 0, 0, DateTimeKind.Utc)
            //                    : _.DateEstablished,
            //            DateClosed = SharedDateClosed != _originalSharedDateClosed
            //                    ? new DateTime(
            //                        SharedDateClosed.Value.Year,
            //                        SharedDateClosed.Value.Month,
            //                        SharedDateClosed.Value.Day,
            //                        0, 0, 0, DateTimeKind.Utc)
            //                    : _.DateClosed
            //        };
            //    }).ToList();

            //    unitOfWork.ObservingFacilities.AddRange(newObservingFacilityRows);
            //    unitOfWork.Complete();

            //    OnObservingFacilitiesUpdated(newObservingFacilityRows);
            //}
        }

        private bool CanApplyChanges()
        {
            return
                SharedName != _originalSharedName ||
                SharedDateEstablished != _originalSharedDateEstablished ||
                SharedDateClosed != _originalSharedDateClosed;
        }

        public ObservableCollection<ValidationError> ValidationMessages
        {
            get
            {
                if (_validationMessages == null)
                {
                    _validationMessages = new ObservableCollection<ValidationError>
                {
                    new ValidationError {PropertyName = "SharedName"},
                    new ValidationError {PropertyName = "SharedDateEstablished"},
                    new ValidationError {PropertyName = "SharedDateClosed"}
                };
                }

                return _validationMessages;
            }
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
                        case "SharedName":
                            {
                                if (string.IsNullOrEmpty(SharedName))
                                {
                                    if (_observingFacilities.Objects.Count() == 1)
                                    {
                                        errorMessage = "Name is required";
                                    }
                                }
                                else if (SharedName.Length > 127)
                                {
                                    errorMessage = "Name cannot exceed 127 characters";
                                }

                                break;
                            }
                    }
                }

                ValidationMessages
                    .First(e => e.PropertyName == columnName).ErrorMessage = errorMessage;

                return errorMessage;
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

        private void RaisePropertyChanges()
        {
            RaisePropertyChanged("SharedName");
            RaisePropertyChanged("SharedDateEstablished");
            RaisePropertyChanged("SharedDateClosed");
        }

        private void UpdateState(StateOfView state)
        {
            _state = state;
            RaisePropertyChanges();
        }

        private void OnObservingFacilitiesUpdated(
            IEnumerable<ObservingFacility> observingFacilities)
        {
            throw new NotImplementedException("Block removed for refactoring");
            //// Make a temporary copy of the event to avoid possibility of
            //// a race condition if the last subscriber unsubscribes
            //// immediately after the null check and before the event is raised.
            //var handler = ObservingFacilitiesUpdated;

            //// Event will be null if there are no subscribers
            //if (handler != null)
            //{
            //    handler(this, new ObservingFacilitiesEventArgs(observingFacilities));
            //}
        }
    }
}