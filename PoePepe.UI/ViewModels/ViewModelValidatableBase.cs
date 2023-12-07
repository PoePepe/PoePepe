using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PoePepe.UI.ViewModels;

public partial class ViewModelValidatableBase : ObservableValidator
{
    private readonly Dictionary<string, List<ValidationResult>> _validationErrors = new();

    [ObservableProperty] private string _commonValidationError;

    [ObservableProperty] private bool _hasValidationErrors;
    public event EventHandler<DataErrorsChangedEventArgs> ValidationErrorsChanged;

    public void SetCommonValidationError(string errorMessage)
    {
        CommonValidationError = errorMessage;
        HasValidationErrors = true;
    }

    public void ClearCommonValidationError()
    {
        CommonValidationError = null;

        HasValidationErrors = _validationErrors.Count > 0;
    }

    public void AddValidationError(string errorMessage, IEnumerable<string> propertyNames)
    {
        var validationResult = new ValidationResult(errorMessage, propertyNames);
        foreach (var rawPropertyName in propertyNames)
        {
            var propertyName = rawPropertyName ?? "";
            if (!_validationErrors.TryAdd(propertyName, new List<ValidationResult> { validationResult }))
            {
                _validationErrors[propertyName].Add(validationResult);
            }
        }

        HasValidationErrors = true;
    }

    public void AddValidationError(string errorMessage, string propertyName)
    {
        var validationResult = new ValidationResult(errorMessage, new[] { propertyName });
        propertyName ??= "";
        if (!_validationErrors.TryAdd(propertyName, new List<ValidationResult> { validationResult }))
        {
            _validationErrors[propertyName].Add(validationResult);
        }

        HasValidationErrors = true;

        ValidationErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public void RemoveValidationError(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return;
        }

        _validationErrors.Remove(propertyName);

        HasValidationErrors = _validationErrors.Count > 0 || CommonValidationError is not null;

        ValidationErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public void RemoveValidationError()
    {
        _validationErrors.Remove("");

        HasValidationErrors = _validationErrors.Count > 0 || CommonValidationError is not null;

        ValidationErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(""));
    }

    public IEnumerable<ValidationResult> GetValidationErrors(string propertyName = null)
    {
        if (HasErrors)
        {
            return GetErrors(propertyName);
        }

        if (HasValidationErrors)
        {
            if (_validationErrors.TryGetValue(propertyName ?? "", out var validationError))
            {
                return validationError;
            }
        }

        return Array.Empty<ValidationResult>();
    }
}