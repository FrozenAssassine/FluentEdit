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
    private static IEnumerable<string> GetLines(StreamReader reader)
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }
    private static (IEnumerable<string> lines, bool mixedEndings, LineEnding lineEnding) GetLinesAndDetectMixed(StreamReader reader)
    {
        var sb = new StringBuilder();
        var lines = new List<string>();
        bool seenCRLF = false;
        bool seenLF = false;
        bool seenCR = false;

        int c;
        while ((c = reader.Read()) != -1)
        {
            if (c == '\r')
            {
                int next = reader.Peek();
                if (next == '\n')
                {
                    reader.Read();
                    seenCRLF = true;
                }
                else
                {
                    seenCR = true;
                }

                lines.Add(sb.ToString());
                sb.Clear();
            }
            else if (c == '\n')
            {
                seenLF = true;
                lines.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append((char)c);
            }
        }

        if (sb.Length > 0)
            lines.Add(sb.ToString());

        bool mixed = (seenCRLF ? 1 : 0) + (seenLF ? 1 : 0) + (seenCR ? 1 : 0) > 1;

        LineEnding ending = LineEnding.CRLF;
        if (!mixed)
        {
            if (seenCRLF) ending = LineEnding.CRLF;
            else if (seenLF) ending = LineEnding.LF;
            else if (seenCR) ending = LineEnding.CR;
        }

        return (lines, mixed, ending);
    }

    public static (string[] lines, Encoding encoding, bool succeeded, bool mixedLineEndings, LineEnding lineEnding) ReadLinesFromFile(string path, Encoding encoding = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return (null, null, false, false, LineEnding.CRLF);

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536, useAsync: false);
            using (var reader = new StreamReader(stream, encoding ?? Encoding.Default, detectEncodingFromByteOrderMarks: true))
            {
                var getLinesResult = GetLinesAndDetectMixed(reader);

                encoding ??= reader.CurrentEncoding;

                return (getLinesResult.lines.ToArray(), encoding, true, getLinesResult.mixedEndings, getLinesResult.lineEnding);
            }
        }
        catch (UnauthorizedAccessException)
        {
            InfoMessages.NoAccessToReadFile();
            return (null, null, false, false, LineEnding.CRLF);
        }
        catch (Exception ex)
        {
            InfoMessages.UnhandledException(ex.Message);
        }
        return (null, null, false, false, LineEnding.CRLF);
    }

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
            await OpenFileAsync(mainpage, document, textbox, file.Path);
    }

    public static async Task<bool> OpenFileAsync(MainPage mainpage, TextDocument document, TextControlBox textbox, string filePath)
    {
        if (filePath == null || filePath.Length == 0)
            return false;

        var res = ReadLinesFromFile(filePath);
        if (res.succeeded)
        {
            LineEnding lineEnding;
            if (res.mixedLineEndings)
            {
                var mixedLineEndingsWarningDlg = await MixedLineEndingsWarningDialog.Show();
                if (!mixedLineEndingsWarningDlg.confirmed)
                    return false;

                lineEnding = mixedLineEndingsWarningDlg.lineEnding;
            } else
                lineEnding = res.lineEnding;

            document.Open(res.encoding, filePath);
            FileExtensions.SelectSyntaxHighlightingByFile(filePath, textbox);

            textbox.LoadLines(res.lines, true, lineEnding);
            textbox.ScrollLineIntoView(0);
            
            mainpage.StatusBar.UpdateAll();
            WindowTitleHelper.UpdateTitle(document);

            return true;
        }
        return false;
    }
}
