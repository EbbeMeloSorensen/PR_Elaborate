using Craft.Utils;
using Craft.ViewModel.Utils;
using Craft.ViewModels.Dialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PR.Persistence;
using PR.ViewModel.GIS.Domain;

namespace PR.ViewModel.GIS
{
    public class GeospatialLocationsViewModel : ViewModelBase
    {
        private readonly IDialogService _applicationDialogService;
        private readonly ObservableObject<DateTime?> _databaseTimeOfInterest;

        private ObjectCollection<ObservingFacility> _observingFacilities;
        private ObservingFacility _selectedObservingFacility;
        private ObservableCollection<GeospatialLocationListItemViewModel> _geospatialLocationListItemViewModels;
        private RelayCommand<object> _selectionChangedCommand;
        private AsyncCommand<object> _deleteSelectedGeospatialLocationsCommand;
        private RelayCommand<object> _createGeospatialLocationCommand;
        private AsyncCommand<object> _updateSelectedGeospatialLocationCommand;

        public IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        public ObjectCollection<GeospatialLocation> SelectedGeospatialLocations { get; private set; }

        public RelayCommand<object> SelectionChangedCommand
        {
            get { return _selectionChangedCommand ?? (_selectionChangedCommand = new RelayCommand<object>(SelectionChanged)); }
        }

        public ObservableCollection<GeospatialLocationListItemViewModel> GeospatialLocationListItemViewModels
        {
            get { return _geospatialLocationListItemViewModels; }
            set
            {
                _geospatialLocationListItemViewModels = value;
                RaisePropertyChanged();
            }
        }

        public AsyncCommand<object> DeleteSelectedGeospatialLocationsCommand
        {
            get
            {
                return _deleteSelectedGeospatialLocationsCommand ?? (
                    _deleteSelectedGeospatialLocationsCommand = new AsyncCommand<object>(DeleteSelectedGeospatialLocations, CanDeleteSelectedGeospatialLocations));
            }
        }

        public RelayCommand<object> CreateGeospatialLocationCommand
        {
            get
            {
                return _createGeospatialLocationCommand ?? (
                    _createGeospatialLocationCommand = new RelayCommand<object>(CreateGeospatialLocation, CanCreateGeospatialLocation));
            }
        }

        public AsyncCommand<object> UpdateSelectedGeospatialLocationCommand
        {
            get
            {
                return _updateSelectedGeospatialLocationCommand ?? (
                    _updateSelectedGeospatialLocationCommand = new AsyncCommand<object>(UpdateSelectedGeospatialLocation, CanUpdateSelectedGeospatialLocation));
            }
        }

        public event EventHandler<CommandInvokedEventArgs> NewGeospatialLocationCalledByUser;
        public event EventHandler<DatabaseWriteOperationOccuredEventArgs> GeospatialLocationsUpdatedOrDeleted;

        public GeospatialLocationsViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            IDialogService applicationDialogService,
            ObservableObject<DateTime?> databaseTimeOfInterest,
            ObjectCollection<ObservingFacility> observingFacilities)
        {
            UnitOfWorkFactory = unitOfWorkFactory;
            _applicationDialogService = applicationDialogService;
            _observingFacilities = observingFacilities;
            _databaseTimeOfInterest = databaseTimeOfInterest;

            SelectedGeospatialLocations = new ObjectCollection<GeospatialLocation>
            {
                Objects = new List<GeospatialLocation>()
            };

            _observingFacilities.PropertyChanged += async (s, e) =>
            {
                await Initialize(s, e);
            };

            _databaseTimeOfInterest.PropertyChanged += (s, e) =>
            {
                CreateGeospatialLocationCommand.RaiseCanExecuteChanged();
                UpdateSelectedGeospatialLocationCommand.RaiseCanExecuteChanged();
                DeleteSelectedGeospatialLocationsCommand.RaiseCanExecuteChanged();
            };
        }

        private async Task Initialize(object sender, PropertyChangedEventArgs e)
        {
            var temp = sender as ObjectCollection<ObservingFacility>;

            if (temp != null && temp.Objects != null && temp.Objects.Count() == 1)
            {
                _selectedObservingFacility = temp.Objects.Single();
                await Populate();
            }
            else
            {
                _selectedObservingFacility = null;
            }
        }

        public async Task Populate()
        {
            using (var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork())
            {
                // Denne finder altså kun den ene, der svarer til historisk tid..
                //var people = await unitOfWork.People.Find(_ => _.ID == _selectedObservingFacility.Id);

                var personVariants = await unitOfWork.People.GetAllVariants(_selectedObservingFacility.Id);

                var temp = personVariants.Select(_ => new Domain.Point
                {
                    From = _.Start,
                    To = _.End,
                    Name = _.FirstName,
                    Coordinate1 = _.Latitude!.Value,
                    Coordinate2 = _.Longitude!.Value
                });

                GeospatialLocationListItemViewModels = new ObservableCollection<GeospatialLocationListItemViewModel>(
                    temp.Select(_ => new GeospatialLocationListItemViewModel(_))
                        .OrderBy(_ => _.From));
            }
        }

