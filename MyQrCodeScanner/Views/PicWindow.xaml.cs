using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyQrCodeScanner
{
    public partial class PicWindow : Window
    {
        #region 构造函数
        public PicWindow(System.Drawing.Bitmap t)
        {
            InitializeComponent();
            back = t;
            image1.Source = BitmapHelper.GetBitmapImage(back);
            bs = BitmapHelper.GetBitmapSource(t);
            sm = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            snackbar1.MessageQueue = sm;
        }

        public PicWindow(BitmapSource t)
        {
            InitializeComponent();
            back = BitmapHelper.GetBitmap(t);
            bs = t;
            image1.Source = t;
            sm = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            snackbar1.MessageQueue = sm;
        }
        #endregion

        #region private fields
        private System.Drawing.Bitmap img,back;
        private BitmapSource bs;
        private string resultstr;
        private SnackbarMessageQueue sm;
        #endregion

        #region Canvas手动框选
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
                PicDecode2(img);
            }
        }
        #endregion

        #region 扫描
        private MyResult myResult;

        public void PreScan()
        {
            if (!PicDecode3(bs))
            {
                snackbar1.MessageQueue.Clear();
                snackbar1.MessageQueue.Enqueue("直接识别失败，请尝试手动框选要识别的二维码。");
                texthint.Text = "请手动框选要识别的二维码";
                enable_select = true;
                this.Show();
            }
        }

        private bool PicDecode2(System.Drawing.Bitmap cur)
        {
            if (cur == null)
                return false;

            var res = MyScanner.ScanCode(cur);
            switch (res.status)
            {
                case result_status.error:
                    resultstr = res.data[0].data;
                    break;
                case result_status.ok:
                    resultstr = stringhelper.ResListToString(res.data);
                    ResultWindow rw = new ResultWindow(res.data[0].data, res.data[0].type, true, true);
                    bool? rv = rw.ShowDialog();
                    if (rv == true)
                    {
                        img = null;
                        this.Show();
                    }
                    else
                        this.Close();
                    return true;
                    break;
                case result_status.nocode:
                    snackbar1.MessageQueue.Clear();
                    snackbar1.MessageQueue.Enqueue("未识别到二维码，请重新选择");
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
                    break;
                case result_status.ok:
                    resultstr = stringhelper.ResListToString(res.data);

                    if (res.data.Count == 1 || !res.haslocation)
                    {
                        ResultWindow rw = new ResultWindow(res.data[0].data, res.data[0].type, true, false);
                        this.Hide();
                        bool? rv = rw.ShowDialog();
                        if (rv == true)
                        {
                            img = null;
                            this.Show();
                        }
                        else
                            this.Close();
                    }
                    else
                    {
                        ProcessMultiCode();
                    }

                    return true;
                    break;
                case result_status.nocode:
                    break;
            }
            return false;
        }
        #endregion

        #region 多个二维码
        public class CanvasCodeResult
        {
            public string data;
            public string type;
            public List<Point> tubao;
            public bool isPanelOpen;

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
            this.Show();
            texthint.Text = "检测到多个二维码，请将鼠标放在二维码上查看结果";
            canvasCodeResults = new List<CanvasCodeResult>();
            foreach (CodeWithLocation t in myResult.data)
            {
                var tubao = GrahamScan.convexHull(t.points);
                canvasCodeResults.Add(new CanvasCodeResult(t.data, t.type, tubao));
            }
            CreatePoly();
        }

        public void ClearResult()
        {
            canvasCodeResults = null;
            canvas1.Children.Clear();
        }

        public void ClearPoly()
        {
            canvas1.Children.Clear();
            for (int i = 0; i < canvasCodeResults.Count; i++)
                canvasCodeResults[i].isPanelOpen = false;
        }

        public void CreatePoly()
        {
            for (int i = 0; i < canvasCodeResults.Count; i++)
            {
                List<Point> list = new List<Point>();
                for (int j = 0; j < canvasCodeResults[i].tubao.Count; j++)
                {
                    list.Add(PicToCanvas(canvasCodeResults[i].tubao[j]));
                }
                CreateOnePoly(list, i);
            }

        }

        public void CreateOnePoly(List<Point> l, int tag)
        {
            PathGeometry p = new PathGeometry();
            PathFigure pf = new PathFigure();
            pf.StartPoint = l[0];
            pf.IsClosed = true;
            pf.Segments.Add(new PolyLineSegment(l, true));
            p.Figures.Add(pf);
            Path myPath = new Path();
            myPath.Stroke = ((Brush)Application.Current.Resources["PrimaryHueDarkBrush"]).Clone();
            myPath.StrokeThickness = 2;
            myPath.Fill = ((Brush)Application.Current.Resources["PrimaryHueLightBrush"]).Clone();
            myPath.Fill.Opacity = 0.3;
            myPath.Data = p;
            myPath.Tag = tag;
            canvas1.Children.Add(myPath);
            Canvas.SetLeft(myPath, 0);
            Canvas.SetTop(myPath, 0);
            myPath.MouseEnter += Poly_MouseEnter;
            myPath.MouseLeave += Poly_MouseLeave;
        }

        private void Poly_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Console.WriteLine(((Path)sender).Tag + " " + "Leave");
        }

        private void Poly_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Console.WriteLine(((Path)sender).Tag + " " + "Enter");
            int i = (int)((Path)sender).Tag;
            if (canvasCodeResults[i].isPanelOpen) return;
            List<Point> list = new List<Point>();
            for (int j = 0; j < canvasCodeResults[i].tubao.Count; j++)
            {
                list.Add(PicToCanvas(canvasCodeResults[i].tubao[j]));
            }
            Point c = GeometryHelper.Center(list);

            GoPanel p = new GoPanel();
            p.Data = canvasCodeResults[i].data;
            p.CodeType = canvasCodeResults[i].type;
            p.Tag = i;
            p.ClosePanel += OnClosePanel;
            canvas1.Children.Add(p);
            Canvas.SetLeft(p, c.X - p.Width / 2);
            Canvas.SetTop(p, c.Y - p.Height / 2);

            canvasCodeResults[i].isPanelOpen = true;
        }

        private void OnClosePanel(object sender)
        {
            var p = ((GoPanel)sender);
            canvas1.Children.Remove(p);
            int i = (int)((GoPanel)sender).Tag;
            canvasCodeResults[i].isPanelOpen = false;
        }

        private void canvas1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (canvasCodeResults != null && canvasCodeResults.Count > 0)
            {
                ClearPoly(); CreatePoly();
            }
        }

        public Point PicToCanvas(Point p)
        {
            return new System.Windows.Point(
                p.X / back.Width * canvas1.ActualWidth,
                p.Y / back.Height * canvas1.ActualHeight);
        }

        public double LengthToCanvas(double x)
        {
            return x / back.Width * canvas1.ActualWidth;
        }

        #endregion
    }
}