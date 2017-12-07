using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Spire.Pdf;
using Spire.Pdf.Graphics;
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
            DataPath = _config.DataPath;
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
            var resizedimages = new List<Bitmap>();
            var pagecount = pdf.Pages.Count;

            for (var i = 0; i < pagecount; i++)
            {
                Image bmp = pdf.SaveAsImage(i);
                var bitmap = new Bitmap(bmp);
                images.Add(bitmap);
            }
            foreach (var image in images)
            {
                resizedimages.Add(ResizeBitmap(image));
            }
            return resizedimages;
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

        public Bitmap ResizeBitmap(Bitmap originalBitmap)
        {
            int maxHeight = 3508;
            int maxWidth = 2480;
            double ratio = (double) originalBitmap.Width / (double) originalBitmap.Height;

            var resiedbitmap = new Bitmap(originalBitmap, maxWidth, maxHeight);
            originalBitmap.Dispose();
            return resiedbitmap;


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

        public string DataPath { get; set; }

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
                PictureModel pictureModel = new PictureModel();
                pictureModel.FileName = pictureItem.FileName;
                if (index < pages.Count)
                {
                    var bitmap = DrawonBitmap(pictureItem.Page, pages.ElementAt(index));
                    pictureModel.Page = bitmap;
                    pictureModel.PageImage = BitmapToImageSource(bitmap);
                    pictureModels.Add(pictureModel);
                }
                else
                {
                    pictureModels.Add(pictureItem);
                }
                
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

            List<LinesModel> lines = profilePage.DrawLines;

            foreach (var line in lines)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Pen redPen = new Pen(Brushes.Red, 5);
                redPen.Alignment = PenAlignment.Outset;
                g.DrawRectangle(redPen,
                    new Rectangle(line.X, line.Y, line.Width, line.Height));
            }        
            g.Flush();

            return bitmap;
        }

        public void CutOutBitmaps(RechnungsModel rechnung, ProfileModel profile)
        {
            var bitmaps = new List<Bitmap>();
            var pages = rechnung.Pages;
            
            foreach (var page in pages)
            {
                bitmaps.Add(page.Page);
            }
            List<Bitmap> cutOutBitmaps = _rechnungsLogic.GetCutOutBitmaps(bitmaps, profile.Pages);
            List<string> directories = _rechnungsLogic.GetCategoryDirectories(DataPath);
            List<Bitmap> logoList = new List<Bitmap>();
            foreach (var directory in directories)
            {
                if (!directory.EndsWith("_Undefined"))
                {
                    var logo = directory + "\\logo.jpg";
                    if (File.Exists(logo))
                    {
                        logoList.Add(ConvertImageToBitmap(logo));
                    }
                }           
            }
            int logoindex = 0;
            int chosenlogo = -1;
            foreach (var logoBitmap in logoList)
            {
                
                var difference = _rechnungsLogic.Test(logoBitmap, cutOutBitmaps.First());
                if (difference <= 20.0f)
                {
                    chosenlogo = logoindex;
                    break;
                }

                logoindex++;
            }
            if (chosenlogo != -1)
            {
                var category = GetChosenCategory(directories, logoindex);
                SaveRechnungAsPDF(rechnung, category);
            }
            else
            {
                var category = directories.Last();
                SaveRechnungAsPDF(rechnung, category);
            }
            
            

            var values = _rechnungsLogic.GetOcrInfo(cutOutBitmaps);
            SaveToCsv(values);
            DisposeBitMaps(rechnung);
        }

        public void DisposeBitMaps(RechnungsModel rechnung)
        {
            foreach (var page in rechnung.Pages)
            {
                page.Page.Dispose();
            }
        }

        public void SaveToCsv(List<string> values)
        {
            var filepath = @"..\..\Database\RechnungsInfos.csv";

            if (File.Exists(filepath))
            {
                var line = AddtoCsv(values);
                File.AppendAllText(filepath, line);
            }
            else
            {
               CreateCsv(filepath);
                var line = AddtoCsv(values);
                File.AppendAllText(filepath, line);
            }
        }

        public string AddtoCsv(List<string> values)
        {
            var csv = new StringBuilder();

            foreach (var value in values)
            {
                csv.Append(value);
                csv.Append(",");
            }

            csv.Length--;

            return csv.ToString();
        }

        public void CreateCsv(string filepath)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(filepath,
                FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine("Vertragspartnerinfos, Anlagenadresse, Zeitraum, Gaskosten, Stromkosten, Gasdetailinfos, Stromdetailinfos");
            }
        }

        public string GetChosenCategory(List<string> directories, int index)
        {
            return directories[index];
        }

        public void SaveRechnungAsPDF(RechnungsModel rechnung, string category)
        {
            PdfDocument doc = new PdfDocument();
            foreach (var rechnungPage in rechnung.Pages)
            {
                PdfPageBase page = doc.Pages.Add();
                PdfImage image = PdfImage.FromImage(rechnungPage.Page);

                float widthFitRate = image.PhysicalDimension.Width / page.Canvas.ClientSize.Width;
                float heightFitRate = image.PhysicalDimension.Height / page.Canvas.ClientSize.Height;
                float fitRate = Math.Max(widthFitRate, heightFitRate);

                page.Canvas.DrawImage(image, 0, 0, page.Canvas.ClientSize.Width, page.Canvas.ClientSize.Height);
            }
            var sb = new StringBuilder();
            sb.Append(category);
            sb.Append("\\");
            sb.Append(rechnung.Name);
            sb.Append(".pdf");
            var savepath = sb.ToString();

            doc.SaveToFile(savepath);
            doc.Close();
        }

        public void SortDirectoryFiles(List<FileModel> fileModels, ProfileModel selectedProfile)
        {
            List<RechnungsModel> pdfRechnungen = new List<RechnungsModel>();
            List<FileModel> imageFiles = new List<FileModel>();
            foreach (var file in fileModels)
            {
                if (file.FileName.EndsWith(".pdf"))
                {
                   var bitmaps = GetBitMapFromPDF(GetPdfDocument(file.FilePath+"\\"+file.FileName));
                   List<PictureModel> pages = new List<PictureModel>();
                    foreach (var bitmap in bitmaps)
                    {
                        PictureModel pictureModel = new PictureModel();
                        pictureModel.FileName = file.FileName;
                        pictureModel.Page = ResizeBitmap(bitmap);
                        pictureModel.PageImage = BitmapToImageSource(pictureModel.Page);
                        pages.Add(pictureModel);
                    }

                    RechnungsModel rechnung = new RechnungsModel();
                    rechnung.Pages = pages;
                    rechnung.Name = pages.First().FileName;
                    rechnung.PageCount = pages.Count;
                    CutOutBitmaps(rechnung, selectedProfile);
                    DisposeBitMaps(rechnung);
                }
                else
                {
                    imageFiles.Add(file);
                }         
            }

            MoveUnidentifiedImages(imageFiles);

        }

        public void MoveUnidentifiedImages(List<FileModel> files)
        {
           
        }

    }


}
