using FluentEdit.Core;
using FluentEdit.Dialogs;
using FluentEdit.Helper;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentEdit.Views;
using TextControlBoxNS;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using System.Linq;

namespace FluentEdit.Core.Storage;

internal class OpenFileHelper
{
    public static async Task OpenFile(MainPage mainpage, TextDocument document, TextControlBox textbox)
    {
        if (await AskSaveDialog.CheckUnsavedChanges(mainpage, document, textbox))
            return;

        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, App.m_window.WindowHandle);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
            OpenFile(mainpage, document, textbox, file.Path);
    }

    public static void OpenFile(MainPage mainpage, TextDocument document, TextControlBox textbox, string filePath)
    {
        if (filePath == null || filePath.Length == 0)
            return;

        var res = ReadLinesFromFile(filePath);
        if (res.succeeded)
        {
            document.Open(res.encoding, filePath);
            FileExtensions.SelectSyntaxHighlightingByFile(filePath, textbox);

            textbox.LoadLines(res.lines);
            textbox.ScrollLineIntoView(0);
            
            mainpage.StatusBar.UpdateAll();
            WindowTitleHelper.UpdateTitle(document);
        }
    }

    private static IEnumerable<string> GetLines(StreamReader reader)
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }

    public static (string[] lines, Encoding encoding, bool succeeded) ReadLinesFromFile(string path, Encoding encoding = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return (null, null, false);

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            using (var reader = new StreamReader(stream, encoding ?? Encoding.Default, detectEncodingFromByteOrderMarks: true))
            {
                var lines = GetLines(reader).ToArray();

                encoding ??= reader.CurrentEncoding;

                return (lines, encoding, true);
            }
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToReadFile();
            return (null, null, false);
        }
        catch (Exception ex)
        {
            InfoMessages.UnhandledException(ex.Message);
        }
        return (null, null, false);
    }
}
