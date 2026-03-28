using Craft.ViewModel.Utils;
using Craft.ViewModels.Dialogs;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace PR.ViewModel
{
    public class SelectTimeDialogViewModel : DialogViewModelBase, IDataErrorInfo
    {
        private AsyncCommand<object> _okCommand;
        private RelayCommand<object> _cancelCommand;

        public AsyncCommand<object> OKCommand
        {
            get { return _okCommand ?? (_okCommand = new AsyncCommand<object>(OK, CanOK)); }
        }

        public RelayCommand<object> CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand<object>(Cancel, CanCancel)); }
        }

        public string Error { get; }

        public string this[string columnName] => throw new NotImplementedException();

        private async Task OK(object parameter)
        {
            throw new NotImplementedException();
        }

        private bool CanOK(object parameter)
        {
            return true;
        }

        private void Cancel(object parameter)
        {
            CloseDialogWithResult(parameter as Window, DialogResult.Cancel);
        }

        private bool CanCancel(object parameter)
        {
            return true;
        }
    }
}
