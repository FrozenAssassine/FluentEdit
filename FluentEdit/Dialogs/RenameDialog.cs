using FluentEdit.Core;
using FluentEdit.Helper;
using FluentEdit.Core.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FluentEdit.Extensions;

namespace FluentEdit.Dialogs
{
    internal class RenameDialog
    {
        public static async Task<bool> ShowAsync(TextDocument document)
        {
            TextBox renameTextbox;
            var dialog = new ContentDialog
            {
                Title = "Rename File".Localized("Dialog_RenameFile_Headline/Text"),
                Content = renameTextbox = new TextBox { Text = document.FileName, HorizontalAlignment = HorizontalAlignment.Stretch},
                PrimaryButtonText = "Rename".Localized("Dialog_RenameFile_Primary/Text"),
                CloseButtonText = "Cancel".Localized("Dialog_Button_Cancel/Text"),
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
