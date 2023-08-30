using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ModernWpf.Controls;
using System.Windows.Navigation;
using Windows.System;
using Windows.Gaming.Input;
using Windows.System.Profile;
using System.Windows.Automation;
using Windows.Devices.Input;
using System.Windows.Controls;
using Page = ModernWpf.Controls.Page;
using Frame = ModernWpf.Controls.Frame;
using ModernWpf.Navigation;
using QRCodeMagic.Pages;
using QRCodeMagic.Services.Contracts;
using QRCodeMagic.Services;
using NavigationService = QRCodeMagic.Services.NavigationService;
using Wish.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ModernWpf.SampleApp
{
    /// <summary>
    /// NavigationRootPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class NavigationRootPage : Page
    {
        private bool _isBackEnabled;
        private object? _selected;

        public INavigationService NavigationService
        {
            get;
        }

        public INavigationViewService NavigationViewService
        {
            get;
        }

        public bool IsBackEnabled
        {
            get => _isBackEnabled;
            set => SetProperty(ref _isBackEnabled, value);
        }

        public object? Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public static string GetAppTitleFromSystem
        {
            get
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    return Windows.ApplicationModel.Package.Current.DisplayName;
                }
                else
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();
                }
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = NavigationService.CanGoBack;

            if (e.SourcePageType() == typeof(SettingsPage))
            {
                Selected = NavigationViewService.SettingsItem;
                return;
            }

            var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType());
            if (selectedItem != null)
            {
                Selected = selectedItem;
            }
        }

        //public PageHeader PageHeader
        //{
        //    get
        //    {
        //        return VisualTree.FindDescendants<PageHeader>(NavigationViewControl).FirstOrDefault();
        //    }
        //}

        public NavigationRootPage()
        {
            InitializeComponent();
            NavigationService = new NavigationService(PageService.Default);
            NavigationService.Navigated += OnNavigated;
            NavigationViewService = new NavigationViewService(NavigationService, PageService.Default);
            NavigationService.Frame = rootFrame;
            NavigationViewService.Initialize(NavigationViewControl);

            this.DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void OnNavigationViewControlLoaded(object sender, RoutedEventArgs e)
        {
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
        }

        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness((sender.CompactPaneLength * 2), currMargin.Top, currMargin.Right, currMargin.Bottom);

            }
            else
            {
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            AppTitleBar.Visibility = sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top ? Visibility.Collapsed : Visibility.Visible;
            UpdateAppTitleMargin(sender);
        }

        private void UpdateAppTitleMargin(NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;


            Thickness currMargin = AppTitle.Margin;

            if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                     sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else
            {
                AppTitle.Margin = new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }

    }
}
