using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
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
    /// Interaction logic for ImageButton.xaml
    /// </summary>
    public partial class ImageButton : Button
    {
        public MenuItem DeleteItem;
        public string ImageLocation;
        public readonly System.Drawing.Image ButtonImage;
        public ImageButton(System.Drawing.Image buttonImage, string imageLocation)
        {
            Image imageControl = new Image();
            ButtonImage = buttonImage;
            ImageLocation = imageLocation;
            InitializeComponent();
            Style = FindResource("ImageButtonStyle") as Style;

            imageControl = ConvertDrawingToControlImage(buttonImage);
            imageControl.Stretch = Stretch.UniformToFill;
            Content = imageControl;

            // Bind button border to buttonImage width
            ApplyTemplate();
            BindWidth(imageControl);

            // Add ContextMenu for button
            AddContextMenu();
        }

        private Image ConvertDrawingToControlImage(System.Drawing.Image buttonImage)
        {
            Image convertedImage = new Image();
            MemoryStream imageStream = new MemoryStream();
            buttonImage.Save(imageStream, buttonImage.RawFormat);
            imageStream.Seek(0, SeekOrigin.Begin);
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = imageStream;
            bi.EndInit();
            convertedImage.Source = bi;
            return convertedImage;
        }

        private void BindWidth(Image buttonImage)
        {
            Border buttonBorder = Template.FindName("innerBorder", this) as Border;
            Binding widthBinding = new Binding("ActualWidth")
            {
                Source = buttonImage,
            };
            buttonBorder.SetBinding(WidthProperty, widthBinding);
        }

        private void AddContextMenu()
        {
            Style contextMenuStyle = FindResource("ContextMenuStyle") as Style;
            Style menuItemStyle = FindResource("MenuItemStyle") as Style;
            System.Windows.Shapes.Path deleteImageIcon = FindResource("Trashcan") as System.Windows.Shapes.Path;
            System.Windows.Shapes.Path saveImageIcon = FindResource("SaveIcon") as System.Windows.Shapes.Path;
            System.Windows.Shapes.Path viewImageIcon = FindResource("ViewImageIcon") as System.Windows.Shapes.Path;

            MenuItem viewImageItem = new MenuItem() { 
                Style = menuItemStyle,
                Header = "View image",
                Icon = viewImageIcon
            };
            MenuItem saveToDeviceItem = new MenuItem() {
                Style = menuItemStyle,
                Header = "Save to device",
                Icon = saveImageIcon
            };
            MenuItem deleteImageItem = new MenuItem() {
                Style = menuItemStyle,
                Header = "Delete image",
                Icon = deleteImageIcon
            };
            saveToDeviceItem.PreviewMouseLeftButtonUp += HandleSaveItemMouseLeftButtonUp;
            deleteImageItem.PreviewMouseLeftButtonUp += HandleDeleteItemMouseLeftButtonUp;
            DeleteItem = deleteImageItem;

            ContextMenu = new ContextMenu();
            ContextMenu.Style = contextMenuStyle;


            ContextMenu.Items.Add(viewImageItem);
            ContextMenu.Items.Add(saveToDeviceItem);
            ContextMenu.Items.Add(deleteImageItem);
        }

        public void HandleDeleteItemMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            StackPanel imageStackPanel = Parent as StackPanel;

            // Remove from StackPanel
            imageStackPanel.Children.Remove(this);

            // Remove from "Media" directory
            ButtonImage.Dispose();
            File.Delete(ImageLocation);
        }

        private void HandleSaveItemMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.AddExtension = true;
            saveImageDialog.FileName = DateTime.Now.Ticks.ToString();
            saveImageDialog.InitialDirectory = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Pictures");

            if (ButtonImage.RawFormat.Guid == ImageFormat.Png.Guid)
                saveImageDialog.Filter = "PNG|*.png;";
            else if (ButtonImage.RawFormat.Guid == ImageFormat.Jpeg.Guid)
                saveImageDialog.Filter = "JPEG|*.jpg;";

            saveImageDialog.FileOk += (s, args) => { ButtonImage.Save(saveImageDialog.FileName, ButtonImage.RawFormat); };
            saveImageDialog.ShowDialog();
        }
    }
}
