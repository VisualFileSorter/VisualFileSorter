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

        public string ShortcutButtonContent
        {
            get => mShortcutButtonContent;
            set => this.RaiseAndSetIfChanged(ref mShortcutButtonContent, value);
        }

        public string ShortcutLabel
        {
            get => mShortcutLabel;
            set => this.RaiseAndSetIfChanged(ref mShortcutLabel, value);
        }

        public bool IsShortcutFlashing
        {
            get => mIsShortcutFlashing;
            set => this.RaiseAndSetIfChanged(ref mIsShortcutFlashing, value);
        }

        public bool FolderExists
        {
            get => mFolderExists;
            set => this.RaiseAndSetIfChanged(ref mFolderExists, value);
        }


        private List<string> mSortSrcFiles = new List<string>();
        private KeyGesture mShortcut = null;
        private string mShortcutButtonContent = "Add Shortcut";
        private string mShortcutLabel = "            ";
        private bool mIsShortcutFlashing = false;
        private bool mFolderExists = true;
    }
}
