using FluentEdit.Helper;
using FluentEdit.Models;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace FluentEdit.Controls
{
    public sealed partial class QuickAccess : UserControl
    {
        public List<IQuickAccessItem> Items { get; set; } = new List<IQuickAccessItem>();
        QuickAccessSubItem currentPage = null;

        public QuickAccess()
        {
            this.InitializeComponent();
        }
        public void UpdateColors()
        {
            UpdateColors(Items);
        }

        private Brush GetTextColor()
        {
            var DefaultTheme = new Windows.UI.ViewManagement.UISettings();
            var uiTheme = DefaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
            if (uiTheme == "#FF000000")
                return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            else if (uiTheme == "#FFFFFFFF")
                return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            return null;
        }

        public void UpdateColors(List<IQuickAccessItem> items)
        {
            var textcolor = GetTextColor();
            foreach (var item in items)
            {
                if (item is QuickAccessSubItem sub_item)
                {
                    UpdateColors(sub_item.Items);
                }
                item.TextColor = textcolor;
            }
            grid.RequestedTheme = DialogHelper.DialogTheme;
        }

        public void Toggle()
        {
            if (this.Visibility == Visibility.Visible)
                Hide();
            else
                Show();
        }
        public void Show()
        {
            if (itemHostListView == null)
            {
                itemHostListView = FindName("itemHostListView") as ListView;
            }
            UpdateColors();

            searchbox.Text = "";
            this.Visibility = Visibility.Visible;
            showControlAnimation.Begin();
            searchbox.Focus(FocusState.Programmatic);
            searchbox_TextChanged(null, null);
        }
        public void Hide()
        {
            itemHostListView.SelectedItem = null;
            currentPage = null;
            hideControlAnimation.Begin();
        }

        private void searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentPage != null)
            {
                var source = currentPage.Items.Where(x => x.Command.ToLower().Contains(searchbox.Text.ToLower()));
                itemHostListView.ItemsSource = source.OrderBy(x => x.Command);
                return;
            }

            var newsource = Items.Where(x => x.Command.ToLower().Contains(searchbox.Text.ToLower()));

            itemHostListView.ItemsSource = newsource.OrderBy(x => x.Command);
            itemHostListView.SelectedIndex = 0;
        }
        private void itemHostListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ItemClicked(e.ClickedItem);
        }
        private void ItemClicked(object clickedItem)
        {
            if (clickedItem is QuickAccessItem item)
            {
                item.InvokeEvent();
                Hide();
            }
            else if (clickedItem is QuickAccessSubItem subItem)
            {
                //change the source -> like switching to sub page:
                currentPage = subItem;
                searchbox.Text = "";
                itemHostListView.ItemsSource = subItem.Items;
                itemHostListView.SelectedIndex = 1;
            }
        }
        private void UserControl_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                if (currentPage != null)
                {
                    currentPage = null;
                    searchbox.Text = "";
                    itemHostListView.ItemsSource = Items;
                    return;
                }
                Hide();
            }
            else if (e.Key == Windows.System.VirtualKey.Down)
            {
                if (itemHostListView.SelectedIndex < itemHostListView.Items.Count - 1)
                {
                    itemHostListView.SelectedIndex++;
                    itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
                }

                if(itemHostListView.SelectedItem != null)
                    itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
            }
            else if (e.Key == Windows.System.VirtualKey.Up)
            {
                if (itemHostListView.SelectedIndex > 0)
                {
                    itemHostListView.SelectedIndex--;
                    itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
                }

            }
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (itemHostListView.SelectedItem == null)
                    return;

                ItemClicked(itemHostListView.SelectedItem);
            }
        }
        private void hideControlAnimation_Completed(object sender, object e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
