using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualFileSorter.Helpers
{
    public class UndoRedoItem
    {
        public UndoRedoItem(string srcFilename, string sortFolderName)
        {
            mSrcFilename = srcFilename;
            mSortFolderName = sortFolderName;
        }

        public string mSrcFilename = string.Empty;
        public string mSortFolderName = string.Empty;
    }
}
