using FluentEdit.Core;
using FluentEdit.Helper;
using FluentEdit.Core.Storage;
using System;
using System.Threading.Tasks;
using FluentEdit.Views;
using Microsoft.UI.Xaml.Controls;
using TextControlBoxNS;

namespace FluentEdit.Dialogs;

internal class AskSaveDialog
{
    public static async Task<bool> CheckUnsavedChanges(MainPage mainpage, TextDocument document, TextControlBox textbox)
    {
        if (!document.UnsavedChanges)
            return false;

        var saveDialog = new ContentDialog
        {
            Title = "Save file?",
            Content = "Would you like to save the file?",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Don't save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            RequestedTheme = DialogHelper.DialogTheme,
            XamlRoot = App.m_window.XamlRoot
        };
        var res = await saveDialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
            return !await SaveFileHelper.SaveFile(mainpage, document, textbox);
        else if (res == ContentDialogResult.None)
            return true;
        return false;
    }

}
