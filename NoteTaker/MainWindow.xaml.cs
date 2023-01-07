using NoteTaker.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
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
        private List<NoteCardModel> NotesLoadedFromDatabase;

        public MainWindowViewModel mwvm = new MainWindowViewModel();

        public Dictionary<int, NoteWindow> ListOfNoteWindows = new Dictionary<int, NoteWindow>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mwvm;
            HandleWindowStateChanged(this, EventArgs.Empty);
            NotesLoadedFromDatabase = SQLiteDatabaseAccess.LoadNotes();
            DisplayAllNotesFromDatabase(NotesLoadedFromDatabase);
        }

        #region Methods
        // ORGANIZE BY TIME
        private void DisplayAllNotesFromDatabase(List<NoteCardModel> allNotes)
        {
            // Descending order - Flip to have ascending order (x.UpdatedTime.CompareTo(y.UpdatedTime))
            NotesLoadedFromDatabase.Sort((x, y) => y.UpdatedTime.CompareTo(x.UpdatedTime));
            foreach (NoteCardModel noteCard in allNotes)
            {
                NoteCard note = new NoteCard();
                note.vm.Id = noteCard.Id;
                note.vm.UpdatedTime = DateTime.ParseExact(noteCard.UpdatedTime, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                note.vm.ImagePaths.Append(noteCard.ImagePaths);

                // Handle RTF formatting
                if (noteCard.Note != null)
                {
                    TextRange tr = new TextRange(note.NoteCardText.Document.ContentStart, note.NoteCardText.Document.ContentEnd);
                    byte[] rtfByteArray = Encoding.ASCII.GetBytes(noteCard.Note);
                    using (MemoryStream ms = new MemoryStream(rtfByteArray))
                    {
                        tr.Load(ms, DataFormats.Rtf);
                    }
                }
                
                // Handle Images
                if (noteCard.ImagePaths != null)
                {
                    List<string> imageFileLocations = noteCard.ImagePaths.Split(new string[] { "[", "][", "]" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (string fileLocation in imageFileLocations)
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromFile(fileLocation);
                        ImageButton imageButton = new ImageButton(image, fileLocation);
                        imageButton.IsEnabled = false;
                        note.NoteCardImageCarousel.ImageStackPanel.Children.Add(imageButton);
                        note.NoteCardImageButton = imageButton;
                    }
                    note.ImageRow.Height = new GridLength(100);
                }
                
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

        public void HandleNoteCardMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var noteCard = sender as NoteCard;
            NoteWindow existingNote;
            noteCard.BentRectangleTop.Visibility = Visibility.Visible;
            noteCard.BentRectangleBottom.Visibility = Visibility.Visible;

            // Fix this to focus textbox each time window is focused
            if (noteCard.vm.IsOpen && ListOfNoteWindows.TryGetValue(noteCard.vm.Id, out existingNote))
            {
                existingNote.Focus();
                FocusManager.SetFocusedElement(existingNote.Griddy, existingNote.NoteTextBox);
            }
            else
            {
                existingNote = new NoteWindow(noteCard, true, true);
                existingNote.Show();
                existingNote.Left = Left + Width;
                existingNote.Top = Top + 35*ListOfNoteWindows.Count;

                noteCard.vm.IsOpen = true;
                ListOfNoteWindows.Add(noteCard.vm.Id, existingNote);
                existingNote.Closed += HandleNoteWindowClosed;
            }
        }

        public void HandleNoteWindowClosed(object sender, EventArgs e)
        {

            var noteWindow = sender as NoteWindow;
            noteWindow.Note.BentRectangleTop.Visibility = Visibility.Hidden;
            noteWindow.Note.BentRectangleBottom.Visibility = Visibility.Hidden;
            noteWindow.Note.vm.IsOpen = false;
            ListOfNoteWindows.Remove(noteWindow.Note.vm.Id);
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

            var cardEnumerator = mwvm.NoteCards.GetEnumerator();
            if (IsUsed && !IsSettingPlaceHolder && searchBox.Text != "")
            {
                while (cardEnumerator.MoveNext())
                {
                    List<TextRange> textRanges = new List<TextRange>();
                    NoteCard currentCard = cardEnumerator.Current as NoteCard;

                    // Reset card to original appearance
                    currentCard.NoteCardText.SelectAll();
                    currentCard.NoteCardText.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, default);
                    currentCard.NoteCardText.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.WhiteSmoke));

                    int numberOfCharacters = currentCard.NoteCardText.Selection.Text.Length;
                    if (!currentCard.NoteCardText.Selection.Text.ToString().ToLower().Contains(searchBox.Text.ToLower()))
                        currentCard.Visibility = Visibility.Collapsed;
                    else
                    {
                        currentCard.Visibility = Visibility.Visible;

                        TextPointer pointer = currentCard.NoteCardText.Document.ContentStart;
                        string searchBoxText = Regex.Escape(searchBox.Text);
                        Regex regEx = new Regex(searchBoxText, RegexOptions.IgnoreCase);
                        while (pointer != null)
                        {
                            if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                            {
                                string run = pointer.GetTextInRun(LogicalDirection.Forward);
                                var matches = regEx.Matches(run);

                                foreach (Match match in matches)
                                {
                                    TextPointer start = pointer.GetPositionAtOffset(match.Index);
                                    TextPointer end = start.GetPositionAtOffset(match.Length);
                                    textRanges.Add(new TextRange(start, end));
                                }
                            }
                            pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                        }

                        foreach (TextRange range in textRanges)
                        {
                            range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
                            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
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
                    currentCard.NoteCardText.SelectAll();
                    currentCard.NoteCardText.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, default);
                    currentCard.NoteCardText.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.WhiteSmoke));
                    currentCard.NoteCardText.Selection.Select(currentCard.NoteCardText.Document.ContentEnd, currentCard.NoteCardText.Document.ContentEnd);
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
