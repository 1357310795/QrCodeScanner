using MyQrCodeScanner.Modules;
using System;
using System.Collections.Generic;
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
   
        private System.Drawing.Bitmap img,back;
        private BitmapSource bs;
        private string resultstr;

        private delegate void NoArgDelegate();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Render, (NoArgDelegate)delegate { });
            Thread.Sleep(200);
            if(!PicDecode3(bs))
            {
                TextHint.Text= "直接识别失败，请尝试手动框选要识别的二维码。";
                enable_select = true;
            }
                
        }

        #region Canvas
        private Point p0, p1, p2, p3;
        private bool enable_select=false;
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!enable_select) return;
            p0 = e.GetPosition(canvas1);
            maskborder.Opacity = 1;
            MaskGrid.Height = 0;
            MaskGrid.Width = 0;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!enable_select) return;
            if (e.LeftButton==MouseButtonState.Pressed)
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
            if (!enable_select) return;
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
            if (!enable_select) return;
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
        #endregion

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

        private MyResult myResult;
        private bool PicDecode2(System.Drawing.Bitmap cur)
        {
            if (cur == null)
                return false;

            var res = MyScanner.ScanCode(cur);
            switch (res.status)
            {
                case result_status.error:
                    resultstr = res.data[0].data;
                    TextHint.Text = resultstr;

                    break;
                case result_status.ok:
                    resultstr = stringhelper.ResListToString(res.data);
                    ResultWindow rw = new ResultWindow(resultstr, false);
                    rw.Show();
                    return true;
                    break;
                case result_status.nocode:
                    TextHint.Text = "未识别到二维码，请重新选择";
                    break;
            }
            return false;
        }

        private bool PicDecode3(BitmapSource cur)
        {
            if (cur == null)
                return false;

            var res = MyScanner.ScanCode(cur);
            myResult = res;
            switch (res.status)
            {
                case result_status.error:
                    resultstr = res.data[0].data;
                    TextHint.Text = resultstr;
                    break;
                case result_status.ok:
                    resultstr = stringhelper.ResListToString(res.data);
                    //if (res.data.Count == 1 || !res.haslocation)
                    //{
                    //    ResultWindow rw = new ResultWindow(resultstr, false);
                    //    rw.Show();
                    //}
                    //else
                    {
                        ProcessMultiCode();
                    }

                    return true;
                    break;
                case result_status.nocode:
                    TextHint.Text = "未识别到二维码，请重新选择";
                    break;
            }
            return false;
        }

        #region 多个二维码
        public class CanvasCodeResult
        {
            public string data;
            public string type;
            public List<Point> tubao;

            public CanvasCodeResult(string data, string type, List<Point> tubao)
            {
                this.data = data;
                this.tubao = tubao;
                this.type = type;
            }
        }

        public List<CanvasCodeResult> canvasCodeResults;

        public void ProcessMultiCode()
        {
            canvasCodeResults = new List<CanvasCodeResult>();
            foreach (CodeWithLocation t in myResult.data)
            {
                for (int i=0;i<t.points.Count;i++)
                {
                    t.points[i] = PicToCanvas(t.points[i]);
                }
                //t.points.ForEach(p => { p = PicToCanvas(p); });
                //canvasCodeResults.Add(new CanvasCodeResult(t.data,t.type, calcConvexHull(t.points)));
                CreatePoly(t.points);
                //GoButton b = new GoButton();
                //b.Click += GoButton_Click;
                //b.Tag = t.data;
                //canvas1.Children.Add(b);
                //Canvas.SetTop(b, PicToCanvas(t.center).Y - 20);
                //Canvas.SetLeft(b, PicToCanvas(t.center).X - 20);
            }
        }

        public void CreatePoly(List<Point> l)
        {
            //M 10,10 L 100,10 90,50 100,100 Z
            PathGeometry p = new PathGeometry();
            PathFigure pf = new PathFigure();
            pf.StartPoint = l[0];
            pf.IsClosed = true;
            pf.Segments.Add(new PolyLineSegment(l, true));
            p.Figures.Add(pf);
            Path myPath = new Path();
            myPath.Stroke = System.Windows.Media.Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.Fill = System.Windows.Media.Brushes.Azure;
            myPath.Data = p;
            canvas1.Children.Add(myPath);
            Canvas.SetLeft(myPath, 0);
            Canvas.SetTop(myPath, 0);
        }

        public System.Windows.Point PicToCanvas(System.Windows.Point p)
        {
            return new System.Windows.Point(
                p.X / back.Width * canvas1.ActualWidth,
                p.Y / back.Height * canvas1.ActualHeight);
        }

        public double LengthToCanvas(double x)
        {
            return x / back.Width * canvas1.ActualWidth;
        }

        private List<Point> calcConvexHull(List<Point> list)
        {
            List<Point> resPoint = new List<Point>();
            //查找最小坐标点
            int minIndex = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].Y < list[minIndex].Y)
                {
                    minIndex = i;
                }
            }
            Point minPoint = list[minIndex];
            resPoint.Add(list[minIndex]);
            list.RemoveAt(minIndex);
            //坐标点排序
            list.Sort(
                delegate (Point p1, Point p2)
                {
                    Vector baseVec = new Vector(1, 0);
                    Vector p1Vec = new Vector(p1.X - minPoint.X, p1.Y - minPoint.Y);
                    Vector p2Vec = new Vector(p2.X - minPoint.X, p2.Y - minPoint.Y);

                    double up1 = p1Vec.X * baseVec.X;
                    double down1 = Math.Sqrt(p1Vec.X * p1Vec.X + p1Vec.Y * p1Vec.Y);

                    double up2 = p2Vec.X * baseVec.X;
                    double down2 = Math.Sqrt(p2Vec.X * p2Vec.X + p2Vec.Y * p2Vec.Y);


                    double cosP1 = up1 / down1;
                    double cosP2 = up2 / down2;

                    if (cosP1 > cosP2)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                );
            resPoint.Add(list[0]);
            resPoint.Add(list[1]);
            for (int i = 2; i < list.Count; i++)
            {
                Point basePt = resPoint[resPoint.Count - 2];
                Vector v1 = new Vector(list[i - 1].X - basePt.X, list[i - 1].Y - basePt.Y);
                Vector v2 = new Vector(list[i].X - basePt.X, list[i].Y - basePt.Y);

                if (v1.X * v2.Y - v1.Y * v2.X < 0)
                {
                    resPoint.RemoveAt(resPoint.Count - 1);
                    while (true)
                    {
                        Point basePt2 = resPoint[resPoint.Count - 2];
                        Vector v12 = new Vector(resPoint[resPoint.Count - 1].X - basePt2.X, resPoint[resPoint.Count - 1].Y - basePt2.Y);
                        Vector v22 = new Vector(list[i].X - basePt2.X, list[i].Y - basePt2.Y);
                        if (v12.X * v22.Y - v12.Y * v22.X < 0)
                        {
                            resPoint.RemoveAt(resPoint.Count - 1);
                        }
                        else
                        {
                            break;
                        }
                    }
                    resPoint.Add(list[i]);
                }
                else
                {
                    resPoint.Add(list[i]);
                }
            }
            return resPoint;
        }

        #endregion
    }
}