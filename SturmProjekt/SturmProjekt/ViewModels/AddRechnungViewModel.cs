using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Events;
using SturmProjekt.Models;

namespace SturmProjekt.ViewModels
{
    public class AddRechnungViewModel: BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private List<PictureModel> _pictureModels;
        private PictureModel _currentPage;
        private string _rechnungsName;
        private bool _lockedEdit = false;

        public AddRechnungViewModel(BusinessLayer business, IEventAggregator eventAggregator)
        {
            _bl = business;
            _eventAggregator = eventAggregator;
            OpenFileCommand = new DelegateCommand(Open, CanOpen).ObservesProperty(() => LockedEdit);
            DeletePageCommand = new DelegateCommand(Delete, CanDelete).ObservesProperty(() => CurrentPage).ObservesProperty(() => PictureModels).ObservesProperty(() => LockedEdit);
            ConfirmCommand = new DelegateCommand(Confirm, CanConfirm).ObservesProperty(() => RechnungsName).ObservesProperty(() => CurrentPage).ObservesProperty(() => LockedEdit);
            PictureModels = new List<PictureModel>();
            
            _eventAggregator.GetEvent<SelectedPageEvent>().Subscribe(page =>
            {
                CurrentPage = page;
            });
            _eventAggregator.GetEvent<AddedPageEvent>().Subscribe(page =>
            {
                CurrentPage = page;
            });
            _eventAggregator.GetEvent<FreeLockEvent>().Subscribe(locked =>
            {
                LockedEdit = locked;
            });
        }

        public bool LockedEdit
        {
            get => _lockedEdit;
            set => SetProperty(ref _lockedEdit, value);
        }

        public string RechnungsName
        {
            get => _rechnungsName;
            set => SetProperty(ref _rechnungsName, value);
        }

        private bool CanConfirm()
        {
            return PictureModels.Count > 0 && !(string.IsNullOrEmpty(RechnungsName)) && !LockedEdit;
        }

        private void Confirm()
        {
            var rechnung = new RechnungsModel
            {
                Name = RechnungsName,
                PageCount = PictureModels.Count,
                Pages = PictureModels
            };
            _eventAggregator.GetEvent<CreateRechnungEvent>().Publish(rechnung);
            LockedEdit = true;
        }

        public PictureModel CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage,value);
        }

        public ICommand ConfirmCommand { get; set; }

        private bool CanDelete()
        {
            return CurrentPage != null && PictureModels.Count > 0 && !LockedEdit;
        }

        private void Delete()
        {
            _eventAggregator.GetEvent<RemovePictureEvent>().Publish(CurrentPage);
            int index = PictureModels.IndexOf(CurrentPage);
            CurrentPage.Page.Dispose();
            PictureModels.Remove(CurrentPage);
            
            CurrentPage = null;
            if (PictureModels.Count == 0)
                RechnungsName = "";
            else
            {
                _eventAggregator.GetEvent<DeletedPageEvent>()
                    .Publish(index == 0 ? PictureModels.ElementAt(index) : PictureModels.ElementAt(index - 1));
            } 

            _eventAggregator.GetEvent<PageListEvent>().Publish(PictureModels);
            GC.Collect();
        }

        public List<PictureModel> PictureModels
        {
            get => _pictureModels;
            set => SetProperty(ref _pictureModels, value);
        }

        public ICommand DeletePageCommand { get; set; }

        public ICommand OpenFileCommand { get; set; }

        private bool CanOpen()
        {
            return !LockedEdit;
        }

        private void Open()
        {
            var open = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.PNG |PDFs (*.pdf)|*.pdf"
            };
            if (open.ShowDialog() == true)
            {
                var picture = new PictureModel();
                var pictures = new List<PictureModel>();
                picture.FileName = open.FileName;
                if (_bl.IsPdf(picture.FileName))
                {
                    var document = _bl.GetPdfDocument(picture.FileName).Result;
                    var bitmaps = _bl.GetBitMapFromPDF(document).Result;
                    document.Dispose();

                    pictures.AddRange(bitmaps.Select(bitmap => new PictureModel
                    {
                        FileName = picture.FileName,
                        Page = bitmap,
                        PageImage = _bl.BitmapToImageSource(bitmap).Result
                    }));

                    PictureModels.AddRange(pictures);
                    if (CurrentPage == null)
                    {
                        var filename = _bl.GetFileNameFromFilePath(pictures.First().FileName);
                        RechnungsName = filename.Split('.')[0];
                    }

                }
                else
                {
                   picture.Page =   _bl.ResizeBitmap(_bl.ConvertImageToBitmap(picture.FileName).Result).Result;
                    picture.PageImage = _bl.BitmapToImageSource(picture.Page).Result;
                    PictureModels.Add(picture);
                    if(CurrentPage == null)
                    {
                        var filename = _bl.GetFileNameFromFilePath(picture.FileName);
                        RechnungsName = filename.Split('.')[0];
                    }
                }

                if (_pictureModels.Count > 0)
                        _eventAggregator.GetEvent<AddedPageEvent>().Publish(_pictureModels.Last());
                _eventAggregator.GetEvent<PageListEvent>().Publish(_pictureModels);
            }
        }
    }
}
