using FluentEdit.Core;
using FluentEdit.Dialogs;
using FluentEdit.Helper;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TextControlBox_DemoApp.Views;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;

namespace FluentEdit.Storage
{
    internal class OpenFileHelper
    {
        public static async Task OpenFile(MainPage mainpage, TextDocument document, TextControlBox.TextControlBox textbox)
        {
            if (await AskSaveDialog.CheckUnsavedChanges(mainpage, document, textbox))
                return;

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add("*");
            
            mainpage.FileIsDragDropped = false;
            
            await OpenFile(mainpage, document, textbox, await picker.PickSingleFileAsync());
        }

        public static async Task OpenFile(MainPage mainpage, TextDocument document, TextControlBox.TextControlBox textbox, StorageFile file)
        {
            if (file != null)
            {
                document.FileName = file.Name;
                document.FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);

                var res = await ReadTextFromFileAsync(mainpage, file);
                if (res.Succed)
                {
                    document.CurrentEncoding = res.encoding;
                    document.UnsavedChanges = false;

                    mainpage.SelectCodeLanguageByFile(file);
                    mainpage.UpdateEncodingInfobar();
                    mainpage.UpdateWordCharacterCount();

                    textbox.LoadText(res.text);
                    textbox.ScrollLineIntoView(0);
                    mainpage.UpdateLineEndings();
                    mainpage.SetPositionInInfobar(0, 0);

                    mainpage.UpdateTitle();
                }
            }
        }

        public static async Task<(string text, Encoding encoding, bool Succed)> ReadTextFromFileAsync(MainPage mainpage, StorageFile file, Encoding encoding = null)
        {
            try
            {
                if (file == null)
                    return (null, Encoding.Default, false);

                using (var stream = (await file.OpenReadAsync()).AsStreamForRead())
                {
                    using (var reader = new StreamReader(stream, true))
                    {
                        //Detect the encoding:
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        encoding = EncodingHelper.DetectTextEncoding(buffer, out string text);

                        //read the text with the encoding:
                        return (text, encoding, true);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                mainpage.ShowInfobar(InfoBarSeverity.Error, "No access to read from file", "No access");

            }
            catch (Exception ex)
            {
                mainpage.ShowInfobar(InfoBarSeverity.Error, ex.Message, "Read file exception");
            }
            return (null, Encoding.Default, false);
        }

    }
}
