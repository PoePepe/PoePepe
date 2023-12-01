// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Poe.UIW.Properties;
using Poe.UIW.ViewModels;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace Poe.UIW.Views;

/// <summary>
/// Interaction logic for Container.xaml
/// </summary>
public partial class ContainerView : INavigationWindow
{
    public ContainerViewModel ViewModel { get; }

    public ContainerView()
    {
    }

    public ContainerView(ContainerViewModel viewModel,
        INavigationService navigationService,
        IPageService pageService,
        IDialogService dialogService,
        ISnackbarService snackbarService)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        SetPageService(pageService);
        snackbarService.SetSnackbarControl(RootSnackbar);

        navigationService.SetNavigationControl(RootNavigation);
        dialogService.SetDialogControl(RootDialogYesNo);

        Loaded += OnLoaded;
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
                RootMainGrid.Visibility = Visibility.Visible;

                Navigate(typeof(Pages.LiveSearch));
            });
        });
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

    private void NavigationButtonAlert_OnClick(object sender, RoutedEventArgs e)
    {
        UserSettings.Default.PlayNotificationSound = !UserSettings.Default.PlayNotificationSound;

        AlertNavigationItem.Content = UserSettings.Default.PlayNotificationSound ? "Sound on" : "Sound off";
        AlertNavigationItem.Icon = UserSettings.Default.PlayNotificationSound
            ? SymbolRegular.Alert24
            : SymbolRegular.AlertOff24;

        UserSettings.Default.Save();
    }
}