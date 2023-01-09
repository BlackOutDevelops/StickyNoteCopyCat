using NoteTaker.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoteTaker
{
    /// <summary>
    /// Interaction logic for NoteCard.xaml
    /// </summary>
    public partial class NoteCard : UserControl
    {
        public NoteCardViewModel vm = new NoteCardViewModel();
        public ImageButton NoteCardImageButton;
        public NoteCard()
        {
            InitializeComponent();
            DataContext = vm;
            vm.UpdatedTime = DateTime.Now;
            vm.PropertyChanged += (e, v) => DisplayUpdatedTimeCorrectly();
            NameScope.SetNameScope(NoteSettingsMenu, NameScope.GetNameScope(NoteCardUserControl));
        }

        private void DisplayUpdatedTimeCorrectly()
        {
            if (vm.UpdatedTime.Date == DateTime.Now.Date)
                NoteLastUpdated.Content = vm.UpdatedTime.ToShortTimeString();
            else if (vm.UpdatedTime.Date.Year != DateTime.Now.Date.Year)
                NoteLastUpdated.Content = vm.UpdatedTime.ToShortDateString();
            else
            {
                string monthAndDay = vm.UpdatedTime.GetDateTimeFormats('M')[0];
                NoteLastUpdated.Content = monthAndDay.Substring(0, 3) + " " + monthAndDay.Split()[1];
            }    
        }

        private void HandleMouseEntered(object sender, MouseEventArgs e)
        {
            NoteArea.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            NoteSettings.Visibility = Visibility.Visible;
            NoteLastUpdated.Visibility = Visibility.Hidden;
        }

        private void HandleMouseLeft(object sender, MouseEventArgs e)
        {
            NoteArea.Background = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            NoteSettings.Visibility = Visibility.Hidden;
            NoteLastUpdated.Visibility = Visibility.Visible;
        }

        private void HandleNoteSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            NoteSettingsMenu.PlacementTarget = sender as Button;
            NoteSettingsMenu.IsOpen = true;
        }

        private void HandleNoteSettingsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            Popup contextMenuParent = contextMenu.Parent as Popup;
            contextMenuParent.PopupAnimation = PopupAnimation.None;
        }

        private void HandleImageHolderLayoutUpdated(object sender, EventArgs e)
        {
            if (NoteCardImageCarousel.ImageStackPanel.Children.Count == 0)
            {
                ImageRow.Height = new GridLength(0);
                NoteCardText.Padding = new Thickness(10, 22, 10, 20);
            } 
        }
    }
}
