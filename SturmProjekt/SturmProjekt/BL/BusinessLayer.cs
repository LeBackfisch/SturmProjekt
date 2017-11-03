 using System;
using System.Collections.Generic;
using System.Drawing;
 using System.Drawing.Drawing2D;
 using System.Drawing.Imaging;
 using System.IO;
using System.Linq;
 using System.Runtime;
 using System.Text;
using System.Threading.Tasks;
 using System.Windows;
 using System.Windows.Media;
 using System.Windows.Media.Imaging;
 using Newtonsoft.Json;
 using Spire.Pdf;
 using SturmProjekt.Models;
 using Brushes = System.Drawing.Brushes;
 using Color = System.Drawing.Color;
 using Pen = System.Drawing.Pen;

namespace SturmProjekt.BL
{
    public class BusinessLayer
    {
        private RechnungsLogic _rechnungsLogic;
        private Config _config;
        public BusinessLayer()
        {
            _rechnungsLogic = new RechnungsLogic();
            _config = Config.Instance;
            FilePath = _config.ProfilePath;
        }

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

        public string FilePath { get; set; }

        public List<ProfileModel> GetProfileList()
        {
            var ProfileNames = GetProfileFiles();
            return ParseProfileModel(ProfileNames);
        }

        public List<string> GetProfileFiles()
        {
            var list = new List<string>();
            if (Directory.Exists(FilePath))
            {
                DirectoryInfo d = new DirectoryInfo(FilePath);
                FileInfo[] Files = d.GetFiles("*.json");

                foreach (var file in Files)
                {
                    list.Add(file.Name);
                }
            }
            return list;
        }

        public List<ProfileModel> ParseProfileModel(List<string> FileNames)
        {
            var ProfileList = new List<ProfileModel>();
            foreach (var file in FileNames)
            {
               
                
                 ProfileModel profile = ParseJsonToModel(FilePath + "\\" + file);
                ProfileList.Add(profile);
            }
            return ProfileList;
        }

        public ProfileModel ParseJsonToModel(string filename)
        {
            string content = File.ReadAllText(filename);

            ProfileModel profile = JsonConvert.DeserializeObject<ProfileModel>(content);
            profile.FilePath = filename;
            return profile;
        }

        public RechnungsModel DrawOnRechnungsModel(RechnungsModel Rechnung)
        {
            List<PictureModel> pictureModels = new List<PictureModel>();
            RechnungsModel rechnung = new RechnungsModel();

            foreach (var pictureItem in Rechnung.Pages)
            {
                PictureModel pictureModel = new PictureModel();
                pictureModel.FileName = pictureItem.FileName;
                var bitmap = DrawonBitmap(pictureItem.Page);
                pictureModel.Page = bitmap;
                pictureModel.PageImage = BitmapToImageSource(bitmap);
                pictureModels.Add(pictureModel);
            }
            
            rechnung.Pages = new List<PictureModel>();
            rechnung.Pages.AddRange(pictureModels);
            rechnung.Name = Rechnung.Name;
            rechnung.PageCount = rechnung.Pages.Count;
            return rechnung;
        }

        public Bitmap DrawonBitmap(Bitmap bitmap)
        {

            RectangleF rectf = new RectangleF(70, 90, 90, 50);

            Graphics g = Graphics.FromImage(bitmap);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Pen redPen = new Pen(Brushes.Red);
            g.DrawRectangle(redPen,
                new Rectangle(40, 40, 150, 200));
        
            g.Flush();

            return bitmap;
        }




    }
}
