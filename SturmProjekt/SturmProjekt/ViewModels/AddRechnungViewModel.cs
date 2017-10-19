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
            return _currentPage == null;
        }

        private void Delete()
        {
           // PictureModels = PictureModels.Remove();
        }

        public List<PictureModel> PictureModels
        {
            get { return _pictureModels; }
            set
            {
                SetProperty(ref _pictureModels, value);
                PictureCount = _pictureModels.Count;
                _eventAggregator.GetEvent<AddedPageEvent>().Publish(_pictureModels);
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
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg |PDFs (*.pdf)|*.pdf"
            };
            if (open.ShowDialog() == true)
            {
                var picture = new PictureModel();
                picture.FileName = open.FileName;
                if (_bl.IsPdf(picture.FileName))
                {
                    
                }
                else
                {
                   picture.Page = _bl.ConvertImageToBitmap(picture.FileName);
                    picture.PageImage = _bl.BitmapToImageSource(picture.Page);
                }

                PictureModels.Add(picture);
                
            }
        }





    }
}
