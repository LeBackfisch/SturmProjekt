using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Events;
using SturmProjekt.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SturmProjekt.ViewModels
{
    public class ProfileEditViewModel : BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<ProfileModel> profileModel;
        private ObservableCollection<ProfilePages> _pages;
        private ObservableCollection<LinesModel> _lines;
        private string _name;
        private string _filePath;
        private int _pageCount;
        private int _currentPage;
        private List<ProfilePages> _profilePages;
        private ObservableCollection<LinesModel> _drawLines;

        public ProfileEditViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            _eventAggregator = eventAggregator;
            profileModel = new ObservableCollection<ProfileModel>();
            _pages = new ObservableCollection<ProfilePages>();
            _lines = new ObservableCollection<LinesModel>();
            NewProfileCommand = new DelegateCommand(NewProfile, CanNewProfile);
            OpenProfileCommand = new DelegateCommand(OpenProfile, CanOpenProfile);
            AddPageCommand = new DelegateCommand(AddPage, CanAddPage).ObservesProperty(() => ProfileModel);
            RemovePageCommand = new DelegateCommand(RemovePage, CanRemovePage).ObservesProperty(() => ProfileModel);
            AddLineCommand = new DelegateCommand(AddLine, CanAddLine).ObservesProperty(() => ProfileModel);
            RemoveLinesCommand = new DelegateCommand(RemoveLines, CanRemoveLines).ObservesProperty(() => DrawLines);
            _eventAggregator.GetEvent<MoveProfilePageEvent>().Subscribe(move => 
            {
                CurrentPage = CurrentPage + move;
                if(ProfilePages.Count > CurrentPage)
                {
                    DrawLines = ProfilePages.ElementAt(CurrentPage).DrawLines;
                }
                else
                {
                    DrawLines = new ObservableCollection<LinesModel>();
                }
                _eventAggregator.GetEvent<ToDrawLinesEvent>().Publish(DrawLines);
                _eventAggregator.GetEvent<ProfileCurrentPageEvent>().Publish(CurrentPage);

            });
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name,value);
        }
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
        public int PageCount
        {
            get => _pageCount;
            set {
                SetProperty(ref _pageCount, value);
                _eventAggregator.GetEvent<ProfilePageCountEvent>().Publish(_pageCount);
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public List<ProfilePages> ProfilePages
        {
            get => _profilePages;
            set => SetProperty(ref _profilePages, value);
        }

        public ObservableCollection<LinesModel> DrawLines
        {
            get => _drawLines;
            set => SetProperty(ref _drawLines, value);
        }

        public ObservableCollection<ProfileModel> ProfileModel
        {
            get => profileModel;
            set => SetProperty(ref profileModel, value);
        }
        public ObservableCollection<ProfilePages> Pages
        {
            get => _pages;
            set => SetProperty(ref _pages, value);
        }

        public ObservableCollection<LinesModel> Lines
        {
            get => _lines;
            set => SetProperty(ref _lines, value);
        }

        private bool CanRemovePage()
        {
            return ProfileModel != null && ProfileModel.FirstOrDefault().Pages.Count > 0;
        }

        private bool CanRemoveLines()
        {
            return true;
        }

        private void RemoveLines()
        {

        }
        private void RemovePage()
        {
            ProfileModel.FirstOrDefault().Pages.RemoveAt(ProfileModel.FirstOrDefault().Pages.Count - 1);
        }
        
        private bool CanAddLine()
        {
            return ProfileModel != null && ProfileModel.FirstOrDefault().Pages.Count < ProfileModel.FirstOrDefault().PageCount;
        }

        private void AddLine()
        {
            var model = new LinesModel();
            DrawLines.Add(model);
        }

        private bool CanAddPage()
        {
            return ProfileModel != null && ProfileModel.FirstOrDefault().Pages.Count < ProfileModel.FirstOrDefault().PageCount;
        }

        private void AddPage()
        {
            var page = new ProfilePages();
            ProfileModel.FirstOrDefault().Pages.Add(page);
            Pages.Add(page);
        }

        private bool CanNewProfile()
        {
            return true;
        }

        private void NewProfile()
        {
            ClearPages();
            if(this.ProfileModel.Count > 0)
            {
                this.ProfileModel.RemoveAt(0);
            }          
            Pages.Add(new ProfilePages());
        }

        private bool CanOpenProfile()
        {
            return true;
        }

        private void SaveProfile()
        {
            _bl.SaveProfile(ProfileModel.FirstOrDefault());
        }

        private void ClearPages()
        {
            Pages = null;
            GC.Collect();
            Pages = new ObservableCollection<ProfilePages>();
        }

        private void OpenProfile()
        {
            ClearPages();
            var open = new OpenFileDialog
            {
                Filter = "Profile files (*.json) | *.json;"
            };
            if (open.ShowDialog() == true)
            {
                if (open.CheckFileExists)
                {
                    var filename = open.FileName;
                    var profile = _bl.ParseJsonToModel(filename);
                    ProfileModel.Add(profile);
                    Name = profile.Name;
                    PageCount = profile.PageCount;
                    FilePath = profile.FilePath;
                    ProfilePages = profile.Pages;
                    DrawLines = ProfilePages.FirstOrDefault().DrawLines;
                    CurrentPage = 0;
                    _eventAggregator.GetEvent<ToDrawLinesEvent>().Publish(DrawLines);
                    _eventAggregator.GetEvent<ProfilePageCountEvent>().Publish(PageCount);
                    _eventAggregator.GetEvent<ProfileCurrentPageEvent>().Publish(CurrentPage);
                }
                Pages.AddRange(ProfileModel.FirstOrDefault().Pages);
            }
        }

        public ICommand NewProfileCommand { get; set; }
        public ICommand OpenProfileCommand { get; set; }
        public ICommand AddPageCommand { get; set; }
        public ICommand AddLineCommand { get; set; }
        public ICommand RemovePageCommand { get; set; }
        public ICommand RemoveLinesCommand { get; set; }

       

       
    
    }
}
