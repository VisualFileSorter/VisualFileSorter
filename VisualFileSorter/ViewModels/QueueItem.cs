using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReactiveUI;

namespace VisualFileSorter.ViewModels
{
    public class QueueItem : ViewModelBase
    {
        public string FullName
        {
            get => mFullName;
            set => this.RaiseAndSetIfChanged(ref mFullName, value);
        }

        public string Name
        {
            get => mName;
            set => this.RaiseAndSetIfChanged(ref mName, value);
        }

        private string mFullName = null;
        private string mName = null;
    }
}