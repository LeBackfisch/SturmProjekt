using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using SturmProjekt.Models;
using Brushes = System.Drawing.Brushes;
using Image = System.Drawing.Image;
using PdfDocument = Spire.Pdf.PdfDocument;
using Pen = System.Drawing.Pen;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace SturmProjekt.BL
{
    public class BusinessLayer
    {
        private readonly RechnungsLogic _rechnungsLogic;
        private Config _config;
        public BusinessLayer()
        {
            _rechnungsLogic = new RechnungsLogic();
            _config = Config.Instance;
            FilePath = _config.ProfilePath;
            DataPath = _config.DataPath;
            TessDataPath = _config.TessDataPath;
        }

        public bool IsPdf(string fileName)
        {
            return fileName.EndsWith(".pdf");
        }

        public async Task<PdfDocument> GetPdfDocument(string filepath)
        {
            if (!File.Exists(filepath)) return null;
            var pdf = new PdfDocument();
           pdf.LoadFromFile(filepath); 
            return pdf;
        }

        public async Task<List<Bitmap>> GetBitMapFromPDF(PdfDocument pdf)
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
                resizedimages.Add(ResizeBitmap(image).Result); 
            }
            return resizedimages;
        } 

        public string GetFileNameFromFilePath(string filepath)
        {
            var path = filepath.Split('\\').ToList<string>();
            return path.Last();
        }

        public async Task<Bitmap> ConvertImageToBitmap(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                var image = Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);
            }
            return bitmap;
        }

        public async Task<Bitmap> ResizeBitmap(Bitmap originalBitmap)
        {
            int maxHeight = 3508;
            int maxWidth = 2480;

            var resiedbitmap = new Bitmap(originalBitmap, maxWidth, maxHeight);
            originalBitmap.Dispose();
            return resiedbitmap;
        }

        public async Task<BitmapImage> BitmapToImageSource(Bitmap bitmap)
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
        public string TessDataPath { get; set; }

        public List<ProfileModel> GetProfileList()
        {
            var profileNames = GetProfileFiles();
            return ParseProfileModel(profileNames);
        }

        public List<string> GetProfileFiles()
        {
            var list = new List<string>();
            if (!Directory.Exists(FilePath)) return list;
            var d = new DirectoryInfo(FilePath);
            FileInfo[] files = d.GetFiles("*.json");

            list.AddRange(files.Select(file => file.Name));
            return list;
        }

        public List<ProfileModel> ParseProfileModel(List<string> fileNames)
        {
            return fileNames.Select(file => ParseJsonToModel(FilePath + "\\" + file)).ToList();
        }

        public ProfileModel ParseJsonToModel(string filename)
        {
            string content = File.ReadAllText(filename);
            ProfileModel profile = JsonConvert.DeserializeObject<ProfileModel>(content);
            profile.FilePath = filename;
            return profile;
        }

        public RechnungsModel DrawOnRechnungsModel(RechnungsModel Rechnung, ProfileModel selectedProfile)
        {
            List<ProfilePages> pages = selectedProfile.Pages;

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
                    pictureModel.PageImage = BitmapToImageSource(bitmap).Result;
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

        public async Task<int> CutOutBitmaps(RechnungsModel rechnung, ProfileModel profile, bool single)
        {
            var bitmaps = new List<Bitmap>();
            var pages = rechnung.Pages;
            int moveX = profile.Offset.X;
            int moveY = profile.Offset.Y;
            
            foreach (var page in pages)
            {
                bitmaps.Add(page.Page);
            }

            var bitmapforlogo = await _rechnungsLogic.GetFirstCutOutBitmap(bitmaps, profile.Pages);
            
            List<string> directories = await _rechnungsLogic.GetCategoryDirectories(DataPath);
            List<Bitmap> cutOutBitmaps = new List<Bitmap>();
            List<Rectangle> rectangles = await GetRectangles(bitmapforlogo);

            /*int i = 0;
            foreach (var rect in rectangles)
            {
                PixelFormat format = bitmapforlogo.PixelFormat;
                var copy = bitmapforlogo.Clone(rect, format);
                copy.Save("test" + i + ".jpg", ImageFormat.Jpeg);
                i++;
            } */
           

            List<Bitmap> logoList = GetLogos();
            int logoindex = 0;
            int chosenlogo = -1;
            foreach (var logo in logoList)
            {
                foreach (var rectangle in rectangles)
                {
                    if (logo.Width == rectangle.Width && logo.Height == rectangle.Height)
                    {
                        Bitmap cutBitmap = bitmapforlogo;
                        PixelFormat format = cutBitmap.PixelFormat;
                        Bitmap cutlogo = cutBitmap.Clone(rectangle, format);
                        moveX = rectangle.X - moveX;
                        moveY = rectangle.Y - moveY;

                        var difference = await _rechnungsLogic.CalculateDifference(logo, cutlogo);
                        if (difference <= 14.0f)
                        {
                            chosenlogo = logoindex;
                            cutOutBitmaps = await _rechnungsLogic.GetCutOutBitmaps(bitmaps, profile.Pages, moveX, moveY);
                            break;
                        }
                        cutlogo.Dispose();
                    }
                    else if ((logo.Width + (logo.Width * 0.07) > rectangle.Width) &&
                             (logo.Width - (logo.Width * 0.07) < rectangle.Width) &&
                             (logo.Height + (logo.Height * 0.07) > rectangle.Height) &&
                             (logo.Height - (logo.Height * 0.07) < rectangle.Height))
                    {
                        Bitmap cutBitmap = bitmapforlogo;
                        PixelFormat format = cutBitmap.PixelFormat;
                        moveX = rectangle.X - moveX;
                        moveY = rectangle.Y - moveY;
                        Bitmap cutlogo = cutBitmap.Clone(rectangle, format);
                        var resizedlogo = new Bitmap(cutlogo, logo.Width, logo.Height);
                        cutlogo.Dispose();
                        var difference = await _rechnungsLogic.CalculateDifference(logo, resizedlogo);
                        if (difference <= 14.0f)
                        {
                            chosenlogo = logoindex;
                            cutOutBitmaps = await _rechnungsLogic.GetCutOutBitmaps(bitmaps, profile.Pages, moveX, moveY);
                            break;
                        }
                        resizedlogo.Dispose();
                    } 

                }
                if (chosenlogo != -1)
                    break;
                logoindex++;
            }
            if (chosenlogo != -1)
            {
                var category = GetChosenCategory(directories, logoindex);
                var values = _rechnungsLogic.GetOcrInfo(cutOutBitmaps, TessDataPath);
                var infos = ApplyKeywords(values, profile).Result;
                //SaveToCsv(values);
                AddtoExcelFile(infos);
                await SaveRechnungAsPDF(rechnung, category);
            }
            else
            {
                if (single)
                {
                    var category = directories.Last();
                    await SaveRechnungAsPDF(rechnung, category);
                }   
            }

            bitmapforlogo.Dispose();
            DisposeBitMaps(rechnung);
            DisposeBitMaps(bitmaps);
            GC.Collect();
            return chosenlogo;
        }

        private async Task<List<Tuple<string, int>>> ApplyKeywords(List<string> values, ProfileModel profile)
        {
            List<Tuple<string, int>> infos = new List<Tuple<string, int>>();
           
            infos.Add(new Tuple<string, int>(values[0], 0));
            infos.Add(new Tuple<string, int>(values[1], 1));
            infos.Add(new Tuple<string, int>(values[2], 2));

            bool found = false;
            for (int i = 3; i < values.Count; i++)
            {
                if (profile.Keywords.Any(s => s.BitMapNumber == i))
                {
                    found = false;
                    var keyword = profile.Keywords.FirstOrDefault(s => s.BitMapNumber == i);

                    foreach (var key in keyword.Keywords)
                    {
                        if (found)
                            break;

                        if (values.ElementAtOrDefault(i).Contains(key))
                        {
                            var words = values.ElementAtOrDefault(i).Split(' ');

                            if (keyword.Position.Equals("Next"))
                            {
                                var index = words.ToList().IndexOf(words.ToList().Find(x => x.Contains(key)));
                                if (index == -1)
                                {
                                    infos.Add(new Tuple<string, int>("", i));
                                }
                                else
                                {
                                    infos.Add(new Tuple<string, int>(words[index+1], i));
                                }                              
                                found = true;
                            }
                            else if (keyword.Position.Equals("Last"))
                            {
                                if (String.IsNullOrWhiteSpace(words.LastOrDefault()))
                                {                                 
                                    infos.Add(new Tuple<string, int>(words[words.Length - 2], i));
                                }
                                else
                                { 
                                    infos.Add(new Tuple<string, int>(words.LastOrDefault(), i));
                                }
                                found = true;
                            }
                        }
                    }
                }
            }
            return infos;
        } 

        private async Task<List<Rectangle>> GetRectangles(Bitmap bitmap)
        {
            
            Rectangle rect = new Rectangle(0,0, bitmap.Width, bitmap.Height);
            PixelFormat format = bitmap.PixelFormat;
            var usedrect = bitmap.Clone(rect, format);

            BitmapData bitmapData = usedrect.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, bitmap.PixelFormat);

            ColorFiltering colorFilter = new ColorFiltering
            {
                Red = new IntRange(0, 250),
                Green = new IntRange(0, 250),
                Blue = new IntRange(0, 250),
                FillOutsideRange = false
            };

            colorFilter.ApplyInPlace(bitmapData);
          
            // locate objects using blob counter
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            usedrect.UnlockBits(bitmapData);
            
            return blobs.Select(blob => blob.Rectangle).ToList();
        }

        public List<Bitmap> GetLogos()
        {
            List<string> directories = _rechnungsLogic.GetCategoryDirectories(DataPath).Result;
            return (from directory in directories where !directory.EndsWith("_Undefined") select directory + "\\logo.jpg" into logo where File.Exists(logo) select ConvertImageToBitmap(logo).Result).ToList();
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

        public async Task SaveRechnungAsPDF(RechnungsModel rechnung, string category)
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


        public async Task<int> SortDirectoryFiles(List<FileModel> fileModels, ProfileModel selectedProfile)
        {
            int sorted = 0;
            List<FileModel> imageFiles = new List<FileModel>();
            foreach (var file in fileModels)
            {
                if (file.FileName.EndsWith(".pdf"))
                {  
                    var pdf = GetPdfDocument((file.FilePath + "\\" + file.FileName)).Result;
                    if (pdf.Pages.Count < 10)
                    {
                        var bitmaps = GetBitMapFromPDF(pdf).Result;
                        pdf.Dispose();
                        List<PictureModel> pages = new List<PictureModel>();
                        foreach (var bitmap in bitmaps)
                        {
                            PictureModel pictureModel = new PictureModel();
                            var filename = file.FileName.Split('.')[0];
                            pictureModel.FileName = filename;
                            pictureModel.Page = ResizeBitmap(bitmap).Result;
                            pictureModel.PageImage = BitmapToImageSource(pictureModel.Page).Result;
                            pages.Add(pictureModel);
                        }

                        RechnungsModel rechnung = new RechnungsModel();
                        rechnung.Pages = pages;
                        rechnung.Name = pages.First().FileName;
                        rechnung.PageCount = pages.Count;
                        var found = CutOutBitmaps(rechnung, selectedProfile, false).Result;
                        if (found != -1)
                        {
                            sorted++;
                        }
                        DisposeBitMaps(bitmaps);
                        DisposeBitMaps(rechnung);
                    }
                    else
                    {
                        pdf.Dispose();
                    }
                    
                }
                else
                {
                    imageFiles.Add(file);
                }         
            }

            return sorted;
            // MoveUnidentifiedImages(imageFiles);

        }

        public void MoveUnidentifiedImages(List<FileModel> files)
        {
            foreach (var file in files)
            {
                System.IO.File.Move(file.FilePath+file.FileName, @"I:\Clemens-Projekt\SturmProjekt\SturmProjekt\SturmProjekt\Database\_Undefined\"+file.FileName);
            }
        }


        public async Task SortSammelPDF(string filename, ProfileModel selectedprofile)
        {
            var pdf = GetPdfDocument(filename).Result;
            int moveX = selectedprofile.Offset.X;
            int moveY = selectedprofile.Offset.Y;
            var rechnung = new RechnungsModel();
            int rechcount = 0;
            var firstpage = pdf.SaveAsImage(0);
            firstpage.Dispose();
            var firstpagebmp = new Bitmap(firstpage);
            firstpagebmp = ResizeBitmap(firstpagebmp).Result;
            var bitmaplist = new List<Bitmap>();
            bitmaplist.Add(firstpagebmp);
            var bitmapforlogo = await _rechnungsLogic.GetFirstCutOutBitmap(bitmaplist, selectedprofile.Pages);
            List<Bitmap> cutOutBitmaps = await _rechnungsLogic.GetCutOutBitmaps(bitmaplist, selectedprofile.Pages, moveX, moveY);
            List<Rectangle> rectangles = await GetRectangles(bitmapforlogo);
            List<Bitmap> logoList = GetLogos();
            int chosenlogo = -1;
            int logoindex = 0;
            List<string> directories = await _rechnungsLogic.GetCategoryDirectories(DataPath);
            string category;
            foreach (var logo in logoList)
            {
                foreach (var rectangle in rectangles)
                {
                    if (logo.Width == rectangle.Width && logo.Height == rectangle.Height)
                    {
                        Bitmap cutBitmap = bitmapforlogo;
                        PixelFormat format = cutBitmap.PixelFormat;
                        Bitmap cutlogo = cutBitmap.Clone(rectangle, format);

                        var difference = await _rechnungsLogic.CalculateDifference(logo, cutlogo);
                        if (difference <= 7.0f)
                        {
                            chosenlogo = logoindex;
                            break;
                        }
                        cutlogo.Dispose();
                    }
                    else if ((logo.Width + (logo.Width * 0.05) > rectangle.Width) &&
                             (logo.Width - (logo.Width * 0.05) < rectangle.Width) &&
                             (logo.Height + (logo.Height * 0.05) > rectangle.Height) &&
                             (logo.Height - (logo.Height * 0.05) < rectangle.Height))
                    {
                        Bitmap cutBitmap = bitmapforlogo;
                        PixelFormat format = cutBitmap.PixelFormat;
                        Bitmap cutlogo = cutBitmap.Clone(rectangle, format);
                        var resizedlogo = new Bitmap(cutlogo, logo.Width, logo.Height);
                        cutlogo.Dispose();
                        var difference = await _rechnungsLogic.CalculateDifference(logo, resizedlogo);
                        if (difference <= 7.0f)
                        {
                            chosenlogo = logoindex;
                            break;
                        }
                        resizedlogo.Dispose();
                    }
                }
                if (chosenlogo != -1)
                    break;
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
                picture.PageImage = BitmapToImageSource(firstpagebmp).Result;
                rechnung.Pages = new List<PictureModel>();
                rechnung.Pages.Add(picture);
                rechnung.PageCount = rechnung.Pages.Count;
                category = GetChosenCategory(directories, logoindex);
            }
            for (var index = 1; index < pdf.Pages.Count; index++)
            {
                var page = pdf.SaveAsImage(index);
                var pagebmp = new Bitmap(page);
                page.Dispose();
                pagebmp = ResizeBitmap(pagebmp).Result;
                var bmplist = new List<Bitmap>();            
                bmplist.Add(pagebmp);

                int chosen = -1;
                int logoindx = 0;
                foreach (var logoBitmap in logoList)
                {
                    foreach (var rectangle in rectangles)
                    {
                        if (logoBitmap.Width == rectangle.Width && logoBitmap.Height == rectangle.Height)
                        {
                            Bitmap cutBitmap = cutOutBitmaps.FirstOrDefault();
                            PixelFormat format = cutBitmap.PixelFormat;
                            Bitmap cutlogo = cutBitmap.Clone(rectangle, format);

                            var restDiff = await _rechnungsLogic.CalculateDifference(logoBitmap, cutlogo);
                            if (restDiff <= 7.0f)
                            {
                                chosenlogo = logoindex;
                                break;
                            }
                            cutlogo.Dispose();
                        }
                        else if ((logoBitmap.Width + (logoBitmap.Width * 0.05) > rectangle.Width) &&
                                 (logoBitmap.Width - (logoBitmap.Width * 0.05) < rectangle.Width) &&
                                 (logoBitmap.Height + (logoBitmap.Height * 0.05) > rectangle.Height) &&
                                 (logoBitmap.Height - (logoBitmap.Height * 0.05) < rectangle.Height))
                        {
                            Bitmap cutBitmap = cutOutBitmaps.FirstOrDefault();
                            PixelFormat format = cutBitmap.PixelFormat;
                            Bitmap cutlogo = cutBitmap.Clone(rectangle, format);
                            var resizedlogo = new Bitmap(cutlogo, logoBitmap.Width, logoBitmap.Height);
                            cutlogo.Dispose();
                            var restDiff = await _rechnungsLogic.CalculateDifference(logoBitmap, resizedlogo);
                            if (restDiff <= 7.0f)
                            {
                                chosenlogo = logoindex;
                                break;
                            }
                            resizedlogo.Dispose();
                        }

                    }

                    logoindx++;
                    if (chosenlogo != -1)
                        break;
                }
                if (chosen.Equals(-1))
                {
                    var pic = new PictureModel
                    {
                        FileName = filename,
                        Page = pagebmp,
                        PageImage = BitmapToImageSource(pagebmp).Result
                    };

                    rechnung.Pages.Add(pic);
                    rechnung.PageCount++;
                }
                else
                {
                    await SaveRechnungAsPDF(rechnung, category);
                    DisposeBitMaps(rechnung);
                    rechcount++;
                    rechnung = new RechnungsModel();

                    var pic = new PictureModel
                    {
                        FileName = filename,
                        Page = pagebmp,
                        PageImage = BitmapToImageSource(pagebmp).Result
                    };
                    rechnung.Pages = new List<PictureModel>();
                    rechnung.Pages.Add(pic);
                    rechnung.PageCount = rechnung.Pages.Count;
                    var rech = GetFileNameFromFilePath(filename);
                    rech = rech.Split('.')[0];
                    rechnung.Name = rech + "_" + rechcount;
                }

            }
            pdf.Dispose();

            bitmapforlogo.Dispose();
            DisposeBitMaps(cutOutBitmaps);
        }

         private void AddtoExcelFile(List<Tuple<string, int>> values)
       {
           List<string> buchstaben = new List<string>()
           {
               "A","B","C","D","E","F","G",
           };
           Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
           try
           {
               Microsoft.Office.Interop.Excel.Workbook sheet = excel.Workbooks.Open(DataPath + "\\RechnungsInfos.xlsx");
               Microsoft.Office.Interop.Excel.Worksheet x = excel.ActiveSheet as Microsoft.Office.Interop.Excel.Worksheet;
               Microsoft.Office.Interop.Excel.Range range = x.UsedRange;
               var xlRange = (Microsoft.Office.Interop.Excel.Range)x.Cells[x.Rows.Count, 1];
               long lastRow = (long)xlRange.get_End(Microsoft.Office.Interop.Excel.XlDirection.xlUp).Row;
               long newRow = lastRow + 1;
               int idx = 0;
               foreach(var value in values)
               {
                   if (value.Item2 == idx)
                   {
                       x.Range[buchstaben[idx] + newRow].Value = value.Item1;
                   }
                   else
                   {
                       x.Range[buchstaben[idx] + newRow].Value = "";
                   }
                   
                   idx++;
               }
               sheet.Close(true);
               excel.Quit();
           }
           catch
           {
               string failed = "Could not find or open File";
           }

       } 

    }


}
