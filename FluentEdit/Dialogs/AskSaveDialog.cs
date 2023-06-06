using FluentEdit.Core;
using FluentEdit.Helper;
using FluentEdit.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TextControlBox_DemoApp.Views;
using Windows.UI.Xaml.Controls;

namespace FluentEdit.Dialogs
{
    internal class AskSaveDialog
    {
        public static async Task<bool> CheckUnsavedChanges(MainPage mainpage, TextDocument document, TextControlBox.TextControlBox textbox)
        {
            if (!document.UnsavedChanges)
                return false;

            var SaveDialog = new ContentDialog
            {
                Title = "Save file?",
                Content = "Would you like to save the file?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = DialogHelper.DialogTheme
            };
            var res = await SaveDialog.ShowAsync();
            if (res == ContentDialogResult.Primary)
                return !await SaveFileHelper.SaveFile(mainpage, document, textbox);
            else if (res == ContentDialogResult.None)
                return true;
            return false;
        }

    }
}
