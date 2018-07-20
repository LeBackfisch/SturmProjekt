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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SturmProjekt.ViewModels
{
    public class ProfilePageEditViewModel: BindableBase
    {
        private readonly BusinessLayer _bl;
        private readonly IEventAggregator _eventAggregator;
        private ObservableCollection<LinesModel> _drawLines;

        public ProfilePageEditViewModel(BusinessLayer bl, IEventAggregator eventAggregator)
        {
            _bl = bl;
            _eventAggregator = eventAggregator;
            AddLinesCommand = new DelegateCommand(AddLines, CanAddLines).ObservesProperty(() => DrawLines);
            RemoveItemCommand = new DelegateCommand<object>(RemoveItem);
            _eventAggregator.GetEvent<ToDrawLinesEvent>().Subscribe(lines =>
            {
                DrawLines = lines;
            });
            _eventAggregator.GetEvent<MoveProfilePageEvent>().Subscribe(page =>
            {
                var moved = page * -1;
                var tuple = new Tuple<ObservableCollection<LinesModel>, int>(DrawLines, moved);

                _eventAggregator.GetEvent<FromDrawLinesEvent>().Publish(tuple);
            });
        }

        private bool CanAddLines()
        {
            return DrawLines != null;
        }

        private void AddLines()
        {
            var lines = new LinesModel();
            DrawLines.Add(lines);
        }

        private void RemoveItem(object obj)
        {
            var element = (LinesModel)obj;
            DrawLines.Remove(element);
        }
        public ObservableCollection<LinesModel> DrawLines
        {
            get => _drawLines;
            set
            {
                SetProperty(ref _drawLines, value);
                _eventAggregator.GetEvent<ProfileDrawLinesEvent>().Publish(_drawLines);
            }
        }

        public ICommand AddLinesCommand { get; set; }
        public DelegateCommand<object> RemoveItemCommand { get; set; }
    }
}
