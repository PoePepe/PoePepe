using System;
using System.Threading;
using System.Windows;
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
        private readonly CancellationTokenSource _applicationCancelSource = new();

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
            var historyChannelWorker = Services.GetRequiredService<HistoryChannelWorker>();
            var orderStartSearchWorker = Services.GetRequiredService<OrderStartSearchWorker>();
            liveSearchChannelWorker.Start(_applicationCancelSource.Token);
            foundChannelWorker.Start(_applicationCancelSource.Token);
            whisperChannelWorker.Start(_applicationCancelSource.Token);
            historyChannelWorker.Start(_applicationCancelSource.Token);
            orderStartSearchWorker.Start(_applicationCancelSource.Token);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _applicationCancelSource.Cancel();

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
                .Register<ExportOrdersViewModel, ExportOrdersView>()
                .Register<ImportOrdersViewModel, ImportOrdersView>()
                .Register<OrderItemInfoViewModel, OrderItemInfoView>();
            
            services.AddSingleton<IDialogService>(sp => new DialogService(new DialogManager(locator,
                new DialogFactory()), sp.GetService));
            services.AddScoped<WhisperService>();

            services.AddSingleton<LeagueService>();
            services.AddSingleton<ApplicationHostService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<ITaskBarService, TaskBarService>();
            services.AddSingleton<Wpf.Ui.Mvvm.Contracts.IDialogService, Wpf.Ui.Mvvm.Services.DialogService>();

            services.AddSingleton<INavigationService, NavigationService>();

            services.AddScoped<INavigationWindow, ContainerView>();
            services.AddScoped<ContainerViewModel>();
            
            services.AddSingleton<LiveSearchViewModel>();
            services.AddSingleton<Views.Pages.LiveSearch>();
            
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<Views.Pages.Settings>();

            services.AddSingleton<SoundService>();

            services.AddSingleton<ResourceDownloadService>();
            services.AddHttpClient<ResourceDownloadService>();
        
            services.AddTransient<ManageOrderViewModel>();
            services.AddTransient<ManageOrderView>();

            services.AddTransient<ExportOrdersViewModel>();
            services.AddTransient<ExportOrdersView>();

            services.AddTransient<ImportOrdersViewModel>();
            services.AddTransient<ImportOrdersView>();

            services.AddSingleton<AlwaysOnTopView>();
            services.AddSingleton<AlwaysOnTopViewModel>();
            
            services.AddTransient<OrderItemInfoView>();
            services.AddTransient<OrderItemInfoViewModel>();

            services.AddTransient<OrderHistoryView>();
            services.AddTransient<OrderHistoryViewModel>();
            
            services.AddTransient<OrderItemNotificationViewModel>();

            return services.BuildServiceProvider();
        }
    }
}