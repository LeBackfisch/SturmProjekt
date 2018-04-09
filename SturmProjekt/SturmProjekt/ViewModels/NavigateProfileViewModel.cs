using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Events;

namespace SturmProjekt.ViewModels
{
    public class NavigateProfileViewModel: BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private int _pageCount;
        private int _currentPageNumber;
        private bool _clearData = false;

        public NavigateProfileViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            _eventAggregator = eventAggregator;
            PreviousCommand = new DelegateCommand(Previous, CanPrevious).ObservesProperty(() => CurrentPageNumber).ObservesProperty(() => PageCount);
            NextCommand = new DelegateCommand(Next, CanNext).ObservesProperty(() => CurrentPageNumber).ObservesProperty(() => PageCount);
            _eventAggregator.GetEvent<CreateRechnungEvent>().Subscribe(rechnung =>
            {
                PageCount = rechnung.PageCount;
                CurrentPageNumber = 1;
            });
            _eventAggregator.GetEvent<FreeLockEvent>().Subscribe(freeData =>
            {
                ClearData = freeData;
                PageCount = 0;
                CurrentPageNumber = 0;
                ClearData = false;
            });
        }

        private bool ClearData
        {
            get => _clearData;
            set => SetProperty(ref _clearData, value);
        }
        private bool CanNext()
        {
            return CurrentPageNumber < PageCount && PageCount > 0;
        }

        private void Next()
        {
            CurrentPageNumber++;
            _eventAggregator.GetEvent<ChosenPageEvent>().Publish(CurrentPageNumber);
        }

        private void Previous()
        {
            CurrentPageNumber--;
            _eventAggregator.GetEvent<ChosenPageEvent>().Publish(CurrentPageNumber);
        }

        private bool CanPrevious()
        {
            return CurrentPageNumber - 1 > 0 && PageCount > 0;
        }

        public int PageCount
        {
            get => _pageCount;
            set => SetProperty(ref _pageCount, value);
        }

        public int CurrentPageNumber
        {
            get => _currentPageNumber;
            set => SetProperty(ref _currentPageNumber, value);
        }

        public ICommand PreviousCommand { get; set; }
        public ICommand NextCommand { get; set; }
    }
}
