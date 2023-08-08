using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace QRCodeMagic.Helpers
{
    public class VisualHelper
    {
        public static System.Drawing.Bitmap RenderVisual(UIElement element, int width, int height)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            VisualBrush sourceBrush = new VisualBrush(element);
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(width, height)));

            rtb.Render(drawingVisual);
            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);

            return new System.Drawing.Bitmap(stream);
        }
    }
}
