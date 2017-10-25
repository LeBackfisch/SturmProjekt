using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturmProjekt.Models
{
    public class ProfileModel
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<LinesModel> DrawLines { get; set; }
    }
}
