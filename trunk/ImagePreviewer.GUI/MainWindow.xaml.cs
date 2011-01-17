using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Threading;

namespace ImagePreviewer.GUI
{
    public delegate void DetailButtonHandler(object sender, RoutedEventArgs e);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Images images = new Images();
        FolderBrowserDialog fbd = new FolderBrowserDialog();

        public static DependencyProperty TotalCountProperty = DependencyProperty.Register("TotalCount", typeof(int), typeof(MainWindow));
        public static DependencyProperty LoadedCountProperty = DependencyProperty.Register("LoadedCount", typeof(int), typeof(MainWindow));
        public static DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MainWindow));
        public static DependencyProperty DownloadingCountProperty = DependencyProperty.Register("DownloadingCount", typeof(int), typeof(MainWindow));
        public static DependencyProperty DownloadedCountProperty = DependencyProperty.Register("DownloadedCount", typeof(int), typeof(MainWindow));

        /// <summary>
        /// Celkovy pocet nacitanych obrazky
        /// </summary>
        public int TotalCount
        {
            get { return (int)GetValue(TotalCountProperty); }
            set { SetValue(TotalCountProperty, value); }
        }

        /// <summary>
        /// Jiz nacteny pocet obrazku
        /// </summary>
        public int LoadedCount
        {
            get { return (int)GetValue(LoadedCountProperty); }
            set { SetValue(LoadedCountProperty, value); }
        }

        /// <summary>
        /// Zprava
        /// </summary>
        public string Message {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// Celkovy pocet stahovanych obrazku
        /// </summary>
        public int DownloadingCount
        {
            get { return (int)GetValue(DownloadingCountProperty); }
            set { SetValue(DownloadingCountProperty, value); }
        }

        /// <summary>
        /// Pocet jiz stazenych obrazku
        /// </summary>
        public int DownloadedCount
        {
            get { return (int)GetValue(DownloadedCountProperty); }
            set { SetValue(DownloadedCountProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = images;

            #region ZOOMING
            TransformGroup group = new TransformGroup();

            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);

            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            imgDetail.RenderTransform = group;
            #endregion

            LoadedCount = 5;
            TotalCount = 10;
        }

        private void CreateImage(string url)
        {
            TotalCount++;

            ThreadStart start = delegate()
            {
                DispatcherOperation op = Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action<string>(CreateImageSync),
                    url);

                DispatcherOperationStatus status = op.Status;
                while (status != DispatcherOperationStatus.Completed)
                {
                    status = op.Wait(TimeSpan.FromMilliseconds(1000));
                    if (status == DispatcherOperationStatus.Aborted)
                    {
                        System.Windows.MessageBox.Show("Error add image!");
                    }
                }
            };

            // Create the thread and kick it started!
            new Thread(start).Start();
        }

        private void CreateImageSync(string url)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(url);
                image.DownloadCompleted += delegate
                {
                    Image img = new Image();
                    img.Bitmap = image;
                    img.Checked = false;
                    images.Add(img);
                    LoadedCount++;
                };
                image.DownloadFailed += delegate
                {
                    TotalCount--;
                };
                image.EndInit();
            }
            catch (UriFormatException e)
            {
                //TODO: ???
            }
        }

        private void DownloadImage(string url)
        {
            ThreadStart start = delegate()
            {
                DispatcherOperation op = Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action<string>(DownloadImageAsync),
                    url);

                DispatcherOperationStatus status = op.Status;
                while (status != DispatcherOperationStatus.Completed)
                {
                    status = op.Wait(TimeSpan.FromMilliseconds(1000));
                    if (status == DispatcherOperationStatus.Aborted)
                    {
                        System.Windows.MessageBox.Show("Error downloading image!");
                    }
                }
            };

            new Thread(start).Start();
        }

        private void DownloadImageAsync(string url)
        {
            Guid photoID = System.Guid.NewGuid();
            String photolocation = photoID.ToString() + ".jpg";  //file name 
            WriteBytesToFile(fbd.SelectedPath + @"\" + photolocation, GetBytesFromUrl(url));
            DownloadedCount++;

            if (DownloadingCount == DownloadedCount)
            {
                Message = "All selected images downloaded.";
            }
        }

        private void ClearImages()
        {
            images.Clear();
            TotalCount = 0;
            LoadedCount = 0;
            DownloadedCount = 0;
            DownloadingCount = 0;
        }

        #region ZOOMING

        Point start;
        Point origin;

        private void imgDetail_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)imgDetail.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

            //var position = e.GetPosition(imgDetail);
            //imgDetail.RenderTransformOrigin = new Point(position.X / imgDetail.ActualWidth, position.Y / imgDetail.ActualHeight);

            double zoom = e.Delta > 0 ? .2 : -.2;
            if ((transform.ScaleX + zoom) >= 1)
            {
                transform.ScaleX += zoom;
            }
            if ((transform.ScaleY + zoom) >= 1)
            {
                transform.ScaleY += zoom;
            }
        }
        private void imgDetail_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            imgDetail.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)imgDetail.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(borDetail);
            origin = new Point(tt.X, tt.Y);
        }

        private void imgDetail_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!imgDetail.IsMouseCaptured) return;

            var tt = (TranslateTransform)((TransformGroup)imgDetail.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Vector v = start - e.GetPosition(borDetail);
            Message = String.Format("Origin({0},{1}),v({0},{1})", origin.X, origin.Y, v.X, v.Y);
            tt.X = origin.X - v.X;

            //if ((origin.Y - v.Y) > 0)
            //{
                tt.Y = origin.Y - v.Y;
            //}
        }

        private void imgDetail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imgDetail.ReleaseMouseCapture();
        }

        #endregion

        static public byte[] GetBytesFromUrl(string url)
        {
            byte[] b;
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            WebResponse myResp = myReq.GetResponse();

            Stream stream = myResp.GetResponseStream();
            //int i;
            using (BinaryReader br = new BinaryReader(stream))
            {
                //i = (int)(stream.Length);
                b = br.ReadBytes(500000);
                br.Close();
            }
            myResp.Close();
            return b;
        }

        static public void WriteBytesToFile(string fileName, byte[] content)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            BinaryWriter w = new BinaryWriter(fs);
            try
            {
                w.Write(content);
            }
            finally
            {
                fs.Close();
                w.Close();
            }

        }

        private void lbxImages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxImages.SelectedItem != null)
            {
                splInfo.Visibility = System.Windows.Visibility.Visible;
                imgDetail.Visibility = System.Windows.Visibility.Visible;

                Image image = (Image)lbxImages.SelectedItem;
                imgDetail.Source = image.Bitmap;
                lblDetailUrl.Content = image.Bitmap.UriSource.AbsoluteUri;
                lblDetailWidth.Content = (int)image.Bitmap.Width;
                lblDetailHeight.Content = (int)image.Bitmap.Height;
            }
            else
            {
                splInfo.Visibility = System.Windows.Visibility.Hidden;
                imgDetail.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //fbd.RootFolder = Environment.SpecialFolder.Personal;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Message = "";
                scpDownload.Visibility = System.Windows.Visibility.Visible;
                DownloadedCount = 0;
                foreach (Image lbi in lbxImages.Items)
                {
                    if (lbi.Checked)
                    {
                        DownloadImage(lbi.Bitmap.UriSource.AbsoluteUri);
                    }
                }
            }

        }

        private void btnSeriesInput_Click(object sender, RoutedEventArgs e)
        {
            if (cbxAppend.IsChecked != null && cbxAppend.IsChecked == false)
                ClearImages();

            SeriesInput si = new SeriesInput();
            si.LoadButtonClicked += delegate
            {
                si.Hide();
                foreach(string link in si.Get()) {
                    CreateImage(link);
                }
                si.Close();
            };
            si.Closed += delegate { si = null; };
            si.Show();
        }

        private void btnLinksInput_Click(object sender, RoutedEventArgs e)
        {
            if (cbxAppend.IsChecked != null && cbxAppend.IsChecked == false)
                ClearImages();

            LinkExtractor.MainForm form = new LinkExtractor.MainForm();
            form.SubstractLinksButtonClicked += delegate {
                form.Hide();
                foreach (string link in form.GetFilteredLinks())
                {
                    CreateImage(link);
                }
                form.Close();
            };
            form.FormClosing += delegate { form = null; };
            form.Show();
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(lblDetailUrl.Content.ToString());
        }

        private void ListBoxItem_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (lbxImages.SelectedItem != null && e.Key.Equals(Key.Space))
            {
                if (images[lbxImages.SelectedIndex].Checked)
                {
                    images[lbxImages.SelectedIndex].Checked = false;
                    DownloadingCount--;
                }
                else
                {
                    images[lbxImages.SelectedIndex].Checked = true;
                    DownloadingCount++;
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearImages();
        }
    }
}
