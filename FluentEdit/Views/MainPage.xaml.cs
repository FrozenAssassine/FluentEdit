using System;
using System.Collections.Generic;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core.Preview;
using Windows.Storage.AccessCache;
using FluentEdit.Helper;
using FluentEdit.Core;
using FluentEdit.Dialogs;
using FluentEdit.Storage;
using FluentEdit.Controls;
using Windows.ApplicationModel;
using FluentEdit.Models;

namespace TextControlBox_DemoApp.Views
{
    public sealed partial class MainPage : Page
    {
        private const string UntitledFileName = "Untitled.txt";
        private TextDocument document = new TextDocument();

        private CoreApplicationViewTitleBar coreTitleBar;
        public bool FileIsDragDropped = false;
        DispatcherTimer InfobarCloseTimer = new DispatcherTimer();
        ApplicationView appView = ApplicationView.GetForCurrentView();

        public MainPage()
        {
            this.InitializeComponent();

            //event to handle closing
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += Application_OnCloseRequest;

            document.FileName = UntitledFileName;

            UpdateTitle();
            CustomTitleBar();
            CheckFirstStart();
            CheckNewVersion();

            //Update the infobar
            textbox_ZoomChanged(textbox, 100);
            Infobar_LineEnding.Text = textbox.LineEnding.ToString();
            UpdateEncodingInfobar();
        }

