using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private string _buttonText = "Add Lines";
        private string _sorted = "";
        private bool _buttonclicked;


        public ProfileViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            ProfileList = new ObservableCollection<ProfileModel>(_bl.GetProfileList());
            _eventAggregator = eventAggregator;
            DrawCommand = new DelegateCommand(Draw, CanDraw).ObservesProperty(()=> RechnungsPage).ObservesProperty(() => SelectedProfile);
            CancelCommand = new DelegateCommand(Cancel, CanCancel).ObservesProperty(() => Rechnung);
            SortRechnungCommnad = new DelegateCommand(Sort, CanSort).ObservesProperty(() => Rechnung).ObservesProperty(() => SelectedProfile);
            _eventAggregator.GetEvent<CreateRechnungEvent>().Subscribe(rechnung =>
            {
                Rechnung = rechnung;
                RechnungWithoutLines = rechnung;
                PageCount = rechnung.PageCount;
                RechnungsList = rechnung.Pages;
                RechnungsPage = RechnungsList.First();
                CurrentPageNumber = 1;
            });
            _eventAggregator.GetEvent<ChosenPageEvent>().Subscribe(pagenumber =>
            {
                CurrentPageNumber = pagenumber;
                RechnungsPage = RechnungsList.ElementAt(CurrentPageNumber-1);
            });
            _eventAggregator.GetEvent<NewFileEvent>().Subscribe(x =>
            {
                Sorted = "";
            });

        }

        private bool CanCancel()
        {
            return Rechnung != null;
        }

        private void Cancel()
        {
            Rechnung = null;
            RechnungWithoutLines = null;
            foreach (var rechnung in RechnungsList)
            {
                rechnung.Page.Dispose();
            }
            RechnungsList = null;
            RechnungsPage = null;
            _eventAggregator.GetEvent<FreeLockEvent>().Publish(false);
        }

        private bool CanSort()
        {
            return SelectedProfile != null && Rechnung != null;
        }

        private void Sort()
        {
            var sorted = _bl.CutOutBitmaps(RechnungWithoutLines, SelectedProfile, true).Result;
            Rechnung = null;
            RechnungWithoutLines = null;
            if (sorted != -1)
            {
                Sorted = "Rechnung wurde in " + SelectedProfile.Name + " eingeordnet!";
            }
            else
            {
                Sorted = "Rechnung konnte nicht eingeordnet werden!";
            }

            foreach (var rechnung in RechnungsList)
            {
                rechnung.Page.Dispose();
            }
            RechnungsList = null;
            RechnungsPage = null;
            _buttonclicked = false;
            ButtonText = "Add Lines";
            _eventAggregator.GetEvent<FreeLockEvent>().Publish(true);
        }

        private bool CanDraw()
        {
            return RechnungsPage != null && SelectedProfile != null;
        }

        private void Draw()
        {
            if (_buttonclicked == false)
            {
                var drawLinesRechnung = _bl.DrawOnRechnungsModel(RechnungWithoutLines, SelectedProfile);
                ButtonText = "Remove Lines";
                Rechnung = drawLinesRechnung;
                RechnungsList = drawLinesRechnung.Pages;
                RechnungsPage = drawLinesRechnung.Pages[CurrentPageNumber - 1];
                _buttonclicked = true;
            }
            else
            {
                ButtonText = "Add Lines";
                Rechnung = RechnungWithoutLines;
                RechnungsList = RechnungWithoutLines.Pages;
                RechnungsPage = RechnungWithoutLines.Pages[CurrentPageNumber - 1];
                _buttonclicked = false;
            }
           
        }

        public string Sorted
        {
            get => _sorted;
            set => SetProperty(ref _sorted, value);
        }

        public RechnungsModel RechnungWithoutLines
        {
            get => _rechnungWithoutLines;
            set => SetProperty(ref _rechnungWithoutLines, value);
        }

        public RechnungsModel Rechnung
        {
            get => _rechnung;
            set => SetProperty(ref _rechnung, value);
        }

        public PictureModel RechnungsPage
        {
            get => _rechnungsPage;
            set
            {
                SetProperty(ref _rechnungsPage,value); 
                _eventAggregator.GetEvent<RechnungsPageEvent>().Publish(RechnungsPage);
            }
        }

        public List<PictureModel> RechnungsList
        {
            get => _rechnungsList;
            set => SetProperty(ref _rechnungsList, value);
        }

        public int CurrentPageNumber
        {
            get => _currentPageNumber;
            set => SetProperty(ref _currentPageNumber,value);
        }

        public int PageCount
        {
            get => _pageCount;
            set => SetProperty(ref _pageCount, value);
        }

        public IEnumerable<ProfileModel> ProfileList
        {
            get => _profileList;
            set => SetProperty(ref _profileList, value);
        }

        public ProfileModel SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                    Rechnung = RechnungWithoutLines;
                    RechnungsList = RechnungWithoutLines.Pages;
                    RechnungsPage = RechnungWithoutLines.Pages[CurrentPageNumber - 1];
                    ButtonText = "Add Lines";
                    _buttonclicked = false;
           
                SetProperty(ref _selectedProfile, value);
            }
        }

        public string ButtonText
        {
            get => _buttonText;
            set => SetProperty(ref _buttonText, value);
        }

        public ICommand DrawCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand SortRechnungCommnad { get; set; }
           
    }
}
