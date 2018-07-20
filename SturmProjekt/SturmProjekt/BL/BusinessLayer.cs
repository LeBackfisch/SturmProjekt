using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spire.Pdf;
using SturmProjekt.Models;
using Brushes = System.Drawing.Brushes;
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
            if (!File.Exists(filepath)) return null;
            var pdf = new PdfDocument();
            pdf.LoadFromFile(filepath);
            return pdf;
        }

        public List<Bitmap> GetBitMapFromPDF(PdfDocument pdf)
        {
          var images = new List<Bitmap>();
            var pagecount = pdf.Pages.Count;

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
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
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
            if (!Directory.Exists(FilePath)) return list;
            var d = new DirectoryInfo(FilePath);
            FileInfo[] Files = d.GetFiles("*.json");

            foreach (var file in Files)
            {
                list.Add(file.Name);
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

        public RechnungsModel DrawOnRechnungsModel(RechnungsModel Rechnung, ProfileModel SelectedProfile)
        {
            List<ProfilePages> pages = SelectedProfile.Pages;

            int index = 0;

            List<PictureModel> pictureModels = new List<PictureModel>();
            RechnungsModel rechnung = new RechnungsModel();

            foreach (var pictureItem in Rechnung.Pages)
            {
                PictureModel pictureModel = new PictureModel
                {
                    FileName = pictureItem.FileName
                };
                var bitmap = DrawonBitmap(pictureItem.Page, pages.ElementAt(index));
                pictureModel.Page = bitmap;
                pictureModel.PageImage = BitmapToImageSource(bitmap);
                pictureModels.Add(pictureModel);
                index++;
            }
            
            rechnung.Pages = new List<PictureModel>();
            rechnung.Pages.AddRange(pictureModels);
            rechnung.Name = Rechnung.Name;
            rechnung.PageCount = rechnung.Pages.Count;
            return rechnung;
        }

        public Bitmap DrawonBitmap(Bitmap bitmap, ProfilePages profilePage)
        {
            Graphics g = Graphics.FromImage(bitmap);

            List<LinesModel> lines = new List<LinesModel>(profilePage.DrawLines);

            foreach (var line in lines)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Pen redPen = new Pen(Brushes.Red);
                g.DrawRectangle(redPen,
                    new Rectangle(line.X, line.Y, line.Width, line.Height));
            }        
            g.Flush();

            return bitmap;
        }

        public Bitmap DrawonBitmap(Bitmap bitmap, List<LinesModel> lineModels)
        {
            Graphics g = Graphics.FromImage(bitmap);

            SolidBrush whiteBrush = new SolidBrush(Color.White);
            Rectangle rect = new Rectangle(0, 0, 2480, 3508);
            g.FillRectangle(whiteBrush, rect);

            foreach (var lineModel in lineModels)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                
                Pen redPen = new Pen((Brushes.Red), 3);
                g.DrawRectangle(redPen,
                    new Rectangle(lineModel.X, lineModel.Y, lineModel.Width, lineModel.Height));
            }
            g.Flush();
        
            return bitmap;
        }

        public Bitmap GetClearBitmap(Bitmap bitmap)
        {
            Graphics g = Graphics.FromImage(bitmap);

            SolidBrush whiteBrush = new SolidBrush(Color.White);
            Rectangle rect = new Rectangle(0, 0, 2480, 3508);
            g.FillRectangle(whiteBrush, rect);

            g.Flush();

            return bitmap;
        }

        public void SaveProfile(ProfileModel profileModel)
        {
            if (string.IsNullOrWhiteSpace(profileModel.FilePath))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(FilePath);
                sb.Append(profileModel.Name);
                sb.Append(".json");
                profileModel.FilePath = sb.ToString();
            }
            var content = JsonConvert.SerializeObject(profileModel);
            content = RemoveFilePath(content);

            using (StreamWriter writer = new StreamWriter(profileModel.FilePath, false))
            {
                writer.Write(content);
            }

        }

        private string RemoveFilePath(string XMLString)
        {
            JObject o = JObject.Parse(XMLString);
            o.Property("FilePath").Remove();
            return o.ToString();
        }




    }
}
