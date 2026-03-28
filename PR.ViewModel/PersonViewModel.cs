using GalaSoft.MvvmLight;
using PR.Domain.Entities.PR;

namespace PR.ViewModel;

public class PersonViewModel : ViewModelBase
{
    private Person _person;
    private bool _isHistorical;

    public Person Person
    {
        get { return _person; }
        set
        {
            _person = value;
            RaisePropertyChanged();
        }
    }

    public bool IsHistorical
    {
        get { return _isHistorical; }
        set
        {
            _isHistorical = value;
            RaisePropertyChanged();
        }
    }

    public string DisplayText
    {
        get
        {
            var displayText = _person.FirstName;
            if (_person.Surname != null)
            {
                displayText += $" {_person.Surname}";
            }

            return displayText;
        }
    }
}