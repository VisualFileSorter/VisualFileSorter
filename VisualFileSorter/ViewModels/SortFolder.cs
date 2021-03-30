using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Input;
using ReactiveUI;

namespace VisualFileSorter.ViewModels
{
    public class SortFolder : QueueItem
    {
        public List<string> SortSrcFiles
        {
            get => mSortSrcFiles;
            set => this.RaiseAndSetIfChanged(ref mSortSrcFiles, value);
        }

        public KeyGesture Shortcut
        {
            get => mShortcut;
            set => this.RaiseAndSetIfChanged(ref mShortcut, value);
        }

        public string ShortcutLabel
        {
            get => mShortcutLabel;
            set => this.RaiseAndSetIfChanged(ref mShortcutLabel, value);
        }

        private List<string> mSortSrcFiles = new List<string>();
        private KeyGesture mShortcut = null;
        private string mShortcutLabel = null;
    }
}
