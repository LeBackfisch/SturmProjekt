using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Pdf.Exporting.XPS.Schema;

namespace SturmProjekt.BL
{
    public class Config
    {
        private string _profilePath;


        protected Config()
        {
            
        }

        public string ProfilePath

        {
            get
            {
                if (string.IsNullOrWhiteSpace(_profilePath))
                {
                    _profilePath = Properties.Resources.ProfilesPath;
                }
                return _profilePath;
            }
            set { _profilePath = value; }
        }

        private static Config _instance;
        public static  Config Instance => _instance ?? (_instance = new Config());
    }
}
