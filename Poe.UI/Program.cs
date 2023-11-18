using Avalonia;
using System;
using Serilog;

namespace Poe.UI;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("log_.txt", outputTemplate:
                "[{Timestamp:HH:mm:ss.FFF} {Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval:RollingInterval.Day)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss.FFF} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}