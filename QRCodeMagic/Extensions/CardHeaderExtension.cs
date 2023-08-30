using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QRCodeMagic.Extensions
{
    public static class CardHeaderExtension
    {
        public static string GetGlyph(DependencyObject obj)
        {
            return (string)obj.GetValue(GlyphProperty); 
        }

        public static void SetGlyph(DependencyObject obj, string value)
        {
            obj.SetValue(GlyphProperty, value);
        }

        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.RegisterAttached("Glyph", typeof(string), typeof(CardHeaderExtension), new PropertyMetadata("Default"));

        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(TitleProperty);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(CardHeaderExtension), new PropertyMetadata("Default"));
    }
}
