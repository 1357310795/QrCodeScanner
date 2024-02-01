using MaterialDesignThemes.Wpf;
using MediaFoundation;
using MyQrCodeScanner.Modules;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Teru.Code.Services;
using Teru.Code.Webcam.MF;

namespace MyQrCodeScanner
{
    public partial class CamWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        private WebCamSampleGrabberPresenter player;

        public List<MediaFoundationDeviceInfo> VideoDevices { get; set; }
        public MediaFoundationDeviceInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }

        private MediaFoundationDeviceInfo _currentDevice;

        private WriteableBitmap img, imgbuffer;

        private WriteableBitmap? m_Bmp;
        public WriteableBitmap Image => m_Bmp;

        private LoopWorker worker;
        #endregion

        #region Constructors
        public CamWindow()
        {
            GetVideoDevices();
            var lastdevice = IniHelper.GetKeyValue("main", "LastVideoDevice", "", IniHelper.inipath);
            foreach (MediaFoundationDeviceInfo d in VideoDevices)
                if (d.FriendlyName == lastdevice)
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
            AddWorker();
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
        //private void video_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        //{
        //    try
        //    {
        //        BitmapImage bi;
        //        imgbuffer = img;
        //        img = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
        //        //Console.WriteLine("f");
        //        bi = BitmapHelper.GetBitmapImage(img);

        //        bi.Freeze(); // avoid cross thread operations and prevents leaks
        //        Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; }));
        //    }
        //    catch (Exception exc)
        //    {
        //        MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        StopCamera();
        //    }
        //}


        private void GetVideoDevices()
        {
            VideoDevices = MediaFoundationHelper.GetVideoCaptureDevices();
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
                var f = MediaFoundationHelper.GetVideoFormats(CurrentDevice);
                f.Sort((x, y) => Math.Abs(x.FrameSizeHeight - 1080).CompareTo(Math.Abs(y.FrameSizeHeight - 1080)));

                m_Bmp = new WriteableBitmap((int)f[0].FrameSizeWidth, (int)f[0].FrameSizeHeight, 96, 96, PixelFormats.Bgra32, null);

                if (player != null)
                {
                    StopCamera();
                }
                Init(CurrentDevice, f[0]);

                Thread thread = new Thread(delegate ()
                {
                    this.player.StartSession();
                });
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();

                IniHelper.SetKeyValue("main", "LastVideoDevice", CurrentDevice.FriendlyName, IniHelper.inipath);
            }
            worker.StartRun();
        }

        private void StopCamera()
        {
            if (player != null)
            {
                Thread thread = new Thread(delegate ()
                {
                    this.player.StopSession();
                });
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();

                DetachMediaPresenterEvents();
                player.ShutDown();
            }
            img = null;
            worker.StopRun();
        }

        private void Init(MediaFoundationDeviceInfo videoDevice, MediaFoundationVideoFormatInfo videoFormat)
        {
            this.player = new WebCamSampleGrabberPresenter();
            this.player.FlipVertically = false;
            this.player.FlipHorizontally = false;
            this.AttachMediaPresenterEvents();
            this.player.AudioCaptureDevice = null;
            this.player.VideoCaptureDevice = videoDevice;
            this.player.VideoCaptureFormat = videoFormat;
            this.player.Initialize();
        }

        private void AttachMediaPresenterEvents()
        {
            this.player.FrameReady += this.Player_FrameReady;
            this.player.PropertyChanged += this.Player_PropertyChanged;
            this.player.MediaSessionError += this.Player_MediaSessionError;
        }

        private void DetachMediaPresenterEvents()
        {
            this.player.FrameReady -= this.Player_FrameReady;
            this.player.PropertyChanged -= this.Player_PropertyChanged;
            this.player.MediaSessionError -= this.Player_MediaSessionError;
        }

        private void Player_FrameReady(object sender, FrameEventArgs e)
        {
            //Debug.WriteLine($"Player_FrameReady: {e.Width}x{e.Height}");

            using (MediaBufferLock mediaBufferLock = new MediaBufferLock(e.Buffer))
            {
                int num = 0;
                IntPtr intPtr;
                HResult hresult = mediaBufferLock.LockBuffer(e.Stride, e.Height, out intPtr, out num);
                if (hresult != HResult.S_OK)
                {
                    return;
                }
                try
                {
                    m_Bmp?.Dispatcher.Invoke(() =>
                    {
                        m_Bmp.Lock();
                        //Debug.WriteLine(m_Bmp.BackBufferStride);
                        MediaFoundationHelper.CopyMemory(m_Bmp.BackBuffer, intPtr, (uint)(e.Height * e.Width * 4));
                        m_Bmp.AddDirtyRect(new System.Windows.Int32Rect(0, 0, m_Bmp.PixelWidth, m_Bmp.PixelHeight));
                        m_Bmp.Unlock();

                        imgbuffer = img;
                        img = Image;
                        Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = Image; }));

                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(e);
                }
                finally
                {

                }
            }

        }

        private void Player_MediaSessionError(object sender, HResult result, Exception ex)
        {
            if (result == HResult.MF_E_HW_MFT_FAILED_START_STREAMING)
            {
                MessageBox.Show("The camera is unavailable.");
                return;
            }
            if (result == HResult.E_ACCESSDENIED || result == HResult.MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED)
            {
                MessageBox.Show("Access to the camera is denied.");
            }
        }

        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SessionState")
            {
                if (this.player.SessionState == MFMediaSessionState.Started)
                {
                    base.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate ()
                    {
                        CommandManager.InvalidateRequerySuggested();
                    }));
                    return;
                }
                //if (this.player.SessionState == MFMediaSessionState.Stopped)
                //{
                //    RawColorBGRA black = default(RawColorBGRA);
                //    base.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(delegate ()
                //    {
                //        if (this.d3dDevice != null)
                //        {
                //            this.d3dDevice.ColorFill(this.surface, black);
                //        }
                //    }));
                //    return;
                //}
                //if (this.player.SessionState == MFMediaSessionState.Ready)
                //{
                //    base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate ()
                //    {
                //        this.OnPlayerInitialized();
                //    }));
                //}
            }
        }

        #endregion

        #region Scan

        private MyResult myResult;

        private void AddWorker()
        {
            worker = new LoopWorker();
            worker.Interval = 500;
            worker.CanRun += () => true;
            worker.OnError += Worker_OnError;
            worker.Go += Worker_Go;
        }

        private TaskState Worker_Go(CancellationTokenSource cts)
        {
            if (imgbuffer == null)
            {
                return TaskState.Started;
            }

            var res = MyScanner.ScanCode(imgbuffer);
            myResult = res;
            switch (res.status)
            {
                case result_status.error:
                    return TaskState.Started;
                case result_status.ok:

                    this.Dispatcher.Invoke(() =>
                    {
                        StopCamera();
                        if (res.data.Count == 1 || !res.haslocation)
                        {
                            ResultWindow rw = new ResultWindow(res.data[0].data, res.data[0].type, true, true);
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
                            snackbar1.MessageQueue.Enqueue(LangHelper.GetStr("MultiCode"));
                            ProcessMultiCode();
                        }
                    });

                    return TaskState.Done;
                case result_status.nocode:
                    return TaskState.Started;
            }
            return TaskState.Started;
        }

        private bool Worker_OnError(Exception ex)
        {
            MessageBox.Show(ex.ToString());
            return false;
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
