using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using HanumanInstitute.MvvmDialogs.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poe.LiveSearch.DependencyInjection;
using Poe.LiveSearch.Services;
using Poe.UIW.Services;
using Poe.UIW.ViewModels;
using Poe.UIW.ViewModels.OrderItemInfoViewModels;
using Poe.UIW.Views;
using Poe.UIW.Views.OrderItemInfoViews;
using Serilog;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using DialogService = HanumanInstitute.MvvmDialogs.Wpf.DialogService;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace Poe.UIW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider Services { get; }
        public new static App Current => (App)Application.Current;

        public App()
        {
            Services = ConfigureServices();
        }
        
        
        
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("log_.txt", outputTemplate:
                    "[{Timestamp:HH:mm:ss.FFF} {Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval:RollingInterval.Day)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss.FFF} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            
            var appService = Services.GetRequiredService<ApplicationHostService>();
            appService.Start();
            
            var liveSearchChannelWorker = Services.GetRequiredService<LiveSearchChannelWorker>();
            var foundChannelWorker = Services.GetRequiredService<FoundChannelWorker>();
            var whisperChannelWorker = Services.GetRequiredService<WhisperChannelWorker>();
            liveSearchChannelWorker.Start(CancellationToken.None);
            foundChannelWorker.Start(CancellationToken.None);
            whisperChannelWorker.Start(CancellationToken.None);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var appService = Services.GetRequiredService<ApplicationHostService>();
            appService.Stop();
            
            base.OnExit(e);
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddServices(configuration);
        
            var locator = new StrongViewLocator()
                .Register<ContainerViewModel, ContainerView>()
                .Register<ManageOrderViewModel, ManageOrderView>()
                .Register<OrderItemInfoViewModel, OrderItemInfoView>();
            
            services.AddSingleton<IDialogService>(sp => new DialogService(new DialogManager(locator,
                new DialogFactory()), sp.GetService));
            services.AddScoped<WhisperService>();

            services.AddSingleton<ApplicationHostService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ITaskBarService, TaskBarService>();
            services.AddSingleton<Wpf.Ui.Mvvm.Contracts.IDialogService, Wpf.Ui.Mvvm.Services.DialogService>();

                            // services.AddSingleton<INotifyIconService, CustomNotifyIconService>();

            services.AddSingleton<INavigationService, Wpf.Ui.Mvvm.Services.NavigationService>();

            services.AddScoped<INavigationWindow, Views.ContainerView>();
            services.AddScoped<ContainerViewModel>();
            
            services.AddSingleton<LiveSearchViewModel>();
            services.AddSingleton<Views.Pages.LiveSearch>();
            
            services.AddSingleton<SettingsViewModel>();
            services.AddScoped<Views.Pages.Settings>();


            services.AddSingleton<ResourceDownloadService>();
            services.AddHttpClient<ResourceDownloadService>();
        
            services.AddTransient<ManageOrderViewModel>();
            services.AddTransient<ManageOrderView>();
            
            // services.AddSingleton<MainView>();
            // services.AddSingleton<MainViewModel>();
            
            services.AddSingleton<AlwaysOnTopView>();
            services.AddSingleton<AlwaysOnTopViewModel>();
            
            services.AddTransient<OrderItemInfoView>();
            services.AddTransient<OrderItemInfoViewModel>();

            
            services.AddTransient<OrderItemNotificationViewModel>();
            services.AddTransient<DivinationCardInfoViewModel>();
            services.AddTransient<StackedItemInfoViewModel>();


            return services.BuildServiceProvider();
        }
    }
}