using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyQrCodeScanner
{
    class BitmapHelper
    {
        public static BitmapImage GetBitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        public static System.Drawing.Bitmap CaptureScreenToBitmap(double x, double y, double width, double height)
        {
            int ix = Convert.ToInt32(x);
            int iy = Convert.ToInt32(y);
            int iw = Convert.ToInt32(width);
            int ih = Convert.ToInt32(height);

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(iw, ih);
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(ix, iy, 0, 0, new System.Drawing.Size(iw, ih));
                return bitmap;

            }
        }

        public static BitmapSource GetBitmapSource(System.Drawing.Bitmap bmp)
        {
            BitmapFrame bf = null;

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bf = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            }
            return bf;

        }

        public static System.Drawing.Bitmap GetBitmap(BitmapSource source)
        {
            MemoryStream ms = new MemoryStream();
            source.Dispatcher.Invoke(() =>
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(ms);
            });

            return new System.Drawing.Bitmap(ms);
        }

        public static System.Drawing.Bitmap RenderVisual(UIElement elt, int x, int y)
        {
            PresentationSource source = PresentationSource.FromVisual(elt);
            RenderTargetBitmap rtb = new RenderTargetBitmap(x, y, 96, 96, PixelFormats.Default);
            VisualBrush sourceBrush = new VisualBrush(elt);
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(x, y)));

            rtb.Render(drawingVisual);
            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);

            return new System.Drawing.Bitmap(stream);
        }
    }
}
