using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FileSystem;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Services;
using Poe.UIW.Models;
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

    [ObservableProperty] private bool _isHide;

    [ObservableProperty] private bool _playNotificationSound;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [RegularExpression("^[a-z0-9]{32}$", ErrorMessage = "Incorrect format. Please enter a valid 32-character value.")]
    private string _poeSessionId;

    [ObservableProperty] private string _customSoundName;

    [ObservableProperty] private string _customSoundPath;

    [ObservableProperty] private Sound _currentSound;

    [ObservableProperty] private ObservableCollection<Sound> _defaultSoundNames;

    [ObservableProperty] private bool _hasValidationErrors;
    [ObservableProperty] private string _validationError;

    private IDialogStorageFile _soundFile;

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

        _soundFile = _dialogService.OpenSoundFile(this);
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
        var fileInAppFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources/Sounds/", CurrentSound.Path);

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
                HasValidationErrors = false;
                ValidationError = "Session id invalid";

                return;
            }

            UserSettings.Default.Session = sessValue;
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