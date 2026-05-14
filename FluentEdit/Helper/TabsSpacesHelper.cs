
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using TextControlBoxNS;

namespace FluentEdit.Helper;

internal class TabsSpacesHelper
{

    public static void SelectToggleMenuItemsFromMenu(MenuFlyoutSubItem tabsSpacesflyout, TextControlBox textbox)
    {
        string tag = (textbox.UseSpacesInsteadTabs ? textbox.NumberOfSpacesForTab : -1).ToString();
        IterateItems(tabsSpacesflyout.Items, tag);
    }

    private static void IterateItems(IList<MenuFlyoutItemBase> items, string tag)
    {
        foreach (MenuFlyoutItemBase item in items)
        {
            if (item is ToggleMenuFlyoutItem radioItem)
            {
                radioItem.IsChecked = radioItem.Tag.ToString().Equals(tag);
            }
        }
    }

    public static void RewriteTabsSpaces(TextControlBox textbox, object sender)
    {
        int spaces = ConvertHelper.ToInt((sender as MenuFlyoutItem).Tag, -1);

        bool useSpaces = spaces != -1;
        textbox.RewriteTabsSpaces(spaces == -1 ? 4 : spaces, useSpaces);
    }

    public static void SetTabsSpaces(TextControlBox textbox, int spaces = -1)
    {
        //-1 = use tabs positive values => spaces
        textbox.UseSpacesInsteadTabs = spaces != -1;
        if (spaces > 0)
            textbox.NumberOfSpacesForTab = spaces;
    }
}
