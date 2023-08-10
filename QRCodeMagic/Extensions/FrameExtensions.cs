using ModernWpf.Controls;

namespace QRCodeMagic.Extensions;

public static class FrameExtensions
{
    public static object GetPageViewModel(this Frame frame) => frame?.Content?.GetType().GetProperty("DataContext")?.GetValue(frame.Content, null);
}
