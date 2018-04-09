using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Models;

namespace SturmProjekt.ViewModels
{
    public class SammelPDFViewModel: BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private string _fileName;
        private IEnumerable<ProfileModel> _profileList;
        private ProfileModel _selectedProfile;

        public SammelPDFViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            _eventAggregator = eventAggregator;
            ProfileList = new ObservableCollection<ProfileModel>(_bl.GetProfileList());
            OpenCommand = new DelegateCommand(Open, CanOpen);
            SortPDFCommand = new DelegateCommand(Sort, CanSort).ObservesProperty(() => FileName).ObservesProperty(() => SelectedProfile);
        }

        private bool CanSort()
        {
            return SelectedProfile != null && !string.IsNullOrWhiteSpace(FileName);
        }

        private async void Sort()
        {
            await _bl.SortSammelPDF(FileName, SelectedProfile);
            SelectedProfile = null;
            FileName = null;
        }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        private bool CanOpen()
        {
            return true;
        }

        private void Open()
        {
            var open = new OpenFileDialog
            {
                Filter = "PDFs (*.pdf)|*.pdf"
            };
            if (open.ShowDialog() == true)
            {
                FileName = open.FileName;
            }
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

        public ICommand OpenCommand { get; set; }
        public ICommand SortPDFCommand { get; set; }
    }
}
