using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Poe.UIW.ViewModels;
using Poe.UIW.ViewModels.OrderItemInfoViewModels;
using Poe.UIW.Views;

namespace Poe.UIW;

/// <summary>
/// This class contains static references to all the view models in the
/// application and provides an entry point for the bindings.
/// </summary>
public class ViewModelLocator
{
    public ContainerViewModel ContainerView => App.Current.Services.GetRequiredService<ContainerViewModel>();
    public AlwaysOnTopViewModel AlwaysOnTopView => App.Current.Services.GetRequiredService<AlwaysOnTopViewModel>();
    public OrderItemNotificationViewModel OrderItemNotificationView => App.Current.Services.GetRequiredService<OrderItemNotificationViewModel>();
    public OrderItemInfoViewModel OrderItemInfoView => App.Current.Services.GetRequiredService<OrderItemInfoViewModel>();
}
