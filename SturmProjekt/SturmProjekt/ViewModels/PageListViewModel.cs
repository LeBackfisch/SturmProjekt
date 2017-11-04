using System.Collections.Generic;
using System.Collections.ObjectModel;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Events;
using SturmProjekt.Models;

namespace SturmProjekt.ViewModels
{
    public class PageListViewModel: BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private IEnumerable<PictureModel> _list;
        private PictureModel _currentPage;

        public PageListViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<PageListEvent>().Subscribe(pagelist =>
            {
                List = new ObservableCollection<PictureModel>(pagelist);
            });
            _eventAggregator.GetEvent<DeletedPageEvent>().Subscribe(page =>
            {
                CurrentPage = page;
            });
        }
        
        public IEnumerable<PictureModel> List
        {
            get => _list;
            set => SetProperty(ref _list, value);
        }


        public PictureModel CurrentPage
        {
            get => _currentPage;
            set
            {
                SetProperty(ref _currentPage, value); 
                _eventAggregator.GetEvent<SelectedPageEvent>().Publish(value);
            }
        }
    }
}
