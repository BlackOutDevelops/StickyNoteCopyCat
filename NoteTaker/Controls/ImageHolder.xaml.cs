using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoteTaker.Controls
{
    /// <summary>
    /// Interaction logic for ImageHolder.xaml
    /// </summary>
    public partial class ImageHolder : UserControl
    {
        public ImageHolder()
        {
            InitializeComponent();
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;

            if (e.Delta < 0)
                scrollviewer.LineRight();
            else
                scrollviewer.LineLeft();
        }

        private void HandleLayoutUpdated(object sender, EventArgs e)
        {
            if (ImageScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
                ImageScrollViewer.Padding = new Thickness(0, 0, 0, -15);
            else
                ImageScrollViewer.Padding = new Thickness(0, 0, 0, 2);
        }
    }
}
