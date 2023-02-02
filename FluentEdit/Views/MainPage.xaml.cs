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
using Windows.UI.Core;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core.Preview;
using Windows.Storage.AccessCache;
using Fastedit2.Helper;
using FluentEdit.Helper;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Hosting;
using System.Diagnostics;

namespace TextControlBox_DemoApp.Views
{
    public sealed partial class MainPage : Page
    {
        private const string UntitledFileName = "Untitled.txt";

        private bool UnsavedChanges = false;
        private CoreApplicationViewTitleBar coreTitleBar;
        private Encoding CurrentEncoding = Encoding.UTF8;
        private string FileToken = "";
        private string FileName = "";
        private bool FileIsDragDropped = false;
        DispatcherTimer InfobarCloseTimer = new DispatcherTimer();
        ApplicationView appView = ApplicationView.GetForCurrentView();

        public MainPage()
        {
            this.InitializeComponent();

            //event to handle closing
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += Application_OnCloseRequest;

            FileName = UntitledFileName;

            UpdateTitle();
            CustomTitleBar();
            CheckFirstStart();

            //Update the infobar
            textbox_ZoomChanged(textbox, 100);
            Infobar_LineEnding.Text = textbox.LineEnding.ToString();
            UpdateEncodingInfobar();
        }

        private void UpdateEncodingInfobar()
        {
            Infobar_Encoding.Content = EncodingHelper.GetEncodingName(CurrentEncoding);
        }
        private void CheckFirstStart()
        {
            if (AppSettings.GetSettings("FirstStart") == "")
            {
                //DragDropFile_Info = (TeachingTip)FindName("DragDropFile_Info");
                AppSettings.SaveSettings("FirstStart", "1");
            }
        }
        private void ShowInfobar(InfoBarSeverity severity, string message, string title)
        {
            InfoDisplay.Message = message;
            InfoDisplay.Title = title;
            InfoDisplay.Severity = severity;
            InfoDisplay.IsOpen = true;

            InfobarCloseTimer.Interval = new TimeSpan(0, 0, 4);
            InfobarCloseTimer.Start();
            InfobarCloseTimer.Tick += delegate
            {
                InfoDisplay.IsOpen = false;
                InfobarCloseTimer.Stop();
            };
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
            //items already added
            if (LanguagesMenubarItem.Items.Count > 1)
                return;

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
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = this.ActualTheme
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
            titleDisplay.Text = (appView.Title = (UnsavedChanges ? "*" : "") + FileName) + " - FluentEdit";
            FileNameDisplay.Text = Infobar_FileNameInput.Text = FileName;
        }

