using CommunityToolkit.Mvvm.ComponentModel;
using Poe.LiveSearch.Models;

namespace Poe.UI.ViewModels;

public partial class OrderItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _id;
    
    [ObservableProperty]
    private string _orderName;
    
    [ObservableProperty]
    private string _orderQueryLink;
    
    public string Whisper { get; set; }

    
    [ObservableProperty]
    private int _orderPrice;
    
    [ObservableProperty]
    private bool _isOfferSent; 
}