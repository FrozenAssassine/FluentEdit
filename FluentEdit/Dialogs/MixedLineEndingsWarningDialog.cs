using FluentEdit.Helper;
using FluentEdit.Views.DialogPages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS;
using FluentEdit.Extensions;

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
                Title = "Warning".Localized("Dialog_MixedLineEndings_Headline/Text"),
                Content = dialogPage,
                PrimaryButtonText = "Apply".Localized("Dialog_MixedLineEndings_Primary/Text"),
                CloseButtonText = "Cancel".Localized("Dialog_Button_Cancel/Text"),
                DefaultButton = ContentDialogButton.Primary,
            };

            var dlgRes = await SaveDialog.ShowAsync();

            return (dlgRes == ContentDialogResult.Primary, dialogPage.SelectedLineEnding);
        }
    }
}
