using Microsoft.Graphics.Canvas.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using FluentEdit.Helper;
using FluentEdit.Core.Settings;
using FluentEdit.Models;

namespace FluentEdit.Views
{
    public sealed partial class SettingsPage : Page
    {
        public List<string> Fonts => CanvasTextFormat.GetSystemFontFamilies().OrderBy(f => f).ToList();
        
        public SettingsPage()
        {
            this.InitializeComponent();


            themeCombobox.SelectedIndex = AppSettings.Theme.GetHashCode();
            var ff = AppSettings.FontFamily;
            fontFamilyCombobox.SelectedIndex = Fonts.IndexOf(ff);
            fontSizeNumberBox.Value = AppSettings.FontSize;
            appBackgroundCombobox.SelectedIndex = AppSettings.BackgroundType.GetHashCode();

            acrylicColorPicker.Color = AppSettings.AcrylicBackground;
            staticColorPicker.Color = AppSettings.StaticBackground;

            hideDonationInfosButton.IsOn = AppSettings.HideDonationInfo;
        }

        private void themeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemeHelper.CurrentTheme = AppSettings.Theme = (ElementTheme)themeCombobox.SelectedIndex;
        }
        private void fontFamilyCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.FontFamily = fontFamilyCombobox.SelectedItem.ToString() ?? DefaultValues.FontFamily;
        }
        private void fontSizeNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            AppSettings.FontSize = (int)fontSizeNumberBox.Value;
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
                App.m_window.ShowMainApp();
        }

        private void AppBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (appBackgroundCombobox.SelectedIndex == -1)
                return;

            if(appBackgroundCombobox.SelectedIndex == 0)
            {
                staticGrid.Visibility = acrylicGrid.Visibility = Visibility.Collapsed;
            }
            else if(appBackgroundCombobox.SelectedIndex == 1)
            {
                acrylicGrid.Visibility = Visibility.Visible;
                staticGrid.Visibility = Visibility.Collapsed;
            }
            else if (appBackgroundCombobox.SelectedIndex == 2)
            {
                staticGrid.Visibility = Visibility.Visible;
                acrylicGrid.Visibility = Visibility.Collapsed;
            }

            AppSettings.BackgroundType = (BackgroundType)appBackgroundCombobox.SelectedIndex;
        }

        private void acrylicColorPicker_ColorChanged(ColorPicker args)
        {
            if (!acrylicColorPicker.Color.HasValue)
                return;

            AppSettings.AcrylicBackground = acrylicColorPicker.Color.Value;
        }

        private void staticColorPicker_ColorChanged(ColorPicker args)
        {
            if (!staticColorPicker.Color.HasValue)
                return;

            AppSettings.StaticBackground = staticColorPicker.Color.Value;
        }

        private void hideDonationInfosButton_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.HideDonationInfo = hideDonationInfosButton.IsOn;
        }
    }
}
