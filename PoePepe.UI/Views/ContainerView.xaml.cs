﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PoePepe.UI.Properties;
using PoePepe.UI.Services;
using PoePepe.UI.ViewModels;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace PoePepe.UI.Views;

public partial class ContainerView : INavigationWindow
{
    private readonly ISnackbarService _snackbarService;
    private bool _isTrayClose;

    public ContainerView()
    {
    }

    public ContainerView(ContainerViewModel viewModel,
        INavigationService navigationService,
        IPageService pageService,
        IDialogService dialogService,
        ISnackbarService snackbarService, LeagueService leagueService)
    {
        _snackbarService = snackbarService;
        ViewModel = viewModel;
        DataContext = this;
        leagueService.LeagueNamesLoadFailed += LeagueNamesLoadFailed;

        InitializeComponent();

        SetPageService(pageService);
        _snackbarService.SetSnackbarControl(RootSnackbar);

        navigationService.SetNavigationControl(RootNavigation);
        dialogService.SetDialogControl(RootDialogYesNo);

        Loaded += OnLoaded;
        Closing += OnClosing;
        IsVisibleChanged += OnIsVisibleChanged;
    }

    private void LeagueNamesLoadFailed(object sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _snackbarService.Show(
                "Update your Poe session id!",
                "",
                SymbolRegular.Alert24,
                ControlAppearance.Caution
            );

            Navigate(typeof(Pages.Settings));
        });
    }

    public ContainerViewModel ViewModel { get; }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue && !(bool)e.OldValue)
        {
            ShowInTaskbar = true;
        }
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        if (_isTrayClose)
        {
            return;
        }

        e.Cancel = true;
        Hide();
        ShowInTaskbar = false;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AlertNavigationItem.Content = UserSettings.Default.PlayNotificationSound ? "Sound on" : "Sound off";
        AlertNavigationItem.Icon = UserSettings.Default.PlayNotificationSound
            ? SymbolRegular.Alert24
            : SymbolRegular.AlertOff24;

        Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                var startViewType = typeof(Pages.LiveSearch);
                if (string.IsNullOrEmpty(UserSettings.Default.Session))
                {
                    _snackbarService.Show(
                        "Update your Poe session id!",
                        "",
                        SymbolRegular.Alert24,
                        ControlAppearance.Caution
                    );

                    startViewType = typeof(Pages.Settings);
                }

                RootMainGrid.Visibility = Visibility.Visible;

                Navigate(startViewType);
            });
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    private void MenuItem_OnClick_Open(object sender, RoutedEventArgs e)
    {
        Show();
        Activate();
        ShowInTaskbar = true;
    }

    private void MenuItem_OnClick_Close(object sender, RoutedEventArgs e)
    {
        _isTrayClose = true;
        Close();
        OnClosing(new CancelEventArgs());
    }

    private void NavigationButtonAlert_OnClick(object sender, RoutedEventArgs e)
    {
        UserSettings.Default.PlayNotificationSound = !UserSettings.Default.PlayNotificationSound;

        AlertNavigationItem.Content = UserSettings.Default.PlayNotificationSound ? "Sound on" : "Sound off";
        AlertNavigationItem.Icon = UserSettings.Default.PlayNotificationSound
            ? SymbolRegular.Alert24
            : SymbolRegular.AlertOff24;

        UserSettings.Default.Save();
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

    private void ButtonPatreon_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.patreon.com/PoePepe",
            UseShellExecute = true
        });
        e.Handled = true;
    }

    private void ButtonDiscord_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://discord.com/invite/enMrbEZb",
            UseShellExecute = true
        });
        e.Handled = true;
    }
}