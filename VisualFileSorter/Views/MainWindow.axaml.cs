using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;

using VisualFileSorter.Helpers;
using VisualFileSorter.ViewModels;

namespace VisualFileSorter.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private bool isDefaultStyle = false;
        private bool isDarkTheme = false;
        public MainWindow()
        {
            this.InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif

            // Do not use a custom title bar on Linux, because there are too many possible options.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) == true)
            {
                UseNativeTitleBar();
            }

            this.DataContext = new MainWindowViewModel(this);

            this.Height = 546;
            this.Width = 970;
            this.Padding = new Thickness(
                            this.OffScreenMargin.Left,
                            this.OffScreenMargin.Top,
                            this.OffScreenMargin.Right,
                            this.OffScreenMargin.Bottom);

            Application.Current.Styles[1] = App.FluentLight;
            Application.Current.Resources["WindowsTitleBarBackground"] = new SolidColorBrush { Color = new Color(255, 204, 213, 240) };
            Application.Current.Resources["ButtonForeground"] = new SolidColorBrush { Color = new Color(255, 255, 255, 255) };
            Application.Current.Resources["ButtonForegroundPointerOver"] = new SolidColorBrush { Color = new Color(255, 255, 255, 255) };
            Application.Current.Resources["ButtonForegroundPressed"] = new SolidColorBrush { Color = new Color(255, 255, 255, 255) };

            this.WhenActivated(d => d(ViewModel.ShowDialog.RegisterHandler(DoShowDialogAsync)));
        }

        private async Task DoShowDialogAsync(InteractionContext<MessageWindowViewModel, DialogResultViewModel?> interaction)
        {
            var dialog = new MessageWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<DialogResultViewModel?>(this);
            interaction.SetOutput(result);
        }

        private void UseNativeTitleBar()
        {
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            ExtendClientAreaToDecorationsHint = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Keyboard.Keys.Add(e.Key);
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            Keyboard.Keys.Remove(e.Key);
            base.OnKeyUp(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