        private void CreateGeospatialLocation(
            object owner)
        {
            OnNewGeospatialLocationCalledByUser(owner);
        }

        private bool CanCreateGeospatialLocation(
            object owner)
        {
            return !_databaseTimeOfInterest.Object.HasValue;
        }

        private async Task DeleteSelectedGeospatialLocations(
            object owner)
        {
            throw new NotImplementedException("Block removed for refactoring");
            //var nSelectedGeospatialLocations = SelectedGeospatialLocations.Objects.Count();

            //var message = nSelectedGeospatialLocations == 1
            //    ? "Delete location?"
            //    : $"Delete {nSelectedGeospatialLocations} locations?";

            //var dialogViewModel = new MessageBoxDialogViewModel(message, true);

            //if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) == DialogResult.Cancel)
            //{
            //    return;
            //}

            //var objectIds = SelectedGeospatialLocations.Objects
            //    .Select(_ => _.ObjectId)
            //    .ToList();

            //var geospatialLocationsRemaining = GeospatialLocationListItemViewModels
            //    .Select(_ => _.GeospatialLocation)
            //    .Where(_ => !objectIds.Contains(_.ObjectId))
            //    .ToList();

            //var dateEstablishedAfter = geospatialLocationsRemaining.Min(_ => _.From);
            //var dateClosedAfter = geospatialLocationsRemaining.Max(_ => _.To);

            //var now = DateTime.UtcNow;


            //throw new NotImplementedException("Block removed for refactoring");
            //using (var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork())
            //{
            //    var geospatialLocationsForDeletion = (await unitOfWork.GeospatialLocations
            //        .Find(_ => _.Superseded == DateTime.MaxValue && objectIds.Contains(_.ObjectId)))
            //        .ToList();

            //    geospatialLocationsForDeletion.ForEach(_ => _.Superseded = now);

            //    if (dateEstablishedAfter > _selectedObservingFacility.DateEstablished ||
            //        dateClosedAfter < _selectedObservingFacility.DateClosed)
            //    {
            //        // Update active period of observing facility
            //        var observingFacilityFromRepo = await unitOfWork.ObservingFacilities.Get(_selectedObservingFacility.Id);

            //        observingFacilityFromRepo.Superseded = now;
            //        await unitOfWork.ObservingFacilities.Update(observingFacilityFromRepo);

            //        var newObservingFacility = new ObservingFacility(Guid.NewGuid(), now)
            //        {
            //            ObjectId = observingFacilityFromRepo.ObjectId,
            //            Name = observingFacilityFromRepo.Name,
            //            DateEstablished = dateEstablishedAfter > observingFacilityFromRepo.DateEstablished
            //                ? dateEstablishedAfter
            //                : observingFacilityFromRepo.DateEstablished,
            //            DateClosed = dateClosedAfter < observingFacilityFromRepo.DateClosed
            //                ? dateClosedAfter
            //                : observingFacilityFromRepo.DateClosed
            //        };

            //        await unitOfWork.ObservingFacilities.Add(newObservingFacility);
            //    }

            //    await unitOfWork.GeospatialLocations.UpdateRange(geospatialLocationsForDeletion);
            //    unitOfWork.Complete();
            //}

            //OnGeospatialLocationsUpdatedOrDeleted(now);
        }

        private bool CanDeleteSelectedGeospatialLocations(
            object owber)
        {
            return !_databaseTimeOfInterest.Object.HasValue &&
                   //SelectedGeospatialLocations.Objects != null &&
                   SelectedGeospatialLocations.Objects.Any() &&
                   SelectedGeospatialLocations.Objects.Count() != _geospatialLocationListItemViewModels.Count;
        }

        private async Task UpdateSelectedGeospatialLocation(
            object owner)
        {
            var point = SelectedGeospatialLocations.Objects.Single() as Domain.Point;

            var dialogViewModel = new DefineGeospatialLocationDialogViewModel(
                DefineGeospatialLocationMode.Update,
                point.Coordinate1,
                point.Coordinate2,
                point.From,
                point.To == DateTime.MaxValue ? null : point.To);

