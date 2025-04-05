using FluentEdit.Helper;
using FluentEdit.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace FluentEdit.Controls
{
    public sealed partial class QuickAccess : UserControl
    {
        public List<IQuickAccessItem> Items { get; set; } = new List<IQuickAccessItem>();
        QuickAccessSubItem currentPage = null;

        public delegate void ClosedEvent();
        public event ClosedEvent Closed;

        public QuickAccess()
        {
            this.InitializeComponent();
        }
        public void UpdateColors()
        {
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
            Closed?.Invoke();
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
                itemHostListView.LayoutUpdated += (sender, e) =>
                {
                    if (itemHostListView.SelectedItem == null)
                        itemHostListView.SelectedIndex = 0;
                };
            }
        }

        private void CheckLiveChange()
        {
            //for live change on up/down arrow (syntax highlighting)
            if (currentPage != null && currentPage.TriggerOnSelecting && itemHostListView.SelectedIndex != -1)
                currentPage.CallChangedEvent(itemHostListView.SelectedItem as IQuickAccessItem);
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
                    CheckLiveChange();
                }

                if (itemHostListView.SelectedItem != null)
                    itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
            }
            else if (e.Key == Windows.System.VirtualKey.Up)
            {
                if (itemHostListView.SelectedIndex > 0)
                {
                    itemHostListView.SelectedIndex--;
                    itemHostListView.ScrollIntoView(itemHostListView.Items[itemHostListView.SelectedIndex]);
                    CheckLiveChange();
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

        private void usercontrol_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
