using FluentEdit.Core;
using FluentEdit.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;

namespace FluentEdit.Dialogs
{
    internal class FileInfoDialog
    {
        public static async Task Show(TextDocument document, TextControlBox.TextControlBox textbox)
        {
            StringBuilder content = new StringBuilder();

            //File extension
            string fileExtension = Path.GetExtension(document.FileName);
            var ectension = FileExtensions.FindByExtension(fileExtension);
            if (ectension != null)
                content.AppendLine("Extension: " + fileExtension + " (" + ectension.ExtensionName + ")"); ;

            //only if the tab is based on a file
            if (document.FileToken.Length > 0)
            {
                try
                {
                    var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(document.FileToken);

                    BasicProperties fileProperties = await file.GetBasicPropertiesAsync();
                    //calculate the filesize and extension

                    content.AppendLine("Path: " + file.Path);
                    content.AppendLine("Created: " + file.DateCreated);
                    content.AppendLine("Last modified: " + fileProperties.DateModified);
                    content.AppendLine("Size: " + SizeCalculationHelper.SplitSize(fileProperties.Size));

                }
                catch (FileNotFoundException) { }
            }

            if (textbox.CodeLanguage != null)
                content.AppendLine("Code language: " + textbox.CodeLanguage.Name);

            content.AppendLine("Words: " + CountWordsHelper.CountWordsSpan(textbox.Lines));
            content.AppendLine("Lines: " + textbox.NumberOfLines);
            content.AppendLine("Characters: " + textbox.CharacterCount);
            content.AppendLine("Encoding: " + EncodingHelper.GetEncodingName(document.CurrentEncoding));

            var dialog = new ContentDialog
            {
                Title = "Info " + document.FileName,
                Content = new TextBlock { Text = content.ToString(), IsTextSelectionEnabled = true },
                CloseButtonText = "Ok",
                DefaultButton = ContentDialogButton.Close,
                RequestedTheme = DialogHelper.DialogTheme,
            };
            await dialog.ShowAsync();
        }
    }
}