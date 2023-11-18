using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Poe.LiveSearch.Models;
using Poe.UIW.ViewModels;

namespace Poe.UIW.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView
    {
        public MainViewModel ViewModel { get; }
        private AlwaysOnTopView AlwaysOnTopView { get; }

        public MainView()
        {
            AlwaysOnTopView = new AlwaysOnTopView();
            AlwaysOnTopView.Show();
            ViewModel = new MainViewModel();
            InitializeComponent();
        }
        
        public MainView(MainViewModel viewModel)
        {
            
            AlwaysOnTopView = new AlwaysOnTopView();
            AlwaysOnTopView.Show();
            ViewModel = viewModel;
            
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            AlwaysOnTopView.Close();
            base.OnClosing(e);
        }

        private void SelectingItemsControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox cmb)
            {
                return;
            }
        
            if (!cmb.IsDropDownOpen)
            {
                return;
            }
        
            if (DataContext is not MainViewModel context)
            {
                return;
            }

            // if (cmb.Parent?.Parent?.DataContext is not OrderViewModel orderViewModel)
            // {
            //     return;
            // }

            var newMod = Enum.Parse<OrderMod>(e.AddedItems[0].ToString());
            var oldMod = Enum.Parse<OrderMod>(e.RemovedItems[0].ToString());

            // context.ChangeOrderMod(orderViewModel.Id, newMod, oldMod);
        }

        private void MenuItem_OnClick_Open(object sender, RoutedEventArgs e)
        {
            Show();
            Activate();
        }

        private void MenuItem_OnClick_Close(object sender, RoutedEventArgs e)
        {
            Close();
            OnClosing(new CancelEventArgs());
        }
    }
}