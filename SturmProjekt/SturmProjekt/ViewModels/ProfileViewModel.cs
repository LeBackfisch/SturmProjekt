using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Events;
using SturmProjekt.Models;

namespace SturmProjekt.ViewModels
{
    public class ProfileViewModel: BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private RechnungsModel _rechnung;
        private PictureModel _rechnungsPage;
        private List<PictureModel> _rechnungsList;
        private int _currentPageNumber;
        private int _pageCount;
        private IEnumerable<ProfileModel> _profileList;
        private ProfileModel _selectedProfile;
        private RechnungsModel _rechnungWithoutLines;


        public ProfileViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            ProfileList = new ObservableCollection<ProfileModel>(_bl.GetProfileList());
            _eventAggregator = eventAggregator;
            PreviousCommand = new DelegateCommand(Previous, CanPrevious).ObservesProperty(()=> CurrentPageNumber).ObservesProperty(() => PageCount);
            NextCommand = new DelegateCommand(Next, CanNext).ObservesProperty(()=> CurrentPageNumber).ObservesProperty(() => PageCount);
            DrawCommand = new DelegateCommand(Draw, CanDraw).ObservesProperty(()=> RechnungsPage);
            _eventAggregator.GetEvent<CreateRechnungEvent>().Subscribe(rechnung =>
            {
                Rechnung = rechnung;
                PageCount = rechnung.PageCount;
                RechnungsList = rechnung.Pages;
                RechnungsPage = RechnungsList.First();
                CurrentPageNumber = 1;
            });
            
           

        }

        private bool CanDraw()
        {
            return RechnungsPage != null;
        }

        private void Draw()
        {
           var drawLinesRechnung = _bl.DrawOnRechnungsModel(Rechnung);
           
            RechnungWithoutLines = Rechnung;
            Rechnung = drawLinesRechnung;
            RechnungsList = drawLinesRechnung.Pages;
            RechnungsPage = drawLinesRechnung.Pages[CurrentPageNumber-1];
            _eventAggregator.GetEvent<DrawOnRechnungEvent>().Publish(drawLinesRechnung);
        }

        private bool CanNext()
        {
            return CurrentPageNumber < PageCount && Rechnung != null && PageCount > 0;
        }

        private void Next()
        {
            CurrentPageNumber++;
            RechnungsPage = RechnungsList[CurrentPageNumber - 1];
            _eventAggregator.GetEvent<RechnungsPageEvent>().Publish(RechnungsPage);
        }

        private void Previous()
        {
            CurrentPageNumber--;
            RechnungsPage = RechnungsList[CurrentPageNumber-1];
            _eventAggregator.GetEvent<RechnungsPageEvent>().Publish(RechnungsPage);

        }

        public RechnungsModel RechnungWithoutLines
        {
            get { return _rechnungWithoutLines; }
            set { SetProperty(ref _rechnungWithoutLines, value); }
        }

        private bool CanPrevious()
        {
            return CurrentPageNumber-1 > 0 && Rechnung != null && PageCount > 0;
        }

        public RechnungsModel Rechnung
        {
            get { return _rechnung; }
            set { SetProperty(ref _rechnung, value); }
        }

        public PictureModel RechnungsPage
        {
            get { return _rechnungsPage; }
            set
            {
                SetProperty(ref _rechnungsPage,value); 
                _eventAggregator.GetEvent<RechnungsPageEvent>().Publish(RechnungsPage);
            }
        }

        public List<PictureModel> RechnungsList
        {
            get { return _rechnungsList; }
            set { SetProperty(ref _rechnungsList, value); }
        }

        public int CurrentPageNumber
        {
            get { return _currentPageNumber; }
            set { SetProperty(ref _currentPageNumber,value); }
        }

        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }

        public IEnumerable<ProfileModel> ProfileList
        {
            get { return _profileList; }
            set { SetProperty(ref _profileList, value); }
        }

        public ProfileModel SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                SetProperty(ref _selectedProfile, value);
               // _eventAggregator.GetEvent<DrawOnRechnungEvent>().Publish(_selectedProfile);
            }
        }


        public ICommand PreviousCommand { get; set; }
        public ICommand NextCommand { get; set; }
        public ICommand DrawCommand { get; set; }
        public string FilePath { get; set; }
        
    }
}
