using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace NoteTaker.ViewModels
{
    public class NoteCardViewModel : INotifyPropertyChanged
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

        private Thickness _scrollViewerMargin = new Thickness(10, 0, -5, 1);
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

        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    FirePropertyChanged(nameof(Id));
                }
            }
        }

        private StringBuilder _imagePaths = new StringBuilder();
        public StringBuilder ImagePaths
        {
            get { return _imagePaths; }
            set
            {
                if (value != _imagePaths)
                {
                    _imagePaths = value;
                    FirePropertyChanged(nameof(ImagePaths));
                }
            }
        }

        private DateTime _updatedTime;
        public DateTime UpdatedTime
        {
            get { return _updatedTime; }
            set
            {
                if (value != _updatedTime)
                {
                    _updatedTime = value;
                    FirePropertyChanged(nameof(UpdatedTime));
                }
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (value != _isOpen)
                {
                    _isOpen = value;
                    FirePropertyChanged(nameof(IsOpen));
                }
            }
        }

        private bool _isBold;
        public bool IsBold
        {
            get { return _isBold; }
            set
            {
                if (value != _isBold)
                {
                    _isBold = value;
                    FirePropertyChanged(nameof(IsBold));
                }
            }
        }

        private bool _isItalic;
        public bool IsItalic
        {
            get { return _isItalic; }
            set
            {
                if (value != _isItalic)
                {
                    _isItalic = value;
                    FirePropertyChanged(nameof(IsItalic));
                }
            }
        }

        private bool _isStrikethrough;
        public bool IsStrikethrough
        {
            get { return _isStrikethrough; }
            set
            {
                if (value != _isStrikethrough)
                {
                    _isStrikethrough = value;
                    FirePropertyChanged(nameof(IsStrikethrough));
                }
            }
        }

        private bool _isUnderline;
        public bool IsUnderline
        {
            get { return _isUnderline; }
            set
            {
                if (value != _isUnderline)
                {
                    _isUnderline = value;
                    FirePropertyChanged(nameof(IsUnderline));
                }
            }
        }

        private bool _isBullets;
        public bool IsBullets
        {
            get { return _isBullets; }
            set
            {
                if (value != _isBullets)
                {
                    _isBullets = value;
                    FirePropertyChanged(nameof(IsBullets));
                }
            }
        }

        public ICommand DeleteNoteCommand { get; set; }
        public ICommand OpenOrCloseNoteCommand { get; set; }

        public NoteCardViewModel() 
        {
            DeleteNoteCommand = new Command(DeleteNoteCommandExecute, DeleteNoteCommandCanExecute);
            OpenOrCloseNoteCommand = new Command(OpenOrCloseNoteCommandExecute, OpenOrCloseNoteCommandCanExecute);
        }

        private bool OpenOrCloseNoteCommandCanExecute(object parameter)
        {
            return true;
        }

        private void OpenOrCloseNoteCommandExecute(object parameter)
        {
            var noteCard = parameter as NoteCard;
            NoteWindow noteWindow;

            if (noteCard.vm.IsOpen)
            {
                if(((MainWindow)Application.Current.MainWindow).ListOfNoteWindows.TryGetValue(noteCard.vm.Id, out noteWindow))
                {
                    noteWindow.Close();
                    ((MainWindow)Application.Current.MainWindow).ListOfNoteWindows.Remove(noteCard.vm.Id);
                }
                    
                noteCard.vm.IsOpen = false;
            }
            else
            {
                noteCard.vm.IsOpen = true;
                noteCard.BentRectangleTop.Visibility = Visibility.Visible;
                noteCard.BentRectangleBottom.Visibility = Visibility.Visible;
                noteWindow = new NoteWindow(noteCard, true, true);
                ((MainWindow)Application.Current.MainWindow).ListOfNoteWindows.Add(noteCard.vm.Id, noteWindow);
                noteWindow.Show();
                noteWindow.Closed += ((MainWindow)Application.Current.MainWindow).HandleNoteWindowClosed;
            }
        }

        private bool DeleteNoteCommandCanExecute(object parameter)
        {
            return true;
        }

        private void DeleteNoteCommandExecute(object parameter)
        {
            var noteCard = parameter as NoteCard;
            NoteCardModel noteToDeleteInDatabase = new NoteCardModel()
            {
                Id = Id,
                UpdatedTime = UpdatedTime.ToString()
            };

            // Remove from database
            SQLiteDatabaseAccess.DeleteNote(noteToDeleteInDatabase);

            // Remove all images associated with NoteCard
            string[] imagePaths = noteCard.vm.ImagePaths.ToString().Split(new string[] { "[", "][", "]" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (ImageButton imageButton in noteCard.NoteCardImageCarousel.ImageStackPanel.Children)
                imageButton.ButtonImage.Dispose();

            foreach (string imagePath in imagePaths) 
                File.Delete(imagePath);

            // Remove reference of NoteCard
            ((MainWindow)Application.Current.MainWindow).mwvm.NoteCards.Remove(noteCard);
            
            // If NoteWindow is open, close it
            try
            {
                NoteWindow openWindow;
                ((MainWindow)Application.Current.MainWindow).ListOfNoteWindows.TryGetValue(noteCard.vm.Id, out openWindow);
                openWindow.Close();
                ((MainWindow)Application.Current.MainWindow).ListOfNoteWindows.Remove(noteCard.vm.Id);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
