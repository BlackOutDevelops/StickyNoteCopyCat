using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
