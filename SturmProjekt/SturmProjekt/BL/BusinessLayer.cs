﻿ using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
 using System.Runtime;
 using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Spire.Pdf;

namespace SturmProjekt.BL
{
    public class BusinessLayer
    {
        public bool IsPdf(string fileName)
        {
            return fileName.EndsWith(".pdf");
        }

        public PdfDocument GetPdfDocument(string filepath)
        {
            if (File.Exists(filepath))
            {
                var pdf = new PdfDocument();
                pdf.LoadFromFile(filepath);
                return pdf;
            }
            else
                return null;
        }

        public List<Bitmap> GetBitMapFromPDF(PdfDocument pdf)
        {
          List<Bitmap> images = new List<Bitmap>();
            int pagecount = pdf.Pages.Count;

            for (var i = 0; i < pagecount; i++)
            {
                Image bmp = pdf.SaveAsImage(i);
                var bitmap = new Bitmap(bmp);
                images.Add(bitmap);
            }
            return images;
        } 

        public string GetFileNameFromFilePath(string filepath)
        {
            var path = filepath.Split('\\').ToList<string>();
            return path.Last();
        }

        public Bitmap ConvertImageToBitmap(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                var image = Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);

            }
            return bitmap;
        }

        public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
