using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Craft.Domain;
using Craft.Logging;
using Craft.Utils;
using Craft.ViewModel.Utils;
using Craft.ViewModels.Dialogs;
using Craft.ViewModels.Geometry2D.ScrollFree;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PR.Domain.Entities;
using PR.Domain.Entities.PR;
using PR.Persistence;
using PR.IO;
 
namespace PR.ViewModel.GIS
{
    public class MainWindowViewModel : ViewModelBase
    {
        private enum MapOperation
        {
            None,
            CreateObservingFacility,
            CreateGeospatialLocation
        }

        private readonly Application.Application _application;
        private readonly IDataIOHandler _dataIOHandler;
        private readonly IDialogService _applicationDialogService;
        private List<DateTime> _databaseWriteTimes;
        private List<DateTime> _historicalChangeTimes;
        private readonly ObservableObject<DateTime?> _historicalTimeOfInterest;
        private readonly ObservableObject<DateTime?> _databaseTimeOfInterest;
        private readonly ObservableObject<Tuple<DateTime?, DateTime?>> _bitemporalTimesOfInterest;
        private readonly ObservableObject<bool> _autoRefresh;
        private readonly ObservableObject<bool> _displayNameFilter;
        private readonly ObservableObject<bool> _displayStatusFilter;
        private readonly ObservableObject<bool> _showActiveStations;
        private readonly ObservableObject<bool> _showClosedStations;
        private readonly ObservableObject<bool> _displayRetrospectionControls;
        private readonly ObservableObject<bool> _displayHistoricalTimeControls;
        private readonly ObservableObject<bool> _displayDatabaseTimeControls;
        private readonly Brush _mapBrushSeaCurrent = new SolidColorBrush(new Color { R = 200, G = 200, B = 255, A = 255 });
        private readonly Brush _mapBrushSeaHistoric = new SolidColorBrush(new Color { R = 200, G = 200, B = 225, A = 255 });
        private readonly Brush _mapBrushSeaOutdated = new SolidColorBrush(new Color { R = 239, G = 228, B = 176, A = 255 });
        private readonly Brush _mapBrushLandCurrent = new SolidColorBrush(new Color { R = 100, G = 200, B = 100, A = 255 });
        private readonly Brush _mapBrushLandHistoric = new SolidColorBrush(new Color { R = 100, G = 130, B = 100, A = 255 });
        private readonly Brush _mapBrushLandOutdated = new SolidColorBrush(new Color { R = 185, G = 122, B = 87, A = 255 });
        private readonly Brush _timeStampBrush = new SolidColorBrush(Colors.DarkSlateBlue);
        private readonly Brush _activeObservingFacilityBrush = new SolidColorBrush(Colors.GreenYellow);
        private readonly Brush _closedObservingFacilityBrush = new SolidColorBrush(Colors.Red);
        private readonly Brush _controlBackgroundBrushCurrent = new SolidColorBrush(Colors.WhiteSmoke);
        private readonly Brush _controlBackgroundBrushHistoric = new SolidColorBrush(Colors.DarkGray);
        private readonly Brush _controlBackgroundBrushOutdated = new SolidColorBrush(Colors.BurlyWood);
        private Brush _controlBackgroundBrush;
        private string _messageInMap;
        private string _statusBarText;
        private string _timeText;
        private string _databaseTimeText;
        private string _timeTextColor;
        private bool _displayMessageInMap;
        private bool _displayLog;
        private int _selectedTabIndexForRetrospectionTimeLines;
        private Window _owner;
        private MapOperation _mapOperation;
        private System.Timers.Timer _timer;

        private RelayCommand<object> _createObservingFacilityCommand;
        private AsyncCommand<object> _deleteSelectedObservingFacilitiesCommand;
        private AsyncCommand<object> _clearRepositoryCommand;
        private RelayCommand<object> _importSMSDataSetCommand;
        private RelayCommand _escapeCommand;

