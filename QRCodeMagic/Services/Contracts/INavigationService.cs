
using ModernWpf.Controls;
using System.Windows.Navigation;

namespace QRCodeMagic.Services.Contracts;

public interface INavigationService
{
    event NavigatedEventHandler Navigated;

    bool CanGoBack
    {
        get;
    }

    Frame Frame
    {
        get; set;
    }

    bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false);

    bool GoBack();
}
