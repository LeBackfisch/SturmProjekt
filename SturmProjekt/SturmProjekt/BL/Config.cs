﻿using System;
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
        private string _tessdataPath;

        protected Config()
        {
            
        }

        public string ProfilePath

        {
            get
            {
                if (string.IsNullOrWhiteSpace(_profilePath))
                {
                    _profilePath = Properties.Settings.Default.ProfilePath;
                }
                return _profilePath;
            }
            set => _profilePath = value;
        }

        public string DataPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dataPath))
                {
                    _dataPath = Properties.Settings.Default.DataPath;
                }
                return _dataPath;
            }
            set => _dataPath = value;
        }

        public string TessDataPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tessdataPath))
                {
                    _tessdataPath = Properties.Settings.Default.TessDataPath;
                }
                return _tessdataPath;
            }
            set => _tessdataPath = value;
        }

        private static Config _instance;
        public static  Config Instance => _instance ?? (_instance = new Config());
    }
}
