using System;
using Craft.Utils;
using GalaSoft.MvvmLight;
using PR.Domain.Entities.PR;

namespace PR.ViewModel;

public class PersonVariantListViewItemViewModel : ViewModelBase
{
    private Person _personVariant;

    public Person PersonVariant
    {
        get => _personVariant;
        set
        {
            _personVariant = value;
            RaisePropertyChanged();

            BirthdayAsText = _personVariant.Birthday.HasValue
                ? _personVariant.Birthday.Value.AsDateTimeString(false, true)
                : "";

            StartAsText = _personVariant.Start.AsDateTimeString(false, true);

            EndAsText = _personVariant.End.Year == 9999
                ? "-"
                : _personVariant.End.AsDateTimeString(false, true);
        }
    }

    public string BirthdayAsText { get; private set; }
    public string StartAsText { get; private set; }
    public string EndAsText { get; private set; }
}