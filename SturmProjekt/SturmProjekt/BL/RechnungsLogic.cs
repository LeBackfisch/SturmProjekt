using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SturmProjekt.Models;

namespace SturmProjekt.BL
{
    public class RechnungsLogic
    {
        public List<Bitmap> CutoutBitmap(Bitmap sourceBitmap, ProfilePages profilePage)
        {
            List<Bitmap> cutoutBitmaps = new List<Bitmap>();
            List<LinesModel> lines = profilePage.DrawLines;

            foreach (var line in lines)
            {
                var srcRect = new Rectangle(line.X, line.Y, line.Width, line.Height);
               cutoutBitmaps.Add(sourceBitmap.Clone(srcRect, sourceBitmap.PixelFormat));
            }

            return cutoutBitmaps;
        }

        public List<Bitmap> GetCutOutBitmaps(List<Bitmap> bitmaps, List<ProfilePages> pages)
        {
            int index = 0;
            var cutoutbitmaps = new List<Bitmap>();
            foreach (var bitmap in bitmaps)
            {
                var cutout = CutoutBitmap(bitmap, pages.ElementAt(index));
                cutoutbitmaps.AddRange(cutout);
                index++;
            }
            return cutoutbitmaps;
        }


    }
}
