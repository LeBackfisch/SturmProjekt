using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SturmProjekt.Models;
using Image = System.Drawing.Image;

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
                if (pages.Count > index)
                {
                    var cutout = CutoutBitmap(bitmap, pages.ElementAt(index));
                    cutoutbitmaps.AddRange(cutout);
                }
                
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

        public bool CompareBitmapsFast(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 == null || bmp2 == null)
                return false;
            if (object.Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;

            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            bool result = true;
            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width - 1, bmp1.Height - 1), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width - 1, bmp2.Height - 1), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

            for (int n = 0; n <= bytes - 1; n++)
            {
                if (b1bytes[n] != b2bytes[n])
                {
                    result = false;
                    break;
                }
            }

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);

            return result;
        }
    }
}
