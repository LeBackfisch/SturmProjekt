using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SturmProjekt.Models;

namespace SturmProjekt.BL
{
    public class RechnungsLogic
    {
        public List<Bitmap> CutoutBitmap(Bitmap sourceBitmap, ProfilePages profilePage)
        {
            List<LinesModel> lines = profilePage.DrawLines;

            return lines.Select(line => new Rectangle(line.X, line.Y, line.Width, line.Height)).Select(srcRect => sourceBitmap.Clone(srcRect, sourceBitmap.PixelFormat)).ToList();
        }

        public async Task<List<Bitmap>> GetCutOutBitmaps(List<Bitmap> bitmaps, List<ProfilePages> pages)
        {
            int index = 0;
            var cutoutbitmaps = new List<Bitmap>();
            foreach (var bitmap in bitmaps)
            {
                if (pages.Count > index)
                {
                    var index1 = index;
                    var cutout = CutoutBitmap(bitmap, pages.ElementAt(index1));
                    cutoutbitmaps.AddRange(cutout);
                }
                
                index++;
            }
            return cutoutbitmaps;
        }

        public async Task<List<string>> GetCategoryDirectories(string datapath)
        {
            List<string> directoryList = new List<string>();
            if (Directory.Exists(datapath))
            {
                directoryList = Directory.GetDirectories(datapath).ToList();
            }
            return directoryList;
        }
   
        public List<string> GetOcrInfo(List<Bitmap> infoBitmaps, int x, int y, string datapath)
        {
            var values = new List<string>();
          //  List<Bitmap> CorrectedBitmaps = new List<Bitmap>();
            infoBitmaps.RemoveAt(0);

         //   CorrectedBitmaps = CropBitMaps(infoBitmaps, x, y);
            int i = 0;
            
            foreach (var correctedBitmap in infoBitmaps)
            {
                correctedBitmap.Save("test"+i+".jpg", ImageFormat.Jpeg);
                var ocr = new tessnet2.Tesseract();
                ocr.Init(datapath,
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
                ocr.Dispose();
            }

            return values;
        }

        public async Task<float> CalculateDifference(Bitmap img1, Bitmap img2)
        {
            if (img1.Size != img2.Size) return 100.0f;
            float diff = 0;

            for (int y = 0; y < img1.Height; y++)
            {
                for (int x = 0; x < img1.Width; x++)
                {
                    diff += await Task.Run(() => (float)Math.Abs(img1.GetPixel(x, y).R - img2.GetPixel(x, y).R) / 255);
                    diff += await Task.Run(() => (float)Math.Abs(img1.GetPixel(x, y).G - img2.GetPixel(x, y).G) / 255);
                    diff += await Task.Run(() => (float)Math.Abs(img1.GetPixel(x, y).B - img2.GetPixel(x, y).B) / 255);
                }
            }

            return (100 * diff / (img1.Width * img1.Height * 3));
        }

    }
}
