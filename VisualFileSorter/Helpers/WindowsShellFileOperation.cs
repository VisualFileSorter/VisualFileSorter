using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

//using Microsoft.Windows.Sdk;

namespace VisualFileSorter.Helpers
{
    // TODO Handle case where if a source file is missing then the whole operation fails
    public static class WindowsShellFileOperation
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FO_Func wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public FILEOP_FLAGS fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        private enum FO_Func : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004
        }

        [Flags]
        private enum FILEOP_FLAGS : ushort
        {
            FOF_MULTIDESTFILES = 0x1,
            FOF_CONFIRMMOUSE = 0x2,
            FOF_SILENT = 0x4,
            FOF_RENAMEONCOLLISION = 0x8,
            FOF_NOCONFIRMATION = 0x10,
            FOF_WANTMAPPINGHANDLE = 0x20,
            FOF_ALLOWUNDO = 0x40,
            FOF_FILESONLY = 0x80,
            FOF_SIMPLEPROGRESS = 0x100,
            FOF_NOCONFIRMMKDIR = 0x200,
            FOF_NOERRORUI = 0x400,
            FOF_NOCOPYSECURITYATTRIBS = 0x800,
            FOF_NORECURSION = 0x1000,
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,
            FOF_WANTNUKEWARNING = 0x4000,
            FOF_NORECURSEREPARSE = 0x8000
        }

        public static bool TransferFiles(List<string> src, List<string> dest, bool isMove = false)
        {
            return TransferFiles(MergeFilenames(src), MergeFilenames(dest), isMove);
        }

        private static unsafe bool TransferFiles(string src, string dest, bool isMove)
        {
            bool success = false;

            SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT();
            fileOp.hwnd = IntPtr.Zero;// new HWND(0);
            fileOp.hNameMappings = IntPtr.Zero;// null;
            fileOp.fFlags = FILEOP_FLAGS.FOF_NORECURSION |
                            FILEOP_FLAGS.FOF_NOCONFIRMMKDIR |
                            FILEOP_FLAGS.FOF_MULTIDESTFILES;
            fileOp.fAnyOperationsAborted = false;

            //fixed (char* cSrc = src)
            //{
            fileOp.pFrom = src; //new PCWSTR(cSrc);
                                //}

            //fixed (char* cDest = dest)
            //{
            fileOp.pTo = dest; //new PCWSTR(cDest);
            //}

            if (isMove)
            {
                fileOp.wFunc = FO_Func.FO_MOVE;
                //fixed (char* cTitle = "Moving files")
                //{
                fileOp.lpszProgressTitle = "Moving files"; //new PCWSTR(cTitle);
                //}
            }
            else
            {
                fileOp.wFunc = FO_Func.FO_COPY;
                //fixed (char* cTitle = "Copying files")
                //{
                fileOp.lpszProgressTitle = "Copying files"; //new PCWSTR(cTitle);
                //}
            }

            // Do file operation and check success
            int result = SHFileOperation(ref fileOp);
            if (result == 0)
            {
                success = !fileOp.fAnyOperationsAborted;
            }

            return success;
        }

        private static string MergeFilenames(List<string> filenames)
        {
            StringBuilder result = new StringBuilder();
            foreach (string file in filenames)
            {
                // Separate files with null termination
                result.Append(file);
                result.Append("\0");
            }

            // Double null termination
            result.Append("\0");     

            return result.ToString();
        }
    }
}
