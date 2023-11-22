using System.Windows;
using System.Windows.Controls;
using Poe.UIW.Models;
using Poe.UIW.Services.Influence;
using Poe.UIW.Services.ItemInfoTitle;

namespace Poe.UIW.Views.OrderItemInfoViews;

public partial class ItemInfoTitle : UserControl
{
    public static readonly DependencyProperty OrderItemProperty = DependencyProperty
        .Register("OrderItem", typeof(OrderItemDto), typeof(ItemInfoTitle),
            new FrameworkPropertyMetadata(new OrderItemDto()));

    public OrderItemDto OrderItem
    {
        get { return GetValue(OrderItemProperty) as OrderItemDto; }
        set { SetValue(OrderItemProperty, value); }
    }

    public ItemInfoTitle()
    {
        Loaded += OnLoaded;

        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RenderItemTitle();
        RenderInfluences();
    }

    private void RenderItemTitle()
    {
        var images = ResourceItemTitleService.GetItemTitleImages(OrderItem);
        LeftBackgroundImage.Source = images.LeftImageBitmap;
        MiddleBackgroundImageBrush.ImageSource = images.MiddleImageBitmap;
        RightBackgroundImage.Source = images.RightImageBitmap;

        ItemNameTextBlock.Text = OrderItem.Name;
        ItemNameTextBlock.Visibility = OrderItem.NameExists ? Visibility.Visible : Visibility.Collapsed;
        ItemTypeLineTextBlock.Text = OrderItem.TypeLine;

        var style = FindResource(ResourceItemTitleService.GetItemTitleTextStyleName(OrderItem)) as Style;
        ItemNameTextBlock.Style = style;
        ItemTypeLineTextBlock.Style = style;
    }

    private void RenderInfluences()
    {
        if (!ResourceItemInfluenceService.TryGetItemInfluenceImage(OrderItem, out var images))
        {
            InfluencesGrid.Visibility = Visibility.Collapsed;
            return;
        }

        LeftInfluenceImage.Source = images.LeftImageBitmap;
        RightInfluenceImage.Source = images.RightImageBitmap;
    }
}