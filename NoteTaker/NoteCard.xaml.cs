﻿using NoteTaker.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            vm.PropertyChanged += (e, v) => DisplayUpdatedTimeCorrectly();
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
            var note = sender as Border;

            note.Background = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            NoteSettings.Visibility = Visibility.Hidden;
            NoteLastUpdated.Visibility = Visibility.Visible;
        }


    }
}
