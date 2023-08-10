using CommunityToolkit.Mvvm.ComponentModel;
using QRCodeMagic.Pages;
using QRCodeMagic.Services.Contracts;
using System;
using System.Collections.Generic;

namespace Wish.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    private static PageService _default;
    public static PageService Default
    {
        get
        {
            if (_default == null)
                _default = new PageService();
            return _default;
        }
    }

    public PageService()
    {
        _pages.Add(nameof(EncodePage), typeof(EncodePage));
        _pages.Add(nameof(DecodePage), typeof(DecodePage));
        _pages.Add(nameof(DrawPage), typeof(DrawPage));
        _pages.Add(nameof(SettingsPage), typeof(SettingsPage));
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }
}
