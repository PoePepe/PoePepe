using System;
using System.Threading;
using System.Windows;
using HanumanInstitute.MvvmDialogs.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoePepe.LiveSearch.DependencyInjection;
using PoePepe.LiveSearch.Services;
using PoePepe.UI.Services;
using PoePepe.UI.ViewModels;
using PoePepe.UI.ViewModels.OrderItemInfoViewModels;
using PoePepe.UI.Views;
using PoePepe.UI.Views.OrderItemInfoViews;
using PoePepe.UI.Views.Pages;
using Serilog;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using DialogService = HanumanInstitute.MvvmDialogs.Wpf.DialogService;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace PoePepe.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly CancellationTokenSource _applicationCancelSource = new();

        public App()
        {
            Services = ConfigureServices();
        }

        public IServiceProvider Services { get; }
        public new static App Current => (App)Application.Current;

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
            
            var alwaysOnTopViewModel = Services.GetRequiredService<AlwaysOnTopViewModel>();
            alwaysOnTopViewModel.Start(_applicationCancelSource.Token);
            var workerManager = Services.GetRequiredService<WorkerManager>();
            workerManager.StartWorkers(_applicationCancelSource.Token);

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
                .AddJsonFile("appsettings.json", false, true)
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
            services.AddTransient<Settings>();

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