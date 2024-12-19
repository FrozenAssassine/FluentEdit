using FluentEdit.Core;
using FluentEdit.Helper;
using FluentEdit.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBox_DemoApp.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentEdit.Dialogs
{
    internal class RenameDialog
    {
        public static async Task<(bool res, string newName)> ShowAsync(string currentName)
        {
            TextBox tb;
            var dialog = new ContentDialog
            {
                Title = "Rename File",
                Content = tb = new TextBox { Text = currentName, HorizontalAlignment = HorizontalAlignment.Stretch},
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = DialogHelper.DialogTheme
            };
            var res = await dialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
                return (true, tb.Text);
            return (false, "");
        }
    }
}
