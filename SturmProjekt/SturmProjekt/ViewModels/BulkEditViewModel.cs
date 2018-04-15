using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Models;

namespace SturmProjekt.ViewModels
{
    public class BulkEditViewModel: BindableBase
    {
        private readonly BusinessLayer _businessLayer;
        private readonly IEventAggregator _eventAggregator;
        private DirectoryInfo _directory;
        private int _fileCount;
        private List<FileModel> _files;
        private IEnumerable<ProfileModel> _profileList;
        private ProfileModel _selectedProfile;
        private string _fileCountText;
        private string _filesSorted;

        public BulkEditViewModel(BusinessLayer businessLayer, IEventAggregator eventAggregator)
        {
            _businessLayer = businessLayer;
            ProfileList = new ObservableCollection<ProfileModel>(_businessLayer.GetProfileList());
            _eventAggregator = eventAggregator;
            _files = new List<FileModel>();
            ChooseFolderCommand = new DelegateCommand(Open, CanOpen);
            SortFolderCommand = new DelegateCommand(Sort, CanSort).ObservesProperty(() => FileCountText).ObservesProperty(() => SelectedProfile);
        }

        private bool CanSort()
        {
            return SelectedProfile != null && FileCount > 0;
        }

        private void Sort()
        {
            var count =_businessLayer.SortDirectoryFiles(Files, SelectedProfile).Result;
            FilesSorted = count+ " Dateien wurden in Profil " + SelectedProfile.Name + " einsortiert";
            FileCount = 0;
            FileCountText = null;
            SelectedProfile = null;
            Files = new List<FileModel>();
        }

        private bool CanOpen()
        {
            return true;
        }

        private void Open()
        {
            var open = new FolderBrowserDialog
            {
                ShowNewFolderButton = false,
                SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory
            };
            var result = open.ShowDialog();
            if (result != DialogResult.OK) return;
            FileCount = 0;
            Files = new List<FileModel>();
            var folderpath = open.SelectedPath;
            _directory = new DirectoryInfo(folderpath);
            FilesSorted = "";
            GetFiles();
        }

        public void GetFiles()
        {
            foreach (var file in _directory.GetFiles())
            {
                if (file.Name.EndsWith(".jpg") || file.Name.EndsWith(".pdf") || file.Name.EndsWith(".png"))
                {
                    var newFile = new FileModel();
                    newFile.FilePath = file.DirectoryName;
                    newFile.FileName = file.Name;
                    _files.Add(newFile);
                    FileCount++;
                }
               
            }
            FileCountText = FileCount +" zulässige Datein wurden im ausgewählten Ordner gefunden.";
        }

        public List<FileModel> Files
        {
            get => _files;
            set => _files = value;
        }

        public int FileCount
        {
            get => _fileCount;
            set => SetProperty(ref _fileCount, value);
        }

        public string FilesSorted
        {
            get => _filesSorted;
            set => SetProperty(ref _filesSorted, value);
        }
        public string FileCountText
        {
            get => _fileCountText;
            set => SetProperty(ref _fileCountText, value);
        }

        public IEnumerable<ProfileModel> ProfileList
        {
            get => _profileList;
            set => SetProperty(ref _profileList, value);
        }

        public ProfileModel SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty(ref _selectedProfile, value);
        }

        public ICommand ChooseFolderCommand { get; set; }
        public ICommand SortFolderCommand { get; set; }
    }
}
