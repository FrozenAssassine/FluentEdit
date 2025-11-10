using FluentEdit.Core;
using FluentEdit.Dialogs;
using FluentEdit.Helper;
using FluentEdit.Core.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using System.IO;
using TextControlBoxNS;
using System;

namespace FluentEdit.Controls;

public sealed partial class StatusBar : UserControl
{
    public TextControlBox textbox;
    public TextDocument textDocument;

    public StatusBar()
    {
        this.InitializeComponent();

        LoadLineEndings();
    }

    private void LoadLineEndings()
    {
        int lineEndingIndex = 0;
        foreach (var lineEnding in Enum.GetValues(typeof(TextControlBoxNS.LineEnding)))
        {
            var item = new MenuFlyoutItem { Text = lineEnding.ToString(), Tag = lineEndingIndex++ };
            item.Click += ChangeLineEnding_Click;
            Infobar_LineEndingFlyout.Items.Add(item);
        }
    }


    private void ChangeLineEnding_Click(object sender, RoutedEventArgs e)
    {
        if (textbox == null)
            return;

        textDocument.UnsavedChanges = true;
        textbox.LineEnding = (LineEnding)(int)(sender as MenuFlyoutItem).Tag;
        UpdateLineEndings();
    }

    public void Init(TextControlBox textbox, TextDocument textDocument)
    {
        this.textbox = textbox;
        this.textDocument = textDocument;
    }


    public void UpdateAll()
    {
        this.UpdateCursor();
        this.UpdateZoom();
        this.UpdateFileName();
        this.UpdateLineEndings();
        this.UpdateWordCharacterCount();
        this.UpdateEncodingInfobar();
    }

    public void UpdateCursor()
    {
        this.SetPosition(textbox.CursorPosition.LineNumber, textbox.CursorPosition.CharacterPosition);
    }

    public void UpdateZoom()
    {
        if (textbox == null)
            return;

        textbox.ZoomFactor = (int)ZoomSlider.Value;
    }

    public void UpdateWordCharacterCount()
    {
        int charCount = textbox.CharacterCount();

        Infobar_WordCount.Text = "W: " + (charCount < 5_000_000 ? textbox.WordCount().ToString() : "");
        Infobar_CharacterCount.Text = "C: " + textbox.CharacterCount();
    }
    public void UpdateLineEndings()
    {
        Infobar_LineEnding.Content = textbox.LineEnding.ToString();
    }
    public void UpdateEncodingInfobar()
    {
        Infobar_Encoding.Content = EncodingHelper.GetEncodingName(textDocument.CurrentEncoding);
    }
    public void UpdateFileName()
    {
        fileNameDisplay.Content = textDocument.FileName;
    }


    public void SetPosition(int line, int charPos)
    {
        Infobar_Cursor.Content = "Ln: " + line + ", Col: " + charPos;
    }

    public void SetZoom(int zoom)
    {
        Infobar_Zoom.Content = zoom + "%";
    }

    private void Flyout_Closed(object sender, object e)
    {
        textbox.Focus(FocusState.Programmatic);
    }

    private void ZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateZoom();
    }
    private void ZoomSlider_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        //reset to default
        ZoomSlider.Value = 100;
    }
    private void Zoom_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
        textbox.ZoomFactor += delta / 20;
    }

    private async void RenameFile_Click(object sender, RoutedEventArgs e)
    {
        await RenameDialog.ShowAsync(textDocument);
    }
    private void FileNameInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        string newFileName = Infobar_FileNameInput.Text;

        Infobar_RenameFile.IsEnabled = newFileName.Length > 0;
        if (newFileName.Length < 1)
            return;

        if (textDocument.SavedOnDisk)
        {
            Infobar_RenameFile.IsEnabled = !Directory.Exists(Path.Combine(Path.GetDirectoryName(textDocument.FilePath), newFileName));
        }
    }

    private void GoToLine_Click(object sender, RoutedEventArgs e)
    {
        int line = (int)Infobar_GoToLineTextbox.Value - 1;

        textbox.ClearSelection();
        textbox.GoToLine(line);
        textbox.ScrollLineIntoView(textbox.CursorPosition.LineNumber);
        textbox.Focus(FocusState.Programmatic);

        Infobar_GoToLineFlyout.Hide();
    }
    private void GoToLineFlyout_Opened(object sender, object e)
    {
        Infobar_GoToLineTextbox.Maximum = textbox.NumberOfLines;
        Infobar_GoToLineTextbox.Focus(FocusState.Programmatic);
    }
    private void GoToLineTextbox_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            GoToLine_Click(sender, null);
        }
    }

    private void Encoding_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mfi && mfi.Tag != null)
        {
            textDocument.CurrentEncoding = EncodingHelper.GetEncodingByIndex(ConvertHelper.ToInt(mfi.Tag));
            UpdateEncodingInfobar();
        }
    }

    private void StatusBarZoomFlyout_Opening(object sender, object e)
    {
        ZoomSlider.Value = textbox.ZoomFactor;
    }
}
