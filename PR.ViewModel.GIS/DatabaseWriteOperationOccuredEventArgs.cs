using System;

namespace PR.ViewModel.GIS
{
    public class DatabaseWriteOperationOccuredEventArgs : EventArgs
    {
        public DateTime DateTime { get; }

        public DatabaseWriteOperationOccuredEventArgs(
            DateTime dateTime)
        {
            DateTime = dateTime;
        }
    }
}