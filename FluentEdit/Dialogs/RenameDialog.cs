using FluentEdit.Core;
using FluentEdit.Helper;
using FluentEdit.Core.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentEdit.Dialogs
{
    internal class RenameDialog
    {
        public static async Task<bool> ShowAsync(TextDocument document)
        {
            TextBox renameTextbox;
            var dialog = new ContentDialog
            {
                Title = "Rename File",
                Content = renameTextbox = new TextBox { Text = document.FileName, HorizontalAlignment = HorizontalAlignment.Stretch},
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = DialogHelper.DialogTheme,
                XamlRoot = App.m_window.XamlRoot
            };

            renameTextbox.Select(0, document.FileName.LastIndexOf("."));
            renameTextbox.Focus(FocusState.Programmatic);

            var res = await dialog.ShowAsync();

            if (res == ContentDialogResult.Primary)
                return RenameFileHelper.RenameFile(document, renameTextbox.Text);
            return false;
        }
    }
}
