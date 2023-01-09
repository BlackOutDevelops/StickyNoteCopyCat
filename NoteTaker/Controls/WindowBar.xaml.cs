using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoteTaker.Controls
{
    /// <summary>
    /// Interaction logic for WindowBar.xaml
    /// </summary>
    public partial class WindowBar : UserControl
    {
        private Window CurrentWindow;
        public WindowBar()
        {
            InitializeComponent();
        }

        private void HandleNormalWindowBarPerWindow()
        {
            if (CurrentWindow is MainWindow)
            {
                AddButton.Padding = new Thickness(-7);
                AddButton.Width = 40;
                BackButton.Padding = new Thickness(-7);
                BackButton.Width = 40;
                SettingsButton.Width = 40;
                SettingsButton.Padding = new Thickness(8);
                MaximizeButton.Padding = new Thickness(-9);
                MaximizeButton.Width = 40;
                CloseButton.Padding = new Thickness(-7);
                CloseButton.Width = 40;
            }
            else if (CurrentWindow is NoteWindow)
            {
                AddButton.Padding = new Thickness(-11);
                AddButton.Width = 30;
                SettingsButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Padding = new Thickness(-13);
                MaximizeButton.Width = 30;
                CloseButton.Padding = new Thickness(-11);
                CloseButton.Width = 30;
            }
        }

        private void HandleMaximizedWindowBarPerWindow()
        {
            if (CurrentWindow is MainWindow)
            {
                AddButton.Padding = new Thickness(-7);
                AddButton.Width = 40;
                MaximizeButton.Padding = new Thickness(5);
                MaximizeButton.Width = 40;
                CloseButton.Padding = new Thickness(-7);
                CloseButton.Width = 40;
            }
            else if (CurrentWindow is NoteWindow)
            {
                AddButton.Padding = new Thickness(-11);
                AddButton.Width = 30;
                SettingsButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Padding = new Thickness(0);
                MaximizeButton.Width = 30;
                CloseButton.Padding = new Thickness(-11);
                CloseButton.Width = 30;
            }
        }

        private void HandleWindowStateChanged(object sender, EventArgs e)
        {
            var closeButtonBorder = CloseButton.Template.FindName("ButtonBorder", CloseButton) as Border;
            var addButtonBorder = AddButton.Template.FindName("ButtonBorder", AddButton) as Border;
            var backButtonBorder = BackButton.Template.FindName("ButtonBorder", BackButton) as Border;

            if (CurrentWindow.WindowState == WindowState.Maximized)
            {
                HandleMaximizedWindowBarPerWindow();
                if (closeButtonBorder != null || addButtonBorder != null)
                {
                    closeButtonBorder.CornerRadius = new CornerRadius(0);
                    addButtonBorder.CornerRadius = new CornerRadius(0);
                    if (backButtonBorder != null)
                        backButtonBorder.CornerRadius = new CornerRadius(0);
                }
            }
            else if (CurrentWindow.WindowState == WindowState.Normal)
            {
                HandleNormalWindowBarPerWindow();
                if (closeButtonBorder != null || addButtonBorder != null)
                {
                    closeButtonBorder.CornerRadius = new CornerRadius(0, 7, 0, 0);
                    addButtonBorder.CornerRadius = new CornerRadius(7, 0, 0, 0);
                    if (backButtonBorder != null)
                        backButtonBorder.CornerRadius = new CornerRadius(7, 0, 0, 0);
                }
            }
        }

        private void HandleCloseButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentWindow is MainWindow)
                CurrentWindow.Close();
            else if (CurrentWindow is NoteWindow)
            {
                if (!(CurrentWindow as NoteWindow).IsModified)
                    if (((MainWindow)App.Current.MainWindow) != null)
                        ((MainWindow)App.Current.MainWindow).mwvm.NoteCards.Remove((CurrentWindow as NoteWindow).Note);

                CurrentWindow.Close();
                ((MainWindow)App.Current.MainWindow).ListOfNoteWindows.Remove((CurrentWindow as NoteWindow).Note.vm.Id);
            }
            
        }

        private void HandleBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentWindow is MainWindow)
            {
                SettingsButton.Visibility = Visibility.Visible;
                AddButton.Visibility = Visibility.Visible;
                SettingsLabel.Visibility = Visibility.Collapsed;
                BackButton.Visibility = Visibility.Collapsed;
                (CurrentWindow as MainWindow).Settings.Visibility = Visibility.Collapsed;
                (CurrentWindow as MainWindow).ContentScrollViewer.Visibility = Visibility.Visible;
            }
        }

        private void HandleSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentWindow is MainWindow)
            {
                SettingsButton.Visibility = Visibility.Collapsed;
                AddButton.Visibility = Visibility.Collapsed;
                SettingsLabel.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Visible;
                (CurrentWindow as MainWindow).Settings.Visibility = Visibility.Visible;
                (CurrentWindow as MainWindow).ContentScrollViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void HandleMaximizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentWindow.WindowState == WindowState.Normal)
            {
                CurrentWindow.WindowState = WindowState.Maximized;
                MaximizeButton.Content = "❐";
                MaximizeButton.FontSize = 20;
            }
            else if (CurrentWindow.WindowState == WindowState.Maximized)
            {
                CurrentWindow.WindowState = WindowState.Normal;
                MaximizeButton.Content = "□";
                MaximizeButton.FontSize = 33;
            }
        }

        private void HandleAddButtonClick(object sender, RoutedEventArgs e)
        {
            NoteCard noteCard = new NoteCard();
            NoteWindow newNote = new NoteWindow(noteCard, false, false);
            newNote.Show();
            noteCard.vm.Id = SQLiteDatabaseAccess.LoadRecentNoteDatabaseID();
            ((MainWindow)App.Current.MainWindow).ListOfNoteWindows.Add(newNote.Note.vm.Id ,newNote);
            newNote.WindowStartupLocation = WindowStartupLocation.Manual;
            newNote.Left = CurrentWindow.Left + CurrentWindow.Width;
            // Fix this
            newNote.Top = CurrentWindow.Top + 35 * ((MainWindow)App.Current.MainWindow).ListOfNoteWindows.Count - 35;
            newNote.Closed += ((MainWindow)App.Current.MainWindow).HandleNoteWindowClosed;
            noteCard.Padding = new Thickness(0, 0, 0, 7);
            noteCard.MouseDoubleClick += ((MainWindow)App.Current.MainWindow).HandleNoteCardMouseDoubleClick;
            noteCard.BentRectangleTop.Visibility = Visibility.Visible;
            noteCard.BentRectangleBottom.Visibility = Visibility.Visible;
            ((MainWindow)App.Current.MainWindow).mwvm.NoteCards.Insert(0, noteCard);
        }

        private void HandleMouseDownRow(object sender, MouseButtonEventArgs e)
        {
            var windowBar = sender as Border;
            Point pointToWindow = e.GetPosition(this);
            Point mousePosition = PointToScreen(pointToWindow);

            if (e.ChangedButton == MouseButton.Left && CurrentWindow.WindowState == WindowState.Maximized)
            {
                CurrentWindow.WindowState = WindowState.Normal;

                CurrentWindow.Left = mousePosition.X - windowBar.ActualWidth / 2;
                CurrentWindow.Top = mousePosition.Y - windowBar.ActualHeight / 2;
                CurrentWindow.DragMove();
            }
            else if (e.ChangedButton == MouseButton.Left)
                CurrentWindow.DragMove();
        }

        private void HandleLayoutUpdated(object sender, EventArgs e)
        {
            if (CurrentWindow == null)
            {
                CurrentWindow = Window.GetWindow(this);
                CurrentWindow.StateChanged += HandleWindowStateChanged;
                HandleWindowStateChanged(CurrentWindow, EventArgs.Empty);
            }
        }
    }
}
