using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace SturmProjekt.Models
{
    public class RechnungsModel
    {
        public string Name { get; set; }
        public int PageCount { get; set; }
        public List<PictureModel> Pages { get; set; }

    }
}
