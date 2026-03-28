using System;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Craft.Domain;
using Craft.Logging;
using Craft.Utils;
using Craft.ViewModel.Utils;
using Craft.ViewModels.Dialogs;
using PR.Application;
using PR.IO;
using PR.Persistence;

namespace PR.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Application.Application _application;
        private readonly IDataIOHandler _dataIOHandler;
        private readonly IDialogService _applicationDialogService;
        private ILogger _logger;

        private readonly ObservableObject<Tuple<DateTime?, DateTime?>> _bitemporalTimesOfInterest;

        public string MainWindowTitle { get; }

        public IUnitOfWorkFactory UnitOfWorkFactory
        {
            get => _application.UnitOfWorkFactory;
            set
            {
                _application.UnitOfWorkFactory = value;
                PersonListViewModel.UnitOfWorkFactory = value;
                PersonPropertiesViewModel.UnitOfWorkFactory = value;
            }
        }

        public ILogger Logger
        {
            get => _logger;
            set
            {
                _logger = value;
                _application.Logger = value;
            }
        }


        public PersonListViewModel PersonListViewModel { get; }
        public PeoplePropertiesViewModel PeoplePropertiesViewModel { get; }
        public PersonPropertiesViewModel PersonPropertiesViewModel { get; }
        public LogViewModel LogViewModel { get; }

        private AsyncCommand<object> _createPersonCommand;
        private RelayCommand<object> _showOptionsDialogCommand;
        private AsyncCommand<object> _softDeleteSelectedPeopleCommand;
        private AsyncCommand<object> _hardDeleteSelectedPeopleCommand;
        private AsyncCommand<object> _clearRepositoryCommand;
        private AsyncCommand<object> _reseedRepositoryCommand;
        private AsyncCommand _exportPeopleCommand;
        private RelayCommand _exportSelectionToGraphmlCommand;
        private AsyncCommand _importPeopleCommand;
        private RelayCommand _exitCommand;

        public AsyncCommand<object> CreatePersonCommand
        {
            get { return _createPersonCommand ?? (_createPersonCommand = new AsyncCommand<object>(CreatePerson, CanCreatePerson)); }
        }

        public AsyncCommand<object> SoftDeleteSelectedPeopleCommand
        {
            get { return _softDeleteSelectedPeopleCommand ?? (_softDeleteSelectedPeopleCommand = new AsyncCommand<object>(SoftDeleteSelectedPeople, CanSoftDeleteSelectedPeople)); }
        }

        public AsyncCommand<object> HardDeleteSelectedPeopleCommand
        {
            get { return _hardDeleteSelectedPeopleCommand ?? (_hardDeleteSelectedPeopleCommand = new AsyncCommand<object>(HardDeleteSelectedPeople, CanHardDeleteSelectedPeople)); }
        }

        public AsyncCommand<object> ClearRepositoryCommand
        {
            get
            {
                return _clearRepositoryCommand ?? (_clearRepositoryCommand =
                    new AsyncCommand<object>(ClearRepository, CanClearRepository));
            }
        }

        public AsyncCommand<object> ReseedRepositoryCommand
        {
            get
            {
                return _reseedRepositoryCommand ?? (_reseedRepositoryCommand =
                    new AsyncCommand<object>(ReseedRepository, CanReseedRepository));
            }
        }

        public RelayCommand<object> ShowOptionsDialogCommand
        {
            get { return _showOptionsDialogCommand ?? (_showOptionsDialogCommand = new RelayCommand<object>(ShowOptionsDialog, CanShowOptionsDialog)); }
        }

        public AsyncCommand ExportPeopleCommand
        {
            get { return _exportPeopleCommand ?? (_exportPeopleCommand = new AsyncCommand(ExportPeople, CanExportPeople)); }
        }

        public RelayCommand ExportSelectionToGraphmlCommand
        {
            get { return _exportSelectionToGraphmlCommand ?? (_exportSelectionToGraphmlCommand = new RelayCommand(
                ExportSelectionToGraphml, CanExportSelectionToGraphml)); }
        }

        public AsyncCommand ImportPeopleCommand
        {
            get { return _importPeopleCommand ?? (_importPeopleCommand = new AsyncCommand(ImportPeople, CanImportPeople)); }
        }

        public RelayCommand ExitCommand
        {
            get { return _exitCommand ?? (_exitCommand = new RelayCommand(Exit, CanExit)); }
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
                logger);

            _application.UnitOfWorkFactory = unitOfWorkFactory;
            _dataIOHandler = dataIOHandler;
            _applicationDialogService = applicationDialogService;

            MainWindowTitle = "Person Register";

            // When this is changed, one should call Find - not in connection with the two times being changed separately
            _bitemporalTimesOfInterest = new ObservableObject<Tuple<DateTime?, DateTime?>>
            {
                Object = new Tuple<DateTime?, DateTime?>(null, null)
            };

            LogViewModel = new LogViewModel(200);
            Logger = new ViewModelLogger(logger, LogViewModel);

            PersonListViewModel = new PersonListViewModel(
                unitOfWorkFactory, 
                applicationDialogService,
                _bitemporalTimesOfInterest);

            PeoplePropertiesViewModel = new PeoplePropertiesViewModel(
                _application,
                applicationDialogService,
                PersonListViewModel.SelectedPeople);

            PersonPropertiesViewModel = new PersonPropertiesViewModel(
                _application,
                unitOfWorkFactory,
                applicationDialogService,
                PersonListViewModel.SelectedPeople);

            PersonListViewModel.SelectedPeople.PropertyChanged += HandlePeopleSelectionChanged;
            PeoplePropertiesViewModel.PeopleUpdated += PeoplePropertiesViewModel_PeopleUpdated;

            _bitemporalTimesOfInterest.PropertyChanged += async (s, e) =>
            {
                if (UnitOfWorkFactory is IUnitOfWorkFactoryHistorical unitOfWorkFactoryHistorical)
                {
                    unitOfWorkFactoryHistorical.HistoricalTime = _bitemporalTimesOfInterest.Object.Item1;
                }

                if (UnitOfWorkFactory is IUnitOfWorkFactoryVersioned unitOfWorkFactoryVersioned)
                {
                    unitOfWorkFactoryVersioned.DatabaseTime = _bitemporalTimesOfInterest.Object.Item2;
                }


                // Diagnostics:
                var historicalTime = _bitemporalTimesOfInterest.Object.Item1.HasValue
                    ? _bitemporalTimesOfInterest.Object.Item1.Value.AsDateString()
                    : "Now";

                var databaseTime = _bitemporalTimesOfInterest.Object.Item2.HasValue
                    ? _bitemporalTimesOfInterest.Object.Item2.Value.AsDateString()
                    : "Latest";

                Logger?.WriteLine(
                    LogMessageCategory.Debug, 
                    $"Bitemporal coordinates changed:\n  Historical Time: {historicalTime}\n  Database time: {databaseTime}");

                // Get it from PR.ViewModel.GIS
                //UpdateCommands();
                //UpdateControlStyle();
                //UpdateStatusBar();
                //UpdateTimeText();
                //UpdateDatabaseTimeSeriesView();
                //await AutoFindIfEnabled();
            };

            Logger?.WriteLine(LogMessageCategory.Information, "Application started");
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
                unitOfWorkFactoryHistorical.IncludeCurrentObjects = true;
                unitOfWorkFactoryHistorical.IncludeHistoricalObjects = false;
            }
        }

        private void PeoplePropertiesViewModel_PeopleUpdated(
            object? sender, 
            PeopleEventArgs e)
        {
            PersonListViewModel.UpdatePeople(e.People);
        }

        private void HandlePeopleSelectionChanged(
            object sender, 
            PropertyChangedEventArgs e)
        {
            SoftDeleteSelectedPeopleCommand.RaiseCanExecuteChanged();
            HardDeleteSelectedPeopleCommand.RaiseCanExecuteChanged();
            ExportSelectionToGraphmlCommand.RaiseCanExecuteChanged();
        }

        private async Task CreatePerson(
            object owner)
        {
            var dialogViewModel = new CreateOrUpdatePersonDialogViewModel(
                _application);

            if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) != DialogResult.OK)
            {
                return;
            }

            if (dialogViewModel.Person.End > DateTime.UtcNow)
            {
                PersonListViewModel.AddPerson(dialogViewModel.Person);
            }
        }

        private bool CanCreatePerson(
            object owner)
        {
            return true;
        }

        private async Task SoftDeleteSelectedPeople(
            object owner)
        {
            var people = PersonListViewModel.SelectedPeople.Objects;

            var dialogViewModel = new ProspectiveUpdateDialogViewModel(
                _application, 
                ProspectiveUpdateDialogViewModelMode.Delete, 
                people);

            if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) != DialogResult.OK)
            {
                return;
            }

            PersonListViewModel.RemovePeople(people);
        }

        private bool CanSoftDeleteSelectedPeople(
            object owner)
        {
            return PersonListViewModel.SelectedPeople.Objects != null &&
                   PersonListViewModel.SelectedPeople.Objects.Any() &&
                   PersonListViewModel.SelectedPeople.Objects.All(_ => _.End.Year == 9999);
        }

        private async Task HardDeleteSelectedPeople(
            object owner)
        {
            throw new NotImplementedException();
        }

        private bool CanHardDeleteSelectedPeople(
            object owner)
        {
            return PersonListViewModel.SelectedPeople.Objects != null &&
                   PersonListViewModel.SelectedPeople.Objects.Any() &&
                   PersonListViewModel.SelectedPeople.Objects.All(_ => _.End.Year < 9999);
        }

        private async Task ClearRepository(
            object owner)
        {
            var dialogViewModel = new MessageBoxDialogViewModel("Clear repository?", true);

            if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) != DialogResult.OK)
            {
                return;
            }

            using (var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork())
            {
                await unitOfWork.PersonComments.Clear();
                await unitOfWork.People.Clear();
                unitOfWork.Complete();
            }

            dialogViewModel = new MessageBoxDialogViewModel("Repository was cleared", false);

            _applicationDialogService.ShowDialog(dialogViewModel, owner as Window);
        }

        private bool CanClearRepository(
            object owner)
        {
            return true;
        }

        private async Task ReseedRepository(
            object owner)
        {
            var dialogViewModel1 = new MessageBoxDialogViewModel("Reseed repository?", true);

            if (_applicationDialogService.ShowDialog(dialogViewModel1, owner as Window) != DialogResult.OK)
            {
                return;
            }

            UnitOfWorkFactory.Reseed();

            var dialogViewModel2 = new MessageBoxDialogViewModel("Repository was reseeded", false);

            _applicationDialogService.ShowDialog(dialogViewModel2, owner as Window);
        }

        private bool CanReseedRepository(
            object owner)
        {
            return true;
        }

        private async Task ExportPeople()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Xml Files(*.xml)|*.xml|Json Files(*.json)|*.json|All(*.*)|*"
            };

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            await _application.ExportData(dialog.FileName);
        }

        private bool CanExportPeople()
        {
            return true;
        }

        private void ExportSelectionToGraphml()
        {
            var people = PersonListViewModel.SelectedPeople.Objects.ToList();

            var personIds = people
                .Select(p => p.ID)
                .ToList();

            var prData = new PRData
            {
                People = people,
            };

            _dataIOHandler.ExportDataToGraphML(
                prData,
                @"C:\Temp\People.graphml");
        }

        private bool CanExportSelectionToGraphml()
        {
            return PersonListViewModel.SelectedPeople.Objects != null &&
                   PersonListViewModel.SelectedPeople.Objects.Any();
        }

        private async Task ImportPeople()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Xml Files(*.xml)|*.xml|Json Files(*.json)|*.json|All(*.*)|*"
            };

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            await _application.ImportData(dialog.FileName);
        }

        private bool CanImportPeople()
        {
            return true;
        }

        private void Exit()
        {
            throw new NotImplementedException();
        }

        private bool CanExit()
        {
            return true;
        }

        private void ShowOptionsDialog(
            object owner)
        {
            DateTime? historicalTime = null;
            DateTime? databaseTime = null;

            if (UnitOfWorkFactory is IUnitOfWorkFactoryVersioned unitOfWorkFactoryVersioned)
            {
                databaseTime = unitOfWorkFactoryVersioned.DatabaseTime;
            }

            if (UnitOfWorkFactory is IUnitOfWorkFactoryHistorical unitOfWorkFactoryHistorical)
            {
                historicalTime = unitOfWorkFactoryHistorical.HistoricalTime;
            }

            var dialogViewModel = new OptionsDialogViewModel(
                historicalTime,
                databaseTime);

            _applicationDialogService.ShowDialog(dialogViewModel, owner as Window);

            if (UnitOfWorkFactory is IUnitOfWorkFactoryVersioned unitOfWorkFactoryVersioned2)
            {
                unitOfWorkFactoryVersioned2.DatabaseTime = dialogViewModel.DatabaseTime;
            }

            if (UnitOfWorkFactory is IUnitOfWorkFactoryHistorical unitOfWorkFactoryHistorical2)
            {
                unitOfWorkFactoryHistorical2.HistoricalTime = dialogViewModel.HistoricalTime;
            }
        }

        private bool CanShowOptionsDialog(
            object owner)
        {
            return true;
        }
    }
}
