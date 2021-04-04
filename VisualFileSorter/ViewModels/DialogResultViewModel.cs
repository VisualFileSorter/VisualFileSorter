using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualFileSorter.ViewModels
{
    public enum DialogResult
    {
        Queue,
        OK,
        Cancel
    }

    public class DialogResultViewModel : ViewModelBase
    {
        public DialogResult DiaResult
        {
            get => mDiaResult;
            set => this.RaiseAndSetIfChanged(ref mDiaResult, value);
        }

        private DialogResult mDiaResult = DialogResult.Cancel;
    }
}
