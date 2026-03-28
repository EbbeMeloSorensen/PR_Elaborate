using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Craft.Logging;
using Craft.Utils;
using Craft.ViewModel.Utils;
using Craft.ViewModels.Dialogs;
using PR.Persistence;
using PR.ViewModel.GIS.Domain;
using PR.Domain.Entities.PR;

namespace PR.ViewModel.GIS
{
    public class ObservingFacilityListViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IDialogService _applicationDialogService;
        private readonly ObservableObject<DateTime?> _historicalTimeOfInterest;
        private readonly ObservableObject<DateTime?> _databaseTimeOfInterest;
        private ObservableCollection<ObservingFacilityListItemViewModel> _observingFacilityListItemViewModels;
        private Sorting _sorting;
        private bool _displayFindButton;
        private bool _displayFindButtonNoMatterIfAutoRefreshIsEnabled = false;

        private AsyncCommand<object> _findObservingFacilitiesCommand;

        public IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        public bool DisplayFindButton
        {
            get => _displayFindButton;
            set
            {
                _displayFindButton = value;
                RaisePropertyChanged();
            }
        }

        public ObjectCollection<ObservingFacilityDataExtract> ObservingFacilityDataExtracts { get; }
        public ObjectCollection<ObservingFacility> SelectedObservingFacilities { get; }

