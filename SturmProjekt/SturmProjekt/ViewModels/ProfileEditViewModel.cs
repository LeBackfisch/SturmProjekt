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
        private string _name;
        private string _filePath;
        private int _pageCount;
        private int _currentPage;
        private ObservableCollection<LinesModel> _drawLines;

        public ProfileEditViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            _eventAggregator = eventAggregator;
            profileModel = new ObservableCollection<ProfileModel>();
            _pages = new ObservableCollection<ProfilePages>();
            NewProfileCommand = new DelegateCommand(NewProfile, CanNewProfile);
            OpenProfileCommand = new DelegateCommand(OpenProfile, CanOpenProfile);
            _eventAggregator.GetEvent<MoveProfilePageEvent>().Subscribe(move => 
            {
                CurrentPage = CurrentPage + move;
                if(Pages.Count > CurrentPage)
                {
                    DrawLines = Pages.ElementAt(CurrentPage).DrawLines;
                }
                else
                {
                    DrawLines = new ObservableCollection<LinesModel>();
                }
                _eventAggregator.GetEvent<ToDrawLinesEvent>().Publish(DrawLines);
                _eventAggregator.GetEvent<ProfileCurrentPageEvent>().Publish(CurrentPage);

            });
            _eventAggregator.GetEvent<FromDrawLinesEvent>().Subscribe(tuple => 
            {
                var page = CurrentPage + tuple.Item2;
                var lines = tuple.Item1;
                if(Pages.Count > CurrentPage)
                {
                    Pages.ElementAt(CurrentPage).DrawLines = lines;
                }
                else
                {
                    var profilePage = new ProfilePages();
                    profilePage.DrawLines = lines;
                    Pages.Add(profilePage);
                }
            });
            _eventAggregator.GetEvent<SaveProfileEvent>().Subscribe(() => 
            {               
                SaveProfile();
                Name = string.Empty;
                FilePath = string.Empty;
                PageCount = 0;
                CurrentPage = 0;
                while (DrawLines.Count > 0)
                {
                    DrawLines.RemoveAt(DrawLines.Count - 1);
                }
            });
        }

        private void RemoveAdditionalPages()
        {
            while(Pages.Count > PageCount)
            {
                Pages.RemoveAt(Pages.Count - 1);               
            }
            CurrentPage = Pages.Count - 1;
            _eventAggregator.GetEvent<ProfileCurrentPageEvent>().Publish(CurrentPage);
        }

        public string Name
        {
            get => _name;
            set {
                    SetProperty(ref _name, value);
                    _eventAggregator.GetEvent<ProfileFileNameEvent>().Publish(_name);
                }
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
                RemoveAdditionalPages();
                _eventAggregator.GetEvent<ProfilePageCountEvent>().Publish(_pageCount);
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
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
            ProfileModel.Add(new ProfileModel());
            ProfileModel.FirstOrDefault().Pages = new List<ProfilePages>();
            DrawLines = new ObservableCollection<LinesModel>();
            PageCount = 1;
            CurrentPage = 0;
            _eventAggregator.GetEvent<ToDrawLinesEvent>().Publish(DrawLines);
            _eventAggregator.GetEvent<ProfilePageCountEvent>().Publish(PageCount);
            _eventAggregator.GetEvent<ProfileCurrentPageEvent>().Publish(CurrentPage);
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
                    Pages.AddRange(profile.Pages);
                    DrawLines = Pages.FirstOrDefault().DrawLines;
                    CurrentPage = 0;
                    _eventAggregator.GetEvent<ToDrawLinesEvent>().Publish(DrawLines);
                    _eventAggregator.GetEvent<ProfilePageCountEvent>().Publish(PageCount);
                    _eventAggregator.GetEvent<ProfileCurrentPageEvent>().Publish(CurrentPage);
                }
            }
        }

        public ICommand NewProfileCommand { get; set; }
        public ICommand OpenProfileCommand { get; set; }
       
    }
}
