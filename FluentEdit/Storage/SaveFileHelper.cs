using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using FluentEdit.Core;
using System.IO;
using Microsoft.UI.Xaml.Controls;
using TextControlBox_DemoApp.Views;
using FluentEdit.Helper;

namespace FluentEdit.Storage
{
    internal class SaveFileHelper
    {
        public static async Task<bool> SaveFile(MainPage mainpage, TextDocument document, TextControlBox.TextControlBox textbox, bool ForceSaveNew = false)
        {
            if (document.FileToken.Length == 0 || ForceSaveNew)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

                //Add the extension of the current file
                savePicker.FileTypeChoices.Add("Current extension", new List<string>() { Path.GetExtension(document.FileName) });

                for (int i = 0; i < FileExtensions.FileExtentionList.Count; i++)
                {
                    var item = FileExtensions.FileExtentionList[i];
                    savePicker.FileTypeChoices.TryAdd(item.ExtensionName, item.Extension);
                }

                savePicker.SuggestedFileName = document.FileName;

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);
                    await WriteTextToFileAsync(mainpage, file, textbox.GetText(), document.CurrentEncoding);
                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        //delete the old file permission and add a new one
                        if (document.FileToken.Length > 0)
                            StorageApplicationPermissions.FutureAccessList.Remove(document.FileToken);
                        document.FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);

                        document.FileName = file.Name;
                        document.UnsavedChanges = false;
                        mainpage.UpdateTitle();
                        return true;
                    }
                }
            }
            else
            {
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(document.FileToken))
                {
                    StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(document.FileToken);
                    document.FileName = file.Name;

                    var res = await WriteTextToFileAsync(mainpage, file, textbox.GetText(), document.CurrentEncoding);
                    if (res == true)
                    {
                        document.UnsavedChanges = false;
                        mainpage.UpdateTitle();
                    }
                    return res;
                }
                else
                    return await SaveFile(mainpage, document, textbox, true);
            }
            return false;
        }
        public static async Task<bool> WriteTextToFileAsync(MainPage mainpage, StorageFile file, string text, Encoding encoding)
        {
            try
            {
                if (file == null)
                    return false;

                //Only do this for drag/dropped files
                if (mainpage.FileIsDragDropped)
                {
                    var bytestoWrite = encoding.GetBytes(text);
                    var buffer = encoding.GetPreamble().Concat(bytestoWrite).ToArray();
                    await PathIO.WriteBytesAsync(file.Path, buffer);
                    return true;
                }

                //clear the file content
                await FileIO.WriteTextAsync(file, "");
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (var writer = new StreamWriter(stream, encoding))
                    {
                        writer.Write(text);
                    }
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                mainpage.ShowInfobar(InfoBarSeverity.Error, "No access to write to file", "No access");
            }
            catch (Exception ex)
            {
                mainpage.ShowInfobar(InfoBarSeverity.Error, ex.Message, "Write file exception");
            }
            return false;
        }
    }
}
