using GalaSoft.MvvmLight;
using PR.ViewModel.GIS.Domain;
using System.Text;

namespace PR.ViewModel.GIS
{
    public class ObservingFacilityListItemViewModel : ViewModelBase
    {
        private ObservingFacility _observingFacility;
        private bool _discontinued;

        public ObservingFacility ObservingFacility
        {
            get { return _observingFacility; }
            set
            {
                _observingFacility = value;
                RaisePropertyChanged();
            }
        }

        public ObservingFacilityListItemViewModel(
            ObservingFacility observingFacility,
            bool discontinued)
        {
            ObservingFacility = observingFacility;
            _discontinued = discontinued;
        }

        public string DisplayText
        {
            get
            {
                var sb = new StringBuilder(_observingFacility.Name);

                if (_discontinued)
                {
                    sb.Append(" (dead)");
                }

                return sb.ToString();
            }
        }
    }
}