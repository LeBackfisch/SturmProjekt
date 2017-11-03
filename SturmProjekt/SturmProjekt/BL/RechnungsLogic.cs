using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SturmProjekt.BL
{
    public class RechnungsLogic
    {
        public Bitmap CutoutBitmap(Bitmap sourceBitmap)
        {
            var srcRect = new Rectangle(0, 0, 100, 100);
            return sourceBitmap.Clone(srcRect, sourceBitmap.PixelFormat);
        }

        public List<Bitmap> GetCutOutBitmaps(List<Bitmap> bitmaps)
        {
            var cutoutbitmaps = new List<Bitmap>();
            foreach (var bitmap in bitmaps)
            {
                cutoutbitmaps.Add(CutoutBitmap(bitmap));
            }

            return cutoutbitmaps;
        }


    }
}
