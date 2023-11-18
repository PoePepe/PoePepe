﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Poe.LiveSearch.Models;
using Poe.LiveSearch.Services;

namespace Poe.UI.ViewModels;

public partial class OrderViewModel : ViewModelValidatableBase
{
    public OrderViewModel()
    {
    }
    
    [ObservableProperty] private long _id;

    [ObservableProperty] private string _queryHash;

    [ObservableProperty] 
    [NotifyDataErrorInfo]
    [Required]
    [MaxLength(30)]
    [CustomValidation(typeof(OrderViewModel), nameof(ValidateName))]
    private string _name;
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [RegularExpression("^(https://www\\.pathofexile\\.com/trade/search/)[a-zA-Z]+\\/[a-zA-Z0-9]+$")]
    [CustomValidation(typeof(OrderViewModel), nameof(ValidateLink))]
    private string _link;

    [ObservableProperty] private OrderActivity _activity;

    [ObservableProperty] private OrderMod _mod;

    [ObservableProperty] private int _orderPrice;

    [ObservableProperty] private bool _isActive;
    // [ObservableProperty] private bool _isEnabled;
    // [ObservableProperty] private bool _isDisabled;

    [ObservableProperty] private bool _hasValidationErrors;
    [ObservableProperty] private string _validationError;
    

    public void ReValidateAllProperties()
    {
        ValidateAllProperties();
    }
    
    public static ValidationResult ValidateName(string name, ValidationContext context)
    {
        var instance = (OrderViewModel)context.ObjectInstance;
        var service = App.Current.Services.GetRequiredService<Service>();

        var invalid = service
            .GetOrders()
            .Where(x => x.Id != instance.Id)
            .Any(x => x.Name == name);

        if (invalid)
        {
            return new("The name must be unique");
        }
        
        return ValidationResult.Success;
    }
    
    public static ValidationResult ValidateLink(string link, ValidationContext context)
    {
        var instance = (OrderViewModel)context.ObjectInstance;
        var service = App.Current.Services.GetRequiredService<Service>();

        var invalid = service
            .GetOrders()
            .Where(x => x.Id != instance.Id)
            .Any(x => x.QueryLink == link);

        if (invalid)
        {
            return new("This link has already been added");
        }

        return ValidationResult.Success;

    }
}