using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Craft.Utils;
using Craft.ViewModel.Utils;
using Craft.ViewModels.Dialogs;
using PR.Domain.Entities.PR;
using PR.Persistence;

namespace PR.ViewModel
{
    public class PersonPropertiesViewModel : ViewModelBase
    {
        private Application.Application _application;
        private readonly IDialogService _applicationDialogService;
        private bool _isVisible;
        private ObjectCollection<Person> _people;

        private AsyncCommand<object> _createPersonCommentCommand;
        private AsyncCommand<object> _updatePersonCommentCommand;
        private AsyncCommand<object> _deletePersonCommentsCommand;

        private RelayCommand<object> _personVariantSelectionChangedCommand;

        private AsyncCommand<object> _createPersonVariantCommand;
        private AsyncCommand<object> _updatePersonVariantCommand;
        private AsyncCommand<object> _erasePersonVariantsCommand;

        public IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        public ObservableCollection<PersonCommentListViewItemViewModel> PersonCommentListViewItemViewModels { get; }

        public ObservableCollection<PersonCommentListViewItemViewModel> SelectedPersonCommentListViewItemViewModels { get; }

        public ObjectCollection<PersonComment> SelectedPersonComments { get; }

        public ObservableCollection<PersonVariantListViewItemViewModel> PersonVariantListViewItemViewModels { get; }

        public ObservableCollection<PersonVariantListViewItemViewModel> SelectedPersonVariantListViewItemViewModels { get; }
        
        public ObjectCollection<Person> SelectedPersonVariants { get; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                RaisePropertyChanged();
            }
        }

        private string _error;

