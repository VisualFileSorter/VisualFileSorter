using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
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
using VisualFileSorter.Helpers;
using VisualFileSorter.Models;

namespace VisualFileSorter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(Window hostWindow)
        {
            // Get the main host window
            mHostWindow = hostWindow;
            AttachShortcut(mHostWindow);

            // MainWindow commands
            ImportFilesCmd = ReactiveCommand.Create(ImportFiles);
            TransferFilesCmd = ReactiveCommand.Create(TransferFiles);
            AddSortDirectoryCmd = ReactiveCommand.Create(AddSortDirectory);
            OpenCurrentFileCmd = ReactiveCommand.Create(OpenCurrentFile);
            EditShortcutCmd = ReactiveCommand.Create<SortFolder>(EditShortcut);
            RemapSortFolderLocationCmd = ReactiveCommand.Create<SortFolder>(RemapSortFolderLocation);
            RemoveSortFolderCmd = ReactiveCommand.Create<SortFolder>(RemoveSortFolder);
            RemoveFileCmd = ReactiveCommand.Create<FileQueueItem>(RemoveFile);
            OpenSessionCmd = ReactiveCommand.Create(OpenSession);
            SaveSessionCmd = ReactiveCommand.Create(SaveSession);
            UndoCmd = ReactiveCommand.Create(Undo);
            RedoCmd = ReactiveCommand.Create(Redo);

            // MessageBox Interaction
            ShowDialog = new Interaction<MessageWindowViewModel, DialogResultViewModel?>();
        }

        // MainWindow commands
        public ReactiveCommand<Unit, Unit> ImportFilesCmd { get; }
        public ReactiveCommand<Unit, Unit> TransferFilesCmd { get; }
        public ReactiveCommand<Unit, Unit> AddSortDirectoryCmd { get; }
        public ReactiveCommand<Unit, Unit> OpenCurrentFileCmd { get; }
        public ReactiveCommand<SortFolder, Unit> EditShortcutCmd { get; }
        public ReactiveCommand<SortFolder, Unit> RemapSortFolderLocationCmd { get; }
        public ReactiveCommand<SortFolder, Unit> RemoveSortFolderCmd { get; }
        public ReactiveCommand<FileQueueItem, Unit> RemoveFileCmd { get; }
        public ReactiveCommand<Unit, Unit> OpenSessionCmd { get; }
        public ReactiveCommand<Unit, Unit> SaveSessionCmd { get; }
        public ReactiveCommand<Unit, Unit> UndoCmd { get; }
        public ReactiveCommand<Unit, Unit> RedoCmd { get; }

        // MessageBox Interaction
        public Interaction<MessageWindowViewModel, DialogResultViewModel?> ShowDialog { get; }

        #region Main Operations

        // Import files from the file system
        public async void ImportFiles()
        {
            var dlg = new OpenFileDialog();
            dlg.Title = "Import Files";
            dlg.AllowMultiple = true;
            dlg.InitialFileName = string.Empty;
            dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "Images",
                Extensions = { "tif", "tiff", "bmp", "png", "jpg", "jpeg",
                               "gif", "raw", "cr2", "nef", "orf", "sr2", "webp"}
            });
            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "Video",
                Extensions = { "avi", "divx", "vob", "evo", "m2ts", "flv",
                               "mkv", "mpg", "mpeg", "m1v", "mp4", "m4v",
                               "mp4v", "mpv4", "3gp", "3gpp", "3g2", "3gp2",
                               "ogg", "ogm", "ogv", "rm", "rmvb", "webm",
                               "amv", "mov", "hdmov", "qt"}
            });
            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "Audio",
                Extensions = { "mka", "mp3", "m4a", "oga", "ra", "ram",
                               "flac", "wv", "ac3", "dts", "amr", "alac",
                               "ape", "apl", "aac"}
            });
            dlg.Filters.Add(new FileDialogFilter()
            {
                Name = "Documents",
                Extensions = { "doc", "docx", "htm", "html",
                               "odt", "ods", "pdf", "tex",
                               "xls", "xlsx", "ppt", "pptx", "txt"}
            });

            var result = await dlg.ShowAsync(mHostWindow);
            if (result != null && 0 < result.Count())
            {
                AddFilesToSession(result);
            }
        }

        // Adds an array of files to the session
        private void AddFilesToSession(string[] files)
        {
            List<FileQueueItem> fileItems = new List<FileQueueItem>();
            List<string> alreadyInfileItems = new List<string>();
            foreach (string fileItem in files)
            {
                // Make sure file is not already in VFS
                if (!CheckForFileInSession(fileItem))
                {
                    FileQueueItem tempFileQueueItem = new FileQueueItem();
                    int THUMB_SIZE = 64;
                    try
                    {
                        Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                            fileItem, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                        tempFileQueueItem.SmallImage = ConvertBitmap(thumbnail);
                    }
                    catch (Exception)
                    {
                        // Thumbnail error, set to default error thumbnail
                        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                        tempFileQueueItem.SmallImage = new Avalonia.Media.Imaging.Bitmap(assets.Open(new Uri("avares://VisualFileSorter/Assets/ThumbnailError.png")));
                    }

                    tempFileQueueItem.FullName = fileItem;
                    tempFileQueueItem.Name = Path.GetFileName(fileItem);
                    tempFileQueueItem.IsPlayableMedia = CheckIfPlayableMedia(Path.GetExtension(fileItem));

                    fileItems.Add(tempFileQueueItem);
                }
                else
                {
                    alreadyInfileItems.Add(fileItem);
                }
            }
            FileQueue.EnqueueRange(fileItems);

            // Dequeue the first item
            if (CurrentFileQueueItem?.Name == null && 0 < FileQueue.Count())
            {
                CurrentFileQueueItem = FileQueue.Dequeue();

                try
                {
                    int THUMB_SIZE = 256;
                    Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                       CurrentFileQueueItem.FullName, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                    CurrentFileQueueItem.BigImage = ConvertBitmap(thumbnail);
                }
                catch (Exception)
                {
                    // Thumbnail error, set to default error thumbnail
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    CurrentFileQueueItem.BigImage = new Avalonia.Media.Imaging.Bitmap(assets.Open(new Uri("avares://VisualFileSorter/Assets/ThumbnailError.png")));
                }
            }

            // Display message that some files have already been imported
            if (0 < alreadyInfileItems.Count())
            {
                OpenImportFilesAlreadyInDialog(alreadyInfileItems);
            }
        }

        // Transfer the files
        private async void TransferFiles()
        {
            // Clear the undo and redo buffers
            UndoEnabled = false;
            RedoEnabled = false;
            mUndoBuffer = new CircularBuffer<UndoRedoItem>(30);
            mRedoBuffer = new CircularBuffer<UndoRedoItem>(30);

            // Get all source file locations
            List<string> allSrcFiles = new List<string>();
            List<string> allDestFiles = new List<string>();
            bool allSortDirsExist = true;
            List<string> missingSrcFiles = new List<string>();
            lock (mSortFolderQueueLock)
            {
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
                            if (File.Exists(sortSrcFileItem.Value))
                            {
                                allSrcFiles.Add(sortSrcFileItem.Value);
                                allDestFiles.Add(Path.Combine(sortFolderItem.FullName, Path.GetFileName(sortSrcFileItem.Value)));
                            }
                            else
                            {
                                missingSrcFiles.Add(sortSrcFileItem.Value);
                            }
                        }
                    }
                }
            }

            // Make sure all SortFolders exist
            if (!allSortDirsExist)
            {
                OpenMissingSortFolderDialog();
                return;
            }

            // Open dialog that shows files that no longer exist
            if (0 < missingSrcFiles.Count())
            {
                DialogResult result = await OpenMissingTransferFilesDialog(missingSrcFiles);
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            // Make sure there are files to be transferred
            if (0 < allSrcFiles.Count() && 0 < allDestFiles.Count())
            {
                lock (mSortFolderQueueLock)
                {
                    // Clear the source file transfer dictionary
                    foreach (var sortFolderItem in SortFolderQueue)
                    {
                        sortFolderItem.SortSrcFiles.Clear();
                    }
                }

                // Start an async shell file transfer
                var _TransferTask = Task.Run(() =>
                {
                    TransferProgBarVis = true;
                    bool success = Helpers.WindowsShellFileOperation.TransferFiles(allSrcFiles, allDestFiles, IsMove);
                    if (!success)
                    {
                        for (int i = 0; i < allSrcFiles.Count(); i++)
                        {
                            if (File.Exists(allSrcFiles[i]))
                            {
                                lock (mSortFolderQueueLock)
                                {
                                    SortFolder? foundSortFolder = SortFolderQueue?.FirstOrDefault(x => x.FullName == Path.GetDirectoryName(allDestFiles[i]));
                                    if (foundSortFolder != null)
                                    {
                                        // Add the un-transferred files back into the respective SortFolder
                                        foundSortFolder.SortSrcFiles.TryAdd(allSrcFiles[i], allSrcFiles[i]);
                                    }
                                    else
                                    {
                                        // SortFolder was removed during transfer, Recreate it
                                        SortFolder tempFolderQueueItem = new SortFolder();
                                        tempFolderQueueItem.FullName = Path.GetDirectoryName(allDestFiles[i]);
                                        tempFolderQueueItem.Name = Path.GetFileName(Path.GetDirectoryName(allDestFiles[i]));
                                        tempFolderQueueItem.SortSrcFiles.TryAdd(allSrcFiles[i], allSrcFiles[i]);
                                        SortFolderQueue?.Enqueue(tempFolderQueueItem);
                                    }
                                }
                            }
                        }

                        // Open message box
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            OpenBadFileTransferDialog();
                        }, DispatcherPriority.Normal);
                    }

                    // Hide progress bar
                    TransferProgBarVis = false;
                }
                );
            }
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        // Open current file with Win32 shell
        private void OpenCurrentFile()
        {
            if (File.Exists(CurrentFileQueueItem?.FullName))
            {
                ShellExecute(IntPtr.Zero, null, CurrentFileQueueItem.FullName, null, null, 1);
            }
        }

        // Sort the current file to the respective SortFolder for the given KeyGesture
        private void SortFile(KeyEventArgs e)
        {
            lock (mSortFolderQueueLock)
            {
                // Find the SortFolder for the given KeyGesture
                SortFolder? foundSortFolder = SortFolderQueue.FirstOrDefault(x => x.Shortcut?.Matches(e) ?? false);
                if (foundSortFolder != null && CurrentFileQueueItem != null && EditShortcutButtonsEnabled == true)
                {
                    // Add the operation onto the undo buffer
                    mUndoBuffer.PushBack(new UndoRedoItem(CurrentFileQueueItem.FullName, foundSortFolder.FullName));
                    UndoEnabled = !mUndoBuffer.IsEmpty;
                    RedoEnabled = !mRedoBuffer.IsEmpty;

                    // Add the file to the sort folder and get next file
                    foundSortFolder.SortSrcFiles.TryAdd(CurrentFileQueueItem.FullName, CurrentFileQueueItem.FullName);
                    CurrentFileQueueItem = FileQueue.Dequeue();
                    if (CurrentFileQueueItem != null)
                    {
                        try
                        {
                            int THUMB_SIZE = 256;
                            Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                               CurrentFileQueueItem.FullName, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                            CurrentFileQueueItem.BigImage = ConvertBitmap(thumbnail);
                        }
                        catch (Exception)
                        {
                            // Thumbnail error, set to default error thumbnail
                            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                            CurrentFileQueueItem.BigImage = new Avalonia.Media.Imaging.Bitmap(assets.Open(new Uri("avares://VisualFileSorter/Assets/ThumbnailError.png")));
                        }
                    }
                }
            }
        }

        // Add a new SortFolder
        public async void AddSortDirectory()
        {
            var dlg = new OpenFolderDialog();
            dlg.Title = "Select Sort Directory";
            dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            var result = await dlg.ShowAsync(mHostWindow);
            if (!string.IsNullOrWhiteSpace(result))
            {
                // Prevent adding the same directory twice
                lock (mSortFolderQueueLock)
                {
                    foreach (SortFolder sortFolderItem in SortFolderQueue)
                    {
                        if (sortFolderItem.FullName == result)
                        {
                            return;
                        }
                    }

                    SortFolder tempFolderQueueItem = new SortFolder();
                    tempFolderQueueItem.FullName = result;
                    tempFolderQueueItem.Name = Path.GetFileName(result);
                    SortFolderQueue.Enqueue(tempFolderQueueItem);
                }
            }
        }

        public void AddSortDirectoriesFromSavedSession(SortFolderJson savedSortFolder)
        {
            lock (mSortFolderQueueLock)
            {
                foreach (SortFolder sortFolderItem in SortFolderQueue)
                {
                    if (sortFolderItem.FullName == savedSortFolder.FullName)
                    {
                        return;
                    }
                }

                SortFolder tempFolderQueueItem = new SortFolder();
                tempFolderQueueItem.FullName = savedSortFolder.FullName;
                // Convert from string[] to ConcurrentDictionary<string, string>.
                foreach (string sortedFile in savedSortFolder.SortSrcFiles)
                {
                    tempFolderQueueItem.SortSrcFiles.TryAdd(sortedFile, sortedFile);
                }
                // Set Sort Folders from JSON.
                if (savedSortFolder.Shortcut != "            ") // If shortcut is set in Saved Session.
                {
                    tempFolderQueueItem.Shortcut = KeyGesture.Parse(savedSortFolder.Shortcut);
                    tempFolderQueueItem.ShortcutLabel = savedSortFolder.Shortcut;
                    tempFolderQueueItem.ShortcutButtonContent = "Edit Shortcut";
                }
                else
                {
                    tempFolderQueueItem.ShortcutButtonContent = "Add Shortcut";
                }
                tempFolderQueueItem.Name = Path.GetFileName(savedSortFolder.FullName);
                SortFolderQueue.Enqueue(tempFolderQueueItem);
            }
        }

        // On every keystroke, pass the gesture to the SortFile method
        public IDisposable AttachShortcut(TopLevel root)
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

        // Convert a HashSet of Keys to a KeyGesture
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
                // Alt doesn't work
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

        // Removes File from file queue
        public void RemoveFile(FileQueueItem fileQueueItem)
        {
            // Remove the FileQueueItem
            if (fileQueueItem != null)
            {
                FileQueue.GetCollection()?.Remove(fileQueueItem);
            }
        }

        // Removes a SortFolder
        public async void RemoveSortFolder(SortFolder sortFolder)
        {
            // Message dialog on what to do with un-transfered files
            if (0 < sortFolder?.SortSrcFiles.Count())
            {
                // Clear the undo and redo buffers
                UndoEnabled = false;
                RedoEnabled = false;
                mUndoBuffer = new CircularBuffer<UndoRedoItem>(30);
                mRedoBuffer = new CircularBuffer<UndoRedoItem>(30);

                DialogResult result = await OpenRemoveSortFolderDialog();
                switch (result)
                {
                    // Add files back into the queue
                    case DialogResult.Queue:
                        List<FileQueueItem> fileItems = new List<FileQueueItem>();
                        foreach (var fileItem in sortFolder?.SortSrcFiles)
                        {
                            if (File.Exists(fileItem.Value))
                            {
                                FileQueueItem tempFileQueueItem = new FileQueueItem();

                                try
                                {
                                    int THUMB_SIZE = 64;
                                    Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                                       fileItem.Value, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                                    tempFileQueueItem.SmallImage = ConvertBitmap(thumbnail);
                                }
                                catch (Exception)
                                {
                                    // Thumbnail error, set to default error thumbnail
                                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                                    tempFileQueueItem.SmallImage = new Avalonia.Media.Imaging.Bitmap(assets.Open(new Uri("avares://VisualFileSorter/Assets/ThumbnailError.png")));
                                }

                                tempFileQueueItem.FullName = fileItem.Value;
                                tempFileQueueItem.Name = Path.GetFileName(fileItem.Value);
                                tempFileQueueItem.IsPlayableMedia = CheckIfPlayableMedia(Path.GetExtension(fileItem.Value));

                                fileItems.Add(tempFileQueueItem);
                            }
                        }
                        FileQueue.EnqueueRange(fileItems);

                        // Dequeue the first item
                        if (CurrentFileQueueItem?.Name == null && 0 < FileQueue.Count())
                        {
                            CurrentFileQueueItem = FileQueue.Dequeue();

                            try
                            {
                                int THUMB_SIZE = 256;
                                Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                                   CurrentFileQueueItem.FullName, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                                CurrentFileQueueItem.BigImage = ConvertBitmap(thumbnail);
                            }
                            catch (Exception)
                            {
                                // Thumbnail error, set to default error thumbnail
                                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                                CurrentFileQueueItem.BigImage = new Avalonia.Media.Imaging.Bitmap(assets.Open(new Uri("avares://VisualFileSorter/Assets/ThumbnailError.png")));
                            }
                        }
                        break;

                    // Remove the SortFolder
                    case DialogResult.OK:
                        lock (mSortFolderQueueLock)
                        {
                            SortFolderQueue.GetCollection()?.Remove(sortFolder);
                        }
                        return;

                    // Cancel the operation
                    case DialogResult.Cancel:
                        return;
                    default:
                        break;
                }
            }

            // Remove the SortFolder
            if (sortFolder != null)
            {
                lock (mSortFolderQueueLock)
                {
                    SortFolderQueue.GetCollection()?.Remove(sortFolder);
                }
            }
        }

        // Re-direct a SortFolder to a new location
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

        // Edit or create a shortcut for a SortFolder
        public async void EditShortcut(SortFolder sortFolder)
        {
            if (sortFolder != null)
            {
                // Flash the label on the folder and disable button
                sortFolder.IsShortcutFlashing = true;
                string prevShortcutLabel = sortFolder.ShortcutLabel;
                sortFolder.ShortcutLabel = "            ";
                EditShortcutButtonsEnabled = false;

                // Get currently held keys as KeyGesture
                HashSet<Key> userShortcut = await Task.Run(() => Keyboard.GetUserShortcut());
                KeyGesture convertedGesture = HashSetKeysToGesture(userShortcut);

                // Make sure shortcut is not already being used
                lock (mSortFolderQueueLock)
                {
                    foreach (SortFolder sortFolderItem in SortFolderQueue)
                    {
                        if (sortFolderItem != sortFolder &&
                            convertedGesture == sortFolderItem.Shortcut)
                        {
                            OpenShortcutAlreadyExistsDialog();
                            sortFolder.ShortcutLabel = prevShortcutLabel;
                            sortFolder.IsShortcutFlashing = false;
                            EditShortcutButtonsEnabled = true;
                            return;
                        }
                    }
                }

                // Add the shortcut to the SortFolder
                if (convertedGesture != null)
                {
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

        #endregion Main Operations

        #region Helper Methods

        // Make sure file is not already in VFS
        public bool CheckForFileInSession(string fileItem)
        {
            lock (mSortFolderQueueLock)
            {
                foreach (SortFolder sortFolderItem in SortFolderQueue)
                {
                    string foundFileStr = string.Empty;
                    if (sortFolderItem.SortSrcFiles.TryGetValue(fileItem, out foundFileStr))
                    {
                        return true;
                    }
                }
            }

            foreach (FileQueueItem queueItem in FileQueue)
            {
                if (queueItem.FullName == fileItem)
                {
                    return true;
                }
            }

            if (CurrentFileQueueItem?.FullName == fileItem)
            {
                return true;
            }

            return false;
        }

        // Show play button if playable media
        public bool CheckIfPlayableMedia(string extension)
        {
            return extension == ".avi" || extension == ".divx" ||
                   extension == ".vob" || extension == ".evo" ||
                   extension == ".m2ts" || extension == ".flv" ||
                   extension == ".mkv" || extension == ".mpg" ||
                   extension == ".mpeg" || extension == ".m1v" ||
                   extension == ".mp4" || extension == ".m4v" ||
                   extension == ".mp4v" || extension == ".mpv4" ||
                   extension == ".3gp" || extension == ".3gpp" ||
                   extension == ".3g2" || extension == ".3gp2" ||
                   extension == ".ogg" || extension == ".ogm" ||
                   extension == ".rm" || extension == ".rmvb" ||
                   extension == ".webm" || extension == ".amv" ||
                   extension == ".mov" || extension == ".hdmov" ||
                   extension == ".qt" || extension == ".mka" ||
                   extension == ".mp3" || extension == ".m4a" ||
                   extension == ".oga" || extension == ".ra" ||
                   extension == ".ram" || extension == ".flac" ||
                   extension == ".wv" || extension == ".ac3" ||
                   extension == ".dts" || extension == ".amr" ||
                   extension == ".alac" || extension == ".ape" ||
                   extension == ".apl" || extension == ".aac" ||
                   extension == ".doc" || extension == ".docx" ||
                   extension == ".html" || extension == ".htm" ||
                   extension == ".odt" || extension == ".ods" ||
                   extension == ".pdf" || extension == ".tex" ||
                   extension == ".xls" || extension == ".xlsx" ||
                   extension == ".ppt" || extension == ".pptx" ||
                   extension == ".txt";
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

        #endregion Helper Methods

        #region Show MessageBox Methods

        // Display error message that a SortFolder no longer exists
        public async void OpenMissingSortFolderDialog()
        {
            // Set window properties
            var messageVM = new MessageWindowViewModel();

            messageVM.MessageWindowTitle = "Error";
            messageVM.MessageWindowWidth = 300;
            messageVM.MessageWindowHeight = 120;
            messageVM.MessageWindowErrorIcon = true;
            messageVM.MessageWindowWarningIcon = false;
            messageVM.MessageWindowInfoIcon = false;

            messageVM.MB_MissingSortFolderVisible = true;
            messageVM.MB_RemoveSortFolderVisible = false;
            messageVM.MB_MissingTransferFilesVisible = false;
            messageVM.MB_BadFileTransferVisible = false;
            messageVM.MB_ImportFilesAlreadyInVisible = false;
            messageVM.MB_ShortcutAlreadyExistsVisible = false;
            messageVM.MB_ReplaceSessionVisible = false;

            // Show the message box
            await ShowDialog.Handle(messageVM);
        }

        // Display warning that the removed sort folder has/had un-transferred files
        public async Task<DialogResult> OpenRemoveSortFolderDialog()
        {
            // Set window properties
            var messageVM = new MessageWindowViewModel();

            messageVM.MessageWindowTitle = "Warning";
            messageVM.MessageWindowWidth = 410;
            messageVM.MessageWindowHeight = 135;
            messageVM.MessageWindowErrorIcon = false;
            messageVM.MessageWindowWarningIcon = true;
            messageVM.MessageWindowInfoIcon = false;

            messageVM.MB_MissingSortFolderVisible = false;
            messageVM.MB_RemoveSortFolderVisible = true;
            messageVM.MB_MissingTransferFilesVisible = false;
            messageVM.MB_BadFileTransferVisible = false;
            messageVM.MB_ImportFilesAlreadyInVisible = false;
            messageVM.MB_ShortcutAlreadyExistsVisible = false;
            messageVM.MB_ReplaceSessionVisible = false;

            // Show the message box
            var result = await ShowDialog.Handle(messageVM);
            if (result != null)
            {
                return result.DiaResult;
            }
            else
            {
                return DialogResult.Cancel;
            }
        }

        // Display a warning that un-transferred files no longer exist
        public async Task<DialogResult> OpenMissingTransferFilesDialog(List<string> missingSrcFiles)
        {
            // Set window properties
            var messageVM = new MessageWindowViewModel();

            messageVM.MessageWindowTitle = "Warning";
            messageVM.MessageWindowWidth = 300;
            messageVM.MessageWindowHeight = 267;
            messageVM.MessageWindowErrorIcon = false;
            messageVM.MessageWindowWarningIcon = true;
            messageVM.MessageWindowInfoIcon = false;

            messageVM.MB_MissingSortFolderVisible = false;
            messageVM.MB_RemoveSortFolderVisible = false;
            messageVM.MB_MissingTransferFilesVisible = true;
            messageVM.MB_BadFileTransferVisible = false;
            messageVM.MB_ImportFilesAlreadyInVisible = false;
            messageVM.MB_ShortcutAlreadyExistsVisible = false;
            messageVM.MB_ReplaceSessionVisible = false;
            messageVM.MB_MissingTransferFilesList = string.Join("\n", missingSrcFiles);

            // Show the message box
            var result = await ShowDialog.Handle(messageVM);
            if (result != null)
            {
                return result.DiaResult;
            }
            else
            {
                return DialogResult.Cancel;
            }
        }

        // Display information that something went wrong with the shell moving/copying of files
        public async void OpenBadFileTransferDialog()
        {
            // Set window properties
            var messageVM = new MessageWindowViewModel();

            messageVM.MessageWindowTitle = "Information";
            messageVM.MessageWindowWidth = 315;
            messageVM.MessageWindowHeight = 120;
            messageVM.MessageWindowErrorIcon = false;
            messageVM.MessageWindowWarningIcon = false;
            messageVM.MessageWindowInfoIcon = true;

            messageVM.MB_MissingSortFolderVisible = false;
            messageVM.MB_RemoveSortFolderVisible = false;
            messageVM.MB_MissingTransferFilesVisible = false;
            messageVM.MB_BadFileTransferVisible = true;
            messageVM.MB_ImportFilesAlreadyInVisible = false;
            messageVM.MB_ShortcutAlreadyExistsVisible = false;
            messageVM.MB_ReplaceSessionVisible = false;

            // Show the message box
            await ShowDialog.Handle(messageVM);
        }

        // Display a warning that imported files already are in VFS
        public async void OpenImportFilesAlreadyInDialog(List<string> alreadyInfileItems)
        {
            // Set window properties
            var messageVM = new MessageWindowViewModel();

            messageVM.MessageWindowTitle = "Warning";
            messageVM.MessageWindowWidth = 300;
            messageVM.MessageWindowHeight = 227;
            messageVM.MessageWindowErrorIcon = false;
            messageVM.MessageWindowWarningIcon = true;
            messageVM.MessageWindowInfoIcon = false;

            messageVM.MB_MissingSortFolderVisible = false;
            messageVM.MB_RemoveSortFolderVisible = false;
            messageVM.MB_MissingTransferFilesVisible = false;
            messageVM.MB_BadFileTransferVisible = false;
            messageVM.MB_ImportFilesAlreadyInVisible = true;
            messageVM.MB_ShortcutAlreadyExistsVisible = false;
            messageVM.MB_ReplaceSessionVisible = false;
            messageVM.MB_ImportFilesAlreadyInList = string.Join("\n", alreadyInfileItems);

            // Show the message box
            await ShowDialog.Handle(messageVM);
        }

        // Display information that the shortcut already exists
        public async void OpenShortcutAlreadyExistsDialog()
        {
            // Set window properties
            var messageVM = new MessageWindowViewModel();

            messageVM.MessageWindowTitle = "Information";
            messageVM.MessageWindowWidth = 250;
            messageVM.MessageWindowHeight = 100;
            messageVM.MessageWindowErrorIcon = false;
            messageVM.MessageWindowWarningIcon = false;
            messageVM.MessageWindowInfoIcon = true;

            messageVM.MB_MissingSortFolderVisible = false;
            messageVM.MB_RemoveSortFolderVisible = false;
            messageVM.MB_MissingTransferFilesVisible = false;
            messageVM.MB_BadFileTransferVisible = false;
            messageVM.MB_ImportFilesAlreadyInVisible = false;
            messageVM.MB_ShortcutAlreadyExistsVisible = true;
            messageVM.MB_ReplaceSessionVisible = false;

            // Show the message box
            await ShowDialog.Handle(messageVM);
        }

        // Display warning that the current session will be replaced
        public async Task<DialogResult> OpenReplaceSessionDialog()
        {
            // Set window properties
            var messageVM = new MessageWindowViewModel();

            messageVM.MessageWindowTitle = "Warning";
            messageVM.MessageWindowWidth = 400;
            messageVM.MessageWindowHeight = 100;
            messageVM.MessageWindowErrorIcon = false;
            messageVM.MessageWindowWarningIcon = true;
            messageVM.MessageWindowInfoIcon = false;

            messageVM.MB_MissingSortFolderVisible = false;
            messageVM.MB_RemoveSortFolderVisible = false;
            messageVM.MB_MissingTransferFilesVisible = false;
            messageVM.MB_BadFileTransferVisible = false;
            messageVM.MB_ImportFilesAlreadyInVisible = false;
            messageVM.MB_ShortcutAlreadyExistsVisible = false;
            messageVM.MB_ReplaceSessionVisible = true;

            // Show the message box
            var result = await ShowDialog.Handle(messageVM);
            if (result != null)
            {
                return result.DiaResult;
            }
            else
            {
                return DialogResult.Cancel;
            }
        }

        #endregion Show MessageBox Methods

        #region Undo/Redo Operations

        // Undo the last file sort
        private void Undo()
        {
            // Make sure undo buffer is not empty
            if (!mUndoBuffer.IsEmpty)
            {
                // Get back UndoRedoItem
                UndoRedoItem curUndoItem = mUndoBuffer.Back();
                mUndoBuffer.PopBack();

                // Add the current item back onto the file queue
                if (CurrentFileQueueItem != null)
                {
                    FileQueue.Enqueue(CurrentFileQueueItem);
                }

                // Recreate the FileQueueItem object for the sorted file
                FileQueueItem tempFileQueueItem = new FileQueueItem();
                int THUMB_SIZE = 64;
                int THUMB_SIZE_BIG = 256;
                try
                {
                    Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                        curUndoItem.mSrcFilename, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                    Bitmap thumbnailBig = Helpers.WindowsThumbnailProvider.GetThumbnail(
                        curUndoItem.mSrcFilename, THUMB_SIZE_BIG, THUMB_SIZE_BIG, Helpers.ThumbnailOptions.None);
                    tempFileQueueItem.SmallImage = ConvertBitmap(thumbnail);
                    tempFileQueueItem.BigImage = ConvertBitmap(thumbnailBig);
                }
                catch (Exception)
                {
                    // Thumbnail error, set to default error thumbnail
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    tempFileQueueItem.SmallImage = new Avalonia.Media.Imaging.Bitmap(assets.Open(new Uri("avares://VisualFileSorter/Assets/ThumbnailError.png")));
                    tempFileQueueItem.BigImage = new Avalonia.Media.Imaging.Bitmap(assets.Open(new Uri("avares://VisualFileSorter/Assets/ThumbnailError.png")));
                }

                tempFileQueueItem.FullName = curUndoItem.mSrcFilename;
                tempFileQueueItem.Name = Path.GetFileName(curUndoItem.mSrcFilename);
                tempFileQueueItem.IsPlayableMedia = CheckIfPlayableMedia(Path.GetExtension(curUndoItem.mSrcFilename));
                CurrentFileQueueItem = tempFileQueueItem;

                // Remove the entry from the SortFolder
                lock (mSortFolderQueueLock)
                {
                    SortFolder? foundSortFolder = SortFolderQueue?.FirstOrDefault(x => x.FullName == curUndoItem.mSortFolderName);
                    if (foundSortFolder != null)
                    {
                        string foundStr = string.Empty;
                        foundSortFolder.SortSrcFiles.TryRemove(curUndoItem.mSrcFilename, out foundStr);
                    }
                }

                // Add onto redo buffer and set enable state on undo/redo buttons
                mRedoBuffer.PushBack(curUndoItem);
                UndoEnabled = !mUndoBuffer.IsEmpty;
                RedoEnabled = !mRedoBuffer.IsEmpty;
            }
        }

        // Redo the last file sort
        private void Redo()
        {
            // Make sure redo buffer is not empty
            if (!mRedoBuffer.IsEmpty)
            {
                // Get back UndoRedoItem
                UndoRedoItem curRedoItem = mRedoBuffer.Back();
                mRedoBuffer.PopBack();

                lock (mSortFolderQueueLock)
                {
                    // Find the respective SortFolder
                    SortFolder? foundSortFolder = SortFolderQueue.FirstOrDefault(x => x.FullName == curRedoItem.mSortFolderName);
                    if (foundSortFolder != null && CurrentFileQueueItem != null && EditShortcutButtonsEnabled == true)
                    {
                        // Make sure the CurrentFileQueueItem and curRedoItem.mSrcFilename are equal
                        if (CurrentFileQueueItem.FullName != curRedoItem.mSrcFilename)
                        {
                            throw new ArgumentException("CurrentFileQueueItem.FullName does not match Redo object");
                        }

                        // Re-add the file back into its respective SortFolder and get next file
                        foundSortFolder.SortSrcFiles.TryAdd(CurrentFileQueueItem.FullName, CurrentFileQueueItem.FullName);
                        CurrentFileQueueItem = FileQueue.Dequeue();
                        if (CurrentFileQueueItem != null)
                        {
                            try
                            {
                                int THUMB_SIZE = 256;
                                Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                                   CurrentFileQueueItem.FullName, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                                CurrentFileQueueItem.BigImage = ConvertBitmap(thumbnail);
                            }
                            catch (Exception)
                            {
                                // Thumbnail error, set to default error thumbnail
                                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                                CurrentFileQueueItem.BigImage = new Avalonia.Media.Imaging.Bitmap(assets.Open(new Uri("avares://VisualFileSorter/Assets/ThumbnailError.png")));
                            }
                        }
                    }
                }

                // Add onto undo buffer and set enable state on undo/redo buttons
                mUndoBuffer.PushBack(curRedoItem);
                UndoEnabled = !mUndoBuffer.IsEmpty;
                RedoEnabled = !mRedoBuffer.IsEmpty;
            }
        }

        #endregion Undo/Redo Operations

        #region Open/Save/Clear Session

        // Open a VFS session file
        private async void OpenSession()
        {
            // Check if the current session is not empty.
            if (CurrentFileQueueItem?.FullName != null || 0 < FileQueue.Count() || 0 < SortFolderQueue.Count())
            {
                var result = await OpenReplaceSessionDialog();
                if (result == DialogResult.Cancel)
                {
                    return;
                }
                // If user confirms opening/overwriting of session, clear current session before starting OpenSession procedures.
                else
                {
                    // Clear current session.
                    ClearSession();
                    OpenSession();
                }
            }
            else // If empty.
            {
                // Prompt user for file selection of session to be opened.
                var dlg = new OpenFileDialog();
                dlg.Title = "Open Session";
                dlg.AllowMultiple = false;
                dlg.InitialFileName = "VFS_Session.vfss";
                dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var result = await dlg.ShowAsync(mHostWindow);
                if (result != null)
                {
                    try
                    {
                        // Create "new" session for opened .vfss file.
                        Session openedSession = new Session();
                        // Store serialized JSON into jsonString to be deserialized.
                        string jsonString = File.ReadAllText(result[0]);
                        // Deserialize JSON into openedSession instance.
                        openedSession = openedSession.Deserialize(jsonString);

                        // Take FileQueue List<string> and fill an array with the contents.
                        string[] fileQueueArray = openedSession.FileQueue.ToArray();
                        // Add fileQueue from saved session to the view.
                        AddFilesToSession(fileQueueArray);

                        // Take all SortFolder data from openedSession and present to the view.
                        foreach (SortFolderJson savedSortFolder in openedSession.SortFolders)
                        {
                            AddSortDirectoriesFromSavedSession(savedSortFolder);
                        }
                    }
                    catch (Exception)
                    {
                        //TODO: Create error opening session dialog.
                    }
                }
            }
        }

        // Save current session as json
        private async void SaveSession()
        {
            // Create new session
            Session saveSession = new Session();

            // Get each file in the file queue
            foreach (FileQueueItem fileItem in FileQueue)
            {
                saveSession.FileQueue.Add(fileItem.FullName);
            }

            // Get the CurrentFileQueueItem filepath
            if (CurrentFileQueueItem != null)
            {
                saveSession.FileQueue.Add(CurrentFileQueueItem.FullName);
            }

            // Get each SortFolder
            foreach (SortFolder folderItem in SortFolderQueue)
            {
                SortFolderJson tempSortFolderJson = new SortFolderJson();
                tempSortFolderJson.FullName = folderItem.FullName;
                tempSortFolderJson.Shortcut = folderItem.ShortcutLabel;
                foreach (var sortFileItem in folderItem.SortSrcFiles)
                {
                    tempSortFolderJson.SortSrcFiles.Add(sortFileItem.Value);
                }

                // Add to saveSession
                saveSession.SortFolders.Add(tempSortFolderJson);
            }

            // Open SaveFileDialog
            var dlg = new SaveFileDialog();
            dlg.Title = "Save Session";
            dlg.InitialFileName = "VFS_Session.vfss";
            dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var result = await dlg.ShowAsync(mHostWindow);
            if (result != null)
            {
                try
                {
                    // Save session file to disk
                    File.WriteAllText(result, saveSession.Serialize());
                }
                catch (Exception)
                {
                    // TODO Create error saving/loading session dialog
                }
            }
        }

        private void ClearSession()
        {
            try
            {
                this.FileQueue = new ObservableQueue<FileQueueItem>();
                this.SortFolderQueue = new ObservableQueue<SortFolder>();
                this.CurrentFileQueueItem = new FileQueueItem();
            }
            catch (Exception)
            {
                //TODO: Create error clearing session dialog.
            }
        }

        #endregion Open/Save/Clear Session

        #region Properties and Members

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

        public bool TransferProgBarVis
        {
            get => mTransferProgBarVis;
            set => this.RaiseAndSetIfChanged(ref mTransferProgBarVis, value);
        }

        public bool UndoEnabled
        {
            get => mUndoEnabled;
            set => this.RaiseAndSetIfChanged(ref mUndoEnabled, value);
        }

        public bool RedoEnabled
        {
            get => mRedoEnabled;
            set => this.RaiseAndSetIfChanged(ref mRedoEnabled, value);
        }

        private ObservableQueue<SortFolder> mSortFolderQueue = new ObservableQueue<SortFolder>();
        private ObservableQueue<FileQueueItem> mFileQueue = new ObservableQueue<FileQueueItem>();
        private FileQueueItem mCurrentFileQueueItem = new FileQueueItem();
        private CircularBuffer<UndoRedoItem> mUndoBuffer = new CircularBuffer<UndoRedoItem>(30);
        private CircularBuffer<UndoRedoItem> mRedoBuffer = new CircularBuffer<UndoRedoItem>(30);
        private Window mHostWindow;
        private bool mIsMove = true;
        private bool mEditShortcutButtonsEnabled = true;
        private bool mTransferProgBarVis = false;
        private bool mUndoEnabled = false;
        private bool mRedoEnabled = false;
        private readonly object mSortFolderQueueLock = new object();
        #endregion Properties and Members
    }
}
