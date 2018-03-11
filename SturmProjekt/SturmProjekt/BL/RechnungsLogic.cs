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
using tessnet2;
using System.Drawing.Drawing2D;
using System.Windows.Documents;
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
            int difference = 0;
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
                   // break;
                    difference++;
                }
            }

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);


            return result;
        }

        public bool Equals(Bitmap bmp1, Bitmap bmp2)
        {
            if (!bmp1.Size.Equals(bmp2.Size))
            {
                return false;
            }
            for (int x = 0; x < bmp1.Width; ++x)
            {
                for (int y = 0; y < bmp1.Height; ++y)
                {
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool difference(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Size != bmp2.Size)
            {
                return false;
            }

            float diff = 0;

            for (int y = 0; y < bmp1.Height; y++)
            {
                for (int x = 0; x < bmp1.Width; x++)
                {
                    diff += (float)Math.Abs(bmp1.GetPixel(x, y).R - bmp2.GetPixel(x, y).R) / 255;
                    diff += (float)Math.Abs(bmp1.GetPixel(x, y).G - bmp2.GetPixel(x, y).G) / 255;
                    diff += (float)Math.Abs(bmp1.GetPixel(x, y).B - bmp2.GetPixel(x, y).B) / 255;
                }
            }
            return false;
        }

        private List<Bitmap> CropBitMaps(List<Bitmap> bitmaps, int x, int y)
        {
            List<Bitmap> CorrectedBitmaps = new List<Bitmap>();
            foreach (var bitmap in bitmaps)
            {
                Rectangle rectangle = new Rectangle(x, y, bitmap.Width, bitmap.Height);
                PixelFormat format = bitmap.PixelFormat;
                Bitmap correctedBitmap = bitmap.Clone(rectangle, format);
                CorrectedBitmaps.Add(correctedBitmap);
                bitmap.Dispose();
            }
            GC.Collect();
            return new List<Bitmap>();
        }

        public List<string> GetOcrInfo(List<Bitmap> infoBitmaps, int x, int y)
        {
            var values = new List<string>();
            List<Bitmap> CorrectedBitmaps = new List<Bitmap>();
            infoBitmaps.RemoveAt(0);

            CorrectedBitmaps = CropBitMaps(infoBitmaps, x, y);
            int i = 0;
            
            foreach (var correctedBitmap in CorrectedBitmaps)
            {
                correctedBitmap.Save("test"+i+".jpg", ImageFormat.Jpeg);
                var ocr = new tessnet2.Tesseract();
                ocr.Init(@"I:\Clemens-Projekt\SturmProjekt\SturmProjekt\SturmProjekt\Content\tessdata",
                    "deu", false);
                var res = ocr.DoOCR(correctedBitmap, Rectangle.Empty);

                var builder = new StringBuilder();

                foreach (var re in res)
                {
                    builder.Append(re.Text);
                    builder.Append(" ");
                }

                var value = builder.ToString();
                value = value.Replace(',', '.');
                values.Add(value);
                i++;
            }

            return values;
        }

        public bool Compare(Bitmap bmp1, Bitmap bmp2)
        {
            bool same = true;

            //Test to see if we have the same size of image
            if (bmp1.Size != bmp2.Size)
            {
                same = false;
            }
            else
            {
                //Sizes are the same so start comparing pixels
                for (int x = 0; x < bmp1.Width && same.Equals(true); x++)
                {
                    for (int y = 0; y < bmp1.Height && same.Equals(true); y++)
                    {
                        if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                            same =false;
                    }
                }
            }
            return same;
        }

        public Bitmap getDifferencBitmap(Bitmap bmp1, Bitmap bmp2)
        {
            Size s1 = bmp1.Size;
            Size s2 = bmp2.Size;
            if (s1 != s2) return null;


            Bitmap bmp3 = new Bitmap(s1.Width, s1.Height);

            for (int y = 0; y < s1.Height; y++)
            for (int x = 0; x < s1.Width; x++)
            {
                Color c1 = bmp1.GetPixel(x, y);
                Color c2 = bmp2.GetPixel(x, y);
                if (c1 == c2) bmp3.SetPixel(x, y, c1);
                else bmp3.SetPixel(x, y, Color.Red);
            }
            return bmp3;
        }

        public void BitmapComparison(Bitmap first, Bitmap second)
        {
            Size s1 = first.Size;
            Size s2 = second.Size;

            Bitmap f1 = new Bitmap(first, first.Width/4, first.Height/4);
            Bitmap f2 = new Bitmap(second, second.Width / 4, second.Height / 4);


            if (s1 == s2)
            {
                int DiferentPixels = 0;
                Bitmap container = new Bitmap(first.Width, first.Height);
                for (int i = 0; i < f1.Width; i++)
                {
                    for (int j = 0; j < f1.Height; j++)
                    {
                        Color secondColor = f2.GetPixel(i, j);
                        Color firstColor = f1.GetPixel(i, j);

                        if (firstColor != secondColor)
                        {
                            DiferentPixels++;
                            container.SetPixel(i, j, Color.Red);
                        }
                        else
                        {
                            container.SetPixel(i, j, firstColor);
                        }
                    }
                }
                int TotalPixels = f1.Width * f1.Height;
                float dierence = (float)((float)DiferentPixels / (float)TotalPixels);
                float percentage = dierence * 100;

                container.Save(@"I:\tributes\difference.jpg", ImageFormat.Jpeg);
            } 

            
        }

        public float Test(Bitmap img1, Bitmap img2)
        {

            if (img1.Size == img2.Size)
            {
                float diff = 0;

                for (int y = 0; y < img1.Height; y++)
                {
                    for (int x = 0; x < img1.Width; x++)
                    {
                        diff += (float)Math.Abs(img1.GetPixel(x, y).R - img2.GetPixel(x, y).R) / 255;
                        diff += (float)Math.Abs(img1.GetPixel(x, y).G - img2.GetPixel(x, y).G) / 255;
                        diff += (float)Math.Abs(img1.GetPixel(x, y).B - img2.GetPixel(x, y).B) / 255;
                    }
                }

                return (100 * diff / (img1.Width * img1.Height * 3));
            }
            return 100.0f;
        }

    }
}
