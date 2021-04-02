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
        public Bitmap? SmallImage
        {
            get => mSmallImage;
            set => this.RaiseAndSetIfChanged(ref mSmallImage, value);
        }

        public Bitmap? BigImage
        {
            get => mBigImage;
            set => this.RaiseAndSetIfChanged(ref mBigImage, value);
        }

        public bool IsPlayableMedia
        {
            get => mIsPlayableMedia;
            set => this.RaiseAndSetIfChanged(ref mIsPlayableMedia, value);
        }

        private Bitmap? mSmallImage = null;
        private Bitmap? mBigImage = null;
        private bool mIsPlayableMedia = false;
    }
}
