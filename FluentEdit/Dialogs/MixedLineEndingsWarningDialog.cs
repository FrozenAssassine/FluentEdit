using FluentEdit.Helper;
using FluentEdit.Views.DialogPages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS;

namespace FluentEdit.Dialogs
{
    internal class MixedLineEndingsWarningDialog
    {
        public static async Task<(bool confirmed, LineEnding lineEnding)> Show()
        {
            var dialogPage = new MixedLineEndingWarningDialogPage();

            var SaveDialog = new ContentDialog
            {
                RequestedTheme = DialogHelper.DialogTheme,
                XamlRoot = App.m_window.XamlRoot,
                Title = "Warning",
                Content = dialogPage,
                PrimaryButtonText = "Apply",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
            };

            var dlgRes = await SaveDialog.ShowAsync();

            return (dlgRes == ContentDialogResult.Primary, dialogPage.SelectedLineEnding);
        }
    }
}
