using FluentEdit.Core;
using FluentEdit.Helper;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Controls;
using TextControlBoxNS;
using Windows.Storage;

namespace FluentEdit.Dialogs
{
    internal class FileInfoDialog
    {
        public static async Task Show(TextDocument document, TextControlBox textbox)
        {
            StringBuilder content = new StringBuilder();

            //File extension
            string fileExtension = Path.GetExtension(document.FileName);
            var ectension = FileExtensions.FindByExtension(fileExtension);
            if (ectension != null)
                content.AppendLine("Extension: " + fileExtension + " (" + ectension.ExtensionName + ")"); ;

            //only if the tab is based on a file
            if (document.SavedOnDisk)
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(document.FilePath);
                    BasicProperties fileProperties = await file.GetBasicPropertiesAsync();
                    //calculate the filesize and extension

                    content.AppendLine("Path: " + file.Path);
                    content.AppendLine("Created: " + file.DateCreated);
                    content.AppendLine("Last modified: " + fileProperties.DateModified);
                    content.AppendLine("Size: " + SizeCalculationHelper.SplitSize(fileProperties.Size));

                }
                catch (FileNotFoundException) { }
            }

            if (textbox.SyntaxHighlighting != null)
                content.AppendLine("Code language: " + textbox.SyntaxHighlighting.Name);

            content.AppendLine("Words: " + textbox.WordCount());
            content.AppendLine("Lines: " + textbox.NumberOfLines);
            content.AppendLine("Characters: " + textbox.CharacterCount());
            content.AppendLine("Encoding: " + EncodingHelper.GetEncodingName(document.CurrentEncoding));

            var dialog = new ContentDialog
            {
                Title = "Info " + document.FileName,
                Content = new TextBlock { Text = content.ToString(), IsTextSelectionEnabled = true },
                CloseButtonText = "Ok",
                DefaultButton = ContentDialogButton.Close,
                RequestedTheme = DialogHelper.DialogTheme,
                XamlRoot = App.m_window.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}