        private void CheckFirstStart()
        {
            if (AppSettings.GetSettings("FirstStart") == "")
            {
                AppSettings.SaveSettings("FirstStart", "1");
            }
        }
        private void CheckNewVersion()
        {
            string version = Package.Current.Id.Version.Major + "." + Package.Current.Id.Version.Minor + "." + Package.Current.Id.Version.Build;
            if (!((AppSettings.GetSettings("Version") ?? "").Equals(version)))
            {
                AppSettings.SaveSettings("Version", version);
                NewVersionInfobar = FindName("NewVersionInfobar") as NewVersionInfobar;
                NewVersionInfobar.Show(version);
            }
        }
        public void ShowInfobar(InfoBarSeverity severity, string message, string title)
        {
            InfoDisplay.Message = message;
            InfoDisplay.Title = title;
            InfoDisplay.Severity = severity;
            InfoDisplay.IsOpen = true;

            InfobarCloseTimer.Interval = new TimeSpan(0, 0, 6);
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

                var runCommandWindowItem = new QuickAccessItem
                {
                    Command = item.Value.Name,
                    Tag = item.Key,
                };
                runCommandWindowItem.RunCommandWindowItemClicked += Language_Click;
                RunCommandWindowItem_CodeLanguages.Items.Add(runCommandWindowItem);
            }
            var noneCmdWindowItem = new QuickAccessItem
            {
                Command = "None",
                Tag = "",
            };
            noneCmdWindowItem.RunCommandWindowItemClicked += Language_Click;
            RunCommandWindowItem_CodeLanguages.Items.Add(noneCmdWindowItem);
        }
        private void ApplySettings()
        {
            textbox.FontFamily = new FontFamily(AppSettings.GetSettings("fontFamily", "Consolas"));
            textbox.FontSize = AppSettings.GetSettingsAsInt("fontSize", 18);

            textbox.RequestedTheme = ThemeHelper.CurrentTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), AppSettings.GetSettingsAsInt("theme", 0).ToString());

            int backgroundIndex = AppSettings.GetSettingsAsInt("BackgroundType");
            if(backgroundIndex == 0)
            {
                BackgroundHelper.SetBackground(this, null, 0);
            }
            else if(backgroundIndex == 1)
            {
                var color = ColorConverter.ToColorWithAlpha(AppSettings.GetSettings("AcrylicBackground"), Color.FromArgb(150, 25,25,25));
                BackgroundHelper.SetBackground(this, color, 1);
            }
            else if(backgroundIndex == 2)
            {
                var color = ColorConverter.ToColor(AppSettings.GetSettings("StaticBackground"), Color.FromArgb(255, 25, 25, 25));
                BackgroundHelper.SetBackground(this, color, 2);
            }
        }

        private void CustomTitleBar()
        {
            coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            ApplicationViewTitleBar titleBar =
                ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            Window.Current.SetTitleBar(Titlebar);

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
        }

        public void UpdateWordCharacterCount()
        {
            int charCount = textbox.CharacterCount;

            Infobar_WordCount.Text = "W: " + (charCount < 5_000_000 ? CountWordsHelper.CountWordsSpan(textbox.Lines).ToString() : "");
            Infobar_CharacterCount.Text = "C: " + textbox.CharacterCount;
        }
        public void UpdateLineEndings()
        {
            Infobar_LineEnding.Text = textbox.LineEnding.ToString();
        }
        public void UpdateTitle()
        {
            titleDisplay.Text = (appView.Title = (document.UnsavedChanges ? "*" : "") + document.FileName) + " - FluentEdit";
            FileNameDisplay.Text = Infobar_FileNameInput.Text = document.FileName;
        }
        public void UpdateEncodingInfobar()
        {
            Infobar_Encoding.Content = EncodingHelper.GetEncodingName(document.CurrentEncoding);
        }
        public void SetPositionInInfobar(int line, int charPos)
        {
            Infobar_Cursor.Content = "Ln: " + (line + 1) + ", Col:" + charPos;
        }
        private async void Renamefile(string newName)
        {
            if (document.FileToken.Length == 0)
            {
                document.FileName = newName;
                UpdateTitle();
                return;
            }

            //File has been saved or opened
            var currentFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(document.FileToken);
            if (currentFile != null)
            {
                //Nothing to rename
                if (currentFile.Name == newName)
                    return;

                //Check if the file already exists
                bool FileAlreadyExists = Infobar_RenameFile.IsEnabled = !Directory.Exists(Path.Combine(Path.GetDirectoryName(currentFile.Path), newName));

                if (FileAlreadyExists)
                {
                    ShowInfobar(InfoBarSeverity.Error, "A file with this name already exists\nor there is no access to the path", "File exists/no access");
                    return;
                }

                try
                {
                    await currentFile.RenameAsync(newName, NameCollisionOption.FailIfExists);
                    document.FileToken = StorageApplicationPermissions.FutureAccessList.Add(currentFile);
                }
                catch (Exception ex)
                {
                    ShowInfobar(InfoBarSeverity.Error, ex.Message, "Exception");
                    return;
                }
            }

            document.FileName = newName;
            UpdateTitle();
        }

        public void SelectCodeLanguageByFile(StorageFile file)
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ApplySettings();
            CustomTitleBar();
            CreateMenubarFromLanguage();
            UpdateWordCharacterCount();

            if (e.Parameter is IReadOnlyList<IStorageItem> files)
            {
                if(files.Count >= 1)
                    await OpenFileHelper.OpenFile(this, document, textbox, files[0] as StorageFile);
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

            if (await AskSaveDialog.CheckUnsavedChanges(this, document, textbox))
                e.Handled = true;

            deferral.Complete();
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            await OpenFileHelper.OpenFile(this, document, textbox);
        }
        private async void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (await AskSaveDialog.CheckUnsavedChanges(this, document, textbox))
                return;
            FileIsDragDropped = false;
            document.FileName = UntitledFileName;
            document.FileToken = "";
            document.UnsavedChanges = false;
            UpdateTitle();
            textbox.LoadText("");
        }
        private async void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            await SaveFileHelper.SaveFile(this, document, textbox, false);
        }
        private async void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            await SaveFileHelper.SaveFile(this, document, textbox, true);
        }
        private async void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            if (IsContentDialogOpen())
                return;

            if (await AskSaveDialog.CheckUnsavedChanges(this, document, textbox))
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
            else if (sender is QuickAccessItem rcwitem)
            {
                if (rcwitem != null && rcwitem.Tag != null)
                {
                    if (rcwitem.Tag.ToString().Length == 0)
                        textbox.CodeLanguage = null;

                    textbox.CodeLanguage = TextControlBox.TextControlBox.GetCodeLanguageFromId(rcwitem.Tag.ToString());
                }
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
            var res = await RenameDialog.ShowAsync(document.FileName);
            if (!res.res)
                return;

            Renamefile(res.newName);
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
        private async void ShowFileInfo_Click(object sender, RoutedEventArgs e)
        {
            await FileInfoDialog.Show(document, textbox);
        }
        private void ShowQuickAccess_Click(object sender, RoutedEventArgs e)
        {
            QuickAccess.Show();
        }


        private void textbox_TextChanged(TextControlBox.TextControlBox sender)
        {
            document.UnsavedChanges = true;
            UpdateTitle();
            UpdateWordCharacterCount();
        }
        private void textbox_ZoomChanged(TextControlBox.TextControlBox sender, int ZoomFactor)
        {
            Infobar_Zoom.Content = ZoomFactor + "%";
            ZoomSlider_ValueChanged(null, null);
        }
        private void textbox_SelectionChanged(TextControlBox.TextControlBox sender, TextControlBox.SelectionChangedEventHandler args)
        {
            SetPositionInInfobar(args.LineNumber, args.CharacterPositionInLine);
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
                    if (await AskSaveDialog.CheckUnsavedChanges(this, document, textbox))
                        return;

                    FileIsDragDropped = true;
                    await OpenFileHelper.OpenFile(this, document, textbox, files[0] as StorageFile);
                }
            }
        }
        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
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

                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(document.FileName == "" ? ".txt" : document.FileName, CreationCollisionOption.OpenIfExists);
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
            if (document.FileToken.Length > 0)
            {
                var currentFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(document.FileToken);
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
                document.CurrentEncoding = EncodingHelper.GetEncodingByIndex(ConvertHelper.ToInt(mfi.Tag));
                UpdateEncodingInfobar();
            }
        }
    }
}
