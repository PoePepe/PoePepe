using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FileSystem;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Services;
using Poe.UIW.Properties;
using Poe.UIW.Services;
using Serilog;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace Poe.UIW.ViewModels;

public partial class SettingsViewModel : ViewModelValidatableBase
{
    private readonly ISnackbarService _snackbarService;

    private readonly ServiceState _serviceState;
    private readonly PoeTradeApiService _poeTradeApiService;
    private readonly IDialogService _dialogService;
    private readonly SoundService _soundService;

    public SettingsViewModel(PoeTradeApiService poeTradeApiService, ServiceState serviceState,
        ISnackbarService snackbarService, IDialogService dialogService, SoundService soundService)
    {
        _serviceState = serviceState;
        _snackbarService = snackbarService;
        _dialogService = dialogService;
        _soundService = soundService;
        _poeTradeApiService = poeTradeApiService;

        PoeSessionId = UserSettings.Default.Session[10..];
        IsHide = UserSettings.Default.HideIfPoeUnfocused;
        PlayNotificationSound = UserSettings.Default.PlayNotificationSound;
    }

    [ObservableProperty] private bool _isHide;

    [ObservableProperty] private bool _playNotificationSound;

    [ObservableProperty] private string _notificationSoundPath;

    [ObservableProperty]
    [Required]
    [RegularExpression("^[a-z0-9]{32}$", ErrorMessage = "Incorrect format. Please enter a valid 32-character value.")]
    private string _poeSessionId;

    [ObservableProperty] private bool _hasValidationErrors;
    [ObservableProperty] private string _validationError;

    [RelayCommand]
    private void ResetSoundFile()
    {
        NotificationSoundPath = SoundService.DefaultNotificationPath;
    }

    private IDialogStorageFile _soundFile;

    [RelayCommand]
    private void OpenSoundFile()
    {
        _soundFile?.Dispose();

        _soundFile = _dialogService.OpenSoundFile(this);
        if (_soundFile is null)
        {
            Log.Warning("File has not been chosen");
            return;
        }

        NotificationSoundPath = _soundFile.Name;
    }

    private async Task SaveSoundFile()
    {
        if (NotificationSoundPath == SoundService.DefaultNotificationPath)
        {
            _soundService.Reset();
            return;
        }

        var fileInAppFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources/Sounds/", _soundFile.Name);

        if (File.Exists(fileInAppFolder))
        {
            _soundService.Load(_soundFile.Name);
            return;
        }

        await using var fileStream = await _soundFile.OpenReadAsync();

        await using (Stream destinationStream = File.Create(fileInAppFolder))
        {
            await fileStream.CopyToAsync(destinationStream);
        }

        _soundService.Load(_soundFile.Name);
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
            var isValid = await _poeTradeApiService.IsValidSessionAsync(PoeSessionId);
            if (!isValid)
            {
                HasValidationErrors = false;
                ValidationError = "Session id invalid";

                return;
            }

            UserSettings.Default.Session = $"POESESSID={PoeSessionId}";
        }

        UserSettings.Default.HideIfPoeUnfocused = IsHide;
        UserSettings.Default.PlayNotificationSound = PlayNotificationSound;

        if (UserSettings.Default.NotificationSoundPath != NotificationSoundPath)
        {
            await SaveSoundFile();
            UserSettings.Default.NotificationSoundPath = NotificationSoundPath;
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