using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using FluentEdit.Helper;
using FluentEdit.Core;
using FluentEdit.Dialogs;
using FluentEdit.Storage;
using FluentEdit.Controls;
using Windows.ApplicationModel;
using FluentEdit.Models;
using TextControlBoxNS;
using System.Threading.Tasks;
using FluentEdit.Core.Settings;

namespace FluentEdit.Views;

public sealed partial class MainPage : Page
{
    private const string UntitledFileName = "Untitled.txt";
    private TextDocument textDocument = new TextDocument();
    public StatusBar StatusBar => this.statusBar; 
    
    public MainPage()
    {
        this.InitializeComponent();

        StatusBar.Init(textbox, textDocument);
        this.Loaded += MainPage_Loaded;
    }

    private async void textbox_Loaded(TextControlBox sender)
    {
        await NewFile(false);

        sender.Focus(FocusState.Programmatic);
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        App.m_window.AppWindow.Closing += AppWindow_Closing;

        WindowTitleHelper.UpdateTitle(textDocument);
        CheckFirstStart();
        CheckNewVersion();

        textDocument.FileName = UntitledFileName;
    }

    private void CheckFirstStart()
    {
        if (AppSettings.FirstStart)
        {
            AppSettings.FirstStart = false;
        }
    }
    private void CheckNewVersion()
    {
        string version = Package.Current.Id.Version.Major + "." + Package.Current.Id.Version.Minor + "." + Package.Current.Id.Version.Build;
        if (!(AppSettings.AppVersion).Equals(version))
        {
            AppSettings.AppVersion = version;
            //TODO!
            //NewVersionInfobar = FindName("NewVersionInfobar") as NewVersionInfobar;
            //NewVersionInfobar.Show(version);
        }
    }

    private bool IsContentDialogOpen()
    {
        var openedpopups = VisualTreeHelper.GetOpenPopups(App.m_window);
        for (int i = 0; i < openedpopups.Count; i++)
        {
            if (openedpopups[i].Child is ContentDialog)
            {
                return true;
            }
        }
        return false;
    }
    private void CreateMenubarFromLanguage()
    {
        //items already added
        if (LanguagesMenubarItem.Items.Count > 1)
            return;

        foreach(var item in TextControlBox.SyntaxHighlightings)
        {
            var menuItem = new MenuFlyoutItem
            {
                Text = item.Value == null ? "None" : item.Value.Name,
                Tag = item.Key.GetHashCode(),
            };
            menuItem.Click += SyntaxHighlighting_Clicked;
            LanguagesMenubarItem.Items.Add(menuItem);

            var runCommandWindowItem = new QuickAccessItem
            {
                Command = item.Value == null ? "None" : item.Value.Name,
                Tag = item.Key.GetHashCode(),
            };
            runCommandWindowItem.RunCommandWindowItemClicked += SyntaxHighlighting_Clicked;
            RunCommandWindowItem_CodeLanguages.Items.Add(runCommandWindowItem);
        }
    }
    private void ApplySettings()
    {
        textbox.FontFamily = new FontFamily(AppSettings.FontFamily);
        textbox.FontSize = AppSettings.FontSize;

        textbox.RequestedTheme = ThemeHelper.CurrentTheme = AppSettings.Theme;

        App.m_window.backdropManager.SetBackdrop(AppSettings.BackgroundType);
    }

    private async Task NewFile(bool checkUnsaved = true)
    {
        if (checkUnsaved && await AskSaveDialog.CheckUnsavedChanges(this, textDocument, textbox))
            return;

        statusBar.SetZoom(textbox.ZoomFactor);
        statusBar.UpdateLineEndings();

        textDocument.FileName = UntitledFileName;
        textDocument.FilePath = "";
        textDocument.UnsavedChanges = false;
        WindowTitleHelper.UpdateTitle(textDocument);
        textbox.LoadText("");
    }

    public void SelectSyntaxHighlightingByFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        string extension = Path.GetExtension(filePath).ToLower();

        //search through the dictionary of syntax highlights in the textbox
        foreach (var item in TextControlBox.SyntaxHighlightings)
        {
            for (int i = 0; i < item.Value?.Filter.Length; i++)
            {
                if (item.Value.Filter[i].Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    textbox.SyntaxHighlighting = item.Value;
                    return;
                }
            }
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ApplySettings();
        CreateMenubarFromLanguage();
        statusBar.UpdateWordCharacterCount();

        //if (e.Parameter is IReadOnlyList<IStorageItem> files)
        //{
        //    if(files.Count >= 1)
        //        await OpenFileHelper.OpenFile(this, textDocument, textbox, files[0]);
        //}
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (IsContentDialogOpen())
        {
            args.Cancel = true;
            return;
        }

        if (await AskSaveDialog.CheckUnsavedChanges(this, textDocument, textbox))
            args.Cancel = true;
    }

