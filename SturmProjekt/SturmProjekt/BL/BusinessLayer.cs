 using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
 using System.Runtime;
 using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
 using Newtonsoft.Json;
 using Spire.Pdf;
 using SturmProjekt.Models;

namespace SturmProjekt.BL
{
    public class BusinessLayer
    {
        private Config _config;
        public BusinessLayer()
        {
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
    }
}
