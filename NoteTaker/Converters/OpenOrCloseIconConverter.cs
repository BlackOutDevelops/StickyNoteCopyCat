using NoteTaker.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NoteTaker.Converters
{
    public class OpenOrCloseIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            NoteCard noteCard = values[0] as NoteCard;
            var openIcon = noteCard.TryFindResource("OpenIcon");
            var closeIcon = noteCard.TryFindResource("CloseIcon");
            return (noteCard).vm.IsOpen ?  closeIcon : openIcon;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
