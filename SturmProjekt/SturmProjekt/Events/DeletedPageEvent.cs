using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using SturmProjekt.Models;

namespace SturmProjekt.Events
{
    public class DeletedPageEvent: PubSubEvent<PictureModel>
    {
    }
}
