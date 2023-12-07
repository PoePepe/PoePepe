using System.Windows;
using System.Windows.Controls;
using PoePepe.UI.Models;
using PoePepe.UI.Services.Influence;
using PoePepe.UI.Services.ItemInfoTitle;

namespace PoePepe.UI.Views.OrderItemInfoViews;

public partial class ItemInfoTitle : UserControl
{
    public static readonly DependencyProperty OrderItemProperty = DependencyProperty
        .Register("OrderItem", typeof(OrderItemDto), typeof(ItemInfoTitle),
            new FrameworkPropertyMetadata(new OrderItemDto()));

    public ItemInfoTitle()
    {
        Loaded += OnLoaded;

        InitializeComponent();
    }

    public OrderItemDto OrderItem
    {
        get => GetValue(OrderItemProperty) as OrderItemDto;
        set => SetValue(OrderItemProperty, value);
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