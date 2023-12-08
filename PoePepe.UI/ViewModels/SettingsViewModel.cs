using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using PoePepe.LiveSearch.Api.Trade;
using PoePepe.LiveSearch.Services;
using PoePepe.UI.Models;
using PoePepe.UI.Properties;
using PoePepe.UI.Services;
using PoePepe.UI.Views;
using Serilog;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace PoePepe.UI.ViewModels;

public partial class SettingsViewModel : ViewModelValidatableBase
{
    private readonly IDialogService _dialogService;
    private readonly PoeTradeApiService _poeTradeApiService;

    private readonly ServiceState _serviceState;
    private readonly LeagueService _leagueService;
    private readonly ISnackbarService _snackbarService;
    private readonly SoundService _soundService;

    [ObservableProperty] private Sound _currentSound;

    [ObservableProperty] private string _customSoundName;

    [ObservableProperty] private string _customSoundPath;

    [ObservableProperty] private ObservableCollection<Sound> _defaultSoundNames;

    [ObservableProperty] private bool _isHide;

    [ObservableProperty] private bool _playNotificationSound;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [RegularExpression("^[a-z0-9]{32}$", ErrorMessage = "Incorrect format. Please enter a valid 32-character value.")]
    private string _poeSessionId;

    private IDialogStorageFile _soundFile;

    public SettingsViewModel(PoeTradeApiService poeTradeApiService, ServiceState serviceState,
        ISnackbarService snackbarService, IDialogService dialogService, SoundService soundService, LeagueService leagueService)
    {
        _serviceState = serviceState;
        _snackbarService = snackbarService;
        _dialogService = dialogService;
        _soundService = soundService;
        _leagueService = leagueService;
        _poeTradeApiService = poeTradeApiService;

        if (!string.IsNullOrEmpty(UserSettings.Default.Session))
        {
            PoeSessionId = UserSettings.Default.Session[10..];
        }

        IsHide = UserSettings.Default.HideIfPoeUnfocused;
        PlayNotificationSound = UserSettings.Default.PlayNotificationSound;

        DefaultSoundNames = new ObservableCollection<Sound>(new[]
        {
            new Sound("Baka.wav"),
            new Sound("Doorbell.mp3"),
            new Sound("Electronic ping.mp3"),
            new Sound("Lofi.mp3"),
            new Sound("Music box.mp3"),
            new Sound("Ribbit.mp3"),
            new Sound("Simple ping.mp3"),
            Sound.Custom(UserSettings.Default.NotificationSoundPath)
        });
    }

    public void TestSound()
    {
        _soundService.TestPlay(CurrentSound.Path);
    }

    public void TestCustomSound()
    {
        _soundService.TestPlay(_soundFile.Path);
    }

    [RelayCommand]
    public void OpenSoundFile()
    {
        _soundFile?.Dispose();

        _soundFile = _dialogService.OpenSoundFile();
        if (_soundFile is null)
        {
            Log.Warning("File has not been chosen");
            return;
        }

        DefaultSoundNames.RemoveAt(7);
        DefaultSoundNames.Add(Sound.Custom(_soundFile.Name));

        CurrentSound = DefaultSoundNames.Last();

        TestCustomSound();
    }

    private Task SaveSoundFile()
    {
        var fileInAppFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/Sounds/", CurrentSound.Path);

        if (File.Exists(fileInAppFolder))
        {
            _soundService.Load(CurrentSound.Path);
            return Task.CompletedTask;
        }

        return CopyCustomSoundAsync(fileInAppFolder);
    }

    private async Task CopyCustomSoundAsync(string fileInAppFolder)
    {
        await using var fileStream = await _soundFile.OpenReadAsync();

        await using (Stream destinationStream = File.Create(fileInAppFolder))
        {
            await fileStream.CopyToAsync(destinationStream);
        }

        _soundService.Load(_soundFile.Name);
    }

    [RelayCommand]
    private void ModifyNotificationWindowPosition()
    {
        var view = App.Current.Services.GetRequiredService<AlwaysOnTopView>();
        view.ModifyGrid.Visibility = Visibility.Visible;
        view.StartModifyNotificationWindow();
    }

    [RelayCommand]
    private void RestoreNotificationWindowPosition()
    {
        var view = App.Current.Services.GetRequiredService<AlwaysOnTopView>();
        view.ModifyGrid.Visibility = Visibility.Collapsed;
        UserSettings.Default.NotificationPositionTop = 0;
        UserSettings.Default.NotificationPositionLeft = 0;
        view.Top = 0;
        view.Left = 0;
    }

    [RelayCommand]
    private async Task Save()
    {
        ValidateAllProperties();
        if (HasErrors || HasValidationErrors)
        {
            return;
        }

        if (!UserSettings.Default.Session.Contains(PoeSessionId))
        {
            var sessValue = $"POESESSID={PoeSessionId}";
            var isValid = await _poeTradeApiService.IsValidSessionAsync(sessValue);
            if (!isValid)
            {
                AddValidationError("Session id invalid", "PoeSessionId");

                return;
            }

            if (HasValidationErrors)
            {
                RemoveValidationError("PoeSessionId");
            }

            UserSettings.Default.Session = sessValue;
            _serviceState.Session = UserSettings.Default.Session;
        }

        if (!_leagueService.IsLoaded)
        {
            await _leagueService.LoadActualLeagueNamesAsync();
        }

        UserSettings.Default.HideIfPoeUnfocused = IsHide;

        if (UserSettings.Default.NotificationSoundPath != CurrentSound.Path)
        {
            await SaveSoundFile();
            UserSettings.Default.NotificationSoundPath = CurrentSound.Path;
        }

        UserSettings.Default.Save();
        UserSettings.Default.Reload();

        _serviceState.Session = UserSettings.Default.Session;

        OpenSnackbar();
    }

    private void OpenSnackbar()
    {
        _snackbarService.Show(
            "Settings saved.",
            null,
            SymbolRegular.CheckmarkCircle24,
            ControlAppearance.Success
        );
    }
}