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
        private string _dataPath;


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

        public string DataPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dataPath))
                {
                    _dataPath = Properties.Resources.DataPath;
                }
                return _dataPath;
            }
            set { _dataPath = value; }
        }

        private static Config _instance;
        public static  Config Instance => _instance ?? (_instance = new Config());
    }
}
