using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Poe.UIW.Mapping;
using Poe.UIW.ViewModels;

namespace Poe.UIW.Views;

public partial class OrderHistoryView
{
    public OrderHistoryViewModel ViewModel { get; }

    public OrderHistoryView(OrderHistoryViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;

        Loaded += OnLoaded;

        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var scrollViewer = GetScrollViewer(ItemHistoryDataGrid);
        scrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
    }

    private void ScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var shift = (int)Math.Floor(e.VerticalOffset / 9);
        if (shift == 0)
        {
            return;
        }

        var skip = 20 + (shift - 1) * 10;

        ThreadPool.QueueUserWorkItem(_ =>
        {
            var history =  ViewModel.LoadHistory(10, skip)?.ToArray();
            if (history is null || !history.Any())
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                foreach (var itemHistory in history)
                {
                    ViewModel.HistoryItemViews.Add(itemHistory.ToItemHistoryDto());
                }
            });
        });
    }

    private static ScrollViewer GetScrollViewer(UIElement element)
    {
        if (element == null)
        {
            return null;
        }

        ScrollViewer scrollViewer = null;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element) && scrollViewer == null; i++)
        {
            if (VisualTreeHelper.GetChild(element, i) is ScrollViewer)
            {
                scrollViewer = (ScrollViewer)(VisualTreeHelper.GetChild(element, i));
            }
            else
            {
                scrollViewer = GetScrollViewer(VisualTreeHelper.GetChild(element, i) as UIElement);
            }
        }

        return scrollViewer;
    }
}