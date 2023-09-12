using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;
using Windows.ApplicationModel;
using Windows.UI.Core.Preview;
using FluentEdit.Helper;
using static System.Net.WebRequestMethods;

namespace TextControlBox_DemoApp.Views
{
    public sealed partial class SettingsPage : Page
    {
        public List<string> Fonts
        {
            get
            {
                return CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();
            }
        }
        private CoreApplicationViewTitleBar coreTitleBar;
        public Uri donoURL = new Uri("https://www.paypal.com/donate?business=julius@frozenassassine.de&no_recurring=0&item_name=Support+FrozenAssassines+Work&currency_code=EUR");
        
        public SettingsPage()
        {
            this.InitializeComponent();
            CustomTitleBar();
            FillVersionDisplay();
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += SettingsPage_CloseRequested;

            themeCombobox.SelectedIndex = AppSettings.GetSettingsAsInt("theme");
            fontFamilyCombobox.SelectedIndex = AppSettings.GetSettingsAsInt("fontFamilyIndex", Fonts.IndexOf("Consolas"));
            fontSizeNumberBox.Value = AppSettings.GetSettingsAsInt("fontSize", 18);
        }

        private void SettingsPage_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            CurrentView_BackRequested(null, null);
        }

        private void FillVersionDisplay()
        {
            VersionDisplay.Text = "Version: " + Package.Current.Id.Version.Major + "." +
                    Package.Current.Id.Version.Minor + "." +
                    Package.Current.Id.Version.Build;
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

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested; // Event handler goes here
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            currentView.BackRequested -= CurrentView_BackRequested;

            App.TryGoBack();
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

        private void themeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.SaveSettings("theme", themeCombobox.SelectedIndex);

            ThemeHelper.CurrentTheme = (ElementTheme)Enum.Parse(typeof(ElementTheme), themeCombobox.SelectedIndex.ToString());
        }
        private void fontFamilyCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.SaveSettings("fontFamilyIndex", fontFamilyCombobox.SelectedIndex);
            AppSettings.SaveSettings("fontFamily", fontFamilyCombobox.SelectedItem.ToString() ?? "Consolas");
        }
        private void fontSizeNumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            AppSettings.SaveSettings("fontSize", fontSizeNumberBox.Value);
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
                App.TryGoBack();
        }
    }
}
