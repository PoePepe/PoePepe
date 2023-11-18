using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poe.LiveSearch.DependencyInjection;
using Poe.LiveSearch.Services;
using Poe.UI.Services;
using Poe.UI.ViewModels;
using Poe.UI.ViewModels.OrderItemInfoViewModels;
using Poe.UI.Views;
using Serilog;

namespace Poe.UI;

public partial class App : Application
{
    public IServiceProvider Services { get; }
    public new static App Current => (App)Application.Current;


    public App()
    {
        Services = ConfigureServices();
    }
    

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
        
        GC.KeepAlive(typeof(DialogService));
        // var dialogService = Services.GetRequiredService<IDialogService>();
        // var mainViewModel = Services.GetRequiredService<MainViewModel>();
        // dialogService.Show(null, mainViewModel);
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // desktop.MainWindow = new MainView
            // {
            //     DataContext = Services.GetRequiredService<MainViewModel>(),
            // };
            
            desktop.MainWindow = new AlwaysOnTopView
            {
                DataContext = Services.GetRequiredService<AlwaysOnTopViewModel>(),
            };
            
            desktop.MainWindow.Closing += (_, _) => { Log.CloseAndFlush();};
        }
        
        var liveSearchChannelWorker = Services.GetRequiredService<LiveSearchChannelWorker>();
        var foundChannelWorker = Services.GetRequiredService<FoundChannelWorker>();
        var whisperChannelWorker = Services.GetRequiredService<WhisperChannelWorker>();
        liveSearchChannelWorker.Start(CancellationToken.None);
        foundChannelWorker.Start(CancellationToken.None);
        whisperChannelWorker.Start(CancellationToken.None);

        base.OnFrameworkInitializationCompleted();
    }
    
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddServices(configuration);
        
        services.AddSingleton<IDialogService>(sp => new DialogService(new DialogManager(new ModalViewLocator(),
            new DialogFactory().AddDialogHost().AddMessageBox()), sp.GetService));
        services.AddScoped<WhisperService>();

        services.AddSingleton<ResourceDownloadService>();
        services.AddHttpClient<ResourceDownloadService>();
        
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<AlwaysOnTopViewModel>();
        services.AddTransient<ManageOrderViewModel>();
        services.AddTransient<OrderItemNotificationViewModel>();
        services.AddTransient<OrderItemInfoViewModel>();
        services.AddTransient<DivinationCardInfoViewModel>();
        services.AddTransient<MapInfoViewModel>();
        services.AddTransient<StackedItemInfoViewModel>();


        return services.BuildServiceProvider();
    }
}