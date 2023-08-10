using ModernWpf.Controls;
using System;
using System.Collections.Generic;

namespace QRCodeMagic.Services.Contracts;

public interface INavigationViewService
{
    IList<object> MenuItems
    {
        get;
    }

    object SettingsItem
    {
        get;
    }

    void Initialize(NavigationView navigationView);

    void UnregisterEvents();

    NavigationViewItem GetSelectedItem(Type pageType);
}
