using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Craft.Utils;
using Craft.UI.Utils;
using Craft.ViewModels.Dialogs;
using PR.Application;
using PR.Domain.Entities.PR;

namespace PR.ViewModel;

public class PeoplePropertiesViewModel : ViewModelBase, IDataErrorInfo
{
    private readonly Application.Application _application;
    private readonly IDialogService _applicationDialogService;
    private StateOfView _state;
    private ObjectCollection<Person> _people;
    private List<Person> _updatedPeople;
    private Dictionary<string, string> _errors;

    private string _generalError;
    private bool _displayGeneralError;

    public Person OriginalSharedValues { get; private set; }
    public Person SharedValues { get; }

    private string _latitude;
    private string _longitude;

    private bool _isVisible;

    public string GeneralError
    {
        get => _generalError;
        set
        {
            _generalError = value;
            DisplayGeneralError = !string.IsNullOrEmpty(_generalError);
            RaisePropertyChanged();
        }
    }

    public bool DisplayGeneralError
    {
        get => _displayGeneralError;
        set
        {
            _displayGeneralError = value;
            RaisePropertyChanged();
        }
    }

    private RelayCommand<object> _applyChangesCommand;

    public event EventHandler<PeopleEventArgs> PeopleUpdated;

    public string FirstName
    {
        get => SharedValues.FirstName;
        set
        {
            SharedValues.FirstName = value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public string Surname
    {
        get => SharedValues.Surname ?? "";
        set
        {
            SharedValues.Surname = string.IsNullOrEmpty(value) ? null : value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public string Nickname
    {
        get => SharedValues.Nickname ?? "";
        set
        {
            SharedValues.Nickname = string.IsNullOrEmpty(value) ? null : value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public string Address
    {
        get => SharedValues.Address ?? "";
        set
        {
            SharedValues.Address = string.IsNullOrEmpty(value) ? null : value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public string ZipCode
    {
        get => SharedValues.ZipCode ?? "";
        set
        {
            SharedValues.ZipCode = string.IsNullOrEmpty(value) ? null : value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public string City
    {
        get => SharedValues.City ?? "";
        set
        {
            SharedValues.City = string.IsNullOrEmpty(value) ? null : value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public string Category
    {
        get => SharedValues.Category ?? "";
        set
        {
            SharedValues.Category = string.IsNullOrEmpty(value) ? null : value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public DateTime? Birthday
    {
        get => SharedValues.Birthday;
        set
        {
            SharedValues.Birthday = value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public string Latitude
    {
        get => _latitude;
        set
        {
            _latitude = value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
        }
    }

    public string Longitude
    {
        get => _longitude;
        set
        {
            _longitude = value;
            Validate();
            RaisePropertyChanged();
            ApplyChangesCommand.RaiseCanExecuteChanged();
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

    public RelayCommand<object> ApplyChangesCommand
    {
        get { return _applyChangesCommand ?? (_applyChangesCommand = new RelayCommand<object>(ApplyChanges, CanApplyChanges)); }
    }

    public PeoplePropertiesViewModel(
        Application.Application application,
        IDialogService applicationDialogService,
        ObjectCollection<Person> people)
    {
        _application = application;
        _applicationDialogService = applicationDialogService;
        SharedValues = new Person();
        _errors = new Dictionary<string, string>();
        _people = people;
        _people.PropertyChanged += Initialize;
    }

    private void Initialize(
        object sender,
        PropertyChangedEventArgs e)
    {
        _state = StateOfView.Initial;
        GeneralError = "";

        if (sender is not ObjectCollection<Person> objectCollection) return;

        var people = objectCollection.Objects.ToList();

        if (!people.Any())
        {
            IsVisible = false;
            return;
        }

        IsVisible = true;

        FirstName = people.All(p => p.FirstName == people.First().FirstName)
            ? people.First().FirstName
            : string.Empty;

        Surname = (people.All(p => p.Surname == people.First().Surname)
            ? people.First().Surname
            : string.Empty) ?? string.Empty;

        Nickname = (people.All(p => p.Nickname == people.First().Nickname)
            ? people.First().Nickname
            : string.Empty) ?? string.Empty;

        Address = (people.All(p => p.Address == people.First().Address)
            ? people.First().Address
            : string.Empty) ?? string.Empty;

        ZipCode = (people.All(p => p.ZipCode == people.First().ZipCode)
            ? people.First().ZipCode
            : string.Empty) ?? string.Empty;

        City = (people.All(p => p.City == people.First().City)
            ? people.First().City
            : string.Empty) ?? string.Empty;

        Category = (people.All(p => p.Category == people.First().Category)
            ? people.First().Category
            : string.Empty) ?? string.Empty;

        Birthday = (people.All(p => p.Birthday == people.First().Birthday)
            ? people.First().Birthday
            : null) ?? null;

        Latitude = (AreAllEqualWithinTolerance(people.Select(p => p.Latitude), 0.0000001)
            ? $"{people.First().Latitude!.Value.ToString(CultureInfo.InvariantCulture)}"
            : string.Empty);

        Longitude = (AreAllEqualWithinTolerance(people.Select(p => p.Longitude), 0.0000001)
            ? $"{people.First().Longitude!.Value.ToString(CultureInfo.InvariantCulture)}"
            : string.Empty);

        OriginalSharedValues = new Person
        {
            FirstName = FirstName,
            Surname = Surname,
            Nickname = Nickname,
            Address = Address,
            ZipCode = ZipCode,
            City = City,
            Birthday = Birthday,
            Category = Category,
            Latitude = string.IsNullOrEmpty(Latitude) ? null : people.First().Latitude,
            Longitude = string.IsNullOrEmpty(Longitude) ? null : people.First().Longitude
        };

        ApplyChangesCommand.RaiseCanExecuteChanged();
    }

    private void ApplyChanges(
        object owner)
    {
        UpdateState(StateOfView.Updated);

        if (_errors.Any())
        {
            GeneralError = _errors.First().Value;
            return;
        }
        
        var dialogViewModel = new ProspectiveUpdateDialogViewModel(
            _application, 
            ProspectiveUpdateDialogViewModelMode.Update, 
            _updatedPeople);

        if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) != DialogResult.OK)
        {
            return;
        }

        OnPeopleUpdated(_updatedPeople);
    }

    private bool CanApplyChanges(
        object owner)
    {
        if (OriginalSharedValues == null)
        {
            return false;
        }

        if (_state == StateOfView.Updated && _errors.Any())
        {
            return false;
        }

        return
            FirstName != OriginalSharedValues.FirstName ||
            Surname != OriginalSharedValues.Surname ||
            Nickname != OriginalSharedValues.Nickname ||
            Address != OriginalSharedValues.Address ||
            ZipCode != OriginalSharedValues.ZipCode ||
            City != OriginalSharedValues.City ||
            Birthday != OriginalSharedValues.Birthday ||
            Category != OriginalSharedValues.Category ||
            Latitude != NullableDoubleAsString(OriginalSharedValues.Latitude) ||
            Longitude != NullableDoubleAsString(OriginalSharedValues.Longitude);
    }

    public string this[string columnName]
    {
        get
        {
            if (_state == StateOfView.Initial)
            {
                return string.Empty;
            }

            _errors.TryGetValue(columnName, out var error);

            return error ?? "";
        }
    }

    public string Error => null; // Not used

    private void RaisePropertyChanges()
    {
        RaisePropertyChanged(nameof(FirstName));
        RaisePropertyChanged(nameof(Surname));
        RaisePropertyChanged(nameof(Nickname));
        RaisePropertyChanged(nameof(Address));
        RaisePropertyChanged(nameof(ZipCode));
        RaisePropertyChanged(nameof(City));
        RaisePropertyChanged(nameof(Birthday));
        RaisePropertyChanged(nameof(Category));
        RaisePropertyChanged(nameof(Latitude));
        RaisePropertyChanged(nameof(Longitude));
    }

    private void UpdateState(
        StateOfView state)
    {
        _state = state;
        Validate();
        RaisePropertyChanges();
    }

    private void Validate()
    {
        if (_state != StateOfView.Updated) return;

        _errors.Clear();
        GeneralError = String.Empty;

        if (!Latitude.TryParse(out var latitude, out var error_lat))
        {
            _errors[nameof(Latitude)] = error_lat;
        }

        if (!Longitude.TryParse(out var longitude, out var error_long))
        {
            _errors[nameof(Longitude)] = error_long;
        }

        if (_errors.Any())
        {
            return;
        }

        SharedValues.Latitude = latitude;
        SharedValues.Longitude = longitude;
        SharedValues.Start = DateTime.UtcNow;
        SharedValues.End = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        _updatedPeople = _people.Objects.Select(p => new Person
        {
            ID = p.ID,
            Superseded = p.Superseded,
            Start = DateTime.UtcNow.Date,
            End = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc),
            FirstName = FirstName != OriginalSharedValues.FirstName ? FirstName : p.FirstName,
            Surname = Surname != OriginalSharedValues.Surname ? Surname : p.Surname,
            Nickname = Nickname != OriginalSharedValues.Nickname ? Nickname : p.Nickname,
            Address = Address != OriginalSharedValues.Address ? Address : p.Address,
            ZipCode = ZipCode != OriginalSharedValues.ZipCode ? ZipCode : p.ZipCode,
            City = City != OriginalSharedValues.City ? City : p.City,
            Birthday = Birthday != OriginalSharedValues.Birthday ? Birthday : p.Birthday,
            Category = Category != OriginalSharedValues.Category ? Category : p.Category,
            Latitude = Latitude != NullableDoubleAsString(OriginalSharedValues.Latitude)
                ? double.Parse(Latitude, CultureInfo.InvariantCulture)
                : p.Latitude,
            Longitude = Longitude != NullableDoubleAsString(OriginalSharedValues.Longitude)
                ? double.Parse(Longitude, CultureInfo.InvariantCulture)
                : p.Longitude
        }).ToList();

        _errors = _application.UpdatePeople_ValidateInput(_updatedPeople);
    }

    private void OnPeopleUpdated(
        IEnumerable<Person> people)
    {
        // Make a temporary copy of the event to avoid possibility of
        // a race condition if the last subscriber unsubscribes
        // immediately after the null check and before the event is raised.
        var handler = PeopleUpdated;

        // Event will be null if there are no subscribers
        if (handler != null)
        {
            handler(this, new PeopleEventArgs(people));
        }
    }

    private bool AreAllEqualWithinTolerance(
        IEnumerable<double?> source,
        double tolerance)
    {
        if (source == null || !source.Any())
            return false;

        var values = source.ToList();

        if (values.Any(v => !v.HasValue))
            return false;

        var first = values.First().Value;
        return values.All(v => Math.Abs(v.Value - first) <= tolerance);
    }

    private string NullableDoubleAsString(
        double? value)
    {
        return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "";
    }
}