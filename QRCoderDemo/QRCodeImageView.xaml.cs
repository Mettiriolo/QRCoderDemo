using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QRCoderDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QRCodeImageView : Page, INotifyPropertyChanged
    {
        public QRCodeImageView()
        {
            this.InitializeComponent();
            GetDatas();
        }
        public ObservableCollection<ImageFileInfo> Images { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
         void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        void GetDatas()
        {
            Images = new ObservableCollection<ImageFileInfo>();
        }

        async Task<ImageFileInfo> GetImageFileAsync(string text)
        {
            ImageFileInfo imageFile = new ImageFileInfo();
            imageFile.Name = text;
            string level = "M";
            QRCodeGenerator.ECCLevel eccLevel = (QRCodeGenerator.ECCLevel)(level == "L" ? 0 : level == "M" ? 1 : level == "Q" ? 2 : 3);

            //Create raw qr code data
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, eccLevel);
            PngByteQRCode qrCodePng = new PngByteQRCode(qrCodeData);
            //byte[] qrCodeImagePng = qrCodePng.GetGraphic(20, new byte[] { 144, 201, 111 }, new byte[] { 118, 126, 152 });
            byte[] qrCodeImagePng = qrCodePng.GetGraphic(20);

            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(qrCodeImagePng);
                    await writer.StoreAsync();
                }
                var image = new BitmapImage();
                await image.SetSourceAsync(stream);

                imageFile.Source = image;
            }
            return imageFile;
        }

        private async void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is TextBox tb && e.Key.Equals(VirtualKey.Enter))
            {
                if (!Images.Any(a => a.Name.Equals(tb.Text)))
                {
                    var image = await GetImageFileAsync(tb.Text);
                    if (image != null)
                    {
                        Images.Add(image);
                        OnPropertyChanged("Images");
                        tb.Text = String.Empty;
                    }
                }
            }

        }
    }
}
