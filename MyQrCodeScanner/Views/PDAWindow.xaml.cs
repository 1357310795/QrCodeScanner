using Hardcodet.Wpf.TaskbarNotification;
using MediaFoundation;
using Microsoft.Win32;
using MyQrCodeScanner.Modules;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Teru.Code.Services;
using Teru.Code.Webcam.MF;
using WindowsInput;
using WindowsInput.Native;

namespace MyQrCodeScanner
{
    public partial class PDAWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        private string inputmode;
        public string InputMode
        {
            get { return inputmode; }
            set
            {
                inputmode = value;
                this.OnPropertyChanged("InputMode");
                IniHelper.SetKeyValue("main", "inputmode", inputmode, IniHelper.inipath);
            }
        }
        private string audiopath;
        public string AudioPath
        {
            get { return audiopath; }
            set
            {
                audiopath = value;
                this.OnPropertyChanged("AudioPath");
                IniHelper.SetKeyValue("main", "audiopath", audiopath, IniHelper.inipath);
            }
        }
        private bool playaudio;
        public bool PlayAudio
        {
            get { return playaudio; }
            set
            {
                playaudio = value;
                this.OnPropertyChanged("PlayAudio");
                IniHelper.SetKeyValue("main", "playaudio", playaudio.ToString(), IniHelper.inipath);
            }
        }

        private WebCamSampleGrabberPresenter player;

        public List<MediaFoundationDeviceInfo> VideoDevices { get; set; }
        public MediaFoundationDeviceInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; this.OnPropertyChanged("CurrentDevice"); }
        }

        private MediaFoundationDeviceInfo _currentDevice;
        private WriteableBitmap img,imgbuffer;

        private WriteableBitmap? m_Bmp;
        public WriteableBitmap Image => m_Bmp;

        private LoopWorker worker;
        TaskbarIcon myTaskbarIcon;
        #endregion

        #region Constructors
        public PDAWindow()
        {
            AudioPath = IniHelper.GetKeyValue("main", "audiopath", "", IniHelper.inipath);
            InputMode = IniHelper.GetKeyValue("main", "inputmode", "1", IniHelper.inipath);
            PlayAudio = Convert.ToBoolean(IniHelper.GetKeyValue("main", "playaudio", "false", IniHelper.inipath));
            GetVideoDevices();
            var lastdevice= IniHelper.GetKeyValue("main", "LastVideoDevice", "", IniHelper.inipath);
            foreach (MediaFoundationDeviceInfo d in VideoDevices)
                if (d.FriendlyName == lastdevice)
                    CurrentDevice = d;
            InitializeComponent();
            this.DataContext = this;
            this.Closing += MainWindow_Closing;
        }
        #endregion

        #region Windows Events
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            //StopCamera();
            if (Application.Current.MainWindow == null)
            {
                StopCamera();
                return;
            }
            e.Cancel = true;
            this.Hide();
            myTaskbarIcon.ShowBalloonTip(LangHelper.GetStr("RunInBackTipTitle"), LangHelper.GetStr("RunInBackTipText"), BalloonIcon.Info);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddWorker();
            myTaskbarIcon = (TaskbarIcon)FindResource("Taskbar");
            //_taskbar.DataContext = new NotifyIconViewModel();
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

        private void ButtonOpenAudio_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            dlg.Filter = "All Supported Files | *.wav; *.ogg; *.mp1; *.m1a; *.mp2; " +
                         "*.m2a;*.mpa;*.mus;*.mp3;*.mpg;*.mpeg;*.mp3pro;" +
                         "*.aif;*.aiff;*.bwf;*.wma;*.wmv;*.aac;*.adts;" +
                         "*.mp4;*.m4a;*.m4b;*.m4p;*.mod;*.mdz;*.mo3;*.s3m;" +
                         "*.s3z;*.xm;*.xmz;*.it;*.itz;*.umx;*.mtm;*.m4a;" +
                         "*.m4b;*.mp4;*.ac3;*.ape;*.mac;*.dff;*.dsf;" +
                         "*.flac;*.fla;*.oga;*.ogg;*.midi;*.mid;*.rmi;" +
                         "*.kar;*.opus;*.webm;*.mkv;*.mka";
            var res = dlg.ShowDialog();
            if(res == true)
            {
                AudioPath = dlg.FileName;
            }
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
            ClearResult();
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

        #region Scan Task

        private void AddWorker()
        {
            worker = new LoopWorker();
            worker.Interval = 500;
            worker.CanRun += () => true;
            worker.OnError += Worker_OnError;
            worker.Go += Worker_Go;
        }

        private bool Worker_OnError(Exception ex)
        {
            MessageBox.Show(ex.ToString());
            return false;
        }

        private TaskState Worker_Go(CancellationTokenSource cts)
        {
            if (imgbuffer == null)
            {
                return TaskState.Started;
            }

            var res = MyScanner.ScanCode(imgbuffer);
            switch (res.status)
            {
                case result_status.error:
                    return TaskState.Started;
                case result_status.ok:
                    if (res.data[0].data != MyResult)
                    {
                        if (res.data[0].data != "" && MyResult == "")
                            DoPaste(res.data[0].data);
                        MyResult = res.data[0].data;
                    }
                    return TaskState.Started;
                case result_status.nocode:
                    MyResult = "";
                    return TaskState.Started;
            }
            return TaskState.Started;
        }

        #endregion

        #region After Scan
        private string MyResult;

        public void ClearResult()
        {
            MyResult = "";
        }

        private void DoPaste(string s)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (PlayAudio) 
                    PlayAudioFile();

                InputSimulator k = new InputSimulator();
                if (InputMode == "1")
                {
                    Clipboard.SetText(s);
                    Thread.Sleep(50);
                    k.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    k.Keyboard.KeyPress(VirtualKeyCode.VK_V);
                    k.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                }
                else if (InputMode == "2")
                    k.Keyboard.TextEntry(s);
                else
                    Clipboard.SetText(s);

                if (GlobalSettings.isInputEnter)
                    k.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            });
        }

        private void PlayAudioFile()
        {
            try
            {
                MediaElement1.LoadedBehavior = MediaState.Manual;
                MediaElement1.Source = new Uri(AudioPath);
                MediaElement1.Volume = 100;
                MediaElement1.Play();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(ex.ToString(), LangHelper.GetStr("PlayAudioFail"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
