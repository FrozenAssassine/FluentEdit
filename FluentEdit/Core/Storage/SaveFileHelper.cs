using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentEdit.Core;
using System.IO;
using FluentEdit.Views;
using FluentEdit.Helper;
using TextControlBoxNS;
using FluentEdit.Dialogs;

namespace FluentEdit.Core.Storage;

internal class SaveFileHelper
{
    public static async Task<bool> SaveFile(MainPage mainpage, TextDocument document, TextControlBox textbox, bool ForceSaveNew = false)
    {
        if (!document.SavedOnDisk || ForceSaveNew)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            savePicker.FileTypeChoices.Add("Current extension", new List<string>() { Path.GetExtension(document.FileName) });
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, App.m_window.WindowHandle);

            for (int i = 0; i < FileExtensions.FileExtentionList.Count; i++)
            {
                var item = FileExtensions.FileExtentionList[i];
                savePicker.FileTypeChoices.TryAdd(item.ExtensionName, item.Extension);
            }

            savePicker.SuggestedFileName = document.FileName;

            var file = await savePicker.PickSaveFileAsync();
            if (file == null)
                return false;

            if (await WriteLinesToFile(file.Path, textbox.Lines, document.CurrentEncoding))
            {
                document.SaveAs(file);
                WindowTitleHelper.UpdateTitle(document);
                return true;
            }
            return false;
        }

        //just save it:
        var res = await WriteLinesToFile(document.FilePath, textbox.Lines, document.CurrentEncoding);
        if (res == true)
        {
            document.Save();
            WindowTitleHelper.UpdateTitle(document);
        }
        return res;
    }
    public static async Task<bool> WriteLinesToFile(string path, IEnumerable<string> lines, Encoding encoding)
    {
        if (string.IsNullOrWhiteSpace(path) || lines == null || encoding == null)
            return false;

        try
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            using (var writer = new StreamWriter(stream, encoding))
            {
                foreach (var line in lines)
                {
                    await writer.WriteLineAsync(line);
                }
            }
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToSaveFile();
        }
        catch (Exception ex)
        {
            InfoMessages.UnhandledException(ex.Message);
        }

        return false;
    }

}
