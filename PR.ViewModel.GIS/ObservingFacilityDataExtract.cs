using System.Collections.Generic;
using PR.ViewModel.GIS.Domain;

namespace PR.ViewModel.GIS
{
    public class ObservingFacilityDataExtract
    {
        public ObservingFacility ObservingFacility { get; set; }

        public List<GeospatialLocation> GeospatialLocations { get; set; }
    }
}