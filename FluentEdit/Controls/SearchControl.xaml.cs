using FluentEdit.Helper;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TextControlBoxNS;
using Microsoft.UI;

namespace FluentEdit.Controls;

public sealed partial class SearchControl : UserControl
{
    private TextControlBox currentTextbox = null;
    public bool searchOpen = false;

    private SearchWindowState searchWindowState = SearchWindowState.Hidden;

    public SearchControl()
    {
        this.InitializeComponent();
    }

    private void BeginSearch(string searchword, bool matchCase, bool wholeWord)
    {
        var res = currentTextbox.BeginSearch(searchword, wholeWord, matchCase);
        ColorWindowBorder(res);
    }
    private void ToggleVisibility(bool visible)
    {
        textToReplaceTextBox.Visibility = ReplaceAllButton.Visibility =
            StartReplaceButton.Visibility = ConvertHelper.BoolToVisibility(visible);
    }
    private void ColorWindowBorder(SearchResult result)
    {
        SearchWindow.BorderBrush = new SolidColorBrush(result == SearchResult.Found ? Colors.Green : Colors.Red);
    }
    private void HideWindow()
    {
        hideSearchAnimation.Begin();
        searchWindowState = SearchWindowState.Hidden;
    }
    private void ShowWindow()
    {
        this.Visibility = Visibility.Visible;
        showSearchAnimation.Begin();
        searchWindowState = SearchWindowState.Default;
    }
    private void ExpandReplace()
    {
        expandSearchAnimation.Begin();
        searchWindowState = SearchWindowState.Expanded;
    }
    private void CollapseReplace()
    {
        collapseSearchAnimation.Begin();
        searchWindowState = SearchWindowState.Default;
    }

    public void ShowSearch(TextControlBox textbox)
    {
        //if (currentTextbox != null && currentTextbox != textbox)
        //{
        //    currentTextbox.EndSearch();
        //    this.searchWindowState = SearchWindowState.Hidden;
        //}

        currentTextbox = textbox;
        searchOpen = true;

        if (searchWindowState == SearchWindowState.Expanded)
        {
            CollapseReplace();
        }
        else if (searchWindowState == SearchWindowState.Hidden)
        {
            ShowWindow();
            CollapseReplace();
        }

        if (currentTextbox.HasSelection && currentTextbox.CalculateSelectionPosition().Length < 200)
        {
            textToFindTextbox.Text = currentTextbox.SelectedText;
        }

        textToFindTextbox.Focus(FocusState.Keyboard);
        textToFindTextbox.SelectAll();
    }

    public void ShowReplace(TextControlBox textbox)
    {
        currentTextbox = textbox;

        if (searchWindowState == SearchWindowState.Default)
        {
            ExpandReplace();
        }
        else if (searchWindowState == SearchWindowState.Hidden)
        {
            ShowWindow();
            ExpandReplace();
        }


        if (currentTextbox.HasSelection && currentTextbox.CalculateSelectionPosition().Length < 200)
        {
            textToFindTextbox.Text = currentTextbox.SelectedText;
        }

        textToReplaceTextBox.Focus(FocusState.Keyboard);
        textToReplaceTextBox.SelectAll();
        textToFindTextbox.Focus(FocusState.Keyboard);
        textToFindTextbox.SelectAll();
    }

    public void Close()
    {
        if (!searchOpen || currentTextbox == null)
            return;

        searchOpen = false;
        currentTextbox.EndSearch();
        HideWindow();
        currentTextbox.Focus(FocusState.Programmatic);

        currentTextbox = null;
    }

    private void UpdateSearch()
    {
        BeginSearch(textToFindTextbox.Text, FindMatchCaseButton.IsChecked ?? false, FindWholeWordButton.IsChecked ?? false);
    }

    private void ReplaceTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ReplaceCurrentButton_Click(null, null);
        }
        else if (e.Key == VirtualKey.Escape)
        {
            Close();
        }
    }
    private void SearchTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (currentTextbox == null)
            return;

        //Search down on Enter and up on Shift + Enter//
        var shift = KeyHelper.IsKeyPressed(VirtualKey.Shift);
        if (e.Key == VirtualKey.Enter)
        {
            if (shift)
                currentTextbox.FindPrevious();
            else
                currentTextbox.FindNext();
        }
        else if (e.Key == VirtualKey.Escape)
        {
            Close();
        }
    }
    private void SearchUpButton_Click(object sender, RoutedEventArgs e)
    {
        currentTextbox.FindPrevious();
    }
    private void SearchDownButton_Click(object sender, RoutedEventArgs e)
    {
        currentTextbox.FindNext();
    }
    private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
    {
        var res = currentTextbox.ReplaceAll(
            textToFindTextbox.Text,
            textToReplaceTextBox.Text,
            FindMatchCaseButton.IsChecked ?? false,
            FindWholeWordButton.IsChecked ?? false
            );

        ColorWindowBorder(res);
    }
    private void ReplaceCurrentButton_Click(object sender, RoutedEventArgs e)
    {
        var res = currentTextbox.ReplaceNext(textToReplaceTextBox.Text);
        ColorWindowBorder(res);
    }
    private void SearchWindow_CloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
    private void ExpandSearchBoxForReplaceButton_Click(object sender, RoutedEventArgs e)
    {
        if (searchWindowState == SearchWindowState.Expanded)
            ShowSearch(currentTextbox);
        else
            ShowReplace(currentTextbox);
    }
    private void TextBoxes_GotFocus(object sender, RoutedEventArgs e)
    {
        (sender as TextBox)?.SelectAll();
    }

    private void TextToFindTextbox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSearch();
    }
    private void SearchProperties_Changed(object sender, RoutedEventArgs e)
    {
        UpdateSearch();
    }

    private void CollapseSearchAnimation_Completed(object sender, object e)
    {
        ToggleVisibility(false);
    }
    private void ExpandSearchAnimation_Completed(object sender, object e)
    {
        ToggleVisibility(true);
    }
    private void HideSearchAnimation_Completed(object sender, object e)
    {
        this.Visibility = Visibility.Collapsed;
    }
}
public enum SearchWindowState
{
    Expanded, //replace and search
    Default, //only search
    Hidden //not visible
}