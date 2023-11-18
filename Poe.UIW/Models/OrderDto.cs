using CommunityToolkit.Mvvm.ComponentModel;
using Poe.LiveSearch.Models;

namespace Poe.UIW.Models;

[ObservableObject]
public partial class OrderDto2
{
    public long Id { get; set; }
    public string QueryHash { get; set; }
    public string Title { get; set; }
    public string QueryLink { get; set; }
    public OrderActivity Activity { get; set; }
    
    public bool IsActive => Activity == OrderActivity.Enabled;

    public OrderMod Mod { get; set; }
    public int OrderPrice { get; set; }
}