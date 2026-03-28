using Craft.DataStructures.IO.graphml;
using Craft.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PR.Domain.Entities.PR;
using System;
using System.Linq.Expressions;

namespace PR.ViewModel
{
    public class FindPeopleViewModel : ViewModelBase
    {
        // These belong to the mainViewModel but can be modified by this view model
        private readonly ObservableObject<Tuple<DateTime?, DateTime?>> _bitemporalTimesOfInterest;
        private DateTime? _historicalTime;
        private DateTime? _databaseTime;

        private bool _displayAttributeFilterSection;
        private bool _displayStatusFilterSection;
        private bool _displayRetrospectiveFilterSection;
        private bool _displayHistoricalTimeControls;
        private bool _displayDatabaseTimeControls;
        private string _nameFilter = "";
        private string _nameFilterInUppercase = "";
        private string _categoryFilter = "";
        private string _categoryFilterInUppercase = "";

        private bool _showCurrentPeople;
        private bool _showHistoricalPeople;
        private bool _showCurrentPeopleCheckboxEnabled;
        private bool _showHistoricalPeopleCheckboxEnabled;

        private RelayCommand _clearHistoricalTimeCommand;
        private RelayCommand _clearDatabaseTimeCommand;

        public DateTime? HistoricalTime
        {
            get => _historicalTime;
            set
            {
                if (_historicalTime == value) return;

                _historicalTime = value;
                RaisePropertyChanged();

                var historicalTime = _bitemporalTimesOfInterest.Object.Item1;
                var databaseTime = _bitemporalTimesOfInterest.Object.Item2;

                if (historicalTime == value) return;

                historicalTime = value;

                if (historicalTime.HasValue)
                {
                    if (databaseTime.HasValue && databaseTime.Value < historicalTime)
                    {
                        databaseTime = historicalTime;
                    }
                }
                else
                {
                    databaseTime = null;
                }

                _bitemporalTimesOfInterest.Object = new Tuple<DateTime?, DateTime?>(historicalTime, databaseTime);
            }
        }

        public DateTime? DatabaseTime
        {
            get => _databaseTime;
            set
            {
                if (_databaseTime == value) return;

                _databaseTime = value;
                RaisePropertyChanged();

                var historicalTime = _bitemporalTimesOfInterest.Object.Item1;
                var databaseTime = _bitemporalTimesOfInterest.Object.Item2;

                if (databaseTime == value) return;

                databaseTime = value;

                if (databaseTime.HasValue &&
                    historicalTime.HasValue &&
                    historicalTime.Value > databaseTime.Value)
                {
                    historicalTime = databaseTime;
                }

                _bitemporalTimesOfInterest.Object = new Tuple<DateTime?, DateTime?>(historicalTime, databaseTime);
            }
        }