        public ObservableCollection<ObservingFacilityListItemViewModel> ObservingFacilityListItemViewModels
        {
            get { return _observingFacilityListItemViewModels; }
            set
            {
                _observingFacilityListItemViewModels = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<ObservingFacilityListItemViewModel> SelectedObservingFacilityListItemViewModels { get; set; }

        public ObservingFacilityFilterViewModel ObservingFacilityFilterViewModel { get; }

        public Sorting Sorting
        {
            get { return _sorting; }
            set
            {
                _sorting = value;
                RaisePropertyChanged();
                UpdateSorting();
                UpdateObservingFacilityListItemViewModels();
            }
        }

        public AsyncCommand<object> FindObservingFacilitiesCommand
        {
            get
            {
                return _findObservingFacilitiesCommand ?? (_findObservingFacilitiesCommand = new AsyncCommand<object>(FindObservingFacilities));
            }
        }

        public ObservingFacilityListViewModel(
            ILogger logger,
            IUnitOfWorkFactory unitOfWorkFactory,
            IDialogService applicationDialogService,
            ObservableObject<DateTime?> historicalTimeOfInterest,
            ObservableObject<DateTime?> databaseTimeOfInterest,
            ObservableObject<bool> autoRefresh,
            ObservableObject<bool> displayNameFilter,
            ObservableObject<bool> displayStatusFilter,
            ObservableObject<bool> showActiveStations,
            ObservableObject<bool> showClosedStations,
            ObservableObject<bool> displayHistoricalTimeControls,
            ObservableObject<bool> displayDatabaseTimeControls)
        {
            _logger = logger;
            _applicationDialogService = applicationDialogService;
            _historicalTimeOfInterest = historicalTimeOfInterest;
            _databaseTimeOfInterest = databaseTimeOfInterest;
            _sorting = Sorting.Name;

            UnitOfWorkFactory = unitOfWorkFactory;

            ObservingFacilityFilterViewModel = new ObservingFacilityFilterViewModel(
                historicalTimeOfInterest,
                databaseTimeOfInterest,
                displayNameFilter,
                displayStatusFilter,
                showActiveStations,
                showClosedStations,
                displayHistoricalTimeControls,
                displayDatabaseTimeControls);

            ObservingFacilityDataExtracts = new ObjectCollection<ObservingFacilityDataExtract>
            {
                Objects = new List<ObservingFacilityDataExtract>()
            };

            SelectedObservingFacilities = new ObjectCollection<ObservingFacility>
            {
                Objects = new List<ObservingFacility>()
            };

            SelectedObservingFacilityListItemViewModels = new ObservableCollection<ObservingFacilityListItemViewModel>();

            SelectedObservingFacilityListItemViewModels.CollectionChanged += (s, e) =>
            {
                SelectedObservingFacilities.Objects = SelectedObservingFacilityListItemViewModels.Select(_ => _.ObservingFacility);
            };

            // Indtil videre er den bare synlig hele tiden - skal lige have fixet 
            if (_displayFindButtonNoMatterIfAutoRefreshIsEnabled)
            {
                DisplayFindButton = true;
            }
            else
            {
                autoRefresh.PropertyChanged += (s, e) =>
                {
                    DisplayFindButton = !autoRefresh.Object;
                };

                DisplayFindButton = !autoRefresh.Object;
            }
        }

        public void AddObservingFacility(
            ObservingFacility observingFacility,
            GeospatialLocation geospatialLocation)
        {
            var observingFacilityDataExtracts = ObservingFacilityDataExtracts.Objects.ToList();

            observingFacilityDataExtracts.Add(new ObservingFacilityDataExtract
            {
                ObservingFacility = observingFacility,
                GeospatialLocations = new List<GeospatialLocation>
            {
                geospatialLocation
            }
            });

            ObservingFacilityDataExtracts.Objects = observingFacilityDataExtracts;

            UpdateObservingFacilityListItemViewModels();

            SelectedObservingFacilityListItemViewModels.Clear();

            foreach (var observingFacilityListItemViewModel in ObservingFacilityListItemViewModels)
            {
                if (observingFacilityListItemViewModel.ObservingFacility.Id != observingFacility.Id) continue;

                SelectedObservingFacilityListItemViewModels.Add(observingFacilityListItemViewModel);
                break;
            }
        }

        public void UpdateObservingFacilities(
            IEnumerable<ObservingFacility> observingFacilities)
        {
            throw new NotImplementedException("Block removed for refactoring");

            //var objectIdsOfUpdatedObservingFacilities = observingFacilities.Select(_ => _.ObjectId).ToList();

            //foreach (var observingFacility in ObservingFacilityDataExtracts.Objects)
            //{
            //    if (objectIdsOfUpdatedObservingFacilities.Contains(observingFacility.ObservingFacility.ObjectId))
            //    {
            //        var temp = observingFacilities.Single(_ => _.ObjectId == observingFacility.ObservingFacility.ObjectId);

            //        observingFacility.ObservingFacility.Name = temp.Name;
            //        observingFacility.ObservingFacility.DateEstablished = temp.DateEstablished;
            //        observingFacility.ObservingFacility.DateClosed = temp.DateClosed;
            //    }
            //}

            //ObservingFacilityListItemViewModels = new ObservableCollection<ObservingFacilityListItemViewModel>(
            //    ObservingFacilityDataExtracts.Objects.Select(
            //        _ => new ObservingFacilityListItemViewModel { ObservingFacility = _.ObservingFacility }));

            //SelectedObservingFacilityListItemViewModels.Clear();

            //foreach (var observingFacilityListItemViewModel in ObservingFacilityListItemViewModels)
            //{
            //    if (objectIdsOfUpdatedObservingFacilities.Contains(observingFacilityListItemViewModel.ObservingFacility.ObjectId))
            //    {
            //        SelectedObservingFacilityListItemViewModels.Add(observingFacilityListItemViewModel);
            //    }
            //}
        }

        public void RemoveObservingFacilities(
            IEnumerable<Person> people)
        {
            var idsOfDeletedPeople= people.Select(_ => _.ID);

            ObservingFacilityDataExtracts.Objects = ObservingFacilityDataExtracts.Objects
                .Where(_ => !idsOfDeletedPeople.Contains(_.ObservingFacility.Id))
                .ToList();

            SelectedObservingFacilityListItemViewModels.Clear();
            UpdateObservingFacilityListItemViewModels();
        }

        private async Task RetrieveObservingFacilitiesMatchingFilterFromRepository()
        {
            _logger?.WriteLine(LogMessageCategory.Information, "View Models: Invoking repository..");

            using var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork();
            var people = (await unitOfWork.People.GetAll()).ToList();
            var groups = people.GroupBy(p => p.ID);

            _logger?.WriteLine(LogMessageCategory.Information, $"View Models: Received {groups.Count()} objects");

            var observingFacilityDataExtracts = new List<ObservingFacilityDataExtract>();

            foreach (var grouping in groups)
            {
                var ordered = grouping.OrderBy(p => p.Start).ToList();
                var name = ordered.Last().FirstName;
                var dateEstablished = ordered.First().Start;
                var dateClosed = ordered.Last().End;

                var geospatialLocations = ordered
                    .Select(person => new Point
                    {
                        Coordinate1 = person.Latitude!.Value, 
                        Coordinate2 = person.Longitude!.Value,
                        From = person.Start,
                        To = person.End,
                    }).Cast<GeospatialLocation>().ToList();

                var observingFacilityDataExtract = new ObservingFacilityDataExtract
                {
                    ObservingFacility = new ObservingFacility
                    {
                        Id = grouping.Key,
                        DateEstablished = dateEstablished,
                        DateClosed = dateClosed,
                        Name = name
                    },
                    GeospatialLocations = geospatialLocations
                };

                observingFacilityDataExtracts.Add(observingFacilityDataExtract);
            }

            ObservingFacilityDataExtracts.Objects = observingFacilityDataExtracts;

            // Old

            //using (var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork())
            //{
            //    var observingFacilityDictionary = unitOfWork.ObservingFacilities
            //        .FindIncludingGeospatialLocations(
            //            Application.Helpers.ObservationFacilityFilterAsExpression(
            //                _historicalTimeOfInterest.Object,
            //                _databaseTimeOfInterest.Object,
            //                ObservingFacilityFilterViewModel.ShowActiveObservingFacilities,
            //                ObservingFacilityFilterViewModel.ShowClosedObservingFacilities,
            //                ObservingFacilityFilterViewModel.NameFilter.ToUpper()),
            //            Application.Helpers.GeospatialLocationFilterAsExpression(
            //                _databaseTimeOfInterest.Object));

            //    ObservingFacilityDataExtracts.Objects = observingFacilityDictionary
            //        .Select(_ =>
            //        {
            //            var observingFacility = _.Key;
            //            var geospatalLocations = _.Value;

            //            return new ObservingFacilityDataExtract
            //            {
            //                ObservingFacility = observingFacility,
            //                GeospatialLocations = geospatalLocations
            //            };
            //        });
            //}

            //_logger?.WriteLine(LogMessageCategory.Information,
            //    $"Retrieved {ObservingFacilityDataExtracts.Objects.Count()} Observing Facilities");
        }

        private int CountObservingFacilitiesMatchingFilterFromRepository()
        {
            throw new NotImplementedException("Block removed for refactoring");

            //using (var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork())
            //{
            //    return unitOfWork.ObservingFacilities.Count(
            //        Application.Helpers.ObservationFacilityFilterAsExpression(
            //            _historicalTimeOfInterest.Object,
            //            _databaseTimeOfInterest.Object,
            //            ObservingFacilityFilterViewModel.ShowActiveObservingFacilities,
            //            ObservingFacilityFilterViewModel.ShowClosedObservingFacilities,
            //            ObservingFacilityFilterViewModel.NameFilter.ToUpper()));
            //}
        }

        private void UpdateSorting()
        {
            switch (Sorting)
            {
                case Sorting.Name:
                    {
                        ObservingFacilityDataExtracts.Objects = ObservingFacilityDataExtracts.Objects.OrderBy(_ => _.ObservingFacility.Name);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateObservingFacilityListItemViewModels()
        {
            // Identify the ones that are selected
            var objectIdsOfSelectedObservingFacilities = SelectedObservingFacilities.Objects
                .Select(_ => _.Id)
                .ToList();

            UpdateSorting();

            var time = _historicalTimeOfInterest.Object.HasValue
                ? _historicalTimeOfInterest.Object.Value
                : new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            ObservingFacilityListItemViewModels = new ObservableCollection<ObservingFacilityListItemViewModel>(
                ObservingFacilityDataExtracts.Objects.Select(
                    _ => new ObservingFacilityListItemViewModel(_.ObservingFacility, _.ObservingFacility.DateClosed < time)));

            // Make sure the ones that were selected before are selected again
            SelectedObservingFacilityListItemViewModels.Clear();

            ObservingFacilityListItemViewModels.ToList().ForEach(_ =>
            {
                if (objectIdsOfSelectedObservingFacilities.Contains(_.ObservingFacility.Id))
                {
                    SelectedObservingFacilityListItemViewModels.Add(_);
                }
            });
        }

        private async Task FindObservingFacilities(
            object owner)
        {
            // Commented out while working on other things
            //if (owner != null)
            //{
            //    var limit = 100;
            //    var count = CountObservingFacilitiesMatchingFilterFromRepository();

            //    if (count == 0)
            //    {
            //        var dialogViewModel = new MessageBoxDialogViewModel("No observing facilities the search criteria", false);
            //        _applicationDialogService.ShowDialog(dialogViewModel, owner as Window);
            //    }

            //    if (count > limit)
            //    {
            //        var dialogViewModel = new MessageBoxDialogViewModel($"{count} observing facilities match the search criteria.\nDo you want to retrieve them all from the repository?", true);
            //        if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) == DialogResult.Cancel)
            //        {
            //            return;
            //        }
            //    }
            //}

            await RetrieveObservingFacilitiesMatchingFilterFromRepository();
            UpdateObservingFacilityListItemViewModels();
        }
    }
}