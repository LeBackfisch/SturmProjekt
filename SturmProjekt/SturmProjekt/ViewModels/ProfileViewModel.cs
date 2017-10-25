using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Mvvm;
using SturmProjekt.BL;
using SturmProjekt.Events;
using SturmProjekt.Models;

namespace SturmProjekt.ViewModels
{
    public class ProfileViewModel: BindableBase
    {
        private readonly BusinessLayer _bl;
        private RechnungsModel _rechnung;

        public ProfileViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            eventAggregator.GetEvent<CreateRechnungEvent>().Subscribe(rechnung =>
            {
                Rechnung = rechnung;
            });
        }

        public RechnungsModel Rechnung
        {
            get { return _rechnung; }
            set { SetProperty(ref _rechnung, value); }
        }
    }
}
