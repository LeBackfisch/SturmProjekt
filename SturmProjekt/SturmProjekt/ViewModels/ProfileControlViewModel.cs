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
        private int _currentPage;
        private int _pageCount;
        public ProfileControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            LeftCommand = new DelegateCommand(Left, CanLeft).ObservesProperty(() => CurrentPage);
            RightCommand = new DelegateCommand(Right, CanRight).ObservesProperty(() => CurrentPage).ObservesProperty(() => PageCount);
            _eventAggregator.GetEvent<ProfileCurrentPageEvent>().Subscribe(currentPage =>
            {
                CurrentPage = currentPage;
            });
            _eventAggregator.GetEvent<ProfilePageCountEvent>().Subscribe(pageCount =>
            {
                PageCount = pageCount;
            });
        }

        public int PageCount
        {
            get => _pageCount;
            set => SetProperty(ref _pageCount, value);
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

        public ICommand LeftCommand { get; set; }
        public ICommand RightCommand { get; set; }
        public ICommand SaveCommand { get; set; }
    }
}
