using Craft.Utils;
using GalaSoft.MvvmLight;
using System;
using System.Globalization;
using PR.ViewModel.GIS.Domain;

namespace PR.ViewModel.GIS
{
    public class GeospatialLocationListItemViewModel : ViewModelBase
    {
        private GeospatialLocation _geospatialLocation;

        public GeospatialLocation GeospatialLocation
        {
            get => _geospatialLocation;
            set
            {
                _geospatialLocation = value;
                RaisePropertyChanged();
            }
        }

        public string Name { get; }
        public string Latitude { get; }
        public string Longitude { get; }
        public string From { get; }
        public string To { get; }

        public GeospatialLocationListItemViewModel(
            GeospatialLocation geospatialLocation)
        {
            GeospatialLocation = geospatialLocation;
            Name = (geospatialLocation as Point).Name;
            Latitude = (geospatialLocation as Point).Coordinate1.ToString(CultureInfo.InvariantCulture);
            Longitude = (geospatialLocation as Point).Coordinate2.ToString(CultureInfo.InvariantCulture);
            From = geospatialLocation.From.AsDateString();
            To = geospatialLocation.To == DateTime.MaxValue ? "" : geospatialLocation.To.AsDateString();
        }
    }
}
