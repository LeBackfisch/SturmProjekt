using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Events;
using SturmProjekt.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SturmProjekt.ViewModels
{
    public class ProfileImageViewModel: BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private BitmapImage _pageImage;
        private Bitmap _pageBitmap;

        public ProfileImageViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            _eventAggregator = eventAggregator;
            
            _eventAggregator.GetEvent<ProfileDrawLinesEvent>().Subscribe(drawlines => 
            {               
                var lineslist = new List<LinesModel>(drawlines);
                PageBitmap = new Bitmap(2480, 3508);
                DrawClearImage();
                DrawPageLines(lineslist);
                     
            });
            _eventAggregator.GetEvent<SaveProfileEvent>().Subscribe(() => 
            {
                DrawClearImage();
            });

        }

        public BitmapImage PageImage
        {
            get => _pageImage;
            set => SetProperty(ref _pageImage, value);
        }

        public Bitmap PageBitmap
        {
            get => _pageBitmap;
            set => SetProperty(ref _pageBitmap, value);
        }

        private void DrawPageLines(List<LinesModel> pageLines)
        {
            PageBitmap = _bl.DrawonBitmap(PageBitmap, pageLines);
            PageImage = _bl.BitmapToImageSource(PageBitmap);
        }

        private void DrawClearImage()
        {
            PageBitmap = _bl.GetClearBitmap(PageBitmap);
            PageImage = _bl.BitmapToImageSource(PageBitmap);
        }
     
    }
}
