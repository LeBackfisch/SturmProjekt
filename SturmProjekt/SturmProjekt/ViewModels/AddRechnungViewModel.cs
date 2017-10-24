using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private int _pictureCount;

        public AddRechnungViewModel(BusinessLayer business, IEventAggregator eventAggregator)
        {
            _bl = business;
            _eventAggregator = eventAggregator;
            OpenFileCommand = new DelegateCommand(Open, CanOpen);
            DeletePageCommand = new DelegateCommand(Delete, CanDelete);
            ConfirmCommand = new DelegateCommand(Confirm, CanConfirm);
            PictureModels = new List<PictureModel>();
            
            _eventAggregator.GetEvent<SelectedPageEvent>().Subscribe(page =>
            {
                CurrentPage = page;
            });
        }

        private bool CanConfirm()
        {
            //unfinished
            return true;
        }

        private void Confirm()
        {
            //unfinished
            var rechnung = new RechnungsModel
            {
                Name = "name",
                PageCount = PictureModels.Count,
                Pages = PictureModels
            };
            _eventAggregator.GetEvent<CreateRechnungEvent>().Publish(rechnung);
        }


        public PictureModel CurrentPage
        {
            get { return _currentPage; }
            set { SetProperty(ref _currentPage,value); }
        }

        public ICommand ConfirmCommand { get; set; }

        private bool CanDelete()
        {
            return CurrentPage != null && PictureCount > 0;
        }

        private void Delete()
        {
            _eventAggregator.GetEvent<RemovePictureEvent>().Publish(CurrentPage);
            CurrentPage = null;
        }

        public List<PictureModel> PictureModels
        {
            get { return _pictureModels; }
            set
            {
                SetProperty(ref _pictureModels, value);
                
            }
        }

        public int PictureCount
        {
            get { return _pictureCount; }
            set { SetProperty(ref _pictureCount, value); }
        }

        public ICommand DeletePageCommand { get; set; }

        public ICommand OpenFileCommand { get; set; }

        private bool CanOpen()
        {
            return true;
        }

        private void Open()
        {
            var open = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png |PDFs (*.pdf)|*.pdf"
            };
            if (open.ShowDialog() == true)
            {
                var picture = new PictureModel();
                var pictures = new List<PictureModel>();
                picture.FileName = open.FileName;
                if (_bl.IsPdf(picture.FileName))
                {
                    var document = _bl.GetPdfDocument(picture.FileName);
                    var bitmaps = _bl.GetBitMapFromPDF(document);

                    foreach (var bitmap in bitmaps)
                    {
                        var pic = new PictureModel
                        {
                            FileName = picture.FileName,
                            Page = bitmap,
                            PageImage = _bl.BitmapToImageSource(bitmap)
                        };
                        pictures.Add(pic);
                    }

                    PictureModels.AddRange(pictures);

                }
                else
                {
                   picture.Page = _bl.ConvertImageToBitmap(picture.FileName);
                    picture.PageImage = _bl.BitmapToImageSource(picture.Page);
                    PictureModels.Add(picture);
                }

                
                PictureCount = PictureModels.Count;
                if (_pictureModels.Count > 0)
                        _eventAggregator.GetEvent<AddedPageEvent>().Publish(_pictureModels.Last());
                _eventAggregator.GetEvent<PageListEvent>().Publish(_pictureModels);
            }
        }





    }
}
