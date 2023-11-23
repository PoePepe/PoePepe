using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Poe.LiveSearch.Api.Trade;
using Poe.LiveSearch.Services;
using Poe.UIW.Properties;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace Poe.UIW.ViewModels;

public partial class SettingsViewModel : ViewModelValidatableBase
{
    private readonly ISnackbarService _snackbarService;

    private readonly ServiceState _serviceState;
    private readonly PoeTradeApiService _poeTradeApiService;
    
    public SettingsViewModel(PoeTradeApiService poeTradeApiService, ServiceState serviceState, ISnackbarService snackbarService)
    {
        _serviceState = serviceState;
        _snackbarService = snackbarService;
        _poeTradeApiService = poeTradeApiService;

        PoeSessionId = UserSettings.Default.Session;
        IsHide = UserSettings.Default.HideIfPoeUnfocused;
    }
    
    [ObservableProperty]
    private bool _isHide;
    
    [ObservableProperty]
    [Required]
    [RegularExpression("^POESESSID=[a-z0-9]{32}$")]
    private string _poeSessionId;
    
    [ObservableProperty] private bool _hasValidationErrors;
    [ObservableProperty] private string _validationError;
    
    [RelayCommand]
    private async Task Save()
    {
        ValidateAllProperties();
        if (HasErrors || HasValidationErrors)
        {
            return;
        }

        var isValid = await _poeTradeApiService.IsValidSessionAsync();
        if (!isValid)
        {
            HasValidationErrors = false;
            ValidationError = "Session id invalid";
            return;
        }
        
        UserSettings.Default.Session = PoeSessionId;
        UserSettings.Default.HideIfPoeUnfocused = IsHide;
        
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