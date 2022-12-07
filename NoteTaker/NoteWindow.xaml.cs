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
                    IsInDatabase;
        private double TopMinLocationFromTop,
                       LeftLocation;

        public NoteWindow(NoteCard notes, bool isInDatabase)
        {
            IsInDatabase = isInDatabase;
            Note = notes;
            NoteCardVM = notes.DataContext as NoteCardViewModel;
            DataContext = NoteCardVM;
            InitializeComponent();
            HandleWindowStateChanged(this, EventArgs.Empty);
        }

        #region EventHandlers
        #region Window EventHandlers
        // Crashes on surface pro when dragging with "System.ComponentModel.Win32Exception: 'The parameter is incorrect'"
        private void HandleMouseDownRow(object sender, MouseButtonEventArgs e)
        {
            var windowBar = sender as Border;
            Point pointToWindow = e.GetPosition(this);
            Point mousePosition = PointToScreen(pointToWindow);

            if (e.ChangedButton == MouseButton.Left && WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;

                Left = mousePosition.X - windowBar.ActualWidth / 2;
                Top = mousePosition.Y - windowBar.ActualHeight / 2;
                DragMove();
            } // Crashes right on DragMove(); Line 74 Maybe dragging with touchpad?
            else if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

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
            var closeButtonBorder = CloseButton.Template.FindName("ButtonBorder", CloseButton) as Border;
            var addButtonBorder = AddButton.Template.FindName("ButtonBorder", AddButton) as Border;

            if (window.WindowState == WindowState.Maximized)
            {
                WindowBorder.BorderThickness = new Thickness(0);
                WindowBorder.CornerRadius = new CornerRadius(0);
                WindowBorder.Padding = new Thickness(1, 1, 0, 0);
                WindowBorder.Margin = new Thickness(7);
                if (closeButtonBorder != null || addButtonBorder != null)
                {
                    closeButtonBorder.CornerRadius = new CornerRadius(0);
                    addButtonBorder.CornerRadius = new CornerRadius(0);
                }
            }
            else if (window.WindowState == WindowState.Normal)
            {
                WindowBorder.BorderThickness = new Thickness(1);
                WindowBorder.CornerRadius = new CornerRadius(8);
                WindowBorder.Padding = new Thickness(1, 1, 0, 0);
                WindowBorder.Margin = new Thickness(15);
                if (closeButtonBorder != null || addButtonBorder != null)
                {
                    closeButtonBorder.CornerRadius = new CornerRadius(0, 7, 0, 0);
                    addButtonBorder.CornerRadius = new CornerRadius(7, 0, 0, 0);
                }
            }
        }

        private void HandleCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HandleMaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                MaximizeButton.Content = "❐";
                MaximizeButton.FontSize = 20;
                MaximizeButton.Padding = new Thickness(5);
            }
            else if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeButton.Content = "□";
                MaximizeButton.FontSize = 33;
                MaximizeButton.Padding = new Thickness(-9);
            }
        }

        private void HandleAddButtonClick(object sender, RoutedEventArgs e)
        {
            NoteCard note = new NoteCard();
            NoteWindow newNote = new NoteWindow(note, false);
            newNote.Show();
            note.Padding = new Thickness(0, 0, 0, 7);
            ((MainWindow)App.Current.MainWindow).mwvm.NoteCards.Insert(0, note);
        }
        #endregion
        // GOOD
        #region Thumb EventHandlers
        // OPTIMIZE? FLICKERING...
        private void HandleTopThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsHeightBackToMinHeight && e.VerticalChange <= 0)
                IsHeightBackToMinHeight = false;

            if (Height >= MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(Height - e.VerticalChange <= 0))
                {
                    Height -= e.VerticalChange;
                    Top += e.VerticalChange;
                }
            }
            else
            {
                Top = TopMinLocationFromTop;
                Height = MinHeight + 1;
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

            if (Height > MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(Height - e.VerticalChange <= 0))
                {
                    Height -= e.VerticalChange;
                    Top += e.VerticalChange;
                }
            }
            else
            {
                Top = TopMinLocationFromTop;
                Height = MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }

            if (Width >= MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(Width + e.HorizontalChange <= 0))
                    Width += e.HorizontalChange;
            }
            else
            {
                Width = MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        private void HandleRightThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsWidthBackToMinWidth && e.HorizontalChange >= 0)
                IsWidthBackToMinWidth = false;

            if (ActualWidth >= MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(Width + e.HorizontalChange <= 0))
                    Width += e.HorizontalChange;
            }
            else
            {
                Width = MinWidth + 1;
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

            if (Height >= MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(Height + e.VerticalChange <= 0))
                {
                    Height += e.VerticalChange;
                }
            }
            else
            {
                Height = MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }

            if (Width >= MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(Width + e.HorizontalChange <= 0))
                    Width += e.HorizontalChange;
            }
            else
            {
                Width = MinWidth + 1;
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

            if (Height >= MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(Height + e.VerticalChange <= 0))
                {
                    Height += e.VerticalChange;
                }
            }
            else
            {
                Height = MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }

            if (Width > MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(Width - e.HorizontalChange <= 0))
                {
                    Width -= e.HorizontalChange;
                    Left += e.HorizontalChange;
                }
            }
            else
            {
                Left = LeftLocation;
                Width = MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        private void HandleBottomThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsHeightBackToMinHeight && e.VerticalChange >= 0)
                IsHeightBackToMinHeight = false;

            if (Height >= MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(Height + e.VerticalChange <= 0))
                {
                    Height += e.VerticalChange;
                }
            }
            else
            {
                Height = MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }
        }

        // OPTIMIZE? FLICKERING
        private void HandleLeftThumbDrag(object sender, DragDeltaEventArgs e)
        {
            if (IsWidthBackToMinWidth && e.HorizontalChange <= 0)
                IsWidthBackToMinWidth = false;

            if (ActualWidth > MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(Width - e.HorizontalChange <= 0))
                {
                    Width -= e.HorizontalChange;
                    Left += e.HorizontalChange;
                }
            }
            else
            {
                Left = LeftLocation;
                Width = MinWidth + 1;
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

            if (Height > MinHeight && !IsHeightBackToMinHeight)
            {
                if (!(Height - e.VerticalChange <= 0))
                {
                    Height -= e.VerticalChange;
                    Top += e.VerticalChange;
                }
            }
            else
            {
                Top = TopMinLocationFromTop;
                Height = MinHeight + 1;
                IsHeightBackToMinHeight = true;
            }

            if (ActualWidth > MinWidth && !IsWidthBackToMinWidth)
            {
                if (!(Width - e.HorizontalChange <= 0))
                {
                    Width -= e.HorizontalChange;
                    Left += e.HorizontalChange;
                }
            }
            else
            {
                Left = LeftLocation;
                Width = MinWidth + 1;
                IsWidthBackToMinWidth = true;
            }
        }

        private void HandleThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            TopMinLocationFromTop = Top + (ActualHeight - MinHeight);
            LeftLocation = Left + (ActualWidth - MinWidth);
            //Debug.WriteLine("Top Location: " + TopMinLocationFromTop + "\nTop: " + Top + "\nMinHeight: " + MinHeight);
            //Debug.WriteLine("Left Location: " + LeftLocation + "\nLeft: " + Left + "\nMinWidth: " + MinWidth);
        }
        #endregion
        #region TextBox EventHandlers
        private void HandleTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var searchBox = sender as TextBox;

            if (searchBox.Text == String.Empty || string.IsNullOrWhiteSpace(searchBox.Text))
                searchBox.Text = "Search...";

            if (!searchBox.Text.Equals("Search..."))
                searchBox.Foreground = Brushes.DarkGray;
            else if (searchBox.Text.Equals("Search..."))
                searchBox.Foreground = Brushes.DarkGray;
        }

        private void HandleTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var searchBox = sender as TextBox;

            if (searchBox.Text.Equals("Search..."))
                searchBox.Foreground = Brushes.Gray;
            else
                searchBox.Foreground = Brushes.LightGray;
        }

        private void SetCursorToBeginningHandler(object sender, MouseButtonEventArgs e)
        {
            TextBox searchBox = sender as TextBox;

            if (searchBox != null)
            {
                if (searchBox.Text.Equals("Search..."))
                {
                    e.Handled = true;
                    searchBox.Focus();
                    searchBox.Select(0, 0);
                }
            }
        }

        // COULD BE OPTIMIZED
        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            var noteTextBox = sender as TextBox;

            // Work On
            if (noteTextBox.Text.Equals("Take a note..."))
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

        private void TextDeleteAndSpaceHandler(object sender, KeyEventArgs e)
        {
            var searchBox = sender as TextBox;

            if (searchBox.Text == String.Empty)
            {
                searchBox.Text = "Search...";
                searchBox.Foreground = Brushes.Gray;
            }


            if (searchBox.Text.Equals("Search...") && e.Key == Key.Space)
                searchBox.Text = string.Empty;
        }
        #endregion
        #endregion
    }
}
