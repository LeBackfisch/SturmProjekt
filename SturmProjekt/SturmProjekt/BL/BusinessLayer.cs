using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using SturmProjekt.Models;
using Brushes = System.Drawing.Brushes;
using Image = System.Drawing.Image;
using PdfDocument = Spire.Pdf.PdfDocument;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

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
            Pen redPen = new Pen(Brushes.Red, 5);

            foreach (var line in lines)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                
                redPen.Alignment = PenAlignment.Outset;
                g.DrawRectangle(redPen,
                    new Rectangle(line.X, line.Y, line.Width, line.Height));
            }        
            redPen.Dispose();
            g.Flush();
            g.Dispose();

            return bitmap;
        }

        public void CutOutBitmaps(RechnungsModel rechnung, ProfileModel profile)
        {
            var bitmaps = new List<Bitmap>();
            var pages = rechnung.Pages;
            int moveX = profile.Pages.FirstOrDefault().DrawLines.FirstOrDefault().X;
            int moveY = profile.Pages.FirstOrDefault().DrawLines.FirstOrDefault().Y;
            
            foreach (var page in pages)
            {
                bitmaps.Add(page.Page);
            }
            List<Bitmap> cutOutBitmaps = _rechnungsLogic.GetCutOutBitmaps(bitmaps, profile.Pages);
            List<string> directories = _rechnungsLogic.GetCategoryDirectories(DataPath);

            List<Rectangle> rectangles = GetRectangles(cutOutBitmaps);

           

            List<Bitmap> logoList = GetLogos();
            int logoindex = 0;
            int chosenlogo = -1;
            foreach (var logo in logoList)
            {
                foreach (var rectangle in rectangles)
                {
                    if (logo.Width == rectangle.Width && logo.Height == rectangle.Height)
                    {
                        Bitmap cutBitmap = cutOutBitmaps.FirstOrDefault();
                        PixelFormat format = cutBitmap.PixelFormat;
                        Bitmap cutlogo = cutBitmap.Clone(rectangle, format);
                        moveX += rectangle.X;
                        moveY += rectangle.Y;

                        var difference = _rechnungsLogic.Test(logo, cutlogo);
                        if (difference <= 5.0f)
                        {
                            chosenlogo = logoindex;
                            break;
                        }
                    }

                }
                if (chosenlogo != -1)
                    break;
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

            var values = _rechnungsLogic.GetOcrInfo(cutOutBitmaps, moveX, moveY);
            SaveToCsv(values);
            DisposeBitMaps(rechnung);
            DisposeBitMaps(bitmaps);
            GC.Collect();
        }

        private List<Rectangle> GetRectangles(List<Bitmap> bitmaps)
        {
            Bitmap bitmap = bitmaps.FirstOrDefault();
            Rectangle rect = new Rectangle(0,0, bitmap.Width, bitmap.Height);
            PixelFormat format = bitmap.PixelFormat;
            var usedrect = bitmap.Clone(rect, format);

            BitmapData bitmapData = usedrect.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, bitmap.PixelFormat);

            ColorFiltering colorFilter = new ColorFiltering();

            colorFilter.Red = new IntRange(0, 250);
            colorFilter.Green = new IntRange(0, 250);
            colorFilter.Blue = new IntRange(0, 250);
            colorFilter.FillOutsideRange = false;

            colorFilter.ApplyInPlace(bitmapData);
          
            // locate objects using blob counter
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            usedrect.UnlockBits(bitmapData);
            
            List<Rectangle> rectangles = new List<Rectangle>();
            foreach (var blob in blobs)
            {
                rectangles.Add(blob.Rectangle);
            }
            
          

            //bitmap.Dispose();
            return rectangles;
        }

        public List<Bitmap> GetLogos()
        {
            List<Bitmap> logoList = new List<Bitmap>();
            List<string> directories = _rechnungsLogic.GetCategoryDirectories(DataPath);
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
            return logoList;
        }


        public void DisposeBitMaps(RechnungsModel rechnung)
        {
            foreach (var page in rechnung.Pages)
            {
                page.Page.Dispose();
            }
        }

        private void DisposeBitMaps(List<Bitmap> bitmaps)
        {
            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
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
            var doc = new PdfSharp.Pdf.PdfDocument();

            int index = 0;
            foreach (var rechnungPage in rechnung.Pages)
            {
                doc.Pages.Add(new PdfPage());
                XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[index]);
                XImage img = XImage.FromBitmapSource(rechnungPage.PageImage);

                xgr.DrawImage(img, 0, 0, 592, 840);
                index++;
            }

            var sb = new StringBuilder();
            sb.Append(category);
            sb.Append("\\");
            sb.Append(rechnung.Name);
            sb.Append(".pdf");
            var savepath = sb.ToString();

            doc.Save(savepath);
            doc.Close();
        }


        public void SortDirectoryFiles(List<FileModel> fileModels, ProfileModel selectedProfile)
        {
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
                        var filename = file.FileName.Split('.')[0];
                        pictureModel.FileName = filename;
                        pictureModel.Page = ResizeBitmap(bitmap);
                        pictureModel.PageImage = BitmapToImageSource(pictureModel.Page);
                        pages.Add(pictureModel);
                    }

                    RechnungsModel rechnung = new RechnungsModel();
                    rechnung.Pages = pages;
                    rechnung.Name = pages.First().FileName;
                    rechnung.PageCount = pages.Count;
                    CutOutBitmaps(rechnung, selectedProfile);
                    DisposeBitMaps(bitmaps);
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
            foreach (var file in files)
            {
                System.IO.File.Move(file.FilePath+file.FileName, @"I:\Clemens-Projekt\SturmProjekt\SturmProjekt\SturmProjekt\Database\_Undefined\"+file.FileName);
            }
        }

        public void SortSammelPDF(string filename, ProfileModel selectedprofile)
        {
            var pdf = GetPdfDocument(filename);
            var rechnung = new RechnungsModel();
            int rechcount = 0;
            var firstpage = pdf.SaveAsImage(0);
            var firstpagebmp = new Bitmap(firstpage);
            firstpagebmp = ResizeBitmap(firstpagebmp);
            var bitmaplist = new List<Bitmap>();
            bitmaplist.Add(firstpagebmp);
            List<Bitmap> cutOutBitmaps = _rechnungsLogic.GetCutOutBitmaps(bitmaplist, selectedprofile.Pages);
            List<Bitmap> logoList = GetLogos();
            int chosenlogo = -1;
            int logoindex = 0;
            List<string> directories = _rechnungsLogic.GetCategoryDirectories(DataPath);
            string category;
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
            if (chosenlogo.Equals(-1))
            {
                return;
            }
            else
            {
                var rech = GetFileNameFromFilePath(filename);
                rech = rech.Split('.')[0];
                rechnung.Name = rech + "_" + rechcount;
                var picture = new PictureModel();
                picture.FileName = filename;
                picture.Page = firstpagebmp;
                picture.PageImage = BitmapToImageSource(firstpagebmp);
                rechnung.Pages = new List<PictureModel>();
                rechnung.Pages.Add(picture);
                rechnung.PageCount = rechnung.Pages.Count;
                category = GetChosenCategory(directories, logoindex);
            }
            for (var index = 1; index < pdf.Pages.Count; index++)
            {
                var page = pdf.SaveAsImage(index);
                var pagebmp = new Bitmap(page);
                pagebmp = ResizeBitmap(pagebmp);
                pagebmp.Save(@"I:\vgames\img" + index + ".jpg", ImageFormat.Jpeg);
                var bmplist = new List<Bitmap>();            
                bmplist.Add(pagebmp);

                int chosen = -1;
                int logoindx = 0;
                List<Bitmap> cutout = _rechnungsLogic.GetCutOutBitmaps(bmplist, selectedprofile.Pages);
                foreach (var logoBitmap in logoList)
                {

                    var difference = _rechnungsLogic.Test(logoBitmap, cutout.First());
                    if (difference <= 20.0f)
                    {
                        chosen = logoindx;
                        break;
                    }

                    logoindx++;
                }
                if (chosen.Equals(-1))
                {
                    var pic = new PictureModel
                    {
                        FileName = filename,
                        Page = pagebmp,
                        PageImage = BitmapToImageSource(pagebmp)
                    };

                    rechnung.Pages.Add(pic);
                    rechnung.PageCount++;
                }
                else
                {
                    SaveRechnungAsPDF(rechnung, category);
                    DisposeBitMaps(rechnung);
                    rechcount++;
                    rechnung = new RechnungsModel();

                    var pic = new PictureModel
                    {
                        FileName = filename,
                        Page = pagebmp,
                        PageImage = BitmapToImageSource(pagebmp)
                    };
                    rechnung.Pages = new List<PictureModel>();
                    rechnung.Pages.Add(pic);
                    rechnung.PageCount = rechnung.Pages.Count;
                    var rech = GetFileNameFromFilePath(filename);
                    rech = rech.Split('.')[0];
                    rechnung.Name = rech + "_" + rechcount;
                }

            }


            /* TODO: erste page nachschauen um welchen Rechnungstyp es sich handelt.
             * TODO: dann für jede weitere page nachschauen ob es sich um den selben Rechnungstyp handelt, 
             * TODO: wenn ja, dann Rechnung fertigstellen und cutoutbitmaps ausführen für CSV-Daten ...
             */

        }

     /*   public PictureModel CreateFirstPage(Bitmap bitmap, string filename)
        {
            var first = new PictureModel();
            first.Page = ResizeBitmap(bitmap);
            first.PageImage = BitmapToImageSource(ResizeBitmap(bitmap));
            first.FileName = filename;

            return first;
        }

        public void WriteToExcelFile()
        {
            Microsoft.Office.Interop.Excel.Application oXL;
            Microsoft.Office.Interop.Excel._Workbook oWB;
            Microsoft.Office.Interop.Excel._Worksheet oSheet;
            Microsoft.Office.Interop.Excel.Range oRng;
            object misvalue = System.Reflection.Missing.Value;
            try
            {
                //Start Excel and get Application object.
                oXL = new Microsoft.Office.Interop.Excel.Application();
                oXL.Visible = true;

                //Get a new workbook.
                oWB = (Microsoft.Office.Interop.Excel._Workbook) (oXL.Workbooks.Add(""));
                oSheet = (Microsoft.Office.Interop.Excel._Worksheet) oWB.ActiveSheet;

                //Add table headers going cell by cell.
                oSheet.Cells[1, 1] = "First Name";
                oSheet.Cells[1, 2] = "Last Name";
                oSheet.Cells[1, 3] = "Full Name";
                oSheet.Cells[1, 4] = "Salary";

                //Format A1:D1 as bold, vertical alignment = center.
                oSheet.get_Range("A1", "D1").Font.Bold = true;
                oSheet.get_Range("A1", "D1").VerticalAlignment =
                    Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;

                // Create an array to multiple values at once.
                string[,] saNames = new string[5, 2];

                saNames[0, 0] = "John";
                saNames[0, 1] = "Smith";
                saNames[1, 0] = "Tom";

                saNames[4, 1] = "Johnson";

                //Fill A2:B6 with an array of values (First and Last Names).
                oSheet.get_Range("A2", "B6").Value2 = saNames;

                //Fill C2:C6 with a relative formula (=A2 & " " & B2).
                oRng = oSheet.get_Range("C2", "C6");
                oRng.Formula = "=A2 & \" \" & B2";

                //Fill D2:D6 with a formula(=RAND()*100000) and apply format.
                oRng = oSheet.get_Range("D2", "D6");
                oRng.Formula = "=RAND()*100000";
                oRng.NumberFormat = "$0.00";

                //AutoFit columns A:D.
                oRng = oSheet.get_Range("A1", "D1");
                oRng.EntireColumn.AutoFit();

                oXL.Visible = false;
                oXL.UserControl = false;
                oWB.SaveAs("c:\\test\\test505.xls", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault,
                    Type.Missing, Type.Missing,
                    false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                oWB.Close();
            }
        } */

    }


}
