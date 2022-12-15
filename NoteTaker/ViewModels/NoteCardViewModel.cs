using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NoteTaker.ViewModels
{
    public class NoteCardViewModel : INotifyPropertyChanged
    {
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

        private string _noteString = "Take a note...";
        public string NoteString
        {
            get { return _noteString; }
            set
            {
                if (value != _noteString)
                {
                    _noteString = value;
                    FirePropertyChanged(nameof(NoteString));
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
                Note = NoteString.ToString(),
                UpdatedTime = UpdatedTime.ToString()
            };

            SQLiteDatabaseAccess.DeleteNote(noteToDeleteInDatabase);
            ((MainWindow)Application.Current.MainWindow).mwvm.NoteCards.Remove(noteCard);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
