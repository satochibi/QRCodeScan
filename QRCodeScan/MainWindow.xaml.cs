using com.google.zxing;
using com.google.zxing.common;
using com.google.zxing.qrcode;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
                BitmapSource bitmapSource = (BitmapSource)SampleQRImage.Source;//img.ToBitmapSource()
                String? text = scanQRcode(bitmapSource);
                ResultStringLabel.Content = text;

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

                        //BitmapSource bitmapSource = (BitmapSource)SampleQRImage.Source;//img.ToBitmapSource()
                        //String? text = scanQRcode(bitmapSource);

                        Dispatcher.Invoke(() => { ImageData.Source = WriteableBitmapConverter.ToWriteableBitmap(img);  });
                    }
                });
            }
            return false;
        }


        private String? scanQRcode(BitmapSource bitmapSource)
        {
            try
            {
                // create a barcode reader instance
                var reader = new QRCodeReader();
                // load a bitmap
                var barcodeBitmap = ToBitmap(bitmapSource);

                var source = new RGBLuminanceSource(barcodeBitmap, barcodeBitmap.Width, barcodeBitmap.Height);
                BinaryBitmap binaryBitmap = new BinaryBitmap(new GlobalHistogramBinarizer(source));

                var result = reader.decode(binaryBitmap);

                return result?.Text;
            }catch(Exception) {
                return null;
            }

        }

        // ウィンドウが閉じられたときのイベント
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsLoop = true;
        }


        private static Bitmap ToBitmap(BitmapSource bitmapSource)
        {
            // 処理
            var bitmap = new System.Drawing.Bitmap(
                bitmapSource.PixelWidth,
                bitmapSource.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );
            bitmapSource.CopyPixels(
                System.Windows.Int32Rect.Empty,
                bitmapData.Scan0,
                bitmapData.Height * bitmapData.Stride,
                bitmapData.Stride
            );
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }


       




    }
}
