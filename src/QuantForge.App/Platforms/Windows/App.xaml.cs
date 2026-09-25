using Microsoft.Maui;

namespace QuantForge.App.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => QuantForge.App.MauiProgram.CreateMauiApp();
}
