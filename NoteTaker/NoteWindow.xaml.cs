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
using System.IO;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

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
        private void HandleSelectionChanged(object sender, RoutedEventArgs e)
        {
            TextSelection selection = NoteTextBox.Selection;
            TextDecorationCollection textDecorationCollection = selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
            NoteCardVM.IsStrikethrough = false;
            NoteCardVM.IsUnderline = false;
            NoteCardVM.IsBold = false;
            NoteCardVM.IsItalic = false;
            NoteCardVM.IsBullets = false;

            if (textDecorationCollection != null)
                foreach (TextDecoration td in textDecorationCollection)
                {
                    if (td.Location == TextDecorationLocation.Underline)
                        NoteCardVM.IsUnderline = true;
                    if (td.Location == TextDecorationLocation.Strikethrough)
                        NoteCardVM.IsStrikethrough = true;
                }

            if (selection.GetPropertyValue(FontWeightProperty).Equals(FontWeights.Bold))
                NoteCardVM.IsBold = true;
            if (selection.GetPropertyValue(FontStyleProperty).Equals(FontStyles.Italic))
                NoteCardVM.IsItalic = true;

            try
            {
                Run selectionRun = selection.Start.GetLineStartPosition(0).GetAdjacentElement(LogicalDirection.Forward) as Run;
                Paragraph selectionParagraph = selectionRun.Parent as Paragraph;
                ListItem selectionListItem = selectionParagraph.Parent as ListItem;

                if (selectionListItem != null)
                    NoteCardVM.IsBullets = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        // COULD BE OPTIMIZED
        private void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            var noteTextBox = sender as RichTextBox;
            TextRange noteTextBoxRange = new TextRange(noteTextBox.Document.ContentStart, noteTextBox.Document.ContentEnd);

            //Work On
            if (noteTextBoxRange.Equals("Take a note...") && !IsModified)
            {
                noteTextBox.Foreground = Brushes.Gray;
            }

            if (NoteCardVM.NoteString.ToString() != noteTextBoxRange.Text)
            {
                // Update UI
                NoteCardVM.NoteString = noteTextBoxRange.Text;
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

        private void HandleTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            NoteTextBox.Focus();
            string[] noteStringParagraphSplit = NoteCardVM.NoteString.Trim().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            NoteTextBox.Document = new FlowDocument();
            foreach (string s in noteStringParagraphSplit)
                NoteTextBox.Document.Blocks.Add((new Paragraph(new Run(s)))); 
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                IsModified = true;
        }

        private void HandleMouseEnterScrollBar(object sender, MouseEventArgs e)
        {
            var scrollBar = sender as ScrollBar;

            NoteCardVM.ScrollViewerMargin = new Thickness(10, 0, 0, 1);
            NoteCardVM.ScrollBarArrowButtonHeight = (int)SystemParameters.VerticalScrollBarButtonHeight;
            NoteCardVM.TrackWidth = (int)SystemParameters.VerticalScrollBarWidth;
            NoteCardVM.TrackMargin = new Thickness(0);
        }

        private void HandleMouseLeaveScrollBar(object sender, MouseEventArgs e)
        {
            NoteCardVM.ScrollViewerMargin = new Thickness(10, 0, -5, 1);
            NoteCardVM.ScrollBarArrowButtonHeight = 0;
            NoteCardVM.TrackWidth = 2;
            NoteCardVM.TrackMargin = new Thickness(0, 14, 0, 14);
        }
        #endregion

        #region TextCustomization
        // Fix all to not set property to whole paragraph
        private void HandleToggleStrikethroughButtonClick(object sender, RoutedEventArgs e)
        {
            TextSelection selection = NoteTextBox.Selection;
            TextDecorationCollection textDecorationCollection = selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
            bool IsStrikethrough = false,
                 IsUnderline = false;

            foreach(TextDecoration td in textDecorationCollection)
            {
                if (td.Location == TextDecorationLocation.Underline)
                    IsUnderline = true;
                if (td.Location == TextDecorationLocation.Strikethrough)
                    IsStrikethrough = true;
            }
            
            if (IsStrikethrough && IsUnderline)
            {
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            }
            else if (IsStrikethrough)
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            else if (IsUnderline)
            {
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, new TextDecorationCollection(Enumerable.Concat(TextDecorations.Strikethrough, TextDecorations.Underline)));
            }
            else if (selection.IsEmpty)
                return;
            else
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
        }

        private void HandleToolBarLoaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            ToggleButton toggleButton = toolBar.Template.FindName("OverflowButton", toolBar) as ToggleButton;
            if (toggleButton != null)
            {
                toggleButton.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void HandleInsertImageButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.InitialDirectory = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Pictures");
            open.Multiselect = true;
            open.AddExtension = true;
            open.Filter = "JPEG and PNG Files|*.jpg;*.png;";
            open.ShowDialog();
            Stream[] files = open.OpenFiles();
            int fileNumber = 0;

            foreach(Stream file in files)
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(open.FileNames[fileNumber]);
                Debug.WriteLine(image.RawFormat);
                ImageButton imageButton = new ImageButton(image);
                ImageCarousel.ImageStackPanel.Children.Add(imageButton);
                fileNumber++;
            }

            ImageRow.Height = new GridLength(4, GridUnitType.Star);
        }
        #endregion

        private void HandleImageHolderLayoutUpdated(object sender, EventArgs e)
        {
            if (ImageCarousel.ImageStackPanel.Children.Count == 0)
                ImageRow.Height = new GridLength(0);
        }
        #endregion
    }
}
