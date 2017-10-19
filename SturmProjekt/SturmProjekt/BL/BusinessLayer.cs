 using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        public BitmapImage GetBitMapFromPicture(string filepath)
        {
            if (File.Exists(filepath))
            {
                BitmapImage bitmapImage;
                using (Stream bmpStream = System.IO.File.Open(filepath, System.IO.FileMode.Open))
                {
                    bitmapImage = ImageFromStream(bmpStream);
                }

                return bitmapImage;
            }
            else return null;
        }

        public BitmapImage ImageFromStream(Stream bmpStream)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = bmpStream;
            image.EndInit();
            return image;
        }

      /*  public List<BitmapImage> GetBitMapFromPDF(PdfDocument pdf)
        {
            
        } */

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
