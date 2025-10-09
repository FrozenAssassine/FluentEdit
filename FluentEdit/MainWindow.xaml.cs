using FluentEdit.Core;
using FluentEdit.Views;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using Windows.ApplicationModel;

namespace FluentEdit;

public sealed partial class MainWindow : Window
{
    public string[] ActivationArguments;
    public DispatcherQueue UIDispatcherQueue = null;
    public XamlRoot XamlRoot = null;

    public StackPanel InfoMessagesPanel;
    public readonly RestoreWindowManager restoreWindowManager;

    public IntPtr WindowHandle;

    public BackdropWindowManager backdropManager;
    public bool ShowBackArrow { get => navigateBackButton.Visibility == Visibility.Visible; set => navigateBackButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
    public Frame MainFrame => mainFrame;

    public MainWindow()
    {
        this.InitializeComponent();

        backdropManager = new BackdropWindowManager(this);

        restoreWindowManager = new RestoreWindowManager(this);
        restoreWindowManager.RestoreSettings();

        this.WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        UIDispatcherQueue = DispatcherQueue.GetForCurrentThread();
        InfoMessagesPanel = this.infoMessagesPanel;
        this.ExtendsContentIntoTitleBar = true;

        SetTitleBar(titleBarGrid);
        SetAppTitle("FluentEdit");
        
        this.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets\\AppIcon\\Icon.ico"));
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        XamlRoot = this.Content.XamlRoot;
        ShowMainApp();
    }

    public void SetAppTitle(string title)
    {
        titleDisplay.Text = this.Title = title;
    }

    public void ShowMainApp()
    {
        ShowBackArrow = false;
        this.mainFrame.Navigate(typeof(MainPage));
    }
    public void ShowSettings()
    {
        ShowBackArrow = true;
        this.mainFrame.Navigate(typeof(SettingsPage));
    }
    public void ShowAbout()
    {
        ShowBackArrow = true;
        this.mainFrame.Navigate(typeof(AboutPage));
    }
    private void NavigateBack_Click(object sender, RoutedEventArgs e)
    {
        ShowBackArrow = false;
        this.mainFrame.GoBack();
    }

    public void SendLaunchArguments(string[] args)
    {
        this.ActivationArguments = args;
    }
}
