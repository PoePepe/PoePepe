using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PoePepe.UI.Properties;
using PoePepe.UI.ViewModels;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace PoePepe.UI.Views;

public partial class ContainerView : INavigationWindow
{
    private bool _isTrayClose;

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
        Closing += OnClosing;
        IsVisibleChanged += OnIsVisibleChanged;
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
                RootMainGrid.Visibility = Visibility.Visible;

                Navigate(typeof(Pages.LiveSearch));
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
}