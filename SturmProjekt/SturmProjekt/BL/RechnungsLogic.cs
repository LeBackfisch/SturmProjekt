using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
               // cutout.First().Save(@"I:\vgames\logo.jpg", ImageFormat.Jpeg);
                cutoutbitmaps.AddRange(cutout);
                index++;
            }
            return cutoutbitmaps;
        }

        public List<string> GetCategoryDirectories(string datapath)
        {
            List<string> directoryList = new List<string>();
            if (Directory.Exists(datapath))
            {
                directoryList = Directory.GetDirectories(datapath).ToList();
            }
            return directoryList;
        }


        public bool CompareBitmaps(Bitmap compareBitmap, Bitmap bitmapFromPicture)
        {
            MemoryStream ms = new MemoryStream();
            compareBitmap.Save(ms, ImageFormat.Png);
            string firstBitmap = Convert.ToBase64String(ms.ToArray());
            ms.Position = 0;

            bitmapFromPicture.Save(ms, ImageFormat.Png);
            string secondBitmap = Convert.ToBase64String(ms.ToArray());

            if (firstBitmap.Equals(secondBitmap))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