    private async void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        await OpenFileHelper.OpenFile(this, textDocument, textbox);
    }
    private async void NewFile_Click(object sender, RoutedEventArgs e)
    {
        NewFile();
    }
    private async void SaveFile_Click(object sender, RoutedEventArgs e)
    {
        await SaveFileHelper.SaveFile(this, textDocument, textbox, false);
    }
    private async void SaveFileAs_Click(object sender, RoutedEventArgs e)
    {
        await SaveFileHelper.SaveFile(this, textDocument, textbox, true);
    }
    private async void ExitApp_Click(object sender, RoutedEventArgs e)
    {
        if (IsContentDialogOpen())
            return;

        if (await AskSaveDialog.CheckUnsavedChanges(this, textDocument, textbox))
            return;

        App.m_window.Close();
    }
    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        textbox.Undo();
        textbox.Focus(FocusState.Programmatic);
    }
    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        textbox.Redo();
        textbox.Focus(FocusState.Programmatic);
    }
    private void Cut_Click(object sender, RoutedEventArgs e)
    {
        textbox.Cut();
        textbox.Focus(FocusState.Programmatic);
    }
    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        textbox.Copy();
        textbox.Focus(FocusState.Programmatic);
    }
    private void Paste_Click(object sender, RoutedEventArgs e)
    {
        textbox.Paste();
        textbox.Focus(FocusState.Programmatic);
    }
    private void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        textbox.SelectAll();
        textbox.Focus(FocusState.Programmatic);
    }
    private void SyntaxHighlighting_Clicked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item)
        {
            textbox.SelectSyntaxHighlightingById((SyntaxHighlightID)ConvertHelper.ToInt(item.Tag, 0));
            textbox.Focus(FocusState.Programmatic);
        }
        else if (sender is QuickAccessItem rcwitem)
        {
            textbox.SelectSyntaxHighlightingById((SyntaxHighlightID)ConvertHelper.ToInt(rcwitem.Tag, 0));
        }
    }
    private void DuplicateLine_Click(object sender, RoutedEventArgs e)
    {
        textbox.DuplicateLine(textbox.CurrentLineIndex);
        textbox.Focus(FocusState.Programmatic);
    }
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        App.m_window.ShowSettings();
    }
    private void TabMode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item)
        {
            textbox.UseSpacesInsteadTabs = item.Tag.ToString().Equals("0");
            textbox.Focus(FocusState.Programmatic);
        }
    }
    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        textbox.ZoomFactor += 5;
    }
    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        textbox.ZoomFactor -= 5;
    }
    private void Search_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.ShowSearch(textbox);
    }
    private void Replace_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.ShowReplace(textbox);
    }
    private async void Rename_Click(object sender, RoutedEventArgs e)
    {
        await RenameDialog.ShowAsync(textDocument);
    }

    private void CompactOverlay_Click(object sender, RoutedEventArgs e)
    {
        WindowHelper.ToggleCompactOverlay(App.m_window);
    }
    private void Fullscreen_Click(object sender, RoutedEventArgs e)
    {
        WindowHelper.ToggleFullscreen(App.m_window);
    }
    private async void ShowFileInfo_Click(object sender, RoutedEventArgs e)
    {
        await FileInfoDialog.Show(textDocument, textbox);
    }
    private void ShowQuickAccess_Click(object sender, RoutedEventArgs e)
    {
        //QuickAccess.Show();
    }


    private void textbox_TextChanged(TextControlBox sender)
    {
        if (!textDocument.UnsavedChanges)
        {
            textDocument.UnsavedChanges = true;
            WindowTitleHelper.UpdateTitle(textDocument);
        }

        statusBar.UpdateWordCharacterCount();
    }
    private void textbox_ZoomChanged(TextControlBox sender, int ZoomFactor)
    {
        statusBar.SetZoom(ZoomFactor);
    }
    private void textbox_SelectionChanged(TextControlBox sender, TextControlBoxNS.SelectionChangedEventHandler args)
    {
        statusBar.SetPosition(sender.CursorPosition.LineNumber + 1, sender.CursorPosition.CharacterPosition);
    }

    private void Page_ActualThemeChanged(FrameworkElement sender, object args)
    {
        textbox.RequestedTheme = ThemeHelper.CurrentTheme;
    }
    private async void Page_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var files = await e.DataView.GetStorageItemsAsync();
            if (files.Count >= 1)
            {
                if (await AskSaveDialog.CheckUnsavedChanges(this, textDocument, textbox))
                    return;

                OpenFileHelper.OpenFile(this, textDocument, textbox, files[0].Path);
            }
        }
    }
    private void Page_DragEnter(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }
}
