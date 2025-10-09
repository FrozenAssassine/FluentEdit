using FluentEdit.Helper;
using FluentEdit.Models;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace FluentEdit.Core.Settings;

public class AppSettings
{
    public static bool FirstStart
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.FirstStart, 0) == 0;
        set => SettingsManager.SaveSettings(AppSettingsValues.FirstStart, value);
    }
    public static string FontFamily
    {
        get => SettingsManager.GetSettings(AppSettingsValues.FontFamily, DefaultValues.FontFamily);
        set => SettingsManager.SaveSettings(AppSettingsValues.FontFamily, value);
    }
    public static int FontSize
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.FontSize, DefaultValues.FontSize);
        set => SettingsManager.SaveSettings(AppSettingsValues.FontSize, value);
    }
    public static ElementTheme Theme
    {
        get => (ElementTheme)SettingsManager.GetSettingsAsInt(AppSettingsValues.Theme, DefaultValues.Theme);
        set => SettingsManager.SaveSettings(AppSettingsValues.Theme, value.GetHashCode());
    }
    public static BackgroundType BackgroundType
    {
        get => (BackgroundType)SettingsManager.GetSettingsAsInt(AppSettingsValues.BackgroundType, DefaultValues.BackgroundType);
        set => SettingsManager.SaveSettings(AppSettingsValues.BackgroundType, value.GetHashCode());
    }

    public static string AppVersion
    {
        get => SettingsManager.GetSettings(AppSettingsValues.AppVersion, "");
        set => SettingsManager.SaveSettings(AppSettingsValues.AppVersion, value);
    }
    public static Color AcrylicBackground
    {
        get => ColorConverter.ToColorWithAlpha(
            SettingsManager.GetSettings(AppSettingsValues.AcrylicBackground),
            DefaultValues.DefaultAcrylicBackground
            );
        set => SettingsManager.SaveSettings(AppSettingsValues.AcrylicBackground, ColorConverter.ToStringWithAlpha(value));
    }
    public static Color StaticBackground
    {
        get => ColorConverter.ToColor(
            SettingsManager.GetSettings(AppSettingsValues.StaticBackground),
            DefaultValues.DefaultStaticBackground
            );
        set => SettingsManager.SaveSettings(AppSettingsValues.StaticBackground, ColorConverter.ToString(value));
    }

    public static bool HideDonationInfo
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.HideDonationInfo, 0) == 1;
        set => SettingsManager.SaveSettings(AppSettingsValues.HideDonationInfo, value == true ? 1 : 0);
    }

    public static int WindowWidth
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.windowWidth, DefaultValues.windowWidth);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowWidth, value);
    }
    public static int WindowHeight
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.windowHeight, DefaultValues.windowHeight);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowHeight, value);
    }
    public static int WindowLeft
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.windowLeft, DefaultValues.windowLeft);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowLeft, value);
    }
    public static int WindowTop
    {
        get => SettingsManager.GetSettingsAsInt(AppSettingsValues.windowTop, DefaultValues.windowTop);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowTop, value);
    }
    public static OverlappedPresenterState WindowState
    {
        get => (OverlappedPresenterState)SettingsManager.GetSettingsAsInt(AppSettingsValues.windowState, 2);
        set => SettingsManager.SaveSettings(AppSettingsValues.windowState, value.GetHashCode());
    }
}
