using NoteTaker.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoteTaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsWithinWindowBar,
                    IsHeightBackToMinHeight,
                    IsWidthBackToMinWidth,
                    IsUsed,
                    IsSettingPlaceHolder;
        private double TopMinLocationFromTop,
                       LeftLocation;
        private List<NoteCardModel> AllNotes;

        public MainWindowViewModel mwvm = new MainWindowViewModel();

        private Dictionary<int, NoteWindow> ListOfNoteWindows = new Dictionary<int, NoteWindow>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mwvm;
            HandleWindowStateChanged(this, EventArgs.Empty);
            AllNotes = SQLiteDatabaseAccess.LoadNotes();
            DisplayAllNotesFromDatabase(AllNotes);
        }

        #region Methods
        // ORGANIZE BY TIME
        private void DisplayAllNotesFromDatabase(List<NoteCardModel> allNotes)
        {
            // Descending order - Flip to have ascending order (x.UpdatedTime.CompareTo(y.UpdatedTime))
            AllNotes.Sort((x, y) => y.UpdatedTime.CompareTo(x.UpdatedTime));
            foreach (NoteCardModel noteCard in allNotes)
            {
                NoteCard note = new NoteCard();
                note.vm.Id = noteCard.Id;
                note.vm.NoteString = noteCard.Note;
                note.vm.UpdatedTime = DateTime.ParseExact(noteCard.UpdatedTime, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                note.Padding = new Thickness(0, 0, 0, 7);
                note.MouseDoubleClick += HandleNoteCardMouseDoubleClick;
                mwvm.NoteCards.Add(note);
                OrderNoteCards();
            }
        }

        public void OrderNoteCards()
        {
            mwvm.NoteCards = new ObservableCollection<NoteCard>(mwvm.NoteCards.OrderByDescending(n => n.vm.UpdatedTime));
        }
        #endregion

        #region EventHandlers
        #region Window EventHandlers
        private void HandleMouseDownWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(SearchBox), null);
                Keyboard.ClearFocus();
            }
        }
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
            }
            else if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void HandleWindowActivated(object sender, EventArgs e)
        {
            var window = sender as Window;
            window.Effect = new DropShadowEffect { ShadowDepth = 0, BlurRadius = 30, Color = Color.FromArgb(100, 0, 0, 0), Opacity = .8 };
            WindowBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
            if (SearchBox.Text.Equals("Search...") && SearchBox.IsFocused)
                SearchBox.Foreground = Brushes.Gray;
            else if (!SearchBox.Text.Equals("Search...") && SearchBox.IsFocused)
                SearchBox.Foreground = Brushes.LightGray;
        }

        private void HandleWindowDeactivated(object sender, EventArgs e)
        {
            var window = sender as Window;
            window.Effect = new DropShadowEffect { ShadowDepth = 0, BlurRadius = 30, Color = Color.FromArgb(100, 0, 0, 0), Opacity = .2 };
            WindowBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 100, 100, 100));
            SearchBox.Foreground = Brushes.DarkGray;
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
            NoteCard noteCard = new NoteCard();
            NoteWindow newNote = new NoteWindow(noteCard, false);
            newNote.Show();
            newNote.Closed += HandleNoteWindowClosed;
            noteCard.Padding = new Thickness(0, 0, 0, 7);
            noteCard.MouseDoubleClick += HandleNoteCardMouseDoubleClick;
            noteCard.BentRectangleTop.Visibility = Visibility.Visible;
            noteCard.BentRectangleBottom.Visibility = Visibility.Visible;
            mwvm.NoteCards.Insert(0, noteCard);
        }

        private void HandleNoteCardMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var noteCard = sender as NoteCard;
            NoteWindow existingNote;
            noteCard.BentRectangleTop.Visibility = Visibility.Visible;
            noteCard.BentRectangleBottom.Visibility = Visibility.Visible;
            
            // Fix this to focus textbox each time window is focused
            if (noteCard.IsOpen && ListOfNoteWindows.TryGetValue(noteCard.vm.Id, out existingNote))
            {
                existingNote.Focus();
                FocusManager.SetFocusedElement(existingNote.Griddy, existingNote.NoteTextBox);
            }
            else
            {
                existingNote = new NoteWindow(noteCard, true);
                existingNote.Show();

                noteCard.IsOpen = true;
                ListOfNoteWindows.Add(noteCard.vm.Id, existingNote);
                existingNote.Closed += HandleNoteWindowClosed;
            }
            
        }

        private void HandleNoteWindowClosed(object sender, EventArgs e)
        {
            var noteWindow = sender as NoteWindow;
            noteWindow.Note.BentRectangleTop.Visibility = Visibility.Hidden;
            noteWindow.Note.BentRectangleBottom.Visibility = Visibility.Hidden;
            noteWindow.Note.IsOpen = false;
            ListOfNoteWindows.Remove(noteWindow.Note.vm.Id);
        }
        #endregion
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

        private void HandleSetCursorToBeginning(object sender, MouseButtonEventArgs e)
        {
            TextBox searchBox = sender as TextBox;

            if (searchBox != null)
            {
                if (searchBox.Text.Equals("Search...") && !IsUsed)
                {
                    e.Handled = true;
                    searchBox.Focus();
                    searchBox.Select(0, 0);
                }
            }
        }

        private void HandleTextInput(object sender, TextCompositionEventArgs e)
        {
            var searchBox = sender as TextBox;

            if (searchBox.Text.Equals("Search...") && searchBox.Foreground != Brushes.LightGray)
            {
                searchBox.Text = string.Empty;
                searchBox.Foreground = Brushes.Gray;
            }

            searchBox.Foreground = Brushes.LightGray;
        }

        private void HandleDeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            IsSettingPlaceHolder = true;
            SearchBox.Text = "Search...";
            IsSettingPlaceHolder = false;
            SearchBox.Focus();
            SearchBox.Select(0, 0);

            DeleteTextButton.Visibility = Visibility.Hidden;
            DeleteColumn.Width = new GridLength(0);
            MagnifyingGlassHandle.Stroke = Brushes.DarkGray;
            MagnifyingGlassTop.Stroke = Brushes.DarkGray;
            IsUsed = false;
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;

            if (e.Changes.First().AddedLength > 1 && searchBox.Text.Contains("Search...") && !searchBox.Text.Equals("Search..."))
            {
                searchBox.Text = searchBox.Text.Remove(searchBox.Text.Length - 9);
                searchBox.Select(searchBox.Text.Length, 0);
                searchBox.Foreground = Brushes.LightGray;
            }

            if (e.UndoAction == UndoAction.Create)
            {
                IsUsed = true;
                DeleteTextButton.Visibility = Visibility.Visible;
                DeleteColumn.Width = new GridLength(32);
                MagnifyingGlassHandle.Stroke = Brushes.WhiteSmoke;
                MagnifyingGlassTop.Stroke = Brushes.WhiteSmoke;
            }

            if (mwvm.NoteCards == null)
                return;

            // SEARCH WORKS - LOOKS TERRIBLE, OPTIMIZE AND CLEAN UP
            var cardEnumerator = mwvm.NoteCards.GetEnumerator();
            if (IsUsed && !IsSettingPlaceHolder && searchBox.Text != "")
            {
                while (cardEnumerator.MoveNext())
                {
                    List<int> listOfIndexes = new List<int>();
                    NoteCard currentCard = cardEnumerator.Current as NoteCard;
                    if (!currentCard.vm.NoteString.ToString().Contains(searchBox.Text))
                        currentCard.Visibility = Visibility.Collapsed;
                    else
                    {
                        currentCard.Visibility = Visibility.Visible;

                        for (int index = 0; index < currentCard.NoteCardText.Text.Length; index += searchBox.Text.Length)
                        {
                            index = currentCard.NoteCardText.Text.IndexOf(searchBox.Text, index);
                            if (index == -1)
                                break;
                            listOfIndexes.Add(index);
                        }

                        string temp = currentCard.NoteCardText.Text;
                        int numberOfCharacters = currentCard.NoteCardText.Text.Length;
                        currentCard.NoteCardText.Text = currentCard.NoteCardText.Text.Remove(0, currentCard.NoteCardText.Text.Length);
                        for (int i = 0; i < numberOfCharacters; i++)
                        {
                            if (listOfIndexes.Contains(i))
                            {
                                currentCard.NoteCardText.Inlines.Add(new Run()
                                {
                                    Text = searchBox.Text,
                                    Background = Brushes.Yellow,
                                    Foreground = Brushes.Black
                                });
                                i += searchBox.Text.Length - 1;
                            }
                            else
                                currentCard.NoteCardText.Inlines.Add(new Run(temp[i].ToString()));
                        }
                    }
                }
            }
            else
            {
                while (cardEnumerator.MoveNext())
                {
                    NoteCard currentCard = cardEnumerator.Current as NoteCard;
                    currentCard.Visibility = Visibility.Visible;
                    Binding textBlockBinding = new Binding();
                    textBlockBinding.Source = currentCard.vm;
                    textBlockBinding.Path = new PropertyPath(nameof(currentCard.vm.NoteString));
                    textBlockBinding.Mode = BindingMode.TwoWay;
                    textBlockBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    currentCard.NoteCardText.SetBinding(TextBlock.TextProperty, textBlockBinding);
                }    
            }
        }

        private void HandleTextDeleteAndSpace(object sender, KeyEventArgs e)
        {
            var searchBox = sender as TextBox;

            if (searchBox.Text == String.Empty)
            {
                IsSettingPlaceHolder = true;
                searchBox.Text = "Search...";
                IsSettingPlaceHolder = false;
                searchBox.Foreground = Brushes.Gray;
                DeleteTextButton.Visibility = Visibility.Hidden;
                DeleteColumn.Width = new GridLength(0);
                MagnifyingGlassHandle.Stroke = Brushes.DarkGray;
                MagnifyingGlassTop.Stroke = Brushes.DarkGray;
                IsUsed = false;
            }


            if (searchBox.Text.Equals("Search...") && e.Key == Key.Space && searchBox.SelectionStart == 0)
                searchBox.Text = string.Empty;

            if (searchBox.Text.Equals("Search...") && e.Key == Key.Delete && searchBox.SelectionStart == 0)
                searchBox.IsReadOnly = true;

            if (e.IsUp)
                searchBox.IsReadOnly = false;
        }
        #endregion

        // Maybe add animation later
        private void HandleMouseEnterScrollBar(object sender, MouseEventArgs e)
        {
            var scrollBar = sender as ScrollBar;

            mwvm.StackPanelMargin = new Thickness(0, 0, -5, 0);
            mwvm.ScrollViewerMargin = new Thickness(10, 0, 0, 1);
            mwvm.ScrollBarArrowButtonHeight = (int)SystemParameters.VerticalScrollBarButtonHeight;
            mwvm.TrackWidth = (int)SystemParameters.VerticalScrollBarWidth;
            mwvm.TrackMargin = new Thickness(0);
        }

        private void HandleMouseLeaveScrollBar(object sender, MouseEventArgs e)
        {
            mwvm.StackPanelMargin = new Thickness(0);
            mwvm.ScrollViewerMargin = new Thickness(10, 0, -5, 1);
            mwvm.ScrollBarArrowButtonHeight = 0;
            mwvm.TrackWidth = 2;
            mwvm.TrackMargin = new Thickness(0, 14, 0, 14);
        }
        #endregion
    }
}
