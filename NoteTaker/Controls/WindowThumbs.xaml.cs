using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoteTaker
{
    /// <summary>
    /// Interaction logic for WindowThumbs.xaml
    /// </summary>
    public partial class WindowThumbs : UserControl
    {
        public bool IsWithinWindowBar,
                    IsHeightBackToMinHeight,
                    IsWidthBackToMinWidth;
        private double TopMinLocationFromTop,
                       LeftLocation;
        private Window CurrentWindow;
        public WindowThumbs()
        {
            InitializeComponent();
        }

        #region Thumb EventHandlers
        // OPTIMIZE? FLICKERING...
        private void HandleTopThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsHeightBackToMinHeight && e.VerticalChange <= 0)
                IsHeightBackToMinHeight = false;

            if (CurrentWindow.Height >= CurrentWindow.MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(CurrentWindow.Height - e.VerticalChange <= 0))
                {
                    CurrentWindow.Height -= e.VerticalChange;
                    CurrentWindow.Top += e.VerticalChange;
                }
            }
            else
            {
                CurrentWindow.Top = TopMinLocationFromTop;
                CurrentWindow.Height = CurrentWindow.MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }
        }

        // OPTIMIZE? FLICKERING?
        private void HandleTopRightThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsHeightBackToMinHeight && e.VerticalChange <= 0)
                IsHeightBackToMinHeight = false;

            if (IsWidthBackToMinWidth && e.HorizontalChange >= 0)
                IsWidthBackToMinWidth = false;

            if (CurrentWindow.Height > CurrentWindow.MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(CurrentWindow.Height - e.VerticalChange <= 0))
                {
                    CurrentWindow.Height -= e.VerticalChange;
                    CurrentWindow.Top += e.VerticalChange;
                }
            }
            else
            {
                CurrentWindow.Top = TopMinLocationFromTop;
                CurrentWindow.Height = CurrentWindow.MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }

            if (CurrentWindow.Width >= CurrentWindow.MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(CurrentWindow.Width + e.HorizontalChange <= 0))
                    CurrentWindow.Width += e.HorizontalChange;
            }
            else
            {
                CurrentWindow.Width = CurrentWindow.MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        private void HandleRightThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsWidthBackToMinWidth && e.HorizontalChange >= 0)
                IsWidthBackToMinWidth = false;

            if (CurrentWindow.ActualWidth >= CurrentWindow.MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(CurrentWindow.Width + e.HorizontalChange <= 0))
                    CurrentWindow.Width += e.HorizontalChange;
            }
            else
            {
                CurrentWindow.Width = CurrentWindow.MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        private void HandleBottomRightThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsHeightBackToMinHeight && e.VerticalChange >= 0 || IsWidthBackToMinWidth && e.HorizontalChange <= 0)
            {
                IsHeightBackToMinHeight = false;
                IsWidthBackToMinWidth = false;
            }

            if (CurrentWindow.Height >= CurrentWindow.MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(CurrentWindow.Height + e.VerticalChange <= 0))
                {
                    CurrentWindow.Height += e.VerticalChange;
                }
            }
            else
            {
                CurrentWindow.Height = MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }

            if (CurrentWindow.Width >= CurrentWindow.MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(CurrentWindow.Width + e.HorizontalChange <= 0))
                    CurrentWindow.Width += e.HorizontalChange;
            }
            else
            {
                CurrentWindow.Width = CurrentWindow.MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        // OPTIMIZE? FLICKERING
        private void HandleBottomLeftThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsHeightBackToMinHeight && e.VerticalChange >= 0 || IsWidthBackToMinWidth && e.HorizontalChange <= 0)
            {
                IsHeightBackToMinHeight = false;
                IsWidthBackToMinWidth = false;
            }

            if (CurrentWindow.Height >= CurrentWindow.MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(CurrentWindow.Height + e.VerticalChange <= 0))
                {
                    CurrentWindow.Height += e.VerticalChange;
                }
            }
            else
            {
                CurrentWindow.Height = CurrentWindow.MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }

            if (CurrentWindow.Width > CurrentWindow.MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(CurrentWindow.Width - e.HorizontalChange <= 0))
                {
                    CurrentWindow.Width -= e.HorizontalChange;
                    CurrentWindow.Left += e.HorizontalChange;
                }
            }
            else
            {
                CurrentWindow.Left = LeftLocation;
                CurrentWindow.Width = CurrentWindow.MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        private void HandleBottomThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsHeightBackToMinHeight && e.VerticalChange >= 0)
                IsHeightBackToMinHeight = false;

            if (CurrentWindow.Height >= CurrentWindow.MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(CurrentWindow.Height + e.VerticalChange <= 0))
                {
                    CurrentWindow.Height += e.VerticalChange;
                }
            }
            else
            {
                CurrentWindow.Height = CurrentWindow.MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }
        }

        // OPTIMIZE? FLICKERING
        private void HandleLeftThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsWidthBackToMinWidth && e.HorizontalChange <= 0)
                IsWidthBackToMinWidth = false;

            if (CurrentWindow.ActualWidth > CurrentWindow.MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(CurrentWindow.Width - e.HorizontalChange <= 0))
                {
                    CurrentWindow.Width -= e.HorizontalChange;
                    CurrentWindow.Left += e.HorizontalChange;
                }
            }
            else
            {
                CurrentWindow.Left = LeftLocation;
                CurrentWindow.Width = CurrentWindow.MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        // OPTIMIZE? FIX THIS
        private void HandleTopLeftThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsWidthBackToMinWidth && e.HorizontalChange <= 0)
                IsWidthBackToMinWidth = false;

            if (IsHeightBackToMinHeight && e.VerticalChange <= 0)
                IsHeightBackToMinHeight = false;

            if (CurrentWindow.Height > CurrentWindow.MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(CurrentWindow.Height - e.VerticalChange <= 0))
                {
                    CurrentWindow.Height -= e.VerticalChange;
                    CurrentWindow.Top += e.VerticalChange;
                }
            }
            else
            {
                CurrentWindow.Top = TopMinLocationFromTop;
                CurrentWindow.Height = CurrentWindow.MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }

            if (CurrentWindow.ActualWidth > CurrentWindow.MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(CurrentWindow.Width - e.HorizontalChange <= 0))
                {
                    CurrentWindow.Width -= e.HorizontalChange;
                    CurrentWindow.Left += e.HorizontalChange;
                }
            }
            else
            {
                CurrentWindow.Left = LeftLocation;
                CurrentWindow.Width = CurrentWindow.MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        private void HandleThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            TopMinLocationFromTop = CurrentWindow.Top + (CurrentWindow.ActualHeight - CurrentWindow.MinHeight);
            LeftLocation = CurrentWindow.Left + (CurrentWindow.ActualWidth - CurrentWindow.MinWidth);
            //Debug.WriteLine("Top Location: " + TopMinLocationFromTop + "\nTop: " + Top + "\nMinHeight: " + MinHeight);
            //Debug.WriteLine("Left Location: " + LeftLocation + "\nLeft: " + Left + "\nMinWidth: " + MinWidth);
        }

        private void HandleLayoutUpdated(object sender, EventArgs e)
        {
            if (CurrentWindow == null)
                CurrentWindow = Window.GetWindow(this);
        }
        #endregion
    }
}
