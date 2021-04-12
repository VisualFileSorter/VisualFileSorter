# Visual File Sorter

Visual File Sorter is a Windows utility program to quickly and visually sort files.

![Operation GIF](../master/VFS_Anim.gif?raw=true)


# Download
[Download 64-bit](https://github.com/VisualFileSorter/VisualFileSorter/releases/download/1.1/VisualFileSorter_1.1_x64.zip)\
[Download 32-bit](https://github.com/VisualFileSorter/VisualFileSorter/releases/download/1.1/VisualFileSorter_1.1_x86.zip)

# Cross-platform Support
Although Avalonia is cross-platform, Visual File Sorter makes use of the following Win32 functionality:
- Shell File Transfer {SHFileOperation}
- Shell Open File {ShellExecute}
- Shell Thumbnails {SHCreateItemFromParsingName, DeleteObject, memcpy}

I have no plans to make this program cross-platform, but feel free to either fork this repository or submit a pull request.

# Dependencies

[Avalonia](https://github.com/AvaloniaUI/Avalonia)\
[Avalonia Behaviors](https://github.com/wieslawsoltes/AvaloniaBehaviors)\
[Avalonia Custom TitleBar Template](https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate)\
[ReactiveUI](https://github.com/reactiveui/ReactiveUI)

# Special Thanks
A big special thanks to @maxkatz6 on Gitter for teaching me how to [bind commands](https://github.com/VisualFileSorter/VisualFileSorter/blob/a87fe0218199feacff2ccb3532fec316c91b50db/VisualFileSorter/Views/MainWindow.axaml#L466) from a ListBox template to the MainWindowViewModel.

# Possible Enhancements
- Undo/Redo: Currently undo/redo will only undo/redo sorting of files. Undoing removal of Sort Folders and transferring of files is currently unsupported.
- A.I. Sort: Train a model with your visual sorting
- SortFolder file visibility: Double clicking on a sort folder would show you the current un-transferred files
- Non-Shell file transfer: The shell file transfer is used because I did not want to write a name conflict resolver. A custom name conflict resolver would help enable cross-platform support.
- Adhering to Proper MVVM: All logic is currently done in the ViewModel.
