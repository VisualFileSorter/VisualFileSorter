using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

// TODO
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

        // MessageBox Interaction
        public Interaction<MessageWindowViewModel, DialogResultViewModel?> ShowDialog { get; }

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
                List<FileQueueItem> fileItems = new List<FileQueueItem>();
                List<string> alreadyInfileItems = new List<string>();
                foreach (string fileItem in result)
                {
                    if (!CheckForFileInSession(fileItem))
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
                    int THUMB_SIZE = 256;
                    Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                       CurrentFileQueueItem.FullName, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                    CurrentFileQueueItem.BigImage = ConvertBitmap(thumbnail);
                }

                // Display message that some files have already been imported
                if (0 < alreadyInfileItems.Count())
                {
                    OpenImportFilesAlreadyInDialog(alreadyInfileItems);
                }
            }
        }

        public bool CheckForFileInSession(string fileItem)
        {
            foreach (SortFolder sortFolderItem in SortFolderQueue)
            {
                if (sortFolderItem.SortSrcFiles.Contains(fileItem))
                {
                    return true;
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

            // Show the message box
            var result = await ShowDialog.Handle(messageVM);
        }

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

            // Show the message box
            var result = await ShowDialog.Handle(messageVM);
            return result.DiaResult;
        }

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

            // Show the message box
            var result = await ShowDialog.Handle(messageVM);
            return result.DiaResult;
        }

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

            // Show the message box
            await ShowDialog.Handle(messageVM);
        }

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

            // Show the message box
            var result = await ShowDialog.Handle(messageVM);
        }

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

            // Show the message box
            var result = await ShowDialog.Handle(messageVM);
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

        private async void TransferFiles()
        {
            List<string> allSrcFiles = new List<string>();
            List<string> allDestFiles = new List<string>();

            // Get all source file locations
            bool allSortDirsExist = true;
            List<string> missingSrcFiles = new List<string>();
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
                        if (File.Exists(sortSrcFileItem))
                        {
                            allSrcFiles.Add(sortSrcFileItem);
                            allDestFiles.Add(Path.Combine(sortFolderItem.FullName, Path.GetFileName(sortSrcFileItem)));
                        }
                        else
                        {
                            missingSrcFiles.Add(sortSrcFileItem);
                        }
                    }
                }
            }

            if (!allSortDirsExist)
            {
                OpenMissingSortFolderDialog();
                return;
            }

            if (0 < missingSrcFiles.Count())
            {
                DialogResult result = await OpenMissingTransferFilesDialog(missingSrcFiles);
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            if (0 < allSrcFiles.Count() && 0 < allDestFiles.Count())
            {
                foreach (var sortFolderItem in SortFolderQueue)
                {
                    sortFolderItem.SortSrcFiles.Clear();
                }

                // TODO make thread safe
                var _TransferTask = Task.Run(() =>
                  {
                      bool success = Helpers.WindowsShellFileOperation.TransferFiles(allSrcFiles, allDestFiles, IsMove);
                      if (!success)
                      {
                          for (int i = 0; i < allSrcFiles.Count(); i++)
                          {
                              if (File.Exists(allSrcFiles[i]))
                              {
                                  SortFolder? foundSortFolder = SortFolderQueue?.FirstOrDefault(x => x.FullName == Path.GetDirectoryName(allDestFiles[i]));
                                  if (foundSortFolder != null)
                                  {
                                      foundSortFolder.SortSrcFiles.Add(allSrcFiles[i]);
                                  }
                                  else
                                  {
                                      // SortFolder was removed during transfer, Recreate it
                                      SortFolder tempFolderQueueItem = new SortFolder();
                                      tempFolderQueueItem.FullName = Path.GetDirectoryName(allDestFiles[i]);
                                      tempFolderQueueItem.Name = Path.GetFileName(Path.GetDirectoryName(allDestFiles[i]));
                                      tempFolderQueueItem.SortSrcFiles.Add(allSrcFiles[i]);
                                      SortFolderQueue?.Enqueue(tempFolderQueueItem);
                                  }
                              }
                          }

                          // Open message box
                          Dispatcher.UIThread.InvokeAsync(() =>
                          {
                              OpenBadFileTransferDialog();
                          }, DispatcherPriority.Normal);
                      }
                  }
                );
            }
        }


        public async void AddSortDirectory()
        {
            var dlg = new OpenFolderDialog();
            dlg.Title = "Select Sort Directory";
            dlg.Directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            var result = await dlg.ShowAsync(mHostWindow);
            if (!string.IsNullOrWhiteSpace(result))
            {
                // Prevent adding the same directory twice
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
            if (foundSortFolder != null && CurrentFileQueueItem != null && EditShortcutButtonsEnabled == true)
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

        void MainWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            SortFile(e);
        }

        public IDisposable AttachShortcut(TopLevel root)
        {
            return root.AddDisposableHandler(
                InputElement.KeyDownEvent,
                MainWindowPreviewKeyDown,
                RoutingStrategies.Tunnel);
        }

        public void DetachShortcut(TopLevel root)
        {
            root.RemoveHandler(InputElement.KeyDownEvent, MainWindowPreviewKeyDown);
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

        public async void RemoveSortFolder(SortFolder sortFolder)
        {
            // Message dialog on what to do with un-transfered files
            if (0 < sortFolder?.SortSrcFiles.Count())
            {
                DialogResult result = await OpenRemoveSortFolderDialog();
                switch (result)
                {
                    // Add files back into the queue
                    case DialogResult.Queue:
                        List<FileQueueItem> fileItems = new List<FileQueueItem>();
                        foreach (string fileItem in sortFolder?.SortSrcFiles)
                        {
                            if (File.Exists(fileItem))
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
                        }
                        FileQueue.EnqueueRange(fileItems);

                        // Dequeue the first item
                        if (CurrentFileQueueItem?.Name == null && 0 < FileQueue.Count())
                        {
                            CurrentFileQueueItem = FileQueue.Dequeue();
                            int THUMB_SIZE = 256;
                            Bitmap thumbnail = Helpers.WindowsThumbnailProvider.GetThumbnail(
                               CurrentFileQueueItem.FullName, THUMB_SIZE, THUMB_SIZE, Helpers.ThumbnailOptions.None);
                            CurrentFileQueueItem.BigImage = ConvertBitmap(thumbnail);
                        }
                        break;

                    // Remove the SortFolder and shortcut
                    case DialogResult.OK:
                        DetachShortcut(mHostWindow);
                        SortFolderQueue.GetCollection()?.Remove(sortFolder);
                        break;

                    // Cancel the operation
                    case DialogResult.Cancel:
                        return;
                    default:
                        break;
                }
            }

            // Remove the SortFolder and shortcut
            DetachShortcut(mHostWindow);
            SortFolderQueue.GetCollection()?.Remove(sortFolder);
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
                string prevShortcutLabel = sortFolder.ShortcutLabel;
                sortFolder.ShortcutLabel = "            ";
                EditShortcutButtonsEnabled = false;

                // Get currently held keys as KeyGesture
                HashSet<Key> userShortcut = await Task.Run(() => Keyboard.GetUserShortcut());
                KeyGesture convertedGesture = HashSetKeysToGesture(userShortcut);

                // Make sure shortcut is not already being used
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

                if (convertedGesture != null)
                {
                    AttachShortcut(mHostWindow);
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
    }
}
