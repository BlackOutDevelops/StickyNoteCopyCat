using NoteTaker.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace NoteTaker
{
    /// <summary>
    /// Interaction logic for NoteCard.xaml
    /// </summary>
    public partial class NoteCard : UserControl
    {
        public NoteCardViewModel vm = new NoteCardViewModel();

        public bool IsOpen = false;

        public NoteCard()
        {
            InitializeComponent();
            DataContext = vm;
            vm.UpdatedTime = DateTime.Now;
        }

        private void HandleMouseEntered(object sender, MouseEventArgs e)
        {
            NoteArea.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            NoteSettings.Visibility = Visibility.Visible;
            NoteLastUpdated.Visibility = Visibility.Hidden;
        }

        private void HandleMouseLeft(object sender, MouseEventArgs e)
        {
            var note = sender as Border;

            note.Background = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            NoteSettings.Visibility = Visibility.Hidden;
            NoteLastUpdated.Visibility = Visibility.Visible;
        }


    }
}
