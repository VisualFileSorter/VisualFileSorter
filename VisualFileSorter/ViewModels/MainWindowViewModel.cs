using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Text;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Windows.Sdk;
using ReactiveUI;
using VisualFileSorter.Helpers;

// TODO
// Add sort directory key bindings
// Add open and save json files
// Add shellexecute transparent play button open
// Add undo redo
// clean up code and add comments
// convert thumbnail provider to use CsWin32
// Add credit for whereever I got the observable queue

namespace VisualFileSorter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(Window hostWindow)
        {
            mHostWindow = hostWindow;

            ImportFilesCmd = ReactiveCommand.Create(ImportFiles);
            TransferFilesCmd = ReactiveCommand.Create(TransferFiles);
            AddSortDirectoryCmd = ReactiveCommand.Create(AddSortDirectory);
            OpenCurrentFileCmd = ReactiveCommand.Create(OpenCurrentFile);
            EditShortcutCmd = ReactiveCommand.Create<SortFolder>(EditShortcut);
        }

        public ReactiveCommand<Unit, Unit> ImportFilesCmd { get; }
        public ReactiveCommand<Unit, Unit> TransferFilesCmd { get; }
        public ReactiveCommand<Unit, Unit> AddSortDirectoryCmd { get; }
        public ReactiveCommand<Unit, Unit> OpenCurrentFileCmd { get; }
        public ReactiveCommand<SortFolder, Unit> EditShortcutCmd { get; }

        public async void ImportFiles()
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "Import Files";
            dlg.AllowMultiple = true;
            dlg.InitialFileName = string.Empty;
            dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Images", Extensions = { "png", "jpg" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Video", Extensions = { "mp4", "avi" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Documents", Extensions = { "txt", "doc" } });


            var result = await dlg.ShowAsync(mHostWindow);
            if (result != null)
            {
                foreach (string fileItem in result)
                {
                    FileQueueItem tempFileQueueItem = new FileQueueItem();
                    int THUMB_SIZE = 256;
                    Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                       fileItem, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                    tempFileQueueItem.Image = ConvertBitmap(thumbnail);
                    tempFileQueueItem.FullName = fileItem;
                    tempFileQueueItem.Name = Path.GetFileName(fileItem);
                    // TODO look into fast observable collection and enqueue a range of files
                    FileQueue.Enqueue(tempFileQueueItem);
                }

                if (CurrentFileQueueItem?.Name == null)
                {
                    CurrentFileQueueItem = FileQueue.Dequeue();
                }
            }
        }

        // Convert Drawing.Bitmap to Avalonia.Media.Imaging.Bitmap
        public Avalonia.Media.Imaging.Bitmap ConvertBitmap(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                // Converted Avalonia compatible image
                return new Avalonia.Media.Imaging.Bitmap(memory);
            }
        }

        private void TransferFiles()
        {
            List<string> allSrcFiles = new List<string>();
            List<string> allDestFiles = new List<string>();

            // Get all source file locations
            foreach (var sortFolderItem in SortFolderQueue)
            {
                foreach (var sortSrcFileItem in sortFolderItem.SortSrcFiles)
                {
                    allSrcFiles.Add(sortSrcFileItem);
                    allDestFiles.Add(Path.Combine(sortFolderItem.FullName, Path.GetFileName(sortSrcFileItem)));
                }
            }

            // Make sure all source files still exist
            // Make sure destination folders still exist
            // Double click on folder name on right to edit sort folder location
            // Change label to red when folder does not exist
            // Add tooltip that says folder no longer exists

            Helpers.WindowsShellFileOperation.TransferFiles(allSrcFiles, allDestFiles, false);
        }

        public async void AddSortDirectory()
        {
            var dlg = new OpenFolderDialog();
            dlg.Title = "Select Sort Directory";
            dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            var result = await dlg.ShowAsync(mHostWindow);
            if (!string.IsNullOrWhiteSpace(result))
            {
                SortFolder tempFolderQueueItem = new SortFolder();
                tempFolderQueueItem.FullName = result;
                tempFolderQueueItem.Name = Path.GetFileName(result);
                // TODO look into fast observable collection and enqueue a range of files
                SortFolderQueue.Enqueue(tempFolderQueueItem);
            }
        }

        private void OpenCurrentFile()
        {
            if (File.Exists(CurrentFileQueueItem.FullName))
            {
                PInvoke.ShellExecute(new HWND(0), null, CurrentFileQueueItem.FullName, null, null, 1);
            }
        }

        private void SortFile(KeyEventArgs e)
        {
            SortFolder? foundSortFolder = SortFolderQueue.FirstOrDefault(x => x.Shortcut.Matches(e));
            if (foundSortFolder != null && CurrentFileQueueItem != null)
            {
                foundSortFolder.SortSrcFiles.Add(CurrentFileQueueItem.FullName);
                CurrentFileQueueItem = FileQueue.Dequeue();
            }
        }

        public IDisposable AttachShortcut(TopLevel root, KeyGesture gesture)
        {
            void PreviewKeyDown(object sender, KeyEventArgs e)
            {
                SortFile(e);
            }

            return root.AddDisposableHandler(
                InputElement.KeyDownEvent,
                PreviewKeyDown,
                RoutingStrategies.Tunnel);
        }

        // TODO disable all EditShortcut buttons while waiting for user shortcut
        public async void EditShortcut(SortFolder sortFolder)
        {
            if (sortFolder != null)
            {
                // Get currently held keys
                HashSet<Key> userShortcut = await Task.Run(() => Keyboard.GetUserShortcut());

                KeyGesture test = new KeyGesture(Key.OemTilde, KeyModifiers.Control);
                AttachShortcut(mHostWindow, test);
                sortFolder.Shortcut = test;

                // Add spaces to shortcut string if necessary
                string shortcutStr = test.ToString();
                if (shortcutStr.Contains("+"))
                {
                    string[] splitShortcut = shortcutStr.Split('+');
                    if (2 == splitShortcut.Count())
                    {
                        // TODO convert the Oem[X] to the symbol; ex. OemTilde => ~
                        if (splitShortcut[1].Contains("Oem"))
                        {
                            sortFolder.ShortcutLabel = splitShortcut[0] + " + " + splitShortcut[1].Remove(0, 3);
                        }
                        else
                        {
                            sortFolder.ShortcutLabel = splitShortcut[0] + " + " + splitShortcut[1];
                        } 
                    }
                }
                else
                {
                    sortFolder.ShortcutLabel = shortcutStr;
                }
            }
        }

        private bool OpenSession()
        {
            return true;
        }

        private bool SaveSession()
        {
            return true;
        }

        public FileQueueItem CurrentFileQueueItem
        {
            get => mCurrentFileQueueItem;
            set => this.RaiseAndSetIfChanged(ref mCurrentFileQueueItem, value);
        }

        public ObservableQueue<FileQueueItem> FileQueue
        {
            get => mFileQueue;
            set => this.RaiseAndSetIfChanged(ref mFileQueue, value);
        }

        public ObservableQueue<SortFolder> SortFolderQueue
        {
            get => mSortFolderQueue;
            set => this.RaiseAndSetIfChanged(ref mSortFolderQueue, value);
        }

        private ObservableQueue<SortFolder> mSortFolderQueue = new ObservableQueue<SortFolder>();
        private ObservableQueue<FileQueueItem> mFileQueue = new ObservableQueue<FileQueueItem>();
        private FileQueueItem mCurrentFileQueueItem = new FileQueueItem();
        private Window mHostWindow;
    }
}
