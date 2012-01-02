using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using DesktopCore;

namespace ImagePreviewer
{
    public class Images : NotifyObservableCollection<Image>
    {
        private int selectedCount;

        public int SelectedCount
        {
            get { return selectedCount; }
            set
            {
                selectedCount = value;
                FirePropertyChanged("SelectedCount");
            }
        }

        public void Add(string url, EventHandler onDownloaded = null)
        {
            Image image = new Image();
            image.Url = url;
            image.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Selected")
                {
                    if (image.Selected)
                        SelectedCount++;
                    else
                        SelectedCount--;
                }
            };
            Add(image);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(url);
            bitmap.DownloadProgress += delegate(object sender, DownloadProgressEventArgs e)
            {
                image.DownloadState = e.Progress;
            };
            bitmap.DownloadCompleted += delegate
            {
                image.Bitmap = bitmap;
                image.Downloaded = true;
                image.Width = Convert.ToInt32(image.Bitmap.Width);
                image.Height = Convert.ToInt32(image.Bitmap.Height);

                if (onDownloaded != null)
                    onDownloaded(image, new EventArgs());
            };
            bitmap.DownloadFailed += delegate
            {
                Remove(image);

                if (onDownloaded != null)
                    onDownloaded(null, new EventArgs());
            };
            bitmap.EndInit();
        }

        public new void Clear()
        {
            base.Clear();
            SelectedCount = 0;
        }
    }

    public class Image : NotifyPropertyChanged
    {
        private BitmapImage bitmap;
        private bool selected;
        private string url;
        private bool downloaded;
        private int downloadState;

        private int width;
        private int height;

        public BitmapImage Bitmap
        {
            get { return bitmap; }
            set
            {
                bitmap = value;
                Url = bitmap.UriSource.AbsoluteUri;
                FirePropertyChanged("Bitmap");
            }
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                FirePropertyChanged("Selected");
            }
        }

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                FirePropertyChanged("Url");
            }
        }

        public bool Downloaded
        {
            get { return downloaded; }
            set
            {
                downloaded = value;
                FirePropertyChanged("Downloaded");
            }
        }

        public int DownloadState
        {
            get { return downloadState; }
            set
            {
                downloadState = value;
                FirePropertyChanged("DownloadState");
            }
        }

        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                FirePropertyChanged("Width");
            }
        }

        public int Height
        {
            get { return height; }
            set
            {
                height = value;
                FirePropertyChanged("Height");
            }
        }
    }
}
