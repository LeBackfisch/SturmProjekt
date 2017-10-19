using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SturmProjekt.Models
{
    public class PictureModel
    {
        public string FileName { get; set; }

        public Bitmap Page { get; set; }

        public BitmapImage PageImage { get; set; }


    }
}
