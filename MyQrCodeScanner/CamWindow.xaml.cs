using AForge.Video;
using AForge.Video.DirectShow;
using MyQrCodeScanner.Modules;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZXing;

namespace MyQrCodeScanner
{
    public partial class CamWindow : Window, INotifyPropertyChanged
    {

        #region Public properties

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }
        private FilterInfo _currentDevice;
        private System.Drawing.Bitmap img,imgbuffer;
        private System.Timers.Timer timer;


        public string resultstr
        {
            get { return (string)GetValue(resultstrProperty); }
            set { SetValue(resultstrProperty, value); }
        }

        public static readonly DependencyProperty resultstrProperty =
            DependencyProperty.Register("resultstr", typeof(string), typeof(CamWindow));


        #endregion

        #region Private fields

        private IVideoSource _videoSource;

        #endregion

        public CamWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            GetVideoDevices();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            StopCamera();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddTimer();
            
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
            StartCamera();
        }

        #region Camera
        private void video_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                imgbuffer = img;
                img = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
                //Console.WriteLine("f");
                bi = BitmapHelper.GetBitmapImage(img);
                
                bi.Freeze(); // avoid cross thread operations and prevents leaks
                Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; }));
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        private void GetVideoDevices()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            foreach (FilterInfo filterInfo in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                VideoDevices.Add(filterInfo);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartCamera()
        {
            hinttext.Visibility= Visibility.Collapsed; 
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
            timer.Start();
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            }
            img = null;
            timer.Stop();
        }

        #endregion

        #region QrCode

        private MyResult myResult;

        private void AddTimer()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 500;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timertick);
        }

        private void timertick(object sender, System.Timers.ElapsedEventArgs e)
        {
            PicDecode1();
        }

        //private void PicDecode()
        //{
        //    timer.Stop();
        //    if (img == null)
        //    {
        //        timer.Start();return;
        //    }
                
        //    BinaryBitmap bitmap = null;
        //    try
        //    {
        //        MemoryStream ms = new MemoryStream();
        //        img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        //        byte[] bt = ms.GetBuffer();
        //        ms.Close();
        //        LuminanceSource source = new RGBLuminanceSource(bt, img.Width, img.Height);
        //        bitmap = new BinaryBitmap(new ZXing.Common.HybridBinarizer(source));
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "内部错误");
        //        return;
        //    }

        //    Result result = null;
        //    try
        //    {
        //        result = new MultiFormatReader().decode(bitmap);
        //    }
        //    catch (ReaderException ex)
        //    {
        //        resultstr = ex.ToString();
        //    }
        //    if (result != null)
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            resultstr = result.Text;
        //            StopCamera();

        //            ResultWindow rw = new ResultWindow(result.Text, true);
        //            bool? rv = rw.ShowDialog();
        //            if (rv == true)
        //            {
        //                img = null;
        //                StartCamera();
        //                timer.Start();
        //            }
        //        });
        //    }
        //    else
        //        timer.Start();
        //}

        private void PicDecode1()
        {
            timer.Stop();
            if (imgbuffer == null)
            {
                timer.Start(); return;
            }

            var res = MyScanner.ScanCode(imgbuffer);
            myResult = res;
            switch (res.status)
            {
                case result_status.error:
                    timer.Start();
                    break;
                case result_status.ok:

                    this.Dispatcher.Invoke(() =>
                    {
                        resultstr = stringhelper.ResListToString(res.data);
                        StopCamera();
                        if (res.data.Count == 1 || !res.haslocation)
                        {
                            ResultWindow rw = new ResultWindow(resultstr, true);
                            bool? rv = rw.ShowDialog();
                            if (rv == true)
                            {
                                img = null;
                                imgbuffer = null;
                                StartCamera();
                            }
                        }
                        else
                        {
                            ProcessMultiCode();
                        }
                    });

                    break;
                case result_status.nocode:
                    timer.Start();
                    break;
            }
                
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            ResultWindow rw = new ResultWindow((string)((Button)sender).Tag, true);
            rw.ShowDialog();
        }
        #endregion

        #region 多个二维码
        public class CanvasCodeResult
        {
            public string data;
            public string type;
            public List<Point> tubao;
            public bool isPanelOpen;
            
            public CanvasCodeResult(string data, string type,List<Point> tubao)
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
                var tubao = calcConvexHull(t.points);
                canvasCodeResults.Add(new CanvasCodeResult(t.data,t.type, tubao));
            }
            CreatePoly();
        }

        public void CreatePoly()
        {
            for(int i=0;i< canvasCodeResults.Count;i++)
            {
                List<Point> list = new List<Point>();
                for (int j = 0; j < canvasCodeResults[i].tubao.Count; j++)
                {
                    list.Add(PicToCanvas(canvasCodeResults[i].tubao[j]));
                }
                CreateOnePoly(list, i);
            }
                
        }

        public void ClearPoly()
        {
            canvas1.Children.Clear();
        }

        public void CreateOnePoly(List<Point> l,int tag)
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
            myPath.Tag = tag;
            canvas1.Children.Add(myPath);
            Canvas.SetLeft(myPath, 0);
            Canvas.SetTop(myPath, 0);
            myPath.MouseEnter += Poly_MouseEnter;
            myPath.MouseLeave += Poly_MouseLeave;
            myPath.MouseMove += Poly_MouseMove;
        }

        private void Poly_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Console.WriteLine(((Path)sender).Tag + " " + "Move");
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
            p.MouseLeave += Panel_MouseLeave;
            canvas1.Children.Add(p);
            Canvas.SetLeft(p, c.X - p.Width / 2);
            Canvas.SetTop(p, c.Y - p.Height / 2);

            canvasCodeResults[i].isPanelOpen = true;
        }

        private void Panel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
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

        public System.Windows.Point PicToCanvas(System.Windows.Point p)
        {
            return new System.Windows.Point(
                p.X / imgbuffer.Width * canvas1.ActualWidth,
                p.Y / imgbuffer.Height * canvas1.ActualHeight);
        }

        public double LengthToCanvas(double x)
        {
            return x / imgbuffer.Width * canvas1.ActualWidth;
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
                    Vector baseVec=new Vector(1,0);
                    Vector p1Vec=new Vector(p1.X - minPoint.X, p1.Y - minPoint.Y);
                    Vector p2Vec=new Vector(p2.X - minPoint.X, p2.Y - minPoint.Y);

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
                Vector v1=new Vector(list[i - 1].X - basePt.X, list[i - 1].Y - basePt.Y);
                Vector v2=new Vector(list[i].X - basePt.X, list[i].Y - basePt.Y);

                if (v1.X * v2.Y - v1.Y * v2.X < 0)
                {
                    resPoint.RemoveAt(resPoint.Count - 1);
                    while (true)
                    {
                        Point basePt2 = resPoint[resPoint.Count - 2];
                        Vector v12=new Vector(resPoint[resPoint.Count - 1].X - basePt2.X, resPoint[resPoint.Count - 1].Y - basePt2.Y);
                        Vector v22=new Vector(list[i].X - basePt2.X, list[i].Y - basePt2.Y);
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

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion
    }
}
