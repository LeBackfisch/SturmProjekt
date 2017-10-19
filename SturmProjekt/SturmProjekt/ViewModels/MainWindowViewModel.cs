using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MainWindowViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<AddedPageEvent>().Subscribe(page =>
            {
                CurrentPage = page.First();
            });
        }

        public MainWindowViewModel()
        {

        }

        public PictureModel CurrentPage
        {
            get { return _currentPage; }
            set { SetProperty(ref _currentPage, value); }
        }

        
    }
}
