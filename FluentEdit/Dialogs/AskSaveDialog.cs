using FluentEdit.Core;
using FluentEdit.Helper;
using FluentEdit.Core.Storage;
using System;
using System.Threading.Tasks;
using FluentEdit.Views;
using Microsoft.UI.Xaml.Controls;
using TextControlBoxNS;
using FluentEdit.Extensions;

namespace FluentEdit.Dialogs;

internal class AskSaveDialog
{
    public static async Task<bool> CheckUnsavedChanges(MainPage mainpage, TextDocument document, TextControlBox textbox)
    {
        if (!document.UnsavedChanges)
            return false;

        var saveDialog = new ContentDialog
        {
            Title = "Save file?".Localized("Dialog_SaveFile_Headline/Text"),
            Content = "Would you like to save the file?".Localized("Dialog_SaveFile_Message/Text"),
            PrimaryButtonText = "Save".Localized("Dialog_Button_Save/Text"),
            SecondaryButtonText = "Don't save".Localized("Dialog_Button_DontSave/Text"),
            CloseButtonText = "Cancel".Localized("Dialog_Button_Cancel/Text"),
            DefaultButton = ContentDialogButton.Primary,
            RequestedTheme = DialogHelper.DialogTheme,
            XamlRoot = App.m_window.XamlRoot
        };
        ContentDialogResult res = ContentDialogResult.None;
        try
        {
            res = await saveDialog.ShowAsync();
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            return true;
        }

        if (res == ContentDialogResult.Primary)
            return !await SaveFileHelper.SaveFile(mainpage, document, textbox);
        else if (res == ContentDialogResult.None)
            return true;
        return false;
    }

}
