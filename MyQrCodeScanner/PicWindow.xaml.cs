using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using WPFCaptureScreenShot;
using ZXing;
using BarcodeReader = ZXing.Presentation.BarcodeReader;

namespace MyQrCodeScanner
{
    public partial class PicWindow : Window
    {
        public PicWindow(System.Drawing.Bitmap t)
        {
            InitializeComponent();
            back = t;
            image1.Source = BitmapHelper.GetBitmapImage(back);
            bs = BitmapHelper.GetBitmapSource(t);
        }

        public PicWindow(BitmapSource t)
        {
            InitializeComponent();
            back = BitmapHelper.GetBitmap(t);
            bs = t;
            image1.Source = t;
        }

        private Point p0, p1,p2,p3;
        private System.Drawing.Bitmap img,back;
        private BitmapSource bs;
        private string resultstr;

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            p0 = e.GetPosition(canvas1);
            maskborder.Opacity = 1;
            MaskGrid.Height = 0;
            MaskGrid.Width = 0;
        }

        private delegate void NoArgDelegate();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render, (NoArgDelegate)delegate { });
            Thread.Sleep(200);
            if(!PicDecode3(bs))
                TextHint.Text= "直接识别失败，请尝试手动框选要识别的二维码。";
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton==MouseButtonState.Pressed)
            {
                p1 = e.GetPosition(canvas1);
                Canvas.SetLeft(MaskGrid,Math.Min(p0.X,p1.X));
                Canvas.SetTop(MaskGrid, Math.Min(p0.Y, p1.Y));
                MaskGrid.Width = Math.Abs(p0.X - p1.X);
                MaskGrid.Height = Math.Abs(p0.Y - p1.Y);
                p2 = new Point(Math.Min(p0.X, p1.X),Math.Min(p0.Y,p1.Y));
                p3 = new Point(Math.Max(p0.X, p1.X),Math.Max(p0.Y,p1.Y));
                p2.X = p2.X / canvas1.ActualWidth;
                p2.Y = p2.Y / canvas1.ActualHeight;
                p3.X = p3.X / canvas1.ActualWidth;
                p3.Y = p3.Y / canvas1.ActualHeight;
                brush1.Viewbox = new Rect(p2, p3);
            }
            
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (p3.X!=p2.X && p3.Y!=p2.Y)
            {
                maskborder.Opacity = 0;
                img = BitmapHelper.RenderVisual(MaskGrid,
                    Convert.ToInt32((p3.X - p2.X) * back.Width),
                    Convert.ToInt32((p3.Y - p2.Y) * back.Height));
                p3 = p2;
                //img.Save(@"E:\1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                PicDecode2(img);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (p3.X != p2.X && p3.Y != p2.Y)
            {
                maskborder.Opacity = 0;
                img = BitmapHelper.RenderVisual(MaskGrid,
                    Convert.ToInt32((p3.X - p2.X) * back.Width),
                    Convert.ToInt32((p3.Y - p2.Y) * back.Height));
                p3 = p2;
                //img.Save(@"E:\1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                if (PicDecode2(img))
                    TextHint.Text = "直接识别失败，请尝试手动框选要识别的二维码。";
            }
        }
        
        //private void PicDecode(System.Drawing.Bitmap cur)
        //{
        //    if (cur == null)
        //        return;
        //    BinaryBitmap bitmap1;
        //    try
        //    {
        //        MemoryStream ms = new MemoryStream();
        //        cur.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        //        byte[] bt = ms.GetBuffer();
        //        ms.Close();
        //        LuminanceSource source = new RGBLuminanceSource(bt, cur.Width, cur.Height);
        //        bitmap1 = new BinaryBitmap(new ZXing.Common.HybridBinarizer(source));
        //    }
        //    catch (Exception ex)
        //    {
        //        return;
        //    }

        //    Result result = null;
        //    try
        //    {
        //        //开始解码
        //        result = new MultiFormatReader().decode(bitmap1);
        //    }
        //    catch (ReaderException ex)
        //    {
        //        resultstr = ex.ToString();
        //        TextHint.Text = resultstr;
        //    }
        //    if (result != null)
        //    {

        //        resultstr = result.Text;

        //        ResultWindow rw = new ResultWindow(result.Text);
        //        rw.Show();

        //    }
        //    else
        //    {
        //        TextHint.Text = "未识别到二维码，请重新选择";
        //    }
        //}
        //private void PicDecode1(System.Drawing.Bitmap cur)
        //{
        //    if (cur == null)
        //        return;
        //    resultstr = "";
        //    try
        //    {
        //        QRCodeDecoder qrDecoder = new QRCodeDecoder();
        //        QRCodeImage qrImage = new QRCodeBitmapImage(cur);
        //        resultstr = qrDecoder.decode(qrImage, Encoding.UTF8);
        //    }
        //    catch (Exception ex)
        //    {
        //        resultstr = ex.ToString();
        //        TextHint.Text = resultstr;
        //        TextHint.Text = "未识别到二维码，请重新选择";
        //        return;
        //    }
            
        //        ResultWindow rw = new ResultWindow(resultstr);
        //        rw.Show();
            


        //}

        private bool PicDecode2(System.Drawing.Bitmap cur)
        {
            if (cur == null)
                return false;
            BarcodeReader reader = new BarcodeReader();
            Result result = null;
            try
            {
                result = reader.Decode(BitmapHelper.GetBitmapSource(cur));
            }
            catch (ReaderException ex)
            {
                resultstr = ex.ToString();
                TextHint.Text = resultstr;
            }
            if (result != null)
            {
                resultstr = result.Text;
                ResultWindow rw = new ResultWindow(result.Text,false);
                rw.Show();
                return true;
            }
            else
            {
                TextHint.Text = "未识别到二维码，请重新选择";
                return false;
            }
        }
        private bool PicDecode3(BitmapSource cur)
        {
            if (cur == null)
                return false;
            BarcodeReader reader = new BarcodeReader();
            Result result = null;
            try
            {
                result = reader.Decode(cur);
            }
            catch (ReaderException ex)
            {
                resultstr = ex.ToString();
                TextHint.Text = resultstr;
            }
            if (result != null)
            {
                resultstr = result.Text;
                ResultWindow rw = new ResultWindow(result.Text, false);
                rw.Show();
                return true;
            }
            else
            {
                TextHint.Text = "未识别到二维码，请重新选择";
                return false;
            }
        }
    }
}