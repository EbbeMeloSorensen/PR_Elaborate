using System;

namespace PR.ViewModel.GIS.Domain
{
    public class ObservingFacility : AbstractEnvironmentalMonitoringFacility
    {
        public string? Name { get; set; }
        public DateTime DateEstablished { get; set; }
        public DateTime DateClosed { get; set; }

        public override string ToString()
        {
            return $"Observing Facility: {Name}";
        }
    }
}
