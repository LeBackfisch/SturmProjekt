using Prism.Events;
using SturmProjekt.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturmProjekt.Events
{
    public class ToDrawLinesEvent: PubSubEvent<ObservableCollection<LinesModel>>
    {
    }
}
