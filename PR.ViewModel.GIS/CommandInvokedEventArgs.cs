using System;

namespace PR.ViewModel.GIS
{
    public class CommandInvokedEventArgs : EventArgs
    {
        public object Owner { get; }

        public CommandInvokedEventArgs(
            object owner)
        {
            Owner = owner;
        }
    }
}
