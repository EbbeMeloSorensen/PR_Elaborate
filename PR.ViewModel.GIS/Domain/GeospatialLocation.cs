using System;

namespace PR.ViewModel.GIS.Domain
{
    public abstract class GeospatialLocation
    {
        public Guid Id { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
