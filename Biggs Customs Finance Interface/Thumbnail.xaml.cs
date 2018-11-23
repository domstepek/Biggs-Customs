using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Biggs_Customs_Finance_Interface
{
    /// <summary>
    /// Interaction logic for Thumbnail.xaml
    /// </summary>
    public partial class Thumbnail : Window
    {
        public Thumbnail(string image_location)
        {
            InitializeComponent();
            try
            {
                var source = new Uri(image_location);
                imagePictureBox.Source = new BitmapImage(source);
            } catch (Exception)
            {
                MessageBox.Show("Problem loading image");
            }
        }
    }
}
