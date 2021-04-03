using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;

using VisualFileSorter.Helpers;

// TODO
// error/warning/info messages
// Add open and save json files
// Add undo redo
// clean up code and add comments

namespace VisualFileSorter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(Window hostWindow)
        {
            // Get the main host window
            mHostWindow = hostWindow;

            // MainWindow commands
            ImportFilesCmd = ReactiveCommand.Create(ImportFiles);
            TransferFilesCmd = ReactiveCommand.Create(TransferFiles);
            AddSortDirectoryCmd = ReactiveCommand.Create(AddSortDirectory);
            OpenCurrentFileCmd = ReactiveCommand.Create(OpenCurrentFile);
            EditShortcutCmd = ReactiveCommand.Create<SortFolder>(EditShortcut);
            RemapSortFolderLocationCmd = ReactiveCommand.Create<SortFolder>(RemapSortFolderLocation);
            RemoveSortFolderCmd = ReactiveCommand.Create<SortFolder>(RemoveSortFolder);

            // MessageBox commands
            TestMsgCmd = ReactiveCommand.Create(TestMsg);
            CloseMsgBoxCmd = ReactiveCommand.Create<Window>(CloseMsgBox);
            ShowDialog = new Interaction<MainWindowViewModel, Unit>();
        }

        public ReactiveCommand<Unit, Unit> ImportFilesCmd { get; }
        public ReactiveCommand<Unit, Unit> TransferFilesCmd { get; }
        public ReactiveCommand<Unit, Unit> AddSortDirectoryCmd { get; }
        public ReactiveCommand<Unit, Unit> OpenCurrentFileCmd { get; }
        public ReactiveCommand<SortFolder, Unit> EditShortcutCmd { get; }
        public ReactiveCommand<SortFolder, Unit> RemapSortFolderLocationCmd { get; }
        public ReactiveCommand<SortFolder, Unit> RemoveSortFolderCmd { get; }

        // MessageBox commands
        public ReactiveCommand<Unit, Unit> TestMsgCmd { get; }
        public ReactiveCommand<Window, Unit> CloseMsgBoxCmd { get; }
        public Interaction<MainWindowViewModel, Unit> ShowDialog { get; }


        // TODO check that file hasn't already been sorted or added to the queue
        // TODO add warning message box with file paths of already queued/sorted files
        public async void ImportFiles()
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "Import Files";
            dlg.AllowMultiple = true;
            dlg.InitialFileName = string.Empty;
            dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Images", Extensions = { "tif", "tiff", "bmp", "png", "jpg", "jpeg",
                                                                                     "gif", "raw", "cr2", "nef", "orf", "sr2", "webp"} });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Video", Extensions = { "avi", "divx", "vob", "evo", "m2ts", "flv",
                                                                                    "mkv", "mpg", "mpeg", "m1v", "mp4", "m4v",
                                                                                    "mp4v", "mpv4", "3gp", "3gpp", "3g2", "3gp2",
                                                                                    "ogg", "ogm", "ogv", "rm", "rmvb", "webm",
                                                                                    "amv", "mov", "hdmov", "qt"} });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Audio", Extensions = { "mka", "mp3", "m4a", "oga", "ra", "ram",
                                                                                    "flac", "wv", "ac3", "dts", "amr", "alac",
                                                                                    "ape", "apl", "aac"} });
            dlg.Filters.Add(new FileDialogFilter() { Name = "Documents", Extensions = { "doc", "docx", "htm", "html", 
                                                                                        "odt", "ods", "pdf", "tex", 
                                                                                        "xls", "xlsx", "ppt", "pptx", "txt"} });

            var result = await dlg.ShowAsync(mHostWindow);
            if (result != null && 0 < result.Count())
            {
                // TODO disallow adding folders?
                List<FileQueueItem> fileItems = new List<FileQueueItem>();
                foreach (string fileItem in result)
                {
                    FileQueueItem tempFileQueueItem = new FileQueueItem();
                    int THUMB_SIZE = 64;
                    Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                       fileItem, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                    tempFileQueueItem.SmallImage = ConvertBitmap(thumbnail);
                    tempFileQueueItem.FullName = fileItem;
                    tempFileQueueItem.Name = Path.GetFileName(fileItem);
                    tempFileQueueItem.IsPlayableMedia = CheckIfPlayableMedia(Path.GetExtension(fileItem));

                    fileItems.Add(tempFileQueueItem);
                    
                }
                FileQueue.EnqueueRange(fileItems);

                if (CurrentFileQueueItem?.Name == null)
                {
                    CurrentFileQueueItem = FileQueue.Dequeue();
                    int THUMB_SIZE = 256;
                    Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                       CurrentFileQueueItem.FullName, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                    CurrentFileQueueItem.BigImage = ConvertBitmap(thumbnail);
                }
            }
        }

        public bool CheckIfPlayableMedia(string extension)
        {
            return extension == ".avi"  || extension == ".divx"  ||
                   extension == ".vob"  || extension == ".evo"   ||
                   extension == ".m2ts" || extension == ".flv"   ||
                   extension == ".mkv"  || extension == ".mpg"   ||
                   extension == ".mpeg" || extension == ".m1v"   ||
                   extension == ".mp4"  || extension == ".m4v"   ||
                   extension == ".mp4v" || extension == ".mpv4"  ||
                   extension == ".3gp"  || extension == ".3gpp"  ||
                   extension == ".3g2"  || extension == ".3gp2"  ||
                   extension == ".ogg"  || extension == ".ogm"   ||
                   extension == ".rm"   || extension == ".rmvb"  ||
                   extension == ".webm" || extension == ".amv"   ||
                   extension == ".mov"  || extension == ".hdmov" ||
                   extension == ".qt"   || extension == ".mka"   ||
                   extension == ".mp3"  || extension == ".m4a"   ||
                   extension == ".oga"  || extension == ".ra"    ||
                   extension == ".ram"  || extension == ".flac"  ||
                   extension == ".wv"   || extension == ".ac3"   ||
                   extension == ".dts"  || extension == ".amr"   ||
                   extension == ".alac" || extension == ".ape"   ||
                   extension == ".apl"  || extension == ".aac"   ||
                   extension == ".doc"  || extension == ".docx"  ||
                   extension == ".html" || extension == ".htm"   ||
                   extension == ".odt"  || extension == ".ods"   ||
                   extension == ".pdf"  || extension == ".tex"   ||
                   extension == ".xls"  || extension == ".xlsx"  ||
                   extension == ".ppt"  || extension == ".pptx"  ||
                   extension == ".txt";
        }

        
        public async void OpenMissingSortFolderDialog()
        {
            // Set window properties
            MessageWindowTitle = "Error";
            MessageWindowWidth = 300;
            MessageWindowHeight = 120;
            MessageWindowErrorIcon = true;
            MessageWindowWarningIcon = false;
            MessageWindowInfoIcon = false;

            MB_MissingSortFolderVisible = true;
            MB_RemoveSortFolderVisible = false;
            MB_MissingTransferFilesVisible = false;
            MB_BadFileTransferVisible = false;

            // Show the message box
            var result = await ShowDialog.Handle(this);
        }

        public async void OpenRemoveSortFolderDialog()
        {
            // Set window properties
            MessageWindowTitle = "Warning";
            MessageWindowWidth = 410;
            MessageWindowHeight = 135;
            MessageWindowErrorIcon = false;
            MessageWindowWarningIcon = true;
            MessageWindowInfoIcon = false;

            MB_MissingSortFolderVisible = false;
            MB_RemoveSortFolderVisible = true;
            MB_MissingTransferFilesVisible = false;
            MB_BadFileTransferVisible = false;

            // Show the message box
            var result = await ShowDialog.Handle(this);
        }

        public async void OpenMissingTransferFilesDialog()
        {
            // Set window properties
            MessageWindowTitle = "Warning";
            MessageWindowWidth = 300;
            MessageWindowHeight = 267;
            MessageWindowErrorIcon = false;
            MessageWindowWarningIcon = true;
            MessageWindowInfoIcon = false;

            MB_MissingSortFolderVisible = false;
            MB_RemoveSortFolderVisible = false;
            MB_MissingTransferFilesVisible = true;
            MB_BadFileTransferVisible = false;

            // Show the message box
            var result = await ShowDialog.Handle(this);
        }

        public async void OpenBadFileTransferDialog()
        {
            // Set window properties
            MessageWindowTitle = "Information";
            MessageWindowWidth = 315;
            MessageWindowHeight = 120;
            MessageWindowErrorIcon = false;
            MessageWindowWarningIcon = false;
            MessageWindowInfoIcon = true;

            MB_MissingSortFolderVisible = false;
            MB_RemoveSortFolderVisible = false;
            MB_MissingTransferFilesVisible = false;
            MB_BadFileTransferVisible = true;

            // Show the message box
            var result = await ShowDialog.Handle(this);
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

        private void TestMsg()
        {
            OpenBadFileTransferDialog();
        }

        private void TransferFiles()
        {
            List<string> allSrcFiles = new List<string>();
            List<string> allDestFiles = new List<string>();

            // Get all source file locations
            bool allSortDirsExist = true;
            foreach (var sortFolderItem in SortFolderQueue)
            {
                if (!Directory.Exists(sortFolderItem.FullName))
                {
                    sortFolderItem.FolderExists = false;
                    allSortDirsExist = false;
                }
                else
                {
                    foreach (var sortSrcFileItem in sortFolderItem.SortSrcFiles)
                    {
                        allSrcFiles.Add(sortSrcFileItem);
                        allDestFiles.Add(Path.Combine(sortFolderItem.FullName, Path.GetFileName(sortSrcFileItem)));
                    }
                }
            }

            if (!allSortDirsExist)
            {
                OpenMissingSortFolderDialog();
                return;
            }

            // Make sure all source files still exist
            // Make sure source files and dest lists are not empty
            // Double click on folder name on right to edit sort folder location
            // Change label to red and flashing when folder does not exist
            // Add tooltip that says folder no longer exists

            //Helpers.WindowsShellFileOperation.TransferFiles(allSrcFiles, allDestFiles, IsMove);
        }

        // TODO Prevent adding the same directory twice
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
                SortFolderQueue.Enqueue(tempFolderQueueItem);
            }
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        private void OpenCurrentFile()
        {
            if (File.Exists(CurrentFileQueueItem?.FullName))
            {
                ShellExecute(IntPtr.Zero, null, CurrentFileQueueItem.FullName, null, null, 1);
            }
        }

        private void SortFile(KeyEventArgs e)
        {
            SortFolder? foundSortFolder = SortFolderQueue.FirstOrDefault(x => x.Shortcut?.Matches(e) ?? false);
            if (foundSortFolder != null && CurrentFileQueueItem != null)
            {
                foundSortFolder.SortSrcFiles.Add(CurrentFileQueueItem.FullName);
                CurrentFileQueueItem = FileQueue.Dequeue();
                if (CurrentFileQueueItem != null)
                {
                    int THUMB_SIZE = 256;
                    Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                       CurrentFileQueueItem.FullName, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                    CurrentFileQueueItem.BigImage = ConvertBitmap(thumbnail);
                }
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

        private KeyGesture HashSetKeysToGesture(HashSet<Key> keys)
        {
            if (keys.Count != 0)
            {
                if (keys.Contains(Key.LeftCtrl) || keys.Contains(Key.RightCtrl))
                {
                    return new KeyGesture(
                        keys.FirstOrDefault(x => x != Key.LeftCtrl && x != Key.RightCtrl),
                        KeyModifiers.Control);
                }
                else if (keys.Contains(Key.LeftAlt) || keys.Contains(Key.RightAlt))
                {
                    return new KeyGesture(
                        keys.FirstOrDefault(x => x != Key.LeftAlt && x != Key.RightAlt),
                        KeyModifiers.Alt);
                }
                else if (keys.Contains(Key.LeftShift) || keys.Contains(Key.RightShift))
                {
                    return new KeyGesture(
                        keys.FirstOrDefault(x => x != Key.LeftShift && x != Key.RightShift),
                        KeyModifiers.Shift);
                }
                else
                {
                    return new KeyGesture(keys.FirstOrDefault());
                }
            }
            return null;
        }

        public void RemoveSortFolder(SortFolder sortFolder)
        {
            // TODO message dialog on what to do with un-transfered files
            if (0 < sortFolder?.SortSrcFiles.Count())
            {

            }

            SortFolderQueue.GetCollection()?.Remove(sortFolder);
        }

        public void CloseMsgBox(Window msgBoxWindow)
        {
            msgBoxWindow?.Close();
        }

        public async void RemapSortFolderLocation(SortFolder sortFolder)
        {
            var dlg = new OpenFolderDialog();
            dlg.Title = "Select Sort Directory";
            dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            var result = await dlg.ShowAsync(mHostWindow);
            if (!string.IsNullOrWhiteSpace(result))
            {
                sortFolder.FullName = result;
                sortFolder.FolderExists = true;
                sortFolder.Name = Path.GetFileName(result);
            }
        }

        public async void EditShortcut(SortFolder sortFolder)
        {
            if (sortFolder != null)
            {
                // Flash the label on the folder and disable button
                sortFolder.IsShortcutFlashing = true;
                sortFolder.ShortcutLabel = "            ";
                EditShortcutButtonsEnabled = false;

                // Get currently held keys
                HashSet<Key> userShortcut = await Task.Run(() => Keyboard.GetUserShortcut());

                
                KeyGesture convertedGesture = HashSetKeysToGesture(userShortcut);

                // TODO make sure shortcut is not already being used
                //if (convertedGesture.Matches(all other sort folders)) { show error msg and return;}

                if (convertedGesture != null)
                {
                    AttachShortcut(mHostWindow, convertedGesture);
                    sortFolder.Shortcut = convertedGesture;
                    sortFolder.ShortcutButtonContent = "Edit Shortcut";
                    sortFolder.IsShortcutFlashing = false;
                    EditShortcutButtonsEnabled = true;

                    // Add spaces to shortcut string if necessary
                    string shortcutStr = convertedGesture.ToString();
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
                        // TODO convert the Oem[X] to the symbol; ex. OemTilde => ~
                        if (shortcutStr.Contains("Oem"))
                        {
                            sortFolder.ShortcutLabel = shortcutStr.Remove(0, 3);
                        }
                        else
                        {
                            sortFolder.ShortcutLabel = shortcutStr;
                        }
                    }
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

        public bool IsMove
        {
            get => mIsMove;
            set => this.RaiseAndSetIfChanged(ref mIsMove, value);
        }

        public bool EditShortcutButtonsEnabled
        {
            get => mEditShortcutButtonsEnabled;
            set => this.RaiseAndSetIfChanged(ref mEditShortcutButtonsEnabled, value);
        }

        private ObservableQueue<SortFolder> mSortFolderQueue = new ObservableQueue<SortFolder>();
        private ObservableQueue<FileQueueItem> mFileQueue = new ObservableQueue<FileQueueItem>();
        private FileQueueItem mCurrentFileQueueItem = new FileQueueItem();
        private Window mHostWindow;
        private bool mIsMove = true;
        private bool mEditShortcutButtonsEnabled = true;

        #region Message Window Properties

        public string MessageWindowTitle
        {
            get => mMessageWindowTitle;
            set => this.RaiseAndSetIfChanged(ref mMessageWindowTitle, value);
        }

        public int MessageWindowWidth
        {
            get => mMessageWindowWidth;
            set => this.RaiseAndSetIfChanged(ref mMessageWindowWidth, value);
        }

        public int MessageWindowHeight
        {
            get => mMessageWindowHeight;
            set => this.RaiseAndSetIfChanged(ref mMessageWindowHeight, value);
        }

        public bool MessageWindowErrorIcon
        {
            get => mMessageWindowErrorIcon;
            set => this.RaiseAndSetIfChanged(ref mMessageWindowErrorIcon, value);
        }

        public bool MessageWindowWarningIcon
        {
            get => mMessageWindowWarningIcon;
            set => this.RaiseAndSetIfChanged(ref mMessageWindowWarningIcon, value);
        }

        public bool MessageWindowInfoIcon
        {
            get => mMessageWindowInfoIcon;
            set => this.RaiseAndSetIfChanged(ref mMessageWindowInfoIcon, value);
        }

        public bool MB_MissingSortFolderVisible
        {
            get => mMB_MissingSortFolderVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_MissingSortFolderVisible, value);
        }

        public bool MB_RemoveSortFolderVisible
        {
            get => mMB_RemoveSortFolderVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_RemoveSortFolderVisible, value);
        }

        public bool MB_MissingTransferFilesVisible
        {
            get => mMB_MissingTransferFilesVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_MissingTransferFilesVisible, value);
        }

        public bool MB_BadFileTransferVisible
        {
            get => mMB_BadFileTransferVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_BadFileTransferVisible, value);
        }

        private string mMessageWindowTitle = String.Empty;
        private int mMessageWindowWidth = 300;
        private int mMessageWindowHeight = 120;
        private bool mMessageWindowErrorIcon = false;
        private bool mMessageWindowWarningIcon = false;
        private bool mMessageWindowInfoIcon = false;
        private bool mMB_MissingSortFolderVisible = false;
        private bool mMB_RemoveSortFolderVisible = false;
        private bool mMB_MissingTransferFilesVisible = false;
        private bool mMB_BadFileTransferVisible = false;
        #endregion Message Window Properties
    }
}
