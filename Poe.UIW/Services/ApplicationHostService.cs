using System;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Poe.LiveSearch.Services;
using Poe.UIW.Properties;
using Poe.UIW.Views;
using Wpf.Ui.Mvvm.Contracts;

namespace Poe.UIW.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
public class ApplicationHostService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly IPageService _pageService;
    private readonly IThemeService _themeService;
    private readonly ITaskBarService _taskBarService;

    private INavigationWindow _navigationWindow;
    private AlwaysOnTopView _alwaysOnTopView;

    public ApplicationHostService(
        IServiceProvider serviceProvider,
        INavigationService navigationService,
        IPageService pageService,
        IThemeService themeService,
        ITaskBarService taskBarService
    )
    {
        // If you want, you can do something with these services at the beginning of loading the application.
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        _pageService = pageService;
        _themeService = themeService;
        _taskBarService = taskBarService;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public void Start(CancellationToken cancellationToken = default)
    {
        PrepareNavigation();

        ActivateNavigationContainer();
        
        _alwaysOnTopView = _serviceProvider.GetRequiredService<AlwaysOnTopView>();
        _alwaysOnTopView.Show();

        var serviceState = App.Current.Services.GetRequiredService<ServiceState>();
        serviceState.Session = UserSettings.Default.Session;

        var leagueService = App.Current.Services.GetRequiredService<LeagueService>();
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            await leagueService.LoadActualLeagueNamesAsync(cancellationToken);
        });
    }
    
    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public void Stop(CancellationToken cancellationToken = default)
    {
        _alwaysOnTopView.Close();
    }

    /// <summary>
    /// Creates main window during activation.
    /// </summary>
    private void ActivateNavigationContainer()
    {
        if (!Application.Current.Windows.OfType<ContainerView>().Any())
        {
            _navigationWindow = _serviceProvider.GetService<INavigationWindow>();
            _navigationWindow!.ShowWindow();
        }

        // var notifyIconManager = _serviceProvider.GetService<INotifyIconService>();
        //
        // if (!notifyIconManager!.IsRegistered)
        // {
        //     notifyIconManager!.SetParentWindow(_navigationWindow as Window);
        //     notifyIconManager.Register();
        // }
    }

    private void PrepareNavigation()
    {
        _navigationService.SetPageService(_pageService);
    }
}