        public async Task<(string text, Encoding encoding, bool Succed)> ReadTextFromFileAsync(StorageFile file, Encoding encoding = null)
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
                ShowInfobar(InfoBarSeverity.Error, "No access to read from file", "No access");

            }
            catch (Exception ex)
            {
                ShowInfobar(InfoBarSeverity.Error, ex.Message, "Read file exception");
            }
            return (null, Encoding.Default, false);
        }
        public async Task<bool> WriteTextToFileAsync(StorageFile file, string text, Encoding encoding)
        {
            try
            {
                if (file == null)
                    return false;

                //Only do this for drag/dropped files
                if (FileIsDragDropped)
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
                ShowInfobar(InfoBarSeverity.Error, "No access to write to file", "No access");
            }
            catch (Exception ex)
            {
                ShowInfobar(InfoBarSeverity.Error, ex.Message, "Write file exception");
            }
            return false;
        }
        private async Task<bool> SaveFile(bool ForceSaveNew = false)
        {
            if (FileToken.Length == 0 || ForceSaveNew)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

                //Add the extension of the current file
                savePicker.FileTypeChoices.Add("Current extension", new List<string>() { Path.GetExtension(FileName) });
                //Add the other extensions
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

                 savePicker.SuggestedFileName = FileName;

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);
                    await WriteTextToFileAsync(file, textbox.GetText(), CurrentEncoding);
                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        //delete the old file permission and add a new one
                        if (FileToken.Length > 0)
                            StorageApplicationPermissions.FutureAccessList.Remove(FileToken);
                        FileToken = StorageApplicationPermissions.FutureAccessList.Add(file);
                     
                        FileName = file.Name;
                        UnsavedChanges = false;
                        UpdateTitle();
                        return true;
                    }
                }
            }
            else
            {
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(FileToken))
                {
                    StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
                    FileName = file.Name;

                    var res = await WriteTextToFileAsync(file, textbox.GetText(), CurrentEncoding);
                    if (res == true)
                    {
                        UnsavedChanges = false;
                        UpdateTitle();
                    }
                    return res;
                }
                else
                    return await SaveFile(true);
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
                    UpdateEncodingInfobar();

                    textbox.LoadText(res.text);
                    Infobar_LineEnding.Text = textbox.LineEnding.ToString();
                    textbox.ScrollLineIntoView(0);
                    SetPositionInInfobar(0, 0);
                    UnsavedChanges = false;

                    UpdateTitle();
                }
            }
        }
        private void SelectCodeLanguageByFile(StorageFile file)
        {
            if (file == null)
                return;

            string extension = file.FileType.ToLower();

            //search through the dictionary of codelanguages in the textbox
            foreach (var item in TextControlBox.TextControlBox.CodeLanguages)
            {
                for (int i = 0; i < item.Value.Filter.Length; i++)
                {
                    if (item.Value.Filter[i].Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        textbox.CodeLanguage = item.Value;
                        return;
                    }
                }
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
        private void SetPositionInInfobar(int line, int charPos)
        {
            Infobar_Cursor.Content = "Ln: " + (line + 1) + ", Col:" + charPos;
        }
        private async void Renamefile(string newName)
        {
            string newFileName = Infobar_FileNameInput.Text;

            Infobar_RenameFile.IsEnabled = newFileName.Length > 0;
            if (newFileName.Length < 1)
                return;

            //File has been saved or opened
            if (FileToken.Length > 0)
            {
                var currentFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
                if (currentFile != null)
                {
                    //Nothing to rename
                    if (currentFile.Name == newFileName)
                        return;

                    //Check if the file already exists
                    bool FileAlreadyExists = Infobar_RenameFile.IsEnabled = !Directory.Exists(Path.Combine(Path.GetDirectoryName(currentFile.Path), newFileName));

                    if (FileAlreadyExists)
                    {
                        ShowInfobar(InfoBarSeverity.Error, "A file with this name already exists\nor there is no access to the path", "File exists/no access");
                        return;
                    }

                    try
                    {
                        await currentFile.RenameAsync(newFileName, NameCollisionOption.FailIfExists);
                        FileToken = StorageApplicationPermissions.FutureAccessList.Add(currentFile);
                    }
                    catch (Exception ex)
                    {
                        ShowInfobar(InfoBarSeverity.Error, ex.Message, "Exception");
                        return;
                    }

                }
            }
            FileName = newFileName;
            UpdateTitle();
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
            {
                e.Handled = true;
                return;
            }

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
            FileIsDragDropped = false;
            await OpenFile(await picker.PickSingleFileAsync());
        }
        private async void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (await CheckUnsavedChanges())
                return;
            FileIsDragDropped = false;
            FileName = UntitledFileName;
            FileToken = "";
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
        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item)
            {
                textbox.CodeLanguage = item.Tag == null ? null : TextControlBox.TextControlBox.GetCodeLanguageFromId(item.Tag.ToString());

                textbox.Focus(FocusState.Programmatic);
            }
        }
        private void DuplicateLine_Click(object sender, RoutedEventArgs e)
        {
            textbox.DuplicateLine(textbox.CurrentLineIndex);
            textbox.Focus(FocusState.Programmatic);
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
                textbox.Focus(FocusState.Programmatic);
            }
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Visibility == Visibility.Visible)
            {
                SearchBox.Visibility = Visibility.Collapsed;
                return;
            }


            OpenSearch();
        }

        private void textbox_TextChanged(TextControlBox.TextControlBox sender)
        {
            UnsavedChanges = true;
            UpdateTitle();
        }
        private void textbox_ZoomChanged(TextControlBox.TextControlBox sender, int ZoomFactor)
        {
            Infobar_Zoom.Content = ZoomFactor + "%";
            ZoomSlider_ValueChanged(null, null);
        }
        private void textbox_SelectionChanged(TextControlBox.TextControlBox sender, TextControlBox.Text.SelectionChangedEventHandler args)
        {
            SetPositionInInfobar(args.LineNumber, args.CharacterPositionInLine);
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

                    FileIsDragDropped = true;
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
        private void ZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ZoomSlider.Tag != null && (ZoomSlider.Tag.ToString() ?? "") == "LOCK")
            {
                ZoomSlider.Tag = "";
                return;
            }

            //called by the textbox event:
            if (sender == null)
            {
                ZoomSlider.Value = textbox.ZoomFactor;
                ZoomSlider.Tag = "LOCK";
            }
            else
                textbox.ZoomFactor = (int)ZoomSlider.Value;
        }

        private void ZoomSlider_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            //reset to default
            ZoomSlider.Value = 100;
        }
        private void Infobar_Zoom_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
            textbox.ZoomFactor += delta / 20;
        }

        //Infobar-FileName:
        private async void FileNameDisplay_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            try
            {
                args.AllowedOperations = DataPackageOperation.Copy;

                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(FileName == "" ? ".txt" : FileName, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, textbox.GetText());

                args.Data.SetStorageItems(new IStorageItem[] { file });
            }
            catch
            {
                ShowInfobar(InfoBarSeverity.Error, "Could not drag the document", "Drag file error");
            }
        }
        private void Infobar_RenameFile_Click(object sender, RoutedEventArgs e)
        {
            Renamefile(Infobar_FileNameInput.Text);
        }
        private async void Infobar_FileNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newFileName = Infobar_FileNameInput.Text;
            
            Infobar_RenameFile.IsEnabled = newFileName.Length > 0;
            if (newFileName.Length < 1)
                return;

            //File has been saved or opened
            if (FileToken.Length > 0)
            {
                var currentFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(FileToken);
                if (currentFile != null)
                {
                    Infobar_RenameFile.IsEnabled = !Directory.Exists(Path.Combine(Path.GetDirectoryName(currentFile.Path), newFileName));
                }
            }
        }

        //refocus the textbox after the flyout closes
        private void Flyout_Closed(object sender, object e)
        {
            textbox.Focus(FocusState.Programmatic);
        }

        //Infobar-Gotoline flyout:
        private void Infobar_GoToLine_Click(object sender, RoutedEventArgs e)
        {
            int line = (int)Infobar_GoToLineTextbox.Value - 1;

            textbox.GoToLine(line);
            textbox.ScrollLineIntoView(textbox.CursorPosition.LineNumber);
            textbox.ClearSelection();
            textbox.Focus(FocusState.Programmatic);

            Infobar_GoToLineFlyout.Hide();
        }    
        private void Infobar_GoToLineFlyout_Opened(object sender, object e)
        {
            Infobar_GoToLineTextbox.Maximum = textbox.NumberOfLines;
            Infobar_GoToLineTextbox.Focus(FocusState.Programmatic);
        }
        private void GoToLineTextbox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Infobar_GoToLine_Click(sender, null);
            }
        }

        //Infobar-Encodings:
        private void Infobar_Encoding_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mfi && mfi.Tag != null)
            {
                CurrentEncoding = EncodingHelper.GetEncodingByIndex(ConvertHelper.ToInt(mfi.Tag));
                UpdateEncodingInfobar();
            }
        }

        private async void CompactOverlay_Click(object sender, RoutedEventArgs e)
        {
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(
              ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.Default ? ApplicationViewMode.CompactOverlay : ApplicationViewMode.Default);
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (!ApplicationView.GetForCurrentView().IsFullScreenMode)
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            else
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
        }
    }
}