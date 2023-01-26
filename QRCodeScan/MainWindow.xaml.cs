using System.Threading.Tasks;
using System.Windows;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;


namespace QRCodeScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {


        //映像読み取りを続行するかしないか
        private bool IsLoop = false;


        public MainWindow()
        {
            InitializeComponent();

            
            // https://qiita.com/tricogimmick/items/79e85baa1e99eec840d8
            // 画面が読み込まれた最初のイベント
            ContentRendered += async (s, e) =>
            {
                await StartRollingCamera(0);
                ImageData.Source = null;
            };
            
        }

        // https://marunaka-blog.com/take-webcamera-image-with-opencvsharp/839/
        private async Task<bool> StartRollingCamera(int index)
        {
            //カメラ画像取得用のVideoCaptureのインスタンス生成
            var capture = new VideoCapture(index);
            capture.FrameHeight = 640;
            capture.FrameWidth = 360;

            //カメラの接続確認
            if (!capture.IsOpened())
            {
                MessageBox.Show("Can't use camera.");
                return false;
            }

            //カメラの映像取得
            IsLoop = true;
            using (capture)
            using (Mat img = new Mat())
            {
                await Task.Run(() =>
                {
                    while (IsLoop)
                    {
                        capture.Read(img);

                        if (img.Empty()) break;

                       
                        Dispatcher.Invoke(() => ImageData.Source = WriteableBitmapConverter.ToWriteableBitmap(img));
                    }
                });
            }
            return false;
        }

        // ウィンドウが閉じられたときのイベント
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsLoop = true;
        }
    }
}
