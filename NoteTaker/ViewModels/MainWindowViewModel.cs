using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NoteTaker.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private int _scrollBarArrowButtonHeight = 0;
        public int ScrollBarArrowButtonHeight
        {
            get { return _scrollBarArrowButtonHeight; }
            set
            {
                if (value != _scrollBarArrowButtonHeight)
                {
                    _scrollBarArrowButtonHeight = value;
                    FirePropertyChanged(nameof(ScrollBarArrowButtonHeight));
                }
            }
        }

        private int _trackWidth = 2;
        public int TrackWidth
        {
            get { return _trackWidth; }
            set
            {
                if (value != _trackWidth)
                {
                    _trackWidth = value;
                    FirePropertyChanged(nameof(TrackWidth));
                }
            }
        }

        private Thickness _trackMargin = new Thickness(0, 14, 0, 14);
        public Thickness TrackMargin
        {
            get { return _trackMargin; }
            set
            {
                if (value != _trackMargin)
                {
                    _trackMargin = value;
                    FirePropertyChanged(nameof(TrackMargin));
                }
            }
        }

        private Thickness _scrollViewerMargin = new Thickness(10, 0, -5, 0);
        public Thickness ScrollViewerMargin
        {
            get { return _scrollViewerMargin; }
            set
            {
                if (value != _scrollViewerMargin)
                {
                    _scrollViewerMargin = value;
                    FirePropertyChanged(nameof(ScrollViewerMargin));
                }
            }
        }

        private Thickness _StackPanelMargin = new Thickness(0, 0, 0, 0);
        public Thickness StackPanelMargin
        {
            get { return _StackPanelMargin; }
            set
            {
                if (value != _StackPanelMargin)
                {
                    _StackPanelMargin = value;
                    FirePropertyChanged(nameof(StackPanelMargin));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
