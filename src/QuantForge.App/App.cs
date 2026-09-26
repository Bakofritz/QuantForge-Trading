using Microsoft.Maui;
using Microsoft.Maui.Controls;
using QuantForge.Core;

namespace QuantForge.App;

public sealed class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage());
        window.Activated += (_, _) => AppDiagnostics.Record(DiagnosticAction.WindowActivated);
        window.Deactivated += (_, _) => AppDiagnostics.Record(DiagnosticAction.WindowDeactivated);
        window.Stopped += (_, _) => AppDiagnostics.Record(DiagnosticAction.WindowStopped);
        window.Resumed += (_, _) => AppDiagnostics.Record(DiagnosticAction.WindowResumed);
        return window;
    }
}