            if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) != DialogResult.OK)
            {
                return;
            }

            var from = new DateTime(
                dialogViewModel.From.Year,
                dialogViewModel.From.Month,
                dialogViewModel.From.Day,
                dialogViewModel.From.Hour,
                dialogViewModel.From.Minute,
                dialogViewModel.From.Second,
                DateTimeKind.Utc);

            var to = dialogViewModel.To.HasValue
                ? dialogViewModel.To == DateTime.MaxValue
                    ? DateTime.MaxValue
                    : new DateTime(
                        dialogViewModel.To.Value.Year,
                        dialogViewModel.To.Value.Month,
                        dialogViewModel.To.Value.Day,
                        dialogViewModel.To.Value.Hour,
                        dialogViewModel.To.Value.Minute,
                        dialogViewModel.To.Value.Second,
                        DateTimeKind.Utc)
                : DateTime.MaxValue;

            var latitude = double.Parse(dialogViewModel.Latitude, CultureInfo.InvariantCulture);
            var longitude = double.Parse(dialogViewModel.Longitude, CultureInfo.InvariantCulture);

            var now = DateTime.UtcNow;

            throw new NotImplementedException("Block removed for refactoring");
            //using (var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork())
            //{
            //    var geospatialLocation = unitOfWork.GeospatialLocations.Get(point.Id);
            //    geospatialLocation.Superseded = now;

            //    await unitOfWork.GeospatialLocations.Update(geospatialLocation);

            //    var newPoint = new Point(Guid.NewGuid(), now)
            //    {
            //        ObjectId = geospatialLocation.ObjectId,
            //        AbstractEnvironmentalMonitoringFacilityId = _selectedObservingFacility.Id,
            //        AbstractEnvironmentalMonitoringFacilityObjectId = _selectedObservingFacility.ObjectId,
            //        From = from,
            //        To = to,
            //        Coordinate1 = latitude,
            //        Coordinate2 = longitude,
            //        CoordinateSystem = "WGS_84",
            //    };

            //    await unitOfWork.GeospatialLocations.Add(newPoint);

            //    // Determine if we should change the DateEstablished/DateClosed range of the observing facility
            //    var geospatialLocationPredicates = new List<Expression<Func<GeospatialLocation, bool>>>
            //    {
            //        _ => _.Superseded == DateTime.MaxValue,
            //        _ => _.ObjectId != geospatialLocation.ObjectId
            //    };

            //    var otherGeospatialLocations =
            //        (await unitOfWork.ObservingFacilities.GetIncludingGeospatialLocations(
            //            _selectedObservingFacility.Id,
            //            geospatialLocationPredicates)).Item2;

            //    var minFromDate = otherGeospatialLocations
            //        .Select(_ => _.From)
            //        .Append(newPoint.From)
            //        .Min();

            //    var maxToDate = otherGeospatialLocations
            //        .Select(_ => _.To)
            //        .Append(newPoint.To)
            //        .Max();

            //    if (minFromDate != _selectedObservingFacility.DateEstablished ||
            //        maxToDate != _selectedObservingFacility.DateClosed)
            //    {
            //        var observingFacilityFromRepo = await unitOfWork.ObservingFacilities.Get(_selectedObservingFacility.Id);
            //        observingFacilityFromRepo.Superseded = now;

            //        await unitOfWork.ObservingFacilities.Update(observingFacilityFromRepo);

            //        var newObservingFacility = new ObservingFacility(Guid.NewGuid(), now)
            //        {
            //            Name = observingFacilityFromRepo.Name,
            //            ObjectId = observingFacilityFromRepo.ObjectId,
            //            DateEstablished = minFromDate,
            //            DateClosed = maxToDate
            //        };

            //        await unitOfWork.ObservingFacilities.Add(newObservingFacility);
            //    }

            //    unitOfWork.Complete();
            //}

            OnGeospatialLocationsUpdatedOrDeleted(now);
        }

        private bool CanUpdateSelectedGeospatialLocation(
            object owner)
        {
            return !_databaseTimeOfInterest.Object.HasValue &&
                   //SelectedGeospatialLocations.Objects != null &&
                   SelectedGeospatialLocations.Objects.Count() == 1;
        }

        private void SelectionChanged(
            object obj)
        {
            var temp = (IList)obj;

            var selectedGeospatialLocationListItemViewModels =
                temp.Cast<GeospatialLocationListItemViewModel>();

            SelectedGeospatialLocations.Objects = selectedGeospatialLocationListItemViewModels.Select(_ => _.GeospatialLocation);

            DeleteSelectedGeospatialLocationsCommand.RaiseCanExecuteChanged();
            UpdateSelectedGeospatialLocationCommand.RaiseCanExecuteChanged();
        }

        private void OnNewGeospatialLocationCalledByUser(
            object owner)
        {
            var handler = NewGeospatialLocationCalledByUser;

            if (handler != null)
            {
                handler(this, new CommandInvokedEventArgs(owner));
            }
        }

        private void OnGeospatialLocationsUpdatedOrDeleted(
            DateTime dateTime)
        {
            var handler = GeospatialLocationsUpdatedOrDeleted;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                handler(this, new DatabaseWriteOperationOccuredEventArgs(dateTime));
            }
        }
    }
}
