using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using static System.Net.WebRequestMethods;
using Windows.UI.Core;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using Windows.Storage.Streams;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core.Preview;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using TextControlBox.Helper;
using Windows.Storage.AccessCache;

namespace TextControlBox_DemoApp.Views
{
    public sealed partial class MainPage : Page
    {
        private bool UnsavedChanges = false;
        private CoreApplicationViewTitleBar coreTitleBar;
        private Encoding CurrentEncoding = Encoding.UTF8;
        private string FileToken = "";
        private string FileName = "";
        ApplicationView appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();

        public MainPage()
        {
            this.InitializeComponent();

            //event to handle closing
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += Application_OnCloseRequest;

            UpdateTitle();
            CustomTitleBar();

            //Update the infobar
            textbox_ZoomChanged(textbox, 100);
            Infobar_Encoding.Text = CurrentEncoding.EncodingName;
            Infobar_LineEnding.Text = textbox.LineEnding.ToString();
        }

        private bool IsContentDialogOpen()
        {
            var openedpopups = VisualTreeHelper.GetOpenPopups(Window.Current);
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
            foreach(var item in TextControlBox.TextControlBox.CodeLanguages)
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = item.Value.Name,
                    Tag = item.Key,
                };
                menuItem.Click += Language_Click;
                LanguagesMenubarItem.Items.Add(menuItem);
            }
        }

        private void ApplySettings()
        {
            textbox.FontFamily = new FontFamily(AppSettings.GetSettings("fontFamily") ?? "Consolas");
            textbox.FontSize = AppSettings.GetSettingsAsInt("fontSize", 18);

            if (Window.Current.Content is FrameworkElement rootElement)
            {
                textbox.RequestedTheme = rootElement.RequestedTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), AppSettings.GetSettingsAsInt("theme", 0).ToString());
            }
        }
        private void CustomTitleBar()
        {
            // Hide default title bar.
            coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // Set caption buttons background to transparent.
            ApplicationViewTitleBar titleBar =
                ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Set XAML element as a drag region.
            Window.Current.SetTitleBar(Titlebar);

            // Register a handler for when the size of the overlaid caption control changes.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
        }
        private async Task<bool> CheckUnsavedChanges()
        {
            if (!UnsavedChanges)
                return false;

            var SaveDialog = new ContentDialog
            {
                Title = "Save file?",
                Content = "Would you like to save the file?",
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Don't save",
                CloseButtonText = "Cancel",
            };
            var res = await SaveDialog.ShowAsync();
            if(res == ContentDialogResult.Primary)
                return !await SaveFile();
            else if (res == ContentDialogResult.None)
                return true;
            return false;
        }
        private void UpdateTitle()
        {
            if (FileToken.Length == 0)
                titleDisplay.Text = (appView.Title = (UnsavedChanges ? "*" : "") + "Untitled.txt") + " - FluentEdit";
            else
                titleDisplay.Text = (appView.Title = (UnsavedChanges ? "*" : "") + FileName) + " - FluentEdit";
        }
        public async Task<(string Text, Encoding encoding, bool Succed)> ReadTextFromFileAsync(StorageFile file, Encoding encoding = null)
        {
            try
            {
                if (file == null)
                    return ("", Encoding.Default, false);

                using (var stream = (await file.OpenReadAsync()).AsStreamForRead())
                {
                    //Detect the encoding:
                    using (var reader = new StreamReader(stream, true))
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);

                        reader.Read();
                        encoding = reader.CurrentEncoding;

                        return (encoding.GetString(buffer, 0, buffer.Length), encoding, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return ("", Encoding.Default, false);
            }
        }
        public async Task<bool> WriteTextToFileAsync(StorageFile file, string text, Encoding encoding)
        {
            try
            {
                if (file == null)
                    return false;

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (var writer = new StreamWriter(stream, encoding))
                    {
                        await writer.WriteAsync(text);
                        return true;
                    }
                }
            }
            catch (IOException)
            {
                try
                {
                    Debug.WriteLine("Different");
                    //try a different way of saving files
                    var bytestoWrite = encoding.GetBytes(text);
                    var buffer = encoding.GetPreamble().Concat(bytestoWrite).ToArray();
                    await PathIO.WriteBytesAsync(file.Path, buffer);
                    return true;
                }
                catch
                {
                    return await SaveFile(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        private async Task<bool> SaveFile(bool ForceSaveNew = false)
        {
            if (FileToken.Length == 0|| ForceSaveNew)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

                StorageFile OpenedFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
                //Add the extension of the current file
                if (OpenedFile != null)
                    savePicker.FileTypeChoices.Add("Current extension", new List<string>() { OpenedFile.FileType });
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

                if (OpenedFile != null)
                    savePicker.SuggestedFileName = OpenedFile.DisplayName;
                else
                savePicker.SuggestedFileName = "Untitled.txt";
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);
                    FileName = file.Name;
                    await WriteTextToFileAsync(file, textbox.GetText(), CurrentEncoding);
                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        OpenedFile = file;
                        UnsavedChanges = false;
                        UpdateTitle();
                        return true;
                    }
                }
            }
            else
            {
                StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
                FileName = file.Name;

                var res = await WriteTextToFileAsync(file, textbox.GetText(), CurrentEncoding);
                if(res == true)
                {
                    UnsavedChanges = false;
                    UpdateTitle();
                }
                return res;
            }
            return false;
        }
        private async Task OpenFile(StorageFile file)
        {
            if (file != null)
            {
                FileName = file.Name;
                FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);

                var res = await ReadTextFromFileAsync(file);
                if (res.Succed)
                {
                    SelectCodeLanguageByFile(file);
                    CurrentEncoding = res.encoding;
                    Infobar_Encoding.Text = CurrentEncoding.EncodingName;

                    textbox.LoadText(res.Text);
                    Infobar_LineEnding.Text = textbox.LineEnding.ToString();
                    UnsavedChanges = false;

                    UpdateTitle();
                }
            }
        }
        private void SelectCodeLanguageByFile(StorageFile file)
        {
            if (file == null)
                return;

            switch (file.FileType.ToLower())
            {
                case ".cpp":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("Cpp");
                    break;
                case ".html":
                case ".htm":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("Html");
                    break;
                case ".cs":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("CSharp");
                    break;
                case ".bat":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("Batch");
                    break;
                case ".json":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("Json");
                    break;
                case ".gcode":
                case ".nc":
                case ".cnc":
                case ".tap":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("GCode");
                    break;
                case ".js":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("Javascript");
                    break;
                case ".php":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("PHP");
                    break;
                case ".ini":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("ConfigFile");
                    break;
                case ".hex":
                case ".bin":
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId("HexFile");
                    break;
                default:
                    textbox.CodeLanguage = null;
                    break;
            }
        }
        private void OpenSearch()
        {
            SearchBox.Visibility = Visibility.Visible;
            SearchContent_Textbox.Text = "";
            SearchContent_Textbox.Focus(FocusState.Programmatic);
        }
        private void CloseSearch()
        {
            textbox.EndSearch();
            SearchBox.Visibility = Visibility.Collapsed;
            SearchContent_Textbox.Text = "";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ApplySettings();
            CustomTitleBar();
            CreateMenubarFromLanguage();

            if (e.Parameter is IReadOnlyList<IStorageItem> files)
            {
                if(files.Count >= 1)
                    await OpenFile(files[0] as StorageFile);
            }
        }

        //Titlebar events:
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // Get the size of the caption controls and set padding.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
        }
        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            Titlebar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        private async void Application_OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var deferral = e.GetDeferral();

            if (IsContentDialogOpen())
                e.Handled = true;

            if (await CheckUnsavedChanges())
                e.Handled = true;

            deferral.Complete();
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (await CheckUnsavedChanges())
                return;

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add("*");

            await OpenFile(await picker.PickSingleFileAsync());
        }
        private async void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (await CheckUnsavedChanges())
                return;

            FileName = FileToken = "";
            UnsavedChanges = false;
            UpdateTitle();
            textbox.LoadText("");
        }
        private async void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            await SaveFile(false);
        }
        private async void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            await SaveFile(true);
        }
        private async void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            if (IsContentDialogOpen())
                return;

            if (await CheckUnsavedChanges())
                return;

            await ApplicationView.GetForCurrentView().TryConsolidateAsync();
        }
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            textbox.Undo();
        }
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            textbox.Redo();
        }
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            textbox.Cut();
        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            textbox.Copy();
        }
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            textbox.Paste();
        }
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            textbox.SelectAll();
        }
        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                if (item.Tag.ToString() == "")
                    textbox.CodeLanguage = null;
                else
                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId(item.Tag.ToString());
            }
        }
        private void DuplicateLine_Click(object sender, RoutedEventArgs e)
        {
            textbox.DuplicateLine(textbox.CurrentLineIndex);
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }
        private void TabMode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                textbox.UseSpacesInsteadTabs = item.Tag.ToString().Equals("0");
            }
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            OpenSearch();
        }

        private void textbox_TextChanged(TextControlBox.TextControlBox sender, string Text)
        {
            UnsavedChanges = true;
            UpdateTitle();
        }
        private void textbox_ZoomChanged(TextControlBox.TextControlBox sender, int ZoomFactor)
        {
            Infobar_Zoom.Text = ZoomFactor + "%";
        }
        private void textbox_SelectionChanged(TextControlBox.TextControlBox sender, TextControlBox.Text.SelectionChangedEventHandler args)
        {
            Infobar_Cursor.Text = "Ln: " + (args.LineNumber + 1) + ", Col:" + args.CharacterPositionInLine;
        }

        private void Page_ActualThemeChanged(FrameworkElement sender, object args)
        {
            if (Window.Current.Content is FrameworkElement rootElement)
            {
                textbox.RequestedTheme = rootElement.RequestedTheme;
            }
        }
        private async void Page_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var files = await e.DataView.GetStorageItemsAsync();
                if (files.Count >= 1)
                {
                    if (await CheckUnsavedChanges())
                        return;

                    await OpenFile(files[0] as StorageFile);
                }
            }
        }
        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private void SearchContent_Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            textbox.BeginSearch(SearchContent_Textbox.Text, false, false);
        }
        private void SearchContent_Textbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var shift = Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (shift)
                    textbox.FindPrevious();
                else
                    textbox.FindNext();
            }
            else if (e.Key == Windows.System.VirtualKey.Escape)
            {
                CloseSearch();
            }
        }
        private void FindPrevious_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                textbox.FindPrevious();
            }
            catch (IndexOutOfRangeException)
            {

            }
        }
        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            textbox.FindNext();
        }
        private void CloseSearch_Click(object sender, RoutedEventArgs e)
        {
            CloseSearch();
        }
    }
}
    