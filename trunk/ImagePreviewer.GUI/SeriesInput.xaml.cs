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
using DesktopCore;

namespace ImagePreviewer.GUI
{
    /// <summary>
    /// Interaction logic for SeriesInput.xaml
    /// </summary>
    public partial class SeriesInput : Window
    {
        public bool Selected { get; set; }
        public Manager Manager { get; set; }

        public SeriesInput(Manager manager)
        {
            InitializeComponent();

            Selected = false;
            Manager = manager;
            DataContext = Manager;

            cbxUrl.Focus();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
        }

        public void btnLoad_Click(Object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Manager.CurrentFormat) && !String.IsNullOrWhiteSpace(Manager.CurrentFormat) && !Manager.Formats.Contains(Manager.CurrentFormat))
                Manager.Formats.Add(Manager.CurrentFormat);

            Selected = true;
            Close();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Manager.CurrentFormat = Manager.ExampleFormat;
        }
    }
}