        public bool DisplayAttributeFilterSection
        {
            get => _displayAttributeFilterSection;
            set
            {
                _displayAttributeFilterSection = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayStatusFilterSection
        {
            get => _displayStatusFilterSection;
            set
            {
                _displayStatusFilterSection = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayRetrospectiveFilterSection
        {
            get => _displayRetrospectiveFilterSection;
            set
            {
                _displayRetrospectiveFilterSection = value;
                RaisePropertyChanged();
            }
        }

        public bool DisplayHistoricalTimeControls
        {
            get => _displayHistoricalTimeControls;
            set
            {
                _displayHistoricalTimeControls = value;
                RaisePropertyChanged();

                DisplayRetrospectiveFilterSection =
                    DisplayHistoricalTimeControls ||
                    DisplayDatabaseTimeControls;
            }
        }

        public bool DisplayDatabaseTimeControls
        {
            get => _displayDatabaseTimeControls;
            set
            {
                _displayDatabaseTimeControls = value;
                RaisePropertyChanged();

                DisplayRetrospectiveFilterSection =
                    DisplayHistoricalTimeControls ||
                    DisplayDatabaseTimeControls;
            }
        }

        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                _nameFilter = value;

                _nameFilterInUppercase = _nameFilter == null ? "" : _nameFilter.ToUpper();
                RaisePropertyChanged();
            }
        }

        public string CategoryFilter
        {
            get { return _categoryFilter; }
            set
            {
                _categoryFilter = value;
                _categoryFilterInUppercase = _categoryFilter == null ? "" : _categoryFilter.ToUpper();
                RaisePropertyChanged();
            }
        }

        public bool ShowCurrentPeople
        {
            get { return _showCurrentPeople; }
            set
            {
                _showCurrentPeople = value;
                RaisePropertyChanged();

                ShowCurrentPeopleCheckboxEnabled = ShowHistoricalPeople;
                ShowHistoricalPeopleCheckboxEnabled = ShowCurrentPeople;
            }
        }

        public bool ShowHistoricalPeople
        {
            get { return _showHistoricalPeople; }
            set
            {
                _showHistoricalPeople = value;
                RaisePropertyChanged();

                ShowCurrentPeopleCheckboxEnabled = ShowHistoricalPeople;
                ShowHistoricalPeopleCheckboxEnabled = ShowCurrentPeople;
            }
        }

        public bool ShowCurrentPeopleCheckboxEnabled
        {
            get { return _showCurrentPeopleCheckboxEnabled; }
            set
            {
                _showCurrentPeopleCheckboxEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowHistoricalPeopleCheckboxEnabled
        {
            get { return _showHistoricalPeopleCheckboxEnabled; }
            set
            {
                _showHistoricalPeopleCheckboxEnabled = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand ClearHistoricalTimeCommand
        {
            get
            {
                return _clearHistoricalTimeCommand ?? (_clearHistoricalTimeCommand =
                    new RelayCommand(ClearHistoricalTime, CanClearHistoricalTime));
            }
        }

        public RelayCommand ClearDatabaseTimeCommand
        {
            get
            {
                return _clearDatabaseTimeCommand ??
                       (_clearDatabaseTimeCommand = new RelayCommand(ClearDatabaseTime, CanClearDatabaseTime));
            }
        }

        public FindPeopleViewModel(
            ObservableObject<Tuple<DateTime?, DateTime?>> bitemporalTimesOfInterest)
        {
            _bitemporalTimesOfInterest = bitemporalTimesOfInterest;

            DisplayAttributeFilterSection = true;
            DisplayStatusFilterSection = true;
            ShowCurrentPeople = true;
            DisplayRetrospectiveFilterSection = true;
            DisplayHistoricalTimeControls = true;
            DisplayDatabaseTimeControls = true;

            _bitemporalTimesOfInterest.PropertyChanged += (s, e) =>
            {
                HistoricalTime = _bitemporalTimesOfInterest.Object.Item1;
                DatabaseTime = _bitemporalTimesOfInterest.Object.Item2;

                UpdateControls();
            };
        }

        public Expression<Func<Person, bool>> FilterAsExpression()
        {
            return p => (p.FirstName.ToUpper().Contains(_nameFilterInUppercase) ||
                         p.Surname != null && p.Surname.ToUpper().Contains(_nameFilterInUppercase));
        }

        public bool PersonPassesFilter(Person person)
        {
            var nameOK = string.IsNullOrEmpty(NameFilter) ||
                         person.FirstName.ToUpper().Contains(NameFilter.ToUpper()) ||
                         person.Surname != null && person.Surname.ToUpper().Contains(NameFilter.ToUpper());

            return nameOK;
        }

        private void ClearHistoricalTime()
        {
            _bitemporalTimesOfInterest.Object = new Tuple<DateTime?, DateTime?>(
                null,
                _bitemporalTimesOfInterest.Object.Item2);
        }

        private bool CanClearHistoricalTime()
        {
            return _bitemporalTimesOfInterest.Object.Item1.HasValue;
        }

        private void ClearDatabaseTime()
        {
            _bitemporalTimesOfInterest.Object = new Tuple<DateTime?, DateTime?>(
                _bitemporalTimesOfInterest.Object.Item1,
                null);
        }

        private bool CanClearDatabaseTime()
        {
            return _bitemporalTimesOfInterest.Object.Item2.HasValue;
        }

        private void UpdateControls()
        {
            ClearHistoricalTimeCommand.RaiseCanExecuteChanged();
            ClearDatabaseTimeCommand.RaiseCanExecuteChanged();
        }
    }
}
