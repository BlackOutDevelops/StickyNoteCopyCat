using NoteTaker.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Windows.Markup;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;

namespace NoteTaker
{
    /// <summary>
    /// Interaction logic for NoteWindow.xaml
    /// </summary>
    public partial class NoteWindow : Window
    {
        public NoteCard Note;
        private NoteCardModel NoteCardM = new NoteCardModel();
        private NoteCardViewModel NoteCardVM;

        public bool IsWithinWindowBar,
                    IsHeightBackToMinHeight,
                    IsWidthBackToMinWidth,
                    IsInDatabase,
                    IsModified;
        private double TopMinLocationFromTop,
                       LeftLocation;

        public NoteWindow(NoteCard notes, bool isInDatabase, bool isModified)
        {
            IsModified = isModified;
            IsInDatabase = isInDatabase;
            Note = notes;
            NoteCardVM = notes.DataContext as NoteCardViewModel;
            DataContext = NoteCardVM;
            InitializeComponent();
            HandleWindowStateChanged(this, EventArgs.Empty);
        }
        #region EventHandlers
        #region Window EventHandlers
        private void HandleWindowActivated(object sender, EventArgs e)
        {
            var window = sender as Window;
            window.Effect = new DropShadowEffect { ShadowDepth = 0, BlurRadius = 30, Color = Color.FromArgb(100, 0, 0, 0), Opacity = .8 };
            WindowBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        }

        private void HandleWindowDeactivated(object sender, EventArgs e)
        {
            var window = sender as Window;
            window.Effect = new DropShadowEffect { ShadowDepth = 0, BlurRadius = 30, Color = Color.FromArgb(100, 0, 0, 0), Opacity = .2 };
            WindowBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
        }

        private void HandleWindowStateChanged(object sender, EventArgs e)
        {
            var window = sender as Window;

            if (window.WindowState == WindowState.Maximized)
            {
                WindowBorder.BorderThickness = new Thickness(0);
                WindowBorder.CornerRadius = new CornerRadius(0);
                WindowBorder.Padding = new Thickness(1, 1, 0, 0);
                WindowBorder.Margin = new Thickness(7);
            }
            else if (window.WindowState == WindowState.Normal)
            {
                WindowBorder.BorderThickness = new Thickness(1);
                WindowBorder.CornerRadius = new CornerRadius(8);
                WindowBorder.Padding = new Thickness(1, 1, 0, 0);
                WindowBorder.Margin = new Thickness(15);
            }
        }
        #endregion
        #region TextBox EventHandlers
        // COULD BE OPTIMIZED
        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            var noteTextBox = sender as TextBox;

            // Work On
            if (noteTextBox.Text.Equals("Take a note...") && !IsModified)
            {
                noteTextBox.Foreground = Brushes.Gray;
            }

            if (NoteCardVM.NoteString.ToString() != noteTextBox.Text)
            {
                // Update UI
                NoteCardVM.NoteString = noteTextBox.Text;
                NoteCardVM.FirePropertyChanged(nameof(NoteCardVM.NoteString));
                NoteCardVM.UpdatedTime = DateTime.Now;
                ((MainWindow)App.Current.MainWindow).OrderNoteCards();


                // Update model to send to database
                NoteCardM.Note = NoteCardVM.NoteString;
                NoteCardM.UpdatedTime = NoteCardVM.UpdatedTime.ToString();

                if (!IsInDatabase)
                {
                    SQLiteDatabaseAccess.SaveNewNote(NoteCardM);
                    NoteCardVM.Id = NoteCardM.Id = SQLiteDatabaseAccess.LoadRecentNoteDatabaseID();
                    IsInDatabase = true;
                }
                else
                {
                    NoteCardM.Id = NoteCardVM.Id;
                    SQLiteDatabaseAccess.SaveCurrentNote(NoteCardM);
                }
            }
        }
        
        private void HandleTextInput(object sender, TextCompositionEventArgs e)
        {
            IsModified = true;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                IsModified = true;
        }
        #endregion
        #endregion
    }
}
