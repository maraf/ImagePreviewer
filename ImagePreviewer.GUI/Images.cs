using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace ImagePreviewer.GUI
{
    public class Images : ObservableCollection<Image>
    {

    }

    public class Image : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapImage _bitmap;
        private bool _checked;

        public BitmapImage Bitmap
        {
            get
            {
                return _bitmap;
            }
            set
            {
                _bitmap = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Bitmap"));
                }
            }
        }

        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                _checked = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Checked"));
                }
                
            }
        }
    }
}
