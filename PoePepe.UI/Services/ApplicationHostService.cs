using System;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PoePepe.LiveSearch.Services;
using PoePepe.UI.Properties;
using PoePepe.UI.Views;
using Wpf.Ui.Mvvm.Contracts;

namespace PoePepe.UI.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
public class ApplicationHostService
{
    private readonly INavigationService _navigationService;
    private readonly IPageService _pageService;
    private readonly IServiceProvider _serviceProvider;
    private AlwaysOnTopView _alwaysOnTopView;

    private INavigationWindow _navigationWindow;

    public ApplicationHostService(
        IServiceProvider serviceProvider,
        INavigationService navigationService,
        IPageService pageService)
    {
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        _pageService = pageService;
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
        serviceState.LeagueName = UserSettings.Default.LeagueName;

        if (!string.IsNullOrEmpty(UserSettings.Default.Session))
        {
            serviceState.Session = UserSettings.Default.Session;

            var leagueService = App.Current.Services.GetRequiredService<LeagueService>();
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                await leagueService.LoadActualLeagueNamesAsync(cancellationToken);
            });
        }
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
    }

    private void PrepareNavigation()
    {
        _navigationService.SetPageService(_pageService);
    }
}
