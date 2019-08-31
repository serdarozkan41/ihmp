using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace IHMP.FaceDetect
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string FacesPath = "C:\\IHMP\\Faces";
        public static string MediasPath = "C:\\IHMP\\Medias";

        List<string> Images = new List<string>();
        List<string> labels = new List<string>();
        string[] imgs;
        string[] medias;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        private Capture capture;
        DispatcherTimer timer;
        DispatcherTimer loadTimer;
        HaarCascade face;
        Image<Gray, byte> result, TrainedFace = null;
        int ContTrain;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            capture = new Capture(1);
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 1280);
            capture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 720);
            foreach (var _img in Directory.GetFiles(FacesPath))
            {
                trainingImages.Add(new Image<Gray, byte>(_img));
                labels.Add(_img.Split(Path.DirectorySeparatorChar).Last());
                ContTrain++;
            }

            LoadImg();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();

            loadTimer = new DispatcherTimer();
            loadTimer.Tick += new EventHandler(loadTimer_Tick);
            loadTimer.Interval = new TimeSpan(0, 0, 1);
            loadTimer.Start();
        }
        void loadTimer_Tick(object sender, EventArgs e)
        {
            LoadImg();
        }

        private void LoadImg()
        {
            imgs = Directory.GetFiles(FacesPath);
            medias = Directory.GetDirectories(MediasPath);
            trainingImages = new List<Image<Gray, byte>>();
            labels = new List<string>();
            ContTrain = 0;
            foreach (var _img in imgs)
            {
                trainingImages.Add(new Image<Gray, byte>(_img));
                labels.Add(_img.Split(Path.DirectorySeparatorChar).Last());
                ContTrain++;
            }
        }

        async void timer_Tick(object sender, EventArgs e)
        {
            Image<Bgr, Byte> currentFrame = capture.QueryFrame();

            if (currentFrame != null)
            {
                //Image<Gray, Byte> grayFrame = currentFrame.Convert<Gray, Byte>();
                var gray = capture.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(20, 20));
                    foreach (MCvAvgComp f in facesDetected[0])
                    {
                        result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain - 1, 0.001);
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 3000, ref termCrit);

                        string name = recognizer.Recognize(result);
                        if (name != ",.bmp")
                        {
                            var thisMedia = medias.FirstOrDefault(s => s.Contains(name.Replace(".bmp",string.Empty)));
                            ImageSource imageSource = new BitmapImage(new Uri($"{thisMedia}//image_0.jpeg"));
                            PreviewImg.Source = imageSource;
                        }
                        currentFrame.Draw(name, ref font, new System.Drawing.Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(System.Drawing.Color.LightGreen));
                    }
                }
                CameraImg.Source = ToBitmapSource(currentFrame);
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop
                  .Imaging.CreateBitmapSourceFromHBitmap(
                  ptr,
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }
    }
}
