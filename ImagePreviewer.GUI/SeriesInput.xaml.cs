using System;
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
using System.Windows.Shapes;

namespace ImagePreviewer.GUI
{
    /// <summary>
    /// Interaction logic for SeriesInput.xaml
    /// </summary>
    public partial class SeriesInput : Window
    {
        public event DetailButtonHandler LoadButtonClicked;

        public SeriesInput()
        {
            InitializeComponent();

            tbxUrl.Focus();
        }

        public void btnLoad_Click(Object sender, RoutedEventArgs e)
        {
            Fire(LoadButtonClicked, this, e);
        }

        public List<string> Get() {
            List<string> images = new List<string>();
            int from = Int32.Parse(tbxFrom.Text);
            int to = Int32.Parse(tbxTo.Text);

            for (int i = from; i <= to; i++)
            {
                images.Add(String.Format(tbxUrl.Text, i));
            }

            return images;
        }

        protected void Fire(Delegate dlg, params object[] pList)
        {
            if (dlg != null)
            {
                this.Dispatcher.BeginInvoke(dlg, pList);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbxUrl.Text = tbxExample.Text;
        }
    }
}
