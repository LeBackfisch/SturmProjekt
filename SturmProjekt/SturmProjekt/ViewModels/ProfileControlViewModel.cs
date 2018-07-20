using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SturmProjekt.ViewModels
{
    public class ProfileControlViewModel: BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private int _currentPage = -1;
        private int _pageCount;
        private string _fileName;
        public ProfileControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            LeftCommand = new DelegateCommand(Left, CanLeft).ObservesProperty(() => CurrentPage);
            RightCommand = new DelegateCommand(Right, CanRight).ObservesProperty(() => CurrentPage).ObservesProperty(() => PageCount);
            SaveCommand = new DelegateCommand(Save, CanSave).ObservesProperty(() => CurrentPage).ObservesProperty(() => FileName);
            _eventAggregator.GetEvent<ProfileCurrentPageEvent>().Subscribe(currentPage =>
            {
                CurrentPage = currentPage;
            });
            _eventAggregator.GetEvent<ProfilePageCountEvent>().Subscribe(pageCount =>
            {
                PageCount = pageCount;
            });
            _eventAggregator.GetEvent<ProfileFileNameEvent>().Subscribe(name => 
            {
                FileName = name;
            });
        }

        public int PageCount
        {
            get => _pageCount;
            set => SetProperty(ref _pageCount, value);
        }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }
        private bool CanLeft()
        {
            return CurrentPage > 0;
        }
        private void Left()
        {
            _eventAggregator.GetEvent<MoveProfilePageEvent>().Publish(-1);
        }

        private bool CanRight()
        {
            return CurrentPage < PageCount-1;
        }
        private void Right()
        {
            _eventAggregator.GetEvent<MoveProfilePageEvent>().Publish(1);
        }

        private bool CanSave()
        {
            return CurrentPage != -1 && !string.IsNullOrWhiteSpace(FileName);
        }

        private void Save()
        {
            _eventAggregator.GetEvent<SaveProfileEvent>().Publish();
            CurrentPage = 0;
            PageCount = 0;
            FileName = string.Empty;
        }

        public ICommand LeftCommand { get; set; }
        public ICommand RightCommand { get; set; }
        public ICommand SaveCommand { get; set; }
    }
}
