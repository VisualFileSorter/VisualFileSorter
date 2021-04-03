using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using VisualFileSorter.ViewModels;

namespace VisualFileSorter.Views
{
    public class MessageWindow : Window
    {
        public MessageWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            // Do not use a custom title bar on Linux, because there are too many possible options.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) == true)
            {
                UseNativeTitleBar();
            }

            this.Padding = new Thickness(
                            this.OffScreenMargin.Left,
                            this.OffScreenMargin.Top,
                            this.OffScreenMargin.Right,
                            this.OffScreenMargin.Bottom);
        }


        private void UseNativeTitleBar()
        {
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaToDecorationsHint = false;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
