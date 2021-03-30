using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;
using ReactiveUI;

namespace VisualFileSorter.ViewModels
{
    public class FileQueueItem : QueueItem
    {
        public Bitmap Image
        {
            get => mImage;
            set => this.RaiseAndSetIfChanged(ref mImage, value);
        }

        private Bitmap mImage = null;
    }
}
