// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Poe.UIW.ViewModels;
using Poe.UIW.Views.Pages;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.TaskBar;

namespace Poe.UIW.Views;

/// <summary>
/// Interaction logic for Container.xaml
/// </summary>
public partial class ContainerView : INavigationWindow
{
    private bool _initialized = false;

    private readonly IThemeService _themeService;


    public ContainerViewModel ViewModel { get; }

    public ContainerView()
    {
    }

    public ContainerView(
        ContainerViewModel viewModel,
        INavigationService navigationService,
        IPageService pageService,
        IThemeService themeService,
            Wpf.Ui.Mvvm.Contracts.IDialogService dialogService
    )
    {
        ViewModel = viewModel;
        DataContext = this;

        _themeService = themeService;

        InitializeComponent();

        SetPageService(pageService);

        navigationService.SetNavigationControl(RootNavigation);
        dialogService.SetDialogControl(RootDialogYesNo);

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                RootMainGrid.Visibility = Visibility.Visible;

                Navigate(typeof(Pages.LiveSearch));
            });
        });
    }

    private static void DialogControlOnButtonRightClick(object sender, RoutedEventArgs e)
    {
        var dialogControl = (IDialogControl)sender;
        dialogControl.Hide();
    }


    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    #region INavigationWindow methods

    public Frame GetFrame() => RootFrame;

    public INavigation GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) =>
        RootNavigation.PageService = pageService;

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    #endregion INavigationWindow methods

    private void InvokeSplashScreen()
    {
        if (_initialized)
            return;

        _initialized = true;

        RootMainGrid.Visibility = Visibility.Collapsed;


        Task.Run(async () =>
        {
            // Remember to always include Delays and Sleeps in
            // your applications to be able to charge the client for optimizations later.
            await Task.Delay(4000);

            await Dispatcher.InvokeAsync(() =>
            {
                RootMainGrid.Visibility = Visibility.Visible;

                Navigate(typeof(Settings));

            });

            return true;
        });
    }

    private void NavigationButtonTheme_OnClick(object sender, RoutedEventArgs e)
    {
        _themeService.SetTheme(
            _themeService.GetTheme() == ThemeType.Dark ? ThemeType.Light : ThemeType.Dark
        );
    }
    
    private void MenuItem_OnClick_Open(object sender, RoutedEventArgs e)
    {
        Show();
        Activate();
    }

    private void MenuItem_OnClick_Close(object sender, RoutedEventArgs e)
    {
        Close();
        OnClosing(new CancelEventArgs());
    }

    private void TrayMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
            return;

        System.Diagnostics.Debug.WriteLine(
            $"DEBUG | WPF UI Tray clicked: {menuItem.Tag}",
            "Wpf.Ui.Demo"
        );
    }

    private void RootNavigation_OnNavigated(INavigation sender, RoutedNavigationEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine(
            $"DEBUG | WPF UI Navigated to: {sender?.Current ?? null}",
            "Wpf.Ui.Demo"
        );

        // This funky solution allows us to impose a negative
        // margin for Frame only for the Dashboard page, thanks
        // to which the banner will cover the entire page nicely.
        // RootFrame.Margin = new Thickness(
        //     left: 0,
        //     top: sender?.Current?.PageTag == "liveSearch" ? -69 : 0,
        //     right: 0,
        //     bottom: 0
        // );
    }
}
