using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace Poe.UI;

public static class AppBuilderNotificationsExtensions
{
    // public static AppBuilder AfterSetupDesktopNotifications(this AppBuilder builder)
    // {
    //     builder.AfterSetup(b =>
    //     {
    //         if (!(b.Instance.ApplicationLifetime is IControlledApplicationLifetime applicationLifetime2))
    //         {
    //             return;
    //         }
    //
    //         // var notificationManager = App.Current.Services.GetRequiredService<INotificationManager>();
    //         //
    //         // applicationLifetime2.Exit +=
    //         //     (EventHandler<ControlledApplicationLifetimeExitEventArgs>)((s, e) =>
    //         //         ((IDisposable)notificationManager).Dispose());
    //     });
    //
    //     return builder;
    // }
    
    // public static ServiceCollection SetupDesktopNotifications(this ServiceCollection services)
    // {
    //     if (!OperatingSystem.IsWindows())
    //     {
    //         return services;
    //     }
    //     
    //     var context = WindowsApplicationContext.FromCurrentProcess();
    //     var manager = new WindowsNotificationManager();
    //     
    //     manager.Initialize().GetAwaiter().GetResult();
    //
    //     services.AddSingleton<INotificationManager, WindowsNotificationManager>(_ => manager);
    //     
    //     return services;
    // }
}