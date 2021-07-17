using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
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
        private Bitmap img,imgbuffer;
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
                img = (Bitmap)eventArgs.Frame.Clone();
                Console.WriteLine("f");
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

        private void PicDecode()
        {
            timer.Stop();
            if (img == null)
            {
                timer.Start();return;
            }
                
            BinaryBitmap bitmap = null;
            try
            {
                MemoryStream ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] bt = ms.GetBuffer();
                ms.Close();
                LuminanceSource source = new RGBLuminanceSource(bt, img.Width, img.Height);
                bitmap = new BinaryBitmap(new ZXing.Common.HybridBinarizer(source));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "内部错误");
                return;
            }

            Result result = null;
            try
            {
                result = new MultiFormatReader().decode(bitmap);
            }
            catch (ReaderException ex)
            {
                resultstr = ex.ToString();
            }
            if (result != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    resultstr = result.Text;
                    StopCamera();

                    ResultWindow rw = new ResultWindow(result.Text, true);
                    bool? rv = rw.ShowDialog();
                    if (rv == true)
                    {
                        img = null;
                        StartCamera();
                        timer.Start();
                    }
                });
            }
            else
                timer.Start();
        }
        private void PicDecode1()
        {
            timer.Stop();
            if (imgbuffer == null)
            {
                timer.Start(); return;
            }
            BarcodeReader reader = new BarcodeReader();

            Result result = null;
            try
            {
                result = reader.Decode(imgbuffer);
            }
            catch (ReaderException ex)
            {
                resultstr = ex.ToString();
                //MessageBox.Show(resultstr, "内部错误");
                timer.Start();
                return;
            }

            if (result != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    resultstr = result.Text;
                    StopCamera();

                    ResultWindow rw = new ResultWindow(result.Text, true);
                    bool? rv = rw.ShowDialog();
                    if (rv == true)
                    {
                        img = null;
                        imgbuffer = null;
                        StartCamera();
                        
                    }
                });
            }
            else
                timer.Start();
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
