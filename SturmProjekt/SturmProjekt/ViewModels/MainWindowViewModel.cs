using System;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Events;
using SturmProjekt.Models;

namespace SturmProjekt.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private PictureModel _currentPage;
        private PictureModel _rechnungsPage;

        public MainWindowViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            CurrentPage = null;
            _bl = bl;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<AddedPageEvent>().Subscribe(page =>
            {              
                CurrentPage = page;
            });
            _eventAggregator.GetEvent<SelectedPageEvent>().Subscribe((page =>
            {
                CurrentPage = page;
            }));
            _eventAggregator.GetEvent<RemovePictureEvent>().Subscribe(page =>
            {
                if (CurrentPage.FileName == page.FileName)
                {
                    CurrentPage.Page.Dispose();
                    CurrentPage = null;
                    GC.Collect();
                }
                    
            });
            _eventAggregator.GetEvent<RechnungsPageEvent>().Subscribe(page =>
            {
                RechnungsPage = page;
            });
            _eventAggregator.GetEvent<FreeLockEvent>().Subscribe(locked =>
            {
                if (locked)
                {
                    CurrentPage.Page.Dispose();
                    CurrentPage = null;
                    GC.Collect();
                }
            });
            _eventAggregator.GetEvent<RemovePicturesEvent>().Subscribe(x =>
            {
                CurrentPage.Page.Dispose();
                CurrentPage = null;
                GC.Collect();
            });
        }

        public MainWindowViewModel()
        {

        }

        public PictureModel CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public PictureModel RechnungsPage
        {
            get => _rechnungsPage;
            set => SetProperty(ref _rechnungsPage,value);
        }
    }
}
