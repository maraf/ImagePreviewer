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

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = images;
        }

        private void CreateImage(string url)
        {
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
                };
                image.EndInit();
            }
            catch (UriFormatException e)
            {
                //TODO: ???
            }
        }

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
                foreach (Image lbi in lbxImages.Items)
                {
                    if (lbi.Checked)
                    {
                        Guid photoID = System.Guid.NewGuid();
                        String photolocation = photoID.ToString() + ".jpg";  //file name 
                        WriteBytesToFile(fbd.SelectedPath + @"\" + photolocation, GetBytesFromUrl(lbi.Bitmap.UriSource.AbsoluteUri));
                    }
                }
            }

        }

        private void btnSeriesInput_Click(object sender, RoutedEventArgs e)
        {
            if (cbxAppend.IsChecked != null && cbxAppend.IsChecked == false)
                images.Clear();

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
                images.Clear();

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
                images[lbxImages.SelectedIndex].Checked = !images[lbxImages.SelectedIndex].Checked;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            images.Clear();
        }
    }
}
