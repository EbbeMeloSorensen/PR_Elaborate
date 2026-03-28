using GalaSoft.MvvmLight;
using PR.Domain.Entities.PR;

namespace PR.ViewModel
{
    public class PersonCommentListViewItemViewModel : ViewModelBase
    {
        private PersonComment _personComment;

        public PersonComment PersonComment
        {
            get { return _personComment; }
            set
            {
                _personComment = value;
                RaisePropertyChanged();
            }
        }

        public string DisplayText
        {
            get { return $"{_personComment.Text}"; }
        }
    }
}
