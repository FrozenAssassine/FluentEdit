using System;
using FluentEdit.Core;
using FluentEdit.Helper;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Documents;
using System.Globalization;
using FluentEdit.Extensions;

namespace FluentEdit.Dialogs;

public sealed partial class FileInfoDialog : ContentDialog
{
    private TextDocument document;
    private TextControlBox textbox;
    public FileInfoDialog(TextDocument document, TextControlBox textbox)
    {
        this.InitializeComponent();

        this.document = document;
        this.textbox = textbox;

        this.RequestedTheme = DialogHelper.DialogTheme;
        this.XamlRoot = App.m_window.XamlRoot;
        this.Title = document.FileName;
        this.CloseButtonText = "Ok".Localized("Dialog_FileInfo_Close/Text");
    }

    private void AddSection(string section, string value)
    {
        var span = new Span();
        span.Inlines.Add(new Run {Text = section });
        span.Inlines.Add(new Bold { Inlines = { new Run { Text = value } } });
        fileInfoText.Inlines.Add(span);
        fileInfoText.Inlines.Add(new LineBreak());
    }

    public new async Task ShowAsync()
    {
        string fileExtension = Path.GetExtension(document.FileName);
        var extension = FileExtensions.FindByExtension(fileExtension);
        if (extension != null)
            AddSection("Extension: ".Localized("Dialog_FileInfo_Extension/Text"), fileExtension + " (" + extension.ExtensionName + ")");

        //only if the tab is based on a file
        if (document.SavedOnDisk)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(document.FilePath);
                BasicProperties fileProperties = await file.GetBasicPropertiesAsync();

                AddSection("Path: ".Localized("Dialog_FileInfo_Path/Text"), file.Path);
                AddSection("Created: ".Localized("Dialog_FileInfo_Created/Text"), file.DateCreated.ToLocalTime().ToString("G", CultureInfo.CurrentCulture));
                AddSection("Last Modified: ".Localized("Dialog_FileInfo_LastModified/Text"), fileProperties.DateModified.ToString("G", CultureInfo.CurrentCulture));
                AddSection("Size: ".Localized("Dialog_FileInfo_Size/Text"), SizeCalculationHelper.SplitSize(fileProperties.Size));

            }
            catch (FileNotFoundException) { }
        }

        if (textbox.SyntaxHighlighting != null)
            AddSection("Syntax Highlighting: ".Localized("Dialog_FileInfo_SyntaxHighlighting/Text"), textbox.SyntaxHighlighting.Name);

        AddSection("Words: ".Localized("Dialog_FileInfo_Words/Text"), textbox.WordCount().ToString());
        AddSection("Lines: ".Localized("Dialog_FileInfo_Lines/Text"), textbox.NumberOfLines.ToString());
        AddSection("Characters: ".Localized("Dialog_FileInfo_Characters/Text"), textbox.CharacterCount().ToString());
        AddSection("Encoding: ".Localized("Dialog_FileInfo_Encoding/Text"), EncodingHelper.GetEncodingName(document.CurrentEncoding));

        await base.ShowAsync();
    }
}
