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
using DesktopCore;
using System.Collections.Specialized;

namespace ImagePreviewer.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> newUrls = new List<string>();
        private System.Windows.Forms.FolderBrowserDialog dialog;

        Manager Manager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeZooming();

            Manager = new Manager();
            Manager.Load(Manager.DefaultLocation);

            DataContext = Manager;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
        }

        #region ZOOMING

        Point start;
        Point origin;

        private void InitializeZooming()
        {
            TransformGroup group = new TransformGroup();

            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);

            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);

            imgDetail.RenderTransform = group;
        }

        private void ResetZooming()
        {
            TransformGroup transformGroup = (TransformGroup)imgDetail.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

            transform.ScaleX = 1;
            transform.ScaleY = 1;
        }

        private void CenterZooming()
        {
            TransformGroup transformGroup = (TransformGroup)imgDetail.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];
            double y = 0;//(borDetail.ActualHeight - imgDetail.ActualHeight * transform.ScaleY) / 2;

            var tt = (TranslateTransform)((TransformGroup)imgDetail.RenderTransform).Children.First(tr => tr is TranslateTransform);
            tt.X = 0;
            tt.Y = y;
        }

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

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Manager.Save(Manager.DefaultLocation);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            grdSave.Visibility = Visibility.Visible;
            cbxIndexBy.ItemsSource = Enum.GetNames(typeof(IndexBy));
        }

        private void btnSeriesInput_Click(object sender, RoutedEventArgs e)
        {
            grdSeries.Visibility = Visibility.Visible;
            cbxUrl.Focus();
            newUrls.Clear();

            if (cbxAppend.IsChecked != null && cbxAppend.IsChecked == false)
                Manager.Images.Clear();
        }

        private void StartImagesLoading()
        {
            ThreadStart start = delegate()
            {
                DispatcherOperation op = Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action<object, EventArgs>(Images_ImageDownloaded),
                    null,
                    null);

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

        private void Images_ImageDownloaded(object sender, EventArgs e)
        {
            Image image = (Image)sender;
            if (image != null)
                newUrls.Remove(image.Url);
            else
                newUrls.Remove(newUrls[0]);

            while (true)
            {
                try
                {
                    if (newUrls.Count > 0)
                        Manager.Images.Add(newUrls[0], Images_ImageDownloaded);

                    break;
                }
                catch (UriFormatException)
                {
                    newUrls.Remove(newUrls[0]);
                }
            }
        }

        private void btnLinksInput_Click(object sender, RoutedEventArgs e)
        {
            if (cbxAppend.IsChecked != null && cbxAppend.IsChecked == false)
                Manager.Images.Clear();

            LinkExtractor.MainForm form = new LinkExtractor.MainForm();
            form.SubstractLinksButtonClicked += delegate {
                newUrls.AddRange(form.GetFilteredLinks());
                form.Close();
                StartImagesLoading();
            };
            form.FormClosing += delegate { form = null; };
            form.Show();
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (lbxImages.SelectedItem != null)
                System.Diagnostics.Process.Start(((Image)lbxImages.SelectedItem).Url);
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(Manager.Images[lbxImages.SelectedIndex].Url);
        }

        private void lbxImages_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Space) && lbxImages.SelectedItem != null)
                Manager.Images[lbxImages.SelectedIndex].Selected = !Manager.Images[lbxImages.SelectedIndex].Selected;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Manager.Images.Clear();
        }

        private void btnResetZooming_Click(object sender, RoutedEventArgs e)
        {
            ResetZooming();
            CenterZooming();
        }

        private void btnCenterZoom_Click(object sender, RoutedEventArgs e)
        {
            CenterZooming();
        }

        #region SERIES INPUT

        private void btnLoadSerie_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Manager.CurrentFormat) && !String.IsNullOrWhiteSpace(Manager.CurrentFormat) && !Manager.Formats.Contains(Manager.CurrentFormat))
                Manager.Formats.Add(Manager.CurrentFormat);

            grdSeries.Visibility = Visibility.Hidden;

            for (int i = Manager.StartIndex; i <= Manager.EndIndex; i++)
            {
                string url = String.Format(Manager.CurrentFormat, i);
                newUrls.Add(url);
            }
            StartImagesLoading();
        }

        private void btnCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            Manager.CurrentFormat = Manager.ExampleFormat;
        }

        private void btnCloseSeries_Click(object sender, RoutedEventArgs e)
        {
            grdSeries.Visibility = Visibility.Hidden;
        }

        private void btnBrowseToSave_Click(object sender, RoutedEventArgs e)
        {
            if(dialog == null)
                dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Manager.SelectedPath = dialog.SelectedPath;
        }

        private void btnCloseSave_Click(object sender, RoutedEventArgs e)
        {
            grdSave.Visibility = Visibility.Hidden;
        }

        private void btnSaveSerie_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.SelectedPath != null && Directory.Exists(Manager.SelectedPath))
            {
                int index = 1;
                prgDownload.Value = 0;
                prgDownload.Maximum = Manager.Images.SelectedCount;
                foreach (Image img in Manager.Images.Where(i => i.Selected))
                {
                    string fileName;
                    if (Manager.IndexBy == IndexBy.Guid)
                        fileName = String.Format("{0}{1}", Manager.BaseFileName, Guid.NewGuid());
                    else
                        fileName = String.Format("{0}{1}", Manager.BaseFileName, index);

                    FileStream file = new FileStream(System.IO.Path.Combine(Manager.SelectedPath, String.Format("{0}.jpg", fileName)), FileMode.Create);

                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(img.Bitmap));
                    encoder.QualityLevel = 100;
                    encoder.Save(file);

                    file.Close();
                    index++;
                    prgDownload.Value++;
                }

                grdSave.Visibility = Visibility.Hidden;
            }
        }

        #endregion
    }
}