        private bool _displayError;

        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                DisplayError = !string.IsNullOrEmpty(_error);
                RaisePropertyChanged();
            }
        }

        public bool DisplayError
        {
            get => _displayError;
            set
            {
                _displayError = value;
                RaisePropertyChanged();
            }
        }

        public AsyncCommand<object> CreatePersonCommentCommand
        {
            get
            {
                return _createPersonCommentCommand ??
                       (_createPersonCommentCommand = new AsyncCommand<object>(CreatePersonComment));
            }
        }

        public AsyncCommand<object> UpdatePersonCommentCommand
        {
            get
            {
                return _updatePersonCommentCommand ??
                       (_updatePersonCommentCommand = new AsyncCommand<object>(UpdatePersonComment, CanUpdatePersonComment));
            }
        }

        public AsyncCommand<object> DeletePersonCommentsCommand
        {
            get
            {
                return _deletePersonCommentsCommand ??
                       (_deletePersonCommentsCommand = new AsyncCommand<object>(SoftDeleteSelectedPersonComments, CanSoftDeleteSelectedPersonComments));
            }
        }

        public RelayCommand<object> PersonVariantSelectionChangedCommand
        {
            get { return _personVariantSelectionChangedCommand ?? (_personVariantSelectionChangedCommand = new RelayCommand<object>(PersonVariantSelectionChanged)); }
        }

        public AsyncCommand<object> CreatePersonVariantCommand
        {
            get
            {
                return _createPersonVariantCommand ??
                       (_createPersonVariantCommand = new AsyncCommand<object>(CreatePersonVariant));
            }
        }

        public AsyncCommand<object> UpdatePersonVariantCommand
        {
            get
            {
                return _updatePersonVariantCommand ??
                       (_updatePersonVariantCommand = new AsyncCommand<object>(UpdatePersonVariant, CanUpdatePersonVariant));
            }
        }

        public AsyncCommand<object> ErasePersonVariantsCommand
        {
            get
            {
                return _erasePersonVariantsCommand ??
                       (_erasePersonVariantsCommand = new AsyncCommand<object>(ErasePersonVariants, CanErasePersonVariants));
            }
        }

        public PersonPropertiesViewModel(
            Application.Application application, 
            IUnitOfWorkFactory unitOfWorkFactory,
            IDialogService applicationDialogService,
            ObjectCollection<Person> people)
        {
            UnitOfWorkFactory = unitOfWorkFactory;
            _application = application;
            _applicationDialogService = applicationDialogService;
            _people = people;

            PersonCommentListViewItemViewModels =
                new ObservableCollection<PersonCommentListViewItemViewModel>();

            SelectedPersonCommentListViewItemViewModels =
                new ObservableCollection<PersonCommentListViewItemViewModel>();

            SelectedPersonComments = new ObjectCollection<PersonComment>();

            PersonVariantListViewItemViewModels = new ObservableCollection<PersonVariantListViewItemViewModel>();

            SelectedPersonVariantListViewItemViewModels =
                new ObservableCollection<PersonVariantListViewItemViewModel>();

            SelectedPersonVariants = new ObjectCollection<Person>();

            _people.PropertyChanged += async (s, e) => await Initialize();

            SelectedPersonCommentListViewItemViewModels.CollectionChanged += (s, e) =>
            {
                SelectedPersonComments.Objects = SelectedPersonCommentListViewItemViewModels.Select(_ => _.PersonComment);
            };

            SelectedPersonComments.PropertyChanged += (s, e) =>
            {
                UpdatePersonCommentCommand.RaiseCanExecuteChanged();
                DeletePersonCommentsCommand.RaiseCanExecuteChanged();
            };

            SelectedPersonVariantListViewItemViewModels.CollectionChanged += (s, e) =>
            {
                SelectedPersonVariants.Objects = SelectedPersonVariantListViewItemViewModels.Select(_ => _.PersonVariant);
            };
        }

        private async Task Initialize()
        {
            if (_people.Objects.Count() != 1)
            {
                IsVisible = false;
                return;
            }

            var person = _people.Objects.Single();

            using var unitOfWork1 = UnitOfWorkFactory.GenerateUnitOfWork();
            person = await unitOfWork1.People.GetIncludingComments(person.ID);

            if (person.Comments != null)
            {
                PersonCommentListViewItemViewModels.Clear();

                person.Comments.ToList().ForEach(pc =>
                {
                    PersonCommentListViewItemViewModels.Add(new PersonCommentListViewItemViewModel
                    {
                        PersonComment = pc
                    });
                });
            }

            using var unitOfWork2 = UnitOfWorkFactory.GenerateUnitOfWork();
            var personVariants = await unitOfWork2.People.GetAllVariants(person.ID);

            PersonVariantListViewItemViewModels.Clear();
            personVariants.ToList().ForEach(pv =>
            {
                PersonVariantListViewItemViewModels.Add(new PersonVariantListViewItemViewModel{PersonVariant = pv});
            });

            IsVisible = true;
        }

        private async Task CreatePersonComment(
            object owner)
        {
            throw new NotImplementedException();
        }

        private async Task UpdatePersonComment(
            object owner)
        {
            throw new NotImplementedException();
        }

        private bool CanUpdatePersonComment(
            object owner)
        {
            return SelectedPersonComments.Objects != null && SelectedPersonComments.Objects.Count() == 1;
        }

        private async Task SoftDeleteSelectedPersonComments(
            object owner)
        {
            throw new NotImplementedException();
        }

        private bool CanSoftDeleteSelectedPersonComments(
            object owner)
        {
            return SelectedPersonComments.Objects != null &&
                   SelectedPersonComments.Objects.Any() &&
                   SelectedPersonComments.Objects.All(_ => _.End.Year == 9999);
        }

        private void PersonVariantSelectionChanged(
            object obj)
        {
            var temp = (IList)obj;

            var selectedPersonVariantListViewItemViewModels = temp.Cast<PersonVariantListViewItemViewModel>();

            SelectedPersonVariants.Objects = selectedPersonVariantListViewItemViewModels.Select(_ => _.PersonVariant);

            Error = "";

            UpdatePersonVariantCommand.RaiseCanExecuteChanged();
            ErasePersonVariantsCommand.RaiseCanExecuteChanged();
        }

        private async Task CreatePersonVariant(
            object owner)
        {
            var otherVariants = PersonVariantListViewItemViewModels
                .Select(_ => _.PersonVariant)
                .OrderBy(_ => _.Start)
                .ToList();

            var personID = PersonVariantListViewItemViewModels.Last().PersonVariant.ID;

            var person = new Person
            {
                ID = personID,
                Start = DateTime.UtcNow.Date,
                End = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc)
            };

            var dialogViewModel = new CreateOrUpdatePersonDialogViewModel(
                _application,
                person,
                otherVariants); 

            if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) != DialogResult.OK)
            {
                return;
            }

            await Initialize();
        }

        private async Task UpdatePersonVariant(
            object owner)
        {
            var selectedPersonVariant = SelectedPersonVariants.Objects.Single();

            var otherPersonVariants = PersonVariantListViewItemViewModels
                .Select(_ => _.PersonVariant)
                .Where(_ => _ != selectedPersonVariant);

            var otherVariants= otherPersonVariants
                .OrderBy(_ => _.Start);

            var dialogViewModel = new CreateOrUpdatePersonDialogViewModel(
                _application,
                selectedPersonVariant,
                otherVariants);

            dialogViewModel.FirstName = selectedPersonVariant.FirstName;
            dialogViewModel.Surname = selectedPersonVariant.Surname;
            dialogViewModel.Nickname = selectedPersonVariant.Nickname;
            dialogViewModel.Address = selectedPersonVariant.Address;
            dialogViewModel.ZipCode = selectedPersonVariant.ZipCode;
            dialogViewModel.City = selectedPersonVariant.City;
            dialogViewModel.Birthday = selectedPersonVariant.Birthday;
            dialogViewModel.Category = selectedPersonVariant.Category;
            dialogViewModel.Latitude = selectedPersonVariant.Latitude == null ? "" : selectedPersonVariant.Latitude.Value.ToString(CultureInfo.InvariantCulture);
            dialogViewModel.Longitude = selectedPersonVariant.Longitude == null ? "" : selectedPersonVariant.Longitude.Value.ToString(CultureInfo.InvariantCulture);
            dialogViewModel.StartDate = selectedPersonVariant.Start;
            dialogViewModel.EndDate = selectedPersonVariant.End;

            if (_applicationDialogService.ShowDialog(dialogViewModel, owner as Window) == DialogResult.OK)
            {
                await Initialize();
            }
        }

        private bool CanUpdatePersonVariant(
            object owner)
        {
            return SelectedPersonVariants.Objects != null && SelectedPersonVariants.Objects.Count() == 1;
        }

        private async Task ErasePersonVariants(
            object owner)
        {
            var businessRuleViolations = _application.ErasePersonVariants_ValidateInput(
                SelectedPersonVariants.Objects,
                PersonVariantListViewItemViewModels.Select(_ => _.PersonVariant));

            if (businessRuleViolations.Any())
            {
                Error = businessRuleViolations.First().Value;
            }
            else
            {
                using (var unitOfWork = UnitOfWorkFactory.GenerateUnitOfWork())
                {
                    await unitOfWork.People.EraseRange(SelectedPersonVariants.Objects);
                    unitOfWork.Complete();
                }

                var personVariants = PersonVariantListViewItemViewModels
                    .Select(_ => _.PersonVariant)
                    .Except(SelectedPersonVariants.Objects)
                    .OrderBy(_ => _.Start)
                    .ToList();

                PersonVariantListViewItemViewModels.Clear();

                personVariants.ForEach(pv => PersonVariantListViewItemViewModels.Add(
                    new PersonVariantListViewItemViewModel { PersonVariant = pv }));

            }
        }

        private bool CanErasePersonVariants(
            object owner)
        {
            return SelectedPersonVariants.Objects != null && SelectedPersonVariants.Objects.Any();
        }
    }
}
