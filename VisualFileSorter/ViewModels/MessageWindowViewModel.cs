using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace VisualFileSorter.ViewModels
{
    public class MessageWindowViewModel : ViewModelBase
    {
        public MessageWindowViewModel()
        {
            QueueCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                DialogResultViewModel retDiaResultVM = new DialogResultViewModel();
                retDiaResultVM.DiaResult = DialogResult.Queue;
                return retDiaResultVM;
            });

            OkCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                DialogResultViewModel retDiaResultVM = new DialogResultViewModel();
                retDiaResultVM.DiaResult = DialogResult.OK;
                return retDiaResultVM;
            });

            CancelCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                DialogResultViewModel retDiaResultVM = new DialogResultViewModel();
                retDiaResultVM.DiaResult = DialogResult.Cancel;
                return retDiaResultVM;
            });
        }

        public ReactiveCommand<Unit, DialogResultViewModel> QueueCmd { get; }
        public ReactiveCommand<Unit, DialogResultViewModel> OkCmd { get; }
        public ReactiveCommand<Unit, DialogResultViewModel> CancelCmd { get; }

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

        public bool MB_MissingFilesVisible
        {
            get => mMB_MissingFilesVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_MissingFilesVisible, value);
        }

        public bool MB_BadFileTransferVisible
        {
            get => mMB_BadFileTransferVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_BadFileTransferVisible, value);
        }

        public bool MB_ImportFilesAlreadyInVisible
        {
            get => mMB_ImportFilesAlreadyInVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_ImportFilesAlreadyInVisible, value);
        }

        public bool MB_ShortcutAlreadyExistsVisible
        {
            get => mMB_ShortcutAlreadyExistsVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_ShortcutAlreadyExistsVisible, value);
        }

        public bool MB_ReplaceSessionVisible
        {
            get => mMB_ReplaceSessionVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_ReplaceSessionVisible, value);
        }

        public bool MB_OpenSaveSessionErrorVisible
        {
            get => mMB_OpenSaveSessionErrorVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_OpenSaveSessionErrorVisible, value);
        }

        public string MB_ImportFilesAlreadyInList
        {
            get => mMB_ImportFilesAlreadyInList;
            set => this.RaiseAndSetIfChanged(ref mMB_ImportFilesAlreadyInList, value);
        }

        public string MB_MissingFilesMsg
        {
            get => mMB_MissingFilesMsg;
            set => this.RaiseAndSetIfChanged(ref mMB_MissingFilesMsg, value);
        }

        public bool MB_MissingFilesCancelVisible
        {
            get => mMB_MissingFilesCancelVisible;
            set => this.RaiseAndSetIfChanged(ref mMB_MissingFilesCancelVisible, value);
        }

        public string MB_MissingFilesList
        {
            get => mMB_MissingFilesList;
            set => this.RaiseAndSetIfChanged(ref mMB_MissingFilesList, value);
        }

        public string MB_OpenSaveSessionErrorMsg
        {
            get => mMB_OpenSaveSessionErrorMsg;
            set => this.RaiseAndSetIfChanged(ref mMB_OpenSaveSessionErrorMsg, value);
        }

        private string mMB_ImportFilesAlreadyInList = String.Empty;
        private string mMB_MissingFilesList = String.Empty;
        private string mMB_MissingFilesMsg = String.Empty;
        private string mMB_OpenSaveSessionErrorMsg = String.Empty;
        private string mMessageWindowTitle = String.Empty;
        private int mMessageWindowWidth = 300;
        private int mMessageWindowHeight = 120;
        private bool mMessageWindowErrorIcon = false;
        private bool mMessageWindowWarningIcon = false;
        private bool mMessageWindowInfoIcon = false;
        private bool mMB_MissingSortFolderVisible = false;
        private bool mMB_RemoveSortFolderVisible = false;
        private bool mMB_MissingFilesVisible = false;
        private bool mMB_BadFileTransferVisible = false;
        private bool mMB_ImportFilesAlreadyInVisible = false;
        private bool mMB_ShortcutAlreadyExistsVisible = false;
        private bool mMB_ReplaceSessionVisible = false;
        private bool mMB_OpenSaveSessionErrorVisible = false;
        private bool mMB_MissingFilesCancelVisible = true;

        #endregion Message Window Properties
    }
}
