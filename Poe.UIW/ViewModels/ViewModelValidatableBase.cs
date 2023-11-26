using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Poe.UIW.ViewModels;

public partial class ViewModelValidatableBase : ObservableValidator
{
    [ObservableProperty] private bool _hasValidationErrors;
    [ObservableProperty] private string _commonValidationError;
    [ObservableProperty] private Dictionary<string, List<ValidationResult>> _validationErrors;

    public void AddValidationError(string errorMessage, IEnumerable<string> propertyNames)
    {
        var validationResult = new ValidationResult(errorMessage, propertyNames);
        foreach (var rawPropertyName in propertyNames)
        {
            var propertyName = rawPropertyName ?? "";
            if (!ValidationErrors.TryAdd(propertyName, new List<ValidationResult> { validationResult }))
            {
                ValidationErrors[propertyName].Add(validationResult);
            }
        }
    }

    public void AddValidationError(string errorMessage, string propertyName)
    {
        var validationResult = new ValidationResult(errorMessage, new[] { propertyName });
        propertyName ??= "";
        if (!ValidationErrors.TryAdd(propertyName, new List<ValidationResult> { validationResult }))
        {
            ValidationErrors[propertyName].Add(validationResult);
        }
    }

    public void RemoveValidationError(string propertyName)
    {
        propertyName ??= "";
        ValidationErrors.Remove(propertyName);
    }

    public IEnumerable<ValidationResult> GetValidationErrors(string propertyName = null)
    {
        if (HasErrors)
        {
            return GetErrors(propertyName);
        }

        if (HasValidationErrors)
        {
            return ValidationErrors[propertyName ?? ""];
        }

        return Array.Empty<ValidationResult>();
    }
}