        public bool AutoRefresh
        {
            get => _autoRefresh.Object;
            set
            {
                _autoRefresh.Object = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayNameFilter
        {
            get => _displayNameFilter.Object;
            set
            {
                _displayNameFilter.Object = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayStatusFilter
        {
            get => _displayStatusFilter.Object;
            set
            {
                _displayStatusFilter.Object = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayRetrospectionControls
        {
            get => _displayRetrospectionControls.Object;
            set
            {
                _displayRetrospectionControls.Object = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayHistoricalTimeControls
        {
            get => _displayHistoricalTimeControls.Object;
            set
            {
                _displayHistoricalTimeControls.Object = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayDatabaseTimeControls
        {
            get => _displayDatabaseTimeControls.Object;
            set
            {
                _displayDatabaseTimeControls.Object = value;
                RaisePropertyChanged();
            }
        }

        public string StatusBarText
        {
            get => _statusBarText;
            set
            {
                _statusBarText = value;
                RaisePropertyChanged();
            }
        }

        public string TimeText
        {
            get => _timeText;
            set
            {
                _timeText = value;
                RaisePropertyChanged();
            }
        }

        public string DatabaseTimeText
        {
            get => _databaseTimeText;
            set
            {
                _databaseTimeText = value;
                RaisePropertyChanged();
            }
        }

        public string TimeTextColor
        {
            get => _timeTextColor;
            set
            {
                _timeTextColor = value;
                RaisePropertyChanged();
            }
        }

        public string MessageInMap
        {
            get => _messageInMap;
            set
            {
                _messageInMap = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayMessageInMap
        {
            get => _displayMessageInMap;
            set
            {
                _displayMessageInMap = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayLog
        {
            get => _displayLog;
            set
            {
                _displayLog = value;
                RaisePropertyChanged();
            }
        }

        public int SelectedTabIndexForRetrospectionTimeLines
        {
            get => _selectedTabIndexForRetrospectionTimeLines;
            set
            {
                _selectedTabIndexForRetrospectionTimeLines = value;
                RaisePropertyChanged();
            }
        }

        public Brush ControlBackgroundBrush
        {
            get => _controlBackgroundBrush;
            set
            {
                _controlBackgroundBrush = value;
                RaisePropertyChanged();
            }
        }

        public IUnitOfWorkFactory UnitOfWorkFactory
        {
            get => _application.UnitOfWorkFactory;
            set
            {
                _application.UnitOfWorkFactory = value;
                ObservingFacilityListViewModel.UnitOfWorkFactory = value;
                ObservingFacilitiesDetailsViewModel.UnitOfWorkFactory = value;
            }
        }

        public ILogger Logger
        {
            get => _application.Logger;
            set
            {
                _application.Logger = value;
            }
        }

        public LogViewModel LogViewModel { get; private set; }
        public ObservingFacilityListViewModel ObservingFacilityListViewModel { get; private set; }
        public ObservingFacilitiesDetailsViewModel ObservingFacilitiesDetailsViewModel { get; private set; }
        public GeometryEditorViewModel MapViewModel { get; private set; }
        public TimeSeriesViewModel DatabaseWriteTimesViewModel { get; private set; }
        public TimeSeriesViewModel HistoricalTimeViewModel { get; private set; }

        public RelayCommand<object> CreateObservingFacilityCommand
        {
            get { return _createObservingFacilityCommand ?? (_createObservingFacilityCommand = new RelayCommand<object>(CreateObservingFacility, CanCreateObservingFacility)); }
        }

        public AsyncCommand<object> DeleteSelectedObservingFacilitiesCommand
        {
            get
            {
                return _deleteSelectedObservingFacilitiesCommand ?? (_deleteSelectedObservingFacilitiesCommand =
                    new AsyncCommand<object>(DeleteSelectedObservingFacilities, CanDeleteSelectedObservingFacilities));
            }
        }

        public AsyncCommand<object> ClearRepositoryCommand
        {
            get
            {
                return _clearRepositoryCommand ?? (_clearRepositoryCommand =
                    new AsyncCommand<object>(ClearRepository, CanClearRepository));
            }
        }

        public RelayCommand<object> ImportSMSDataSetCommand
        {
            get
            {
                return _importSMSDataSetCommand ?? (_importSMSDataSetCommand =
                    new RelayCommand<object>(ImportSMSDataSet, CanImportSMSDataSet));
            }
        }

        public RelayCommand EscapeCommand
        {
            get { return _escapeCommand ?? (_escapeCommand = new RelayCommand(Escape)); }
        }

        public MainWindowViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            IBusinessRuleCatalog businessRuleCatalog,
            IDataIOHandler dataIOHandler,
            IDialogService applicationDialogService,
            ILogger logger)
        {
            _application = new Application.Application(
                unitOfWorkFactory,
                businessRuleCatalog,
                dataIOHandler,
                logger)
            {
                UnitOfWorkFactory = unitOfWorkFactory
            };

            _dataIOHandler = dataIOHandler;
            _applicationDialogService = applicationDialogService;

            LogViewModel = new LogViewModel(200);
            Logger = new ViewModelLogger(logger, LogViewModel);

            _historicalChangeTimes = new List<DateTime>();
            _databaseWriteTimes = new List<DateTime>();

            _historicalTimeOfInterest = new ObservableObject<DateTime?>
            {
                Object = null
            };

            _databaseTimeOfInterest = new ObservableObject<DateTime?>
            {
                Object = null
            };

            // Når denne ændres, skal man kalde Find - ikke i forbindelse med at de 2 tidspunkter ændres hver især
            _bitemporalTimesOfInterest = new ObservableObject<Tuple<DateTime?, DateTime?>>
            {
                Object = new Tuple<DateTime?, DateTime?>(null, null)
            };

            DisplayLog = true;
            //DisplayLog = true; // Set to true when diagnosing application behaviour

            _historicalTimeOfInterest.PropertyChanged += async (s, e) =>
            {
                // Mark historical time of interest in the historical time control
                HistoricalTimeViewModel!.StaticXValue = _historicalTimeOfInterest.Object.HasValue
                    ? (_historicalTimeOfInterest.Object.Value - TimeSeriesViewModel.TimeAtOrigo) / TimeSpan.FromDays(1)
                    : null;

                // Set the historical time of interest for the unit of work factory
                if (UnitOfWorkFactory is IUnitOfWorkFactoryHistorical unitOfWorkFactoryHistorical)
                {
                    unitOfWorkFactoryHistorical.HistoricalTime = _historicalTimeOfInterest.Object;
                }

                // Possibly adjust database time if it doesn't exceed historical time
                if (_historicalTimeOfInterest.Object.HasValue)
                {
                    if (_databaseTimeOfInterest.Object.HasValue &&
                        _databaseTimeOfInterest.Object.Value < _historicalTimeOfInterest.Object.Value)
                    {
                        _databaseTimeOfInterest.Object = _historicalTimeOfInterest.Object.Value;
                    }
                }
                else
                {
                    if (_databaseTimeOfInterest.Object.HasValue)
                    {
                        _databaseTimeOfInterest.Object = null;
                    }
                }

                // Set the bitemporal pair to trigger a database query 
                UpdateBitemporalTimePair();
            };

            _databaseTimeOfInterest.PropertyChanged += async (s, e) =>
            {
                // Mark database time of interest in the database time control
                DatabaseWriteTimesViewModel!.StaticXValue = _databaseTimeOfInterest.Object.HasValue
                    ? (_databaseTimeOfInterest.Object.Value - TimeSeriesViewModel.TimeAtOrigo) / TimeSpan.FromDays(1)
                    : null;

                // Set the database time of interest for the unit of work factory
                if (UnitOfWorkFactory is IUnitOfWorkFactoryVersioned unitOfWorkFactoryVersioned)
                {
                    unitOfWorkFactoryVersioned.DatabaseTime = _databaseTimeOfInterest.Object;
                }

                // Possibly adjust historical time so its doesn't exceed database time
                if (_databaseTimeOfInterest.Object.HasValue)
                {
                    if (!_historicalTimeOfInterest.Object.HasValue ||
                        _historicalTimeOfInterest.Object.Value > _databaseTimeOfInterest.Object.Value)
                    {
                        _historicalTimeOfInterest.Object = _databaseTimeOfInterest.Object.Value;
                    }
                }

                await InitializeHistoricalTimestampsOfInterest();

                // Set the bitemporal pair to trigger a database query 
                UpdateBitemporalTimePair();
            };

            _bitemporalTimesOfInterest.PropertyChanged += async (s, e) =>
            {
                UpdateCommands();
                UpdateControlStyle();
                UpdateStatusBar();
                UpdateTimeText();
                UpdateDatabaseTimeSeriesView();
                await AutoFindIfEnabled();
            };

            _autoRefresh = new ObservableObject<bool>
            {
                Object = true
            };

            _displayNameFilter = new ObservableObject<bool>
            {
                Object = false
            };

            _displayStatusFilter = new ObservableObject<bool>
            {
                Object = true
            };

            _showActiveStations = new ObservableObject<bool>
            {
                Object = true
            };

            _showClosedStations = new ObservableObject<bool>
            {
                Object = false
            };

            _displayHistoricalTimeControls = new ObservableObject<bool>
            {
                Object = true
            };

            _displayDatabaseTimeControls = new ObservableObject<bool>
            {
                Object = true
            };

            _displayRetrospectionControls = new ObservableObject<bool>
            {
                Object = _displayHistoricalTimeControls.Object ||
                         _displayDatabaseTimeControls.Object
            };

            _displayHistoricalTimeControls.PropertyChanged += (s, e) =>
                UpdateRetrospectionControls();

            _displayDatabaseTimeControls.PropertyChanged += (s, e) =>
                UpdateRetrospectionControls();

            _showActiveStations.PropertyChanged += async (s, e) =>
            {
                if (UnitOfWorkFactory is IUnitOfWorkFactoryHistorical unitOfWorkFactoryHistorical)
                {
                    unitOfWorkFactoryHistorical.IncludeCurrentObjects = _showActiveStations.Object;
                }

                await AutoFindIfEnabled();
            };

            _showClosedStations.PropertyChanged += async (s, e) =>
            {
                if (UnitOfWorkFactory is IUnitOfWorkFactoryHistorical unitOfWorkFactoryHistorical)
                {
                    unitOfWorkFactoryHistorical.IncludeHistoricalObjects = _showClosedStations.Object;
                }

                await AutoFindIfEnabled();
            };

            InitializeLogViewModel();
            InitializeObservingFacilityListViewModel(UnitOfWorkFactory, _applicationDialogService);
            InitializeObservingFacilitiesDetailsViewModel(UnitOfWorkFactory, _applicationDialogService);
            InitializeMapViewModel();
            InitializeDatabaseWriteTimesViewModel();
            InitializeHistoricalTimeViewModel();

            DrawMapOfDenmark();

            UpdateRetrospectionControls();
            UpdateStatusBar();

            _historicalTimeOfInterest.PropertyChanged += (s, e) =>
            {
                if (!_historicalTimeOfInterest.Object.HasValue)
                {
                    HistoricalTimeViewModel.StaticXValue = null;
                    DatabaseWriteTimesViewModel.LockWorldWindowOnDynamicXValue = true;
                    DatabaseWriteTimesViewModel.StaticXValue = null;
                }
            };

            _timeTextColor = "White";
            _timer = new System.Timers.Timer(1000);

            _timer.Elapsed += (s, e) =>
            {
                UpdateTimeText();
            };

            _timer.Start();

            Logger?.WriteLine(LogMessageCategory.Information, "View Models: Startup complete");
        }

        public void Initialize(
            bool versioned,
            bool reseeding)
        {
            UnitOfWorkFactory.Initialize(versioned);

            if (reseeding)
            {
                UnitOfWorkFactory.Reseed();
            }

            if (UnitOfWorkFactory is IUnitOfWorkFactoryHistorical unitOfWorkFactoryHistorical)
            {
                unitOfWorkFactoryHistorical.IncludeCurrentObjects = _showActiveStations.Object;
                unitOfWorkFactoryHistorical.IncludeHistoricalObjects = _showClosedStations.Object;
            }
        }

        private void CreateObservingFacility(
            object owner)
        {
            _owner = owner as Window;
            _mapOperation = MapOperation.CreateObservingFacility;
            MessageInMap = "Click the map to place new observing facility";
            DisplayMessageInMap = true;
        }

        private void CreateGeospatialLocation(
            object owner)
        {
            _owner = owner as Window;
            _mapOperation = MapOperation.CreateGeospatialLocation;
            MessageInMap = "Click the map to indicate new position of observing facility";
            DisplayMessageInMap = true;
        }

        private bool CanCreateObservingFacility(
            object owner)
        {
            return !_databaseTimeOfInterest.Object.HasValue;
        }

        private async Task DeleteSelectedObservingFacilities(
            object owner)
        {
            var nSelectedObservingFacilities = ObservingFacilityListViewModel.SelectedObservingFacilities.Objects.Count();

            var message = nSelectedObservingFacilities == 1
                ? "Delete Observing Facility?"
                : $"Delete {nSelectedObservingFacilities} Observing Facilities?";

            var dialogViewModel = new MessageBoxDialogViewModel(message, true);

            if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) == DialogResult.Cancel)
            {
                return;
            }

            var ids = ObservingFacilityListViewModel.SelectedObservingFacilities.Objects
                .Select(_ => _.Id)
                .ToList();

            using var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork();

            var peopleForDeletion = (await unitOfWork.People.Find(_ => ids.Contains(_.ID)))
                .ToList();

            var now = DateTime.UtcNow;
                
            await unitOfWork.People.RemoveRange(peopleForDeletion);
            unitOfWork.Complete();

            ObservingFacilityListViewModel.RemoveObservingFacilities(peopleForDeletion);

            _databaseWriteTimes.Add(now);
            RefreshDatabaseTimeSeriesView();
            RefreshHistoricalTimeSeriesView();
        }

        private bool CanDeleteSelectedObservingFacilities(
            object owner)
        {
            return !_databaseTimeOfInterest.Object.HasValue &&
                   ObservingFacilityListViewModel.SelectedObservingFacilities.Objects != null &&
                   ObservingFacilityListViewModel.SelectedObservingFacilities.Objects.Any();
        }

        private async Task ClearRepository(
            object owner)
        {
            var dialogViewModel1 = new MessageBoxDialogViewModel("Clear repository?", true);

            if (_applicationDialogService.ShowDialog(dialogViewModel1, owner as Window) == DialogResult.Cancel)
            {
                return;
            }

            using (var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork())
            {
                await unitOfWork.People.Clear();
                unitOfWork.Complete();
            }

            _historicalChangeTimes.Clear();
            UpdateHistoricalTimeSeriesView(false);

            _databaseWriteTimes.Clear();
            UpdateDatabaseTimeSeriesView();

            await AutoFindIfEnabled();

            var dialogViewModel2 = new MessageBoxDialogViewModel("Repository was cleared", false);

            _applicationDialogService.ShowDialog(dialogViewModel2, owner as Window);
        }

        private bool CanClearRepository(
            object owner)
        {
            return true;
        }

        private void ImportSMSDataSet(
            object owner)
        {
            throw new NotImplementedException("Block removed for refactoring");

            //var dialog = new OpenFileDialog
            //{
            //    Filter = "Json Files(*.json)|*.json"
            //};

            //if (dialog.ShowDialog() == false)
            //{
            //    return;
            //}

            //var dataIOHandler = new IO.DataIOHandler();
            //dataIOHandler.ImportDataFromJson(dialog.FileName, out var stationInformations);

            ////var sorted = stationInformations.OrderBy(_ => _.GdbFromDate);

            //var objects = stationInformations.GroupBy(_ => _.ObjectId);

            //var simpleObjects = new List<StationInformation>();

            //foreach (var obj in objects)
            //{
            //    var nRows = obj.Count();

            //    if (nRows > 1)
            //    {
            //        continue;
            //    }

            //    var stationInformation = obj.Single();

            //    if (string.IsNullOrEmpty(stationInformation.StationName))
            //    {
            //        continue;
            //    }

            //    if (stationInformation.GdbFromDate != stationInformation.DateFrom)
            //    {
            //        continue;
            //    }

            //    if (stationInformation.GdbToDate != stationInformation.DateTo)
            //    {
            //        continue;
            //    }

            //    if (stationInformation.Country == Country.Greenland)
            //    {
            //        continue;
            //    }

            //    if (!stationInformation.Wgs_lat.HasValue ||
            //        !stationInformation.Wgs_long.HasValue)
            //    {
            //        continue;
            //    }

            //    if (stationInformation.DateFrom.HasValue && stationInformation.DateFrom.Value.Year > 2000)
            //    {
            //        continue;
            //    }

            //    simpleObjects.Add(stationInformation);
            //}

            //simpleObjects = simpleObjects.OrderBy(_ => _.DateFrom).ToList();

            //var now = DateTime.UtcNow;

            //using (var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork())
            //{
            //    var count = 0;

            //    foreach (var obj in simpleObjects)
            //    {
            //        var observingFacility = new ObservingFacility(
            //            Guid.NewGuid(),
            //            now)
            //        {
            //            Name = obj.StationName,
            //            DateEstablished = obj.DateFrom.Value,
            //            DateClosed = obj.DateTo.Value,
            //        };

            //        var point = new Domain.Entities.WIGOS.GeospatialLocations.Point(Guid.NewGuid(), now)
            //        {
            //            AbstractEnvironmentalMonitoringFacility = observingFacility,
            //            AbstractEnvironmentalMonitoringFacilityObjectId = observingFacility.ObjectId,
            //            From = obj.DateFrom.Value,
            //            To = obj.DateTo.Value,
            //            Coordinate1 = obj.Wgs_long.Value,
            //            Coordinate2 = obj.Wgs_lat.Value,
            //            CoordinateSystem = "WGS_84"
            //        };

            //        unitOfWork.ObservingFacilities.Add(observingFacility);
            //        unitOfWork.Points_Wigos.Add(point);
            //        unitOfWork.Complete();

            //        var message = $"{obj.StationName}";

            //        if (obj.DateFrom.HasValue &&
            //            obj.DateTo.HasValue)
            //        {
            //            message += $" ({obj.DateFrom.Value.Year} - {obj.DateTo.Value.Year})";
            //        }

            //        Logger?.WriteLine(LogMessageCategory.Information, message);

            //        count++;

            //        if (count > 100)
            //        {
            //            break;
            //        }
            //    }
            //}

            //InitializeTimestampsOfInterest();

            //if (_autoRefresh.Object)
            //{
            //    ObservingFacilityListViewModel.FindObservingFacilitiesCommand.Execute(null);
            //}
        }

        private bool CanImportSMSDataSet(
            object owner)
        {
            return true;
        }

        private void Escape()
        {
            DisplayMessageInMap = false;
        }

        private void InitializeLogViewModel()
        {
            LogViewModel = new LogViewModel(200);
            Logger = new ViewModelLogger(Logger, LogViewModel);
        }

        private void InitializeObservingFacilityListViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            IDialogService applicationDialogService)
        {
            ObservingFacilityListViewModel = new ObservingFacilityListViewModel(
                Logger,
                unitOfWorkFactory,
                applicationDialogService,
                _historicalTimeOfInterest,
                _databaseTimeOfInterest,
                _autoRefresh,
                _displayNameFilter,
                _displayStatusFilter,
                _showActiveStations,
                _showClosedStations,
                _displayHistoricalTimeControls,
                _displayDatabaseTimeControls);

            ObservingFacilityListViewModel.SelectedObservingFacilities.PropertyChanged += (s, e) =>
            {
                DeleteSelectedObservingFacilitiesCommand.RaiseCanExecuteChanged();
            };

            ObservingFacilityListViewModel.ObservingFacilityDataExtracts.PropertyChanged += (s, e) =>
            {
                //_logger?.WriteLine(LogMessageCategory.Information, "Updating Map points");
                UpdateMapPoints();
            };
        }

        private void InitializeObservingFacilitiesDetailsViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            IDialogService applicationDialogService)
        {
            ObservingFacilitiesDetailsViewModel = new ObservingFacilitiesDetailsViewModel(
                unitOfWorkFactory,
                applicationDialogService,
                _databaseTimeOfInterest,
                ObservingFacilityListViewModel.SelectedObservingFacilities);

            // Block commented out for refactoring
            //ObservingFacilitiesDetailsViewModel.ObservingFacilitiesUpdated += (s, e) =>
            //{
            //    ObservingFacilityListViewModel.UpdateObservingFacilities(e.ObservingFacilities);
            //    _databaseWriteTimes.Add(e.ObservingFacilities.First().Created);
            //    RefreshDatabaseTimeSeriesView();
            //};

            //ObservingFacilitiesDetailsViewModel.GeospatialLocationsViewModel.NewGeospatialLocationCalledByUser += (s, e) =>
            //{
            //    CreateGeospatialLocation(e.Owner);
            //};

            //ObservingFacilitiesDetailsViewModel.GeospatialLocationsViewModel.GeospatialLocationsUpdatedOrDeleted += (s, e) =>
            //{
            //    // Todo: consider placing this in a general helper method
            //    _databaseWriteTimes.Add(e.DateTime);
            //    ObservingFacilityListViewModel.FindObservingFacilitiesCommand.Execute(null);
            //    if (ObservingFacilityListViewModel.SelectedObservingFacilities.Objects.Any())
            //    {
            //        ObservingFacilitiesDetailsViewModel.GeospatialLocationsViewModel.Populate();
            //    }
            //    UpdateMapPoints();
            //    RefreshDatabaseTimeSeriesView();
            //    RefreshHistoricalTimeSeriesView(true);
            //};
        }

        private void InitializeMapViewModel()
        {
            var worldWindowBoundingBoxNorthWest = new Point(6.5, 57.95);
            var worldWindowBoundingBoxSouthEast = new Point(15.5, 54.45);

            var worldWindowFocus = new Point(
                (worldWindowBoundingBoxNorthWest.X + worldWindowBoundingBoxSouthEast.X) / 2,
                (worldWindowBoundingBoxNorthWest.Y + worldWindowBoundingBoxSouthEast.Y) / 2);

            var worldWindowSize = new Size(
                Math.Abs(worldWindowBoundingBoxNorthWest.X - worldWindowBoundingBoxSouthEast.X),
                Math.Abs(worldWindowBoundingBoxNorthWest.Y - worldWindowBoundingBoxSouthEast.Y));

            MapViewModel = new GeometryEditorViewModel(-1)
            {
                AspectRatioLocked = true
            };

            MapViewModel.InitializeWorldWindow(worldWindowFocus, worldWindowSize, false);

            MapViewModel.MouseClickOccured += async (s, e) =>
            {
                if (!DisplayMessageInMap || !MapViewModel.MousePositionWorld.Object.HasValue)
                {
                    return;
                }

                DisplayMessageInMap = false;

                switch (_mapOperation)
                {
                    case MapOperation.CreateObservingFacility:
                        {
                            await CreateNewObservingFacility();
                            break;
                        }
                    case MapOperation.CreateGeospatialLocation:
                        {
                            throw new NotImplementedException();
                            CreateNewGeospatialLocation();
                            break;
                        }
                }
            };

            UpdateControlStyle();
        }

        private void InitializeHistoricalTimeViewModel()
        {
            var timeSpan = TimeSpan.FromDays(25 * 365.25);
            var tFocus = DateTime.UtcNow - timeSpan / 2;
            var xFocus = TimeSeriesViewModel.ConvertDateTimeToXValue(tFocus);

            HistoricalTimeViewModel = new TimeSeriesViewModel(
                new Point(xFocus, 0),
                new Size(timeSpan.TotalDays, 3),
                true,
                0,
                40,
                1,
                XAxisMode.Cartesian,
                Logger)
            {
                LockWorldWindowOnDynamicXValue = true,
                ShowHorizontalGridLines = false,
                ShowVerticalGridLines = false,
                ShowHorizontalAxis = true,
                ShowVerticalAxis = false,
                ShowXAxisLabels = true,
                ShowYAxisLabels = false,
                ShowPanningButtons = true,
                Fraction = 0.95,
                LabelForDynamicXValue = "Now"
            };

            HistoricalTimeViewModel.GeometryEditorViewModel.YAxisLocked = true;

            HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpdateOccured += (s, e) =>
            {
                // Når brugeren dragger, træder vi ud af det mode, hvor World Window løbende opdateres
                HistoricalTimeViewModel.LockWorldWindowOnDynamicXValue = false;
            };

            HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowMajorUpdateOccured += (s, e) =>
            {
                UpdateHistoricalTimeSeriesView(false);
            };

            HistoricalTimeViewModel.GeometryEditorViewModel.UpdateModelCallBack = () =>
            {
                // Update the x value of interest (set it to current time)
                var nowAsScalar = (DateTime.UtcNow - TimeSeriesViewModel.TimeAtOrigo).TotalDays;
                HistoricalTimeViewModel.DynamicXValue = nowAsScalar;
            };

            HistoricalTimeViewModel.GeometryEditorViewModel.MouseClickOccured += (s, e) =>
            {
                if (_databaseTimeOfInterest.Object.HasValue &&
                    HistoricalTimeViewModel.TimeAtMousePosition.Object.Value > _databaseTimeOfInterest.Object)
                {
                    var message = "Historical time of interest cannot be later than database time of interest";
                    var dialogViewModel = new MessageBoxDialogViewModel(message, false);

                    _applicationDialogService.ShowDialog(dialogViewModel, _owner);

                    return;
                }

                if (HistoricalTimeViewModel.TimeAtMousePosition.Object > DateTime.UtcNow)
                {
                    return;
                }

                _historicalTimeOfInterest.Object = HistoricalTimeViewModel.TimeAtMousePosition.Object;
            };
        }

        private void InitializeDatabaseWriteTimesViewModel()
        {
            var timeSpan = TimeSpan.FromMinutes(5);
            var utcNow = DateTime.UtcNow;
            var timeAtOrigo = utcNow.Date;
            var tFocus = utcNow - timeSpan / 2 + TimeSpan.FromMinutes(1);
            var xFocus = (tFocus - timeAtOrigo) / TimeSpan.FromDays(1.0);

            DatabaseWriteTimesViewModel = new TimeSeriesViewModel(
                new Point(xFocus, 0),
                new Size(timeSpan.TotalDays, 3),
                true,
                0,
                40,
                1,
                XAxisMode.Cartesian,
                Logger)
            {
                LockWorldWindowOnDynamicXValue = true,
                ShowHorizontalGridLines = false,
                ShowVerticalGridLines = false,
                ShowHorizontalAxis = true,
                ShowVerticalAxis = false,
                ShowXAxisLabels = true,
                ShowYAxisLabels = false,
                ShowPanningButtons = true,
                Fraction = 0.95,
                LabelForDynamicXValue = "Now"
            };

            DatabaseWriteTimesViewModel.GeometryEditorViewModel.YAxisLocked = true;

            DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpdateOccured += (s, e) =>
            {
                // Når brugeren dragger, træder vi ud af det mode, hvor World Window løbende opdateres
                DatabaseWriteTimesViewModel.LockWorldWindowOnDynamicXValue = false;
            };

            DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowMajorUpdateOccured += (s, e) =>
            {
                UpdateDatabaseTimeSeriesView();
            };

            DatabaseWriteTimesViewModel.GeometryEditorViewModel.UpdateModelCallBack = () =>
            {
                // Update the x value of interest (set it to current time)
                // Dette udvirker, at World Window følger med current time
                var nowAsScalar = (DateTime.UtcNow - TimeSeriesViewModel.TimeAtOrigo).TotalDays;
                DatabaseWriteTimesViewModel.DynamicXValue = nowAsScalar;
            };

            DatabaseWriteTimesViewModel.GeometryEditorViewModel.MouseClickOccured += (s, e) =>
            {
                if (DatabaseWriteTimesViewModel.TimeAtMousePosition.Object > DateTime.UtcNow)
                {
                    return;
                }

                _databaseTimeOfInterest.Object = DatabaseWriteTimesViewModel.TimeAtMousePosition.Object;

                // Highlight the position of the time of interest
                DatabaseWriteTimesViewModel.StaticXValue =
                    (_databaseTimeOfInterest.Object.Value - TimeSeriesViewModel.TimeAtOrigo) / TimeSpan.FromDays(1);

                // Der skal altid gælde, at historisk tid er ældre end eller lig med databasetid
                if (!_historicalTimeOfInterest.Object.HasValue ||
                    _historicalTimeOfInterest.Object.Value > _databaseTimeOfInterest.Object.Value)
                {
                    _historicalTimeOfInterest.Object = _databaseTimeOfInterest.Object.Value;
                }
            };

            DatabaseWriteTimesViewModel.PanLeftClicked += (s, e) =>
            {
                DatabaseWriteTimesViewModel.LockWorldWindowOnDynamicXValue = false;

                // Identify the database write time that is to the left of the current focus

                var currentTimeInFocus = TimeSeriesViewModel.TimeAtOrigo +
                                         TimeSpan.FromDays(DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowFocus.X);

                // Af hensyn til afrundingsfejl, så vi sikrer, at den faktisk stepper tilbage i tid
                currentTimeInFocus -= TimeSpan.FromMilliseconds(10);

                var earlierDatabaseWriteTimes = _databaseWriteTimes.Where(_ => _ < currentTimeInFocus);

                if (!earlierDatabaseWriteTimes.Any())
                {
                    return;
                }

                var newTimeInFocus = earlierDatabaseWriteTimes.Max();
                var xValueInFocus = (newTimeInFocus - TimeSeriesViewModel.TimeAtOrigo) / TimeSpan.FromDays(1);

                DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowFocus = new Point(
                    xValueInFocus,
                    DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowFocus.Y);
            };

            DatabaseWriteTimesViewModel.PanRightClicked += (s, e) =>
            {
                // Identify the database write time that is to the right of the current focus

                var currentTimeInFocus = TimeSeriesViewModel.TimeAtOrigo +
                                         TimeSpan.FromDays(DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowFocus.X);

                // Af hensyn til afrundingsfejl, så vi sikrer, at den faktisk stepper frem i tid
                currentTimeInFocus += TimeSpan.FromMilliseconds(10);

                var laterDatabaseWriteTimes = _databaseWriteTimes.Where(_ => _ > currentTimeInFocus);

                if (!laterDatabaseWriteTimes.Any())
                {
                    DatabaseWriteTimesViewModel.LockWorldWindowOnDynamicXValue = true;
                    return;
                }

                var newTimeInFocus = laterDatabaseWriteTimes.Min();
                var xValueInFocus = (newTimeInFocus - TimeSeriesViewModel.TimeAtOrigo) / TimeSpan.FromDays(1);

                DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowFocus = new Point(
                    xValueInFocus,
                    DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowFocus.Y);
            };
        }

        private void UpdateHistoricalTimeSeriesView(
            bool recalculate)
        {
            // Called:
            //   - During upstart (ok)
            //   - When a major world window update occurs (such as after a drag) (ok)
            //   - When the user changes the historical time of interest by clicking in the view (ok)

            //   - When a new observing facility is created
            //   - When selected observing facilities are deleted
            //   - When the user resets the historical time of interest by clicking the Now button

            // Calculate position of world window
            var x0 = HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.X;
            var x1 = HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.X + HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowSize.Width;
            var y0 = -HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y - HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowSize.Height;
            var y1 = -HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y;

            // Calculate y coordinate of the principal axis (so we can make the lines stop there)
            var y2 = HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y +
                     HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowSize.Height * HistoricalTimeViewModel.MarginBottomOffset /
                     HistoricalTimeViewModel.GeometryEditorViewModel.ViewPortSize.Height;

            // Clear lines
            HistoricalTimeViewModel.GeometryEditorViewModel.ClearLines();

            var lineThickness = 1.5;

            var lineViewModels = _historicalChangeTimes
                .Select(_ => (_ - TimeSeriesViewModel.TimeAtOrigo).TotalDays)
                .Where(_ => _ > x0 && _ < x1)
                .Select(_ => new LineViewModel(new PointD(_, y0), new PointD(_, y2), lineThickness, _timeStampBrush))
                .ToList();

            lineViewModels.ForEach(_ => HistoricalTimeViewModel.GeometryEditorViewModel.LineViewModels.Add(_));

            if (!_historicalTimeOfInterest.Object.HasValue)
            {
                HistoricalTimeViewModel.StaticXValue = null;
            }
        }

        private void UpdateDatabaseTimeSeriesView()
        {
            // Called:
            //   - During upstart
            //   - When a new observing facility is created
            //   - When selected observing facilities are updated
            //   - When selected observing facilities are deleted
            //   - When a major world window update occurs (such as after a drag)
            //   - When the user changes the database time of interest by clicking in the view
            //   - When the user resets the database time of interest by clicking the Latest button

            // Calculate position of world window
            var x0 = DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.X;
            var x1 = DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.X + DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowSize.Width;
            var y0 = -DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y - DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowSize.Height;
            var y1 = -DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y;

            // Calculate y coordinate of the principal axis (so we can make the lines stop there)
            var y2 = DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y +
                 DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowSize.Height * DatabaseWriteTimesViewModel.MarginBottomOffset /
                 DatabaseWriteTimesViewModel.GeometryEditorViewModel.ViewPortSize.Height;

            // Clear lines
            DatabaseWriteTimesViewModel.GeometryEditorViewModel.ClearLines();

            var lineThickness = 1.5;

            var lineViewModels = _databaseWriteTimes
                .Select(_ => (_ - TimeSeriesViewModel.TimeAtOrigo).TotalDays)
                .Where(_ => _ > x0 && _ < x1)
                .Select(_ => new LineViewModel(new PointD(_, y0), new PointD(_, y2), lineThickness, _timeStampBrush))
                .ToList();

            lineViewModels.ForEach(_ => DatabaseWriteTimesViewModel.GeometryEditorViewModel.LineViewModels.Add(_));

            if (!_databaseTimeOfInterest.Object.HasValue)
            {
                DatabaseWriteTimesViewModel.StaticXValue = null;
            }
        }

        private void DrawMapOfDenmark()
        {
            // Load GML file of Denmark
            var fileName = @".\Data\Denmark.gml";
            //var fileName = @".\Data\DenmarkAndGreenland.gml";
            Craft.DataStructures.IO.DataIOHandler.ExtractGeometricPrimitivesFromGMLFile(fileName, out var polygons);

            // Add the regions of Denmark to the map as polygons
            var lineThickness = 0.005;

            foreach (var polygon in polygons)
            {
                MapViewModel.AddPolygon(polygon
                    .Select(p => new PointD(p[1], p[0])), lineThickness, _mapBrushLandCurrent);
            }
        }

        private void UpdateMapPoints()
        {
            MapViewModel.PointViewModels.Clear();

            foreach (var observingFacilityDataExtract in ObservingFacilityListViewModel.ObservingFacilityDataExtracts.Objects)
            {
                var timeOfInterest = _historicalTimeOfInterest.Object.HasValue
                    ? _historicalTimeOfInterest.Object.Value
                    : DateTime.UtcNow;

                var point = observingFacilityDataExtract.GeospatialLocations
                    .Where(p => p.From < timeOfInterest)
                    .LastOrDefault() as Domain.Point;

                if (point != null)
                {
                    var brush = point.To > timeOfInterest
                        ? _activeObservingFacilityBrush
                        : _closedObservingFacilityBrush;

                    MapViewModel.PointViewModels.Add(new PointViewModel(
                        new PointD(
                            point.Coordinate1,
                            -point.Coordinate2),
                        10,
                        brush));
                }
            }
        }

        private void UpdateControlStyle()
        {
            TimeTextColor = _historicalTimeOfInterest.Object.HasValue ? "Black" : "White";

            if (!_historicalTimeOfInterest.Object.HasValue)
            {
                MapViewModel.BackgroundBrush = _mapBrushSeaCurrent;
                ControlBackgroundBrush = _controlBackgroundBrushCurrent;

                foreach (var polygonViewModel in MapViewModel.PolygonViewModels)
                {
                    polygonViewModel.Brush = _mapBrushLandCurrent;
                }
            }
            else if (!_databaseTimeOfInterest.Object.HasValue)
            {
                MapViewModel.BackgroundBrush = _mapBrushSeaHistoric;
                ControlBackgroundBrush = _controlBackgroundBrushHistoric;

                foreach (var polygonViewModel in MapViewModel.PolygonViewModels)
                {
                    polygonViewModel.Brush = _mapBrushLandHistoric;
                }
            }
            else
            {
                MapViewModel.BackgroundBrush = _mapBrushSeaOutdated;
                ControlBackgroundBrush = _controlBackgroundBrushOutdated;

                foreach (var polygonViewModel in MapViewModel.PolygonViewModels)
                {
                    polygonViewModel.Brush = _mapBrushLandOutdated;
                }
            }
        }

        private void UpdateRetrospectionControls()
        {
            if (_displayHistoricalTimeControls.Object == false &&
                SelectedTabIndexForRetrospectionTimeLines == 0)
            {
                SelectedTabIndexForRetrospectionTimeLines = 1;
            }
            else if (_displayDatabaseTimeControls.Object == false &&
                     SelectedTabIndexForRetrospectionTimeLines == 1)
            {
                SelectedTabIndexForRetrospectionTimeLines = 0;
            }

            DisplayRetrospectionControls =
                _displayHistoricalTimeControls.Object ||
                _displayDatabaseTimeControls.Object;
        }

        private void UpdateStatusBar()
        {
            if (!_historicalTimeOfInterest.Object.HasValue)
            {
                StatusBarText = "Current situation";
            }
            else if (!_databaseTimeOfInterest.Object.HasValue)
            {
                StatusBarText = $"Historical situation of {_historicalTimeOfInterest.Object.Value.AsDateString()}";
            }
            else if (_historicalTimeOfInterest.Object.Value == _databaseTimeOfInterest.Object.Value)
            {
                StatusBarText = $"Historical situation of {_historicalTimeOfInterest.Object.Value.AsDateString()} as depicted by the database at that time";
            }
            else
            {
                StatusBarText = $"Historical situation of {_historicalTimeOfInterest.Object.Value.AsDateString()} as depicted by the database as of {_databaseTimeOfInterest.Object.Value.AsDateTimeString(false)}";
            }
        }

        private void UpdateBitemporalTimePair()
        {
            if (_bitemporalTimesOfInterest.Object.Item1 != _historicalTimeOfInterest.Object ||
                _bitemporalTimesOfInterest.Object.Item2 != _databaseTimeOfInterest.Object)
            {
                _bitemporalTimesOfInterest.Object = new Tuple<DateTime?, DateTime?>(
                    _historicalTimeOfInterest.Object,
                    _databaseTimeOfInterest.Object);
            }
        }

        private void UpdateCommands()
        {
            CreateObservingFacilityCommand.RaiseCanExecuteChanged();
            DeleteSelectedObservingFacilitiesCommand.RaiseCanExecuteChanged();
            ObservingFacilitiesDetailsViewModel.IsReadOnly = _databaseTimeOfInterest.Object.HasValue;
        }

        private async Task CreateNewObservingFacility()
        {
            try
            {
                Logger?.WriteLine(LogMessageCategory.Debug, "Opening dialog for creating new observing facility");

                var dialogViewModel = new CreateObservingFacilityDialogViewModel(MapViewModel.MousePositionWorld.Object.Value);

                if (_applicationDialogService.ShowDialog(dialogViewModel, _owner) != DialogResult.OK)
                {
                    return;
                }

                Logger?.WriteLine(LogMessageCategory.Debug, "Collecting input from dialog");

                var from = new DateTime(
                    dialogViewModel.From.Year,
                    dialogViewModel.From.Month,
                    dialogViewModel.From.Day,
                    dialogViewModel.From.Hour,
                    dialogViewModel.From.Minute,
                    dialogViewModel.From.Second,
                    DateTimeKind.Utc);

                Logger?.WriteLine(LogMessageCategory.Debug, $"    From: {from}");

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

                Logger?.WriteLine(LogMessageCategory.Debug, $"    From: {to}");

                var latitude = dialogViewModel.Latitude;
                var longitude = dialogViewModel.Longitude;
                var now = DateTime.UtcNow;

                var person = new Person
                {
                    Start = from,
                    End = to,
                    FirstName = dialogViewModel.Name,
                    Latitude = latitude,
                    Longitude = longitude
                };

                using (var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork())
                {
                    await unitOfWork.People.Add(person);
                    unitOfWork.Complete();
                }

                Logger?.WriteLine(LogMessageCategory.Debug, "Creating new Object");

                if (!_historicalChangeTimes.Contains(person.Start))
                {
                    _historicalChangeTimes.Add(person.Start);
                }

                if (person.End < DateTime.MaxValue)
                {
                    if (!_historicalChangeTimes.Contains(person.End))
                    {
                        _historicalChangeTimes.Add(person.End);
                    }
                }

                await AutoFindIfEnabled();

                RefreshHistoricalTimeSeriesView();

                _databaseWriteTimes.Add(now);
                RefreshDatabaseTimeSeriesView();

                Logger?.WriteLine(LogMessageCategory.Debug, "Created new object");
            }
            catch (Exception e)
            {
                Logger?.WriteLine(LogMessageCategory.Error, $"Exception caught, Message: \"{e.Message}\"");
            }
        }

        private async Task CreateNewGeospatialLocation()
        {
            throw new NotImplementedException("Block removed for refactoring");

            //var mousePositionInMap = MapViewModel.MousePositionWorld.Object.Value;

            //var dialogViewModel = new DefineGeospatialLocationDialogViewModel(
            //    DefineGeospatialLocationMode.Create,
            //    Math.Round(mousePositionInMap.X, 4),
            //    -Math.Round(mousePositionInMap.Y, 4),
            //    DateTime.UtcNow.Date,
            //    null);

            //if (_applicationDialogService.ShowDialog(dialogViewModel, _owner) != DialogResult.OK)
            //{
            //    return;
            //}

            //var from = new DateTime(
            //    dialogViewModel.From.Year,
            //    dialogViewModel.From.Month,
            //    dialogViewModel.From.Day,
            //    dialogViewModel.From.Hour,
            //    dialogViewModel.From.Minute,
            //    dialogViewModel.From.Second,
            //    DateTimeKind.Utc);

            //var to = dialogViewModel.To.HasValue
            //    ? dialogViewModel.To == DateTime.MaxValue
            //        ? DateTime.MaxValue
            //        : new DateTime(
            //            dialogViewModel.To.Value.Year,
            //            dialogViewModel.To.Value.Month,
            //            dialogViewModel.To.Value.Day,
            //            dialogViewModel.To.Value.Hour,
            //            dialogViewModel.To.Value.Minute,
            //            dialogViewModel.To.Value.Second,
            //            DateTimeKind.Utc)
            //    : DateTime.MaxValue;

            //var selectedObservingFacility = ObservingFacilityListViewModel.SelectedObservingFacilities.Objects.Single();

            //var now = DateTime.UtcNow;

            //using (var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork())
            //{
            //    var point = new Domain.Entities.WIGOS.GeospatialLocations.Point(Guid.NewGuid(), now)
            //    {
            //        From = from,
            //        To = to,
            //        Coordinate1 = double.Parse(dialogViewModel.Latitude, CultureInfo.InvariantCulture),
            //        Coordinate2 = double.Parse(dialogViewModel.Longitude, CultureInfo.InvariantCulture),
            //        CoordinateSystem = "WGS_84",
            //        AbstractEnvironmentalMonitoringFacilityId = selectedObservingFacility.Id,
            //        AbstractEnvironmentalMonitoringFacilityObjectId = selectedObservingFacility.ObjectId
            //    };

            //    unitOfWork.Points_Wigos.Add(point);

            //    if (point.From < selectedObservingFacility.DateEstablished ||
            //        point.To > selectedObservingFacility.DateClosed)
            //    {
            //        var observingFacilityFromRepo = await unitOfWork.ObservingFacilities.Get(selectedObservingFacility.Id);

            //        observingFacilityFromRepo.Superseded = now;
            //        unitOfWork.ObservingFacilities.Update(observingFacilityFromRepo);

            //        var newObservingFacility = new ObservingFacility(Guid.NewGuid(), now)
            //        {
            //            ObjectId = observingFacilityFromRepo.ObjectId,
            //            Name = observingFacilityFromRepo.Name,
            //            DateEstablished = point.From < observingFacilityFromRepo.DateEstablished
            //                ? point.From
            //                : observingFacilityFromRepo.DateEstablished,
            //            DateClosed = point.To > observingFacilityFromRepo.DateClosed
            //                ? point.To
            //                : observingFacilityFromRepo.DateClosed
            //        };

            //        unitOfWork.ObservingFacilities.Add(newObservingFacility);
            //    }

            //    unitOfWork.Complete();
            //}

            //_databaseWriteTimes.Add(now);
            //ObservingFacilityListViewModel.FindObservingFacilitiesCommand.Execute(null);

            //if (ObservingFacilityListViewModel.SelectedObservingFacilities.Objects.Any())
            //{
            //    ObservingFacilitiesDetailsViewModel.GeospatialLocationsViewModel.Populate();
            //}

            //UpdateMapPoints();
            //RefreshDatabaseTimeSeriesView();
        }

        public async Task Initialize()
        {
            await InitializeDatabaseTimestampsOfInterest();
            await InitializeHistoricalTimestampsOfInterest();
        }

        private async Task InitializeDatabaseTimestampsOfInterest()
        {
            try
            {
                using var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork();

                _databaseWriteTimes = (await unitOfWork.People.GetAllDatabaseWriteTimes()).ToList();
                RefreshDatabaseTimeSeriesView();
            }
            catch (InvalidOperationException ex)
            {
                // Just swallow it for now - write it to the log later
                _databaseWriteTimes = new List<DateTime>();
            }
        }

        private async Task InitializeHistoricalTimestampsOfInterest()
        {
            try
            {
                using var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork();

                _historicalChangeTimes = (await unitOfWork.People.GetAllValidTimeIntervalExtrema()).ToList(); ;
                RefreshHistoricalTimeSeriesView();
            }
            catch (InvalidOperationException ex)
            {
                // Just swallow it for now - write it to the log later
                _historicalChangeTimes = new List<DateTime>();
            }
        }

        private void UpdateTimeText()
        {
            var time = _historicalTimeOfInterest.Object.HasValue 
                ? _historicalTimeOfInterest.Object.Value 
                : DateTime.UtcNow;

            TimeText = TimeAsText(time, _historicalTimeOfInterest.Object.HasValue);

            var databaseTimeText = "";

            if (_databaseTimeOfInterest.Object.HasValue)
            {
                if (_historicalTimeOfInterest.Object.HasValue &&
                    _historicalTimeOfInterest.Object.Value == _databaseTimeOfInterest.Object.Value)
                {
                    databaseTimeText = "(database as of that date)";
                }
                else
                {
                    databaseTimeText = $"(database as of {TimeAsText(_databaseTimeOfInterest.Object.Value, true)})";
                }
            }

            DatabaseTimeText = databaseTimeText;
        }

        private void RefreshHistoricalTimeSeriesView()
        {
            // Called:
            //   - During upstart (ok)
            //   - When a major world window update occurs (such as after a drag) (ok)
            //   - When the user changes the historical time of interest by clicking in the view (ok)

            //   - When a new observing facility is created
            //   - When selected observing facilities are deleted
            //   - When the user resets the historical time of interest by clicking the Now button

            // Calculate position of world window
            var x0 = HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.X;
            var x1 = HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.X + HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowSize.Width;
            var y0 = -HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y - HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowSize.Height;
            var y1 = -HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y;

            // Calculate y coordinate of the principal axis (so we can make the lines stop there)
            var y2 = HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y +
                     HistoricalTimeViewModel.GeometryEditorViewModel.WorldWindowSize.Height * HistoricalTimeViewModel.MarginBottomOffset /
                     HistoricalTimeViewModel.GeometryEditorViewModel.ViewPortSize.Height;

            // Clear lines
            HistoricalTimeViewModel.GeometryEditorViewModel.ClearLines();

            var lineThickness = 1.5;

            var lineViewModels = _historicalChangeTimes
                .Select(_ => (_ - TimeSeriesViewModel.TimeAtOrigo).TotalDays)
                .Where(_ => _ > x0 && _ < x1)
                .Select(_ => new LineViewModel(new PointD(_, y0), new PointD(_, y2), lineThickness, _timeStampBrush))
                .ToList();

            lineViewModels.ForEach(_ => HistoricalTimeViewModel.GeometryEditorViewModel.LineViewModels.Add(_));

            if (!_historicalTimeOfInterest.Object.HasValue)
            {
                HistoricalTimeViewModel.StaticXValue = null;
            }
        }

        private void RefreshDatabaseTimeSeriesView()
        {
            // Calculate position of world window
            var x0 = DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.X;
            var x1 = DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.X + DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowSize.Width;
            var y0 = -DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y - DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowSize.Height;
            var y1 = -DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y;

            // Calculate y coordinate of the principal axis (so we can make the lines stop there)
            var y2 = DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowUpperLeft.Y +
                 DatabaseWriteTimesViewModel.GeometryEditorViewModel.WorldWindowSize.Height * DatabaseWriteTimesViewModel.MarginBottomOffset /
                 DatabaseWriteTimesViewModel.GeometryEditorViewModel.ViewPortSize.Height;

            // Clear lines
            DatabaseWriteTimesViewModel.GeometryEditorViewModel.ClearLines();

            var lineThickness = 1.5;

            var lineViewModels = _databaseWriteTimes
                .Select(_ => (_ - TimeSeriesViewModel.TimeAtOrigo).TotalDays)
                .Where(_ => _ > x0 && _ < x1)
                .Select(_ => new LineViewModel(new PointD(_, y0), new PointD(_, y2), lineThickness, _timeStampBrush))
                .ToList();

            lineViewModels.ForEach(_ => DatabaseWriteTimesViewModel.GeometryEditorViewModel.LineViewModels.Add(_));

            if (!_databaseTimeOfInterest.Object.HasValue)
            {
                DatabaseWriteTimesViewModel.StaticXValue = null;
            }
        }

        private string TimeAsText(
            DateTime time,
            bool omitClockIfZero)
        {
            var sb = new StringBuilder($"{time.ToString("D", CultureInfo.InvariantCulture)}");

            if (time.Hour != 0 && time.Minute != 0 && time.Second != 0 || !omitClockIfZero)
            {
                sb.Append($" {time.ToString("T", CultureInfo.InvariantCulture)}");
            }

            return sb.ToString();
        }

        public async Task AutoFindIfEnabled()
        {
            if (_autoRefresh.Object)
            {
                await ObservingFacilityListViewModel!.FindObservingFacilitiesCommand.ExecuteAsync(null);
            }
        }
    }
}