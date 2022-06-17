using AForge.Video;
using AForge.Video.DirectShow;
using MaterialDesignThemes.Wpf;
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

namespace MyQrCodeScanner
{
    public partial class CamWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        public ObservableCollection<FilterInfo> VideoDevices { get; set; }
        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }
        private FilterInfo _currentDevice;
        private System.Drawing.Bitmap img,imgbuffer;
        private System.Timers.Timer timer;
        private IVideoSource _videoSource;
        #endregion

        #region Constructors
        public CamWindow()
        {
            GetVideoDevices();
            var lastdevice = IniHelper.GetKeyValue("main", "LastVideoDevice", "", IniHelper.inipath);
            foreach (FilterInfo d in VideoDevices)
                if (d.Name == lastdevice)
                    CurrentDevice = d;
            InitializeComponent();
            this.DataContext = this;
            this.Closing += MainWindow_Closing;
            snackbar1.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        }
        #endregion

        #region Window Events
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            StopCamera();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddTimer();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.MainWindow.Show();
        }
        #endregion

        #region Main Function
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
            StartCamera();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }
        #endregion

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
            ClearResult();
            StopCamera();
            hinttext.Visibility= Visibility.Collapsed; 
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
                IniHelper.SetKeyValue("main", "LastVideoDevice", CurrentDevice.Name, IniHelper.inipath);
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

        #region Scan

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
                        StopCamera();
                        if (res.data.Count == 1 || !res.haslocation)
                        {
                            ResultWindow rw = new ResultWindow(res.data[0].data, res.data[0].type, true,true);
                            this.Hide();
                            bool? rv = rw.ShowDialog();
                            if (rv == true)
                            {
                                img = null;
                                imgbuffer = null;
                                videoPlayer.Source = null;
                                this.Show();
                                StartCamera();
                            }
                            else
                                this.Close();
                        }
                        else
                        {
                            snackbar1.MessageQueue.Clear();
                            snackbar1.MessageQueue.Enqueue("检测到多个Code，请将鼠标放在Code上查看结果");
                            ProcessMultiCode();
                        }
                    });

                    break;
                case result_status.nocode:
                    timer.Start();
                    break;
            }
                
        }
        #endregion

        #region Multi Codes
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

        public void CreateOnePoly(List<Point> l,int tag)
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
                p.X / imgbuffer.Width * canvas1.ActualWidth,
                p.Y / imgbuffer.Height * canvas1.ActualHeight);
        }

        public double LengthToCanvas(double x)
        {
            return x / imgbuffer.Width * canvas1.ActualWidth;
